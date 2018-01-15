using Newtonsoft.Json;
using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Trinity.Common.Common;
using Trinity.Common.Utils;

namespace Trinity.Common
{
    public class SmartCardReaderUtils : DeviceUtils
    {
        List<string> _cardReaders;
        PCSC.SCardMonitor _sCardMonitor;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SmartCardReaderUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardReaderUtils()
        {
            _cardReaders = new List<string>();
        }

        public static SmartCardReaderUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCardReaderUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public event Action OnNoDeviceConnected;
        public event EventHandler<SmartCardReaderConnectedArgs> OnDeviceConnected;

        protected virtual void RaiseNoDeviceConnectedEvent()
        {
            OnNoDeviceConnected?.Invoke();
        }

        protected virtual void RaiseDeviceConnectedEvent(SmartCardReaderConnectedArgs e)
        {
            OnDeviceConnected?.Invoke(this, e);
        }

        public bool StartSmartCardReaderMonitor()
        {
            try
            {
                var factory = DeviceMonitorFactory.Instance;
                var smartCardReaderMonitor = factory.Create(SCardScope.System);

                smartCardReaderMonitor.Initialized += OnInitialized;
                smartCardReaderMonitor.StatusChanged += OnStatusChanged;
                smartCardReaderMonitor.MonitorException += OnMonitorException;

                smartCardReaderMonitor.Start();

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartSmartCardReaderMonitor Exception: " + ex.Message);
                return false;
            }
        }

        public SCardReaderStartResult StartSmartCardMonitor(CardInitializedEvent onCardInitialized, CardInsertedEvent onCardInserted, CardRemovedEvent onCardRemoved)
        {
            SCardReaderStartResult returnValue = new SCardReaderStartResult()
            {
                Success = false
            };

            try
            {
                if (_cardReaders != null && _cardReaders.Count > 0)
                {
                    var contextFactory = ContextFactory.Instance;
                    _sCardMonitor = new PCSC.SCardMonitor(contextFactory, SCardScope.System);
                    _sCardMonitor.Initialized += onCardInitialized;
                    _sCardMonitor.CardInserted += onCardInserted;
                    _sCardMonitor.CardRemoved += onCardRemoved;

                    _sCardMonitor.Start(_cardReaders[0]);

                    returnValue.Success = true;
                    return returnValue;
                }

                // if card reader is not found
                returnValue.FailedInfo = new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.SmartCardReaderNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.SmartCardReaderNull)
                };
                return returnValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("StartCardMonitor Exception: " + ex.Message);
                returnValue.FailedInfo = new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                };
                return returnValue;
            }
        }

        public string GetCardUID()
        {
            if (_cardReaders == null || _cardReaders.Count == 0)
            {
                return string.Empty;
            }

            // return card UID
            return GetCardUID(_cardReaders[0]);
        }

        public string GetCardUID(string cardReaderName)
        {
            try
            {
                var contextFactory = ContextFactory.Instance;
                using (var context = contextFactory.Establish(SCardScope.System))
                {
                    using (var rfidReader = new SCardReader(context))
                    {
                        var sc = rfidReader.Connect(cardReaderName, SCardShareMode.Shared, SCardProtocol.Any);
                        if (sc != SCardError.Success)
                        {
                            Debug.WriteLine(string.Format("GetCardUID: Could not connect to reader {0}:\n{1}",
                                cardReaderName,
                                SCardHelper.StringifyError(sc)));
                            return string.Empty;
                        }

                        var apdu = new CommandApdu(IsoCase.Case2Short, rfidReader.ActiveProtocol)
                        {
                            CLA = 0xFF,
                            Instruction = InstructionCode.GetData,
                            P1 = 0x00,
                            P2 = 0x00,
                            Le = 0 // We don't know the ID tag size
                        };

                        sc = rfidReader.BeginTransaction();
                        if (sc != SCardError.Success)
                        {
                            Debug.WriteLine("GetCardUID: Could not begin transaction.");
                            return string.Empty;
                        }

                        Debug.WriteLine("Retrieving the UID .... ");

                        var receivePci = new SCardPCI(); // IO returned protocol control information.
                        var sendPci = SCardPCI.GetPci(rfidReader.ActiveProtocol);

                        var receiveBuffer = new byte[256];
                        var command = apdu.ToArray();

                        sc = rfidReader.Transmit(
                            sendPci, // Protocol Control Information (T0, T1 or Raw)
                            command, // command APDU
                            receivePci, // returning Protocol Control Information
                            ref receiveBuffer); // data buffer

                        if (sc != SCardError.Success)
                        {
                            Debug.WriteLine("Error: " + SCardHelper.StringifyError(sc));
                            return string.Empty;
                        }

                        var responseApdu = new ResponseApdu(receiveBuffer, IsoCase.Case2Short, rfidReader.ActiveProtocol);
                        string cardUID = responseApdu.HasData ? BitConverter.ToString(responseApdu.GetData()) : "No uid received";

                        rfidReader.EndTransaction(SCardReaderDisposition.Leave);
                        rfidReader.Disconnect(SCardReaderDisposition.Reset);

                        return cardUID;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetCardUID exception: CardReaderName: " + cardReaderName + ". Error: " + ex.ToString());
                return string.Empty;
            }
        }

        public void StopSmartCardMonitor()
        {
            if (_sCardMonitor != null)
            {
                _sCardMonitor.Cancel();
            }
        }

        private void OnMonitorException(object sender, DeviceMonitorExceptionEventArgs args)
        {
            Debug.WriteLine($"StartSmartCardReaderMonitor Exception: {args.Exception}");
        }

        private void OnStatusChanged(object sender, DeviceChangeEventArgs e)
        {
            foreach (var removed in e.DetachedReaders)
            {
                Debug.WriteLine($"Reader detached: {removed}");
                _cardReaders.Remove(removed);

                if (_cardReaders == null || _cardReaders.Count() == 0)
                {
                    // Raise NoDeviceConnected Event when system have no conntected cared reader
                    RaiseNoDeviceConnectedEvent();
                }
            }

            foreach (var added in e.AttachedReaders)
            {
                Debug.WriteLine($"New reader attached: {added}");
                _cardReaders.Add(added);

                // raise RaiseDeviceConnectedEvent
                List<string> newReaders = new List<string>();
                newReaders.Add(_cardReaders.Last());
                RaiseDeviceConnectedEvent(new SmartCardReaderConnectedArgs()
                {
                    NewReaders = newReaders,
                    Readers = _cardReaders
                });
            }
        }

        private void OnInitialized(object sender, DeviceChangeEventArgs e)
        {
            Debug.WriteLine("Current connected readers:");
            foreach (var name in e.AllReaders)
            {
                Debug.WriteLine(name);
            }

            _cardReaders.AddRange(e.AllReaders);

            if (_cardReaders == null || _cardReaders.Count() == 0)
            {
                // Raise NoDeviceConnected Event when system have no conntected cared reader
                RaiseNoDeviceConnectedEvent();
            }
            else
            {
                // raise RaiseDeviceConnectedEvent
                RaiseDeviceConnectedEvent(new SmartCardReaderConnectedArgs()
                {
                    NewReaders = _cardReaders,
                    Readers = _cardReaders
                });
            }
        }

        public override EnumDeviceStatuses[] GetDeviceStatus()
        {
            if (_cardReaders == null || _cardReaders.Count() == 0)
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }

            return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
        }

        public bool IsConnected(string cardReaderName)
        {
            if (_cardReaders == null || _cardReaders.Count() == 0)
            {
                return false;
            }
            else if (!_cardReaders.Contains(cardReaderName))
            {
                return false;
            }

            return true;
        }

        public string ReadAllData_MifareClassic(string readerName)
        {
            try
            {
                Debug.WriteLine("starting ReadAllData_MifareClassic...");
                // key slot
                byte MSB = 0x00;
                // authentication keys
                var authenticationKeys = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                // create card context
                var contextFactory = ContextFactory.Instance;
                var context = contextFactory.Establish(SCardScope.System);

                // create IsoReader
                var isoReader = new IsoReader(context, readerName, SCardShareMode.Shared, SCardProtocol.Any, false);

                // number of sectors will be read (1k = 16 sectors)
                int sectors = 16;

                // get list blocks will be read/write
                List<int> lstBlocks_Decimal = GetRead_Write_Blocks_MifareClassic(sectors);

                if (lstBlocks_Decimal == null || lstBlocks_Decimal.Count() == 0)
                {
                    return string.Empty;
                }

                // create mifare card and load authentication keys
                MifareCard card = new MifareCard(isoReader);
                bool loadKeySuccessful = card.LoadKey(
                    KeyStructure.VolatileMemory,
                    MSB, // first key slot
                    authenticationKeys // key
                );

                // load authentication keys
                if (!loadKeySuccessful)
                {
                    throw new Exception("LOAD KEY failed: " + BitConverter.ToString(authenticationKeys));
                }

                // loop all blocks read/write to read binary data
                StringBuilder sbData = new StringBuilder();
                foreach (var blockIndex_Decimal in lstBlocks_Decimal)
                {
                    // get block index in hex
                    byte blockIndex_Hex = Convert.ToByte(blockIndex_Decimal.ToString(), 16);

                    // authentication
                    // typeB
                    bool authSuccessful = card.Authenticate(MSB, blockIndex_Hex, KeyType.KeyB, 0x00);
                    if (!authSuccessful)
                    {
                        //throw new Exception("AUTHENTICATE failed at block " + blockIndex_Decimal + " with key " + BitConverter.ToString(authenticationKeys));
                        Debug.WriteLine("AUTHENTICATE failed at block " + blockIndex_Decimal + " with key " + BitConverter.ToString(authenticationKeys));
                        continue;
                    }

                    // get data 
                    byte[] result = card.ReadBinary(MSB, blockIndex_Hex, 16);

                    // display data writen
                    string data = (result != null) ? Encoding.UTF8.GetString(result) : string.Empty;
                    string dateBit = (result != null) ? BitConverter.ToString(result) : string.Empty;
                    sbData.AppendLine($"Block {blockIndex_Decimal}: " + data);
                }
                
                // return value
                return sbData.ToString();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadAllData_MifareClassic exception: " + ex.ToString());
                return string.Empty;
            }
        }

        private List<int> GetRead_Write_Blocks_MifareClassic(int sectors)
        {
            try
            {
                // get list sectors will be read (decimal index)
                //List<int> lstSectors = new List<int>() { 0, 1, 2 };
                List<int> lstSectors = new List<int>();
                for (int i = 0; i < sectors; i++)
                {
                    lstSectors.Add(i);
                }

                // get list blocks will be read/write (decimal index)
                List<int> lstBlocks_Decimal = new List<int>();
                foreach (var sector in lstSectors)
                {
                    // block 0 at sector 0: keys stored, must not be read/write
                    // 4 blocks per sector, read/write at blocks 0,1,2, trailer block (authentication keys stored) at block 3
                    for (int i = 0; i < 3; i++)
                    {
                        // calculate block to add to lstBlocks
                        int block = (sector * 4) + i;

                        if (block > 0)
                        {
                            lstBlocks_Decimal.Add(block);
                        }
                    }
                }

                return lstBlocks_Decimal;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteData_MifareClassic exception: " + ex.ToString());
                return null;
            }
        }

        public bool WriteData(string printerName, PrintAndWriteSmartcardInfo_Demo data)
        {
            try
            {
                // set storage size of smartcard
                int totalBytes = 300;

                // convert object to json
                var jsonData = JsonConvert.SerializeObject(data);

                // encrypting
                //var encryptedString = CommonUtil.EncryptString(jsonData, passphrase);

                //// check length of encryptContent, remove overflow data
                //while (encryptedString.Length > totalBytes)
                //{
                //    // remove last activities
                //    if (data.CardHolderActivities != null || data.CardHolderActivities.Count() > 0)
                //    {
                //        data.CardHolderActivities.RemoveAt(data.CardHolderActivities.Count());
                //    }

                //    // convert new data to json
                //    jsonData = JsonConvert.SerializeObject(data);

                //    // encrypting
                //    encryptedString = CommonUtil.EncryptString(jsonData, passphrase);
                //}

                // starting write data
                if (jsonData.Length > totalBytes)
                {
                    jsonData = jsonData.Substring(0, totalBytes);
                }

                // data to write
                byte[] byteData = Encoding.ASCII.GetBytes(jsonData);
                //WriteData_MifareClassic(encryptedString);
                //WriteData_MifareClassic(jsonData);
                bool result = WriteData_MifareClassic(printerName, byteData);


                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Override binary data of all blocks of mifare classic 1k
        /// </summary>
        /// <param name="jsonValue">data to write (json format), maximum 1024 chars for card 1k, 4048 chars for card 4k</param>
        /// <returns></returns>
        private bool WriteData_MifareClassic(string readerName, byte[] data)
        {
            try
            {
                Debug.WriteLine("starting WriteBinaryData_MifareClassic...");

                // key slot
                byte MSB = 0x00;
                // authentication keys
                var authenticationKeys = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                // number of sectors will be read (1k = 16 sectors)
                int sectors = 16;

                // get list blocks will be read/write
                List<int> lstBlocks_Decimal = GetRead_Write_Blocks_MifareClassic(sectors);

                if (lstBlocks_Decimal == null || lstBlocks_Decimal.Count() == 0)
                {
                    return false;
                }

                // get mifare card instance
                MifareCard mifareCard = CreateMifareCardInstance(readerName, MSB, authenticationKeys);

                // loop to write binary data to mifare card
                Debug.WriteLine("stating write binary data");
                int startBlock = lstBlocks_Decimal.Min();
                int writtenBlock = 0;
                for (int i = startBlock; i < lstBlocks_Decimal.Max(); i++)
                {
                    // block to write: get block index in hex
                    byte blockIndex_Hex = Convert.ToByte(i.ToString(), 16);

                    // authentication, typeB
                    var authSuccessful = mifareCard.Authenticate(MSB, blockIndex_Hex, KeyType.KeyB, 0x00);

                    if (!authSuccessful)
                    {
                        Debug.WriteLine("AUTHENTICATE failed at block " + i + " with key " + BitConverter.ToString(authenticationKeys));
                        continue;
                    }

                    // starting write binary data
                    byte[] dataToWrite = GetDataToWrite(data, writtenBlock);
                    var updateSuccessful = mifareCard.UpdateBinary(MSB, blockIndex_Hex, dataToWrite);
                    writtenBlock++;

                    if (!updateSuccessful)
                    {
                        Debug.WriteLine("UPDATE BINARY failed at block " + i + " with value " + BitConverter.ToString(dataToWrite));
                        continue;
                    }
                }

                Debug.WriteLine("WriteBinaryData_MifareClassic successful");

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteBinaryData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        private byte[] GetDataToWrite(byte[] sourceData, int writtenBlock)
        {
            // calculate starting index of source data will be written into this block
            int startIndex = writtenBlock * 16;

            // if length of source data is out of index, return null
            //if (sourceData.Length < startIndex)
            //{
            //    return null;
            //}

            // calculate data will be written into this block
            byte[] returnValue = new byte[16];
            for (int i = startIndex; i < sourceData.Length && i < startIndex + 16; i++)
            {
                returnValue[i - startIndex] = sourceData[i];
            }

            // if data will be written is less than 16 bytes, fill data to the rest
            if (sourceData.Length - startIndex < 16)
            {
                int startIndexToFill = sourceData.Length - startIndex;

                // if startIndexToFill < 0: there is nothing to fill, create empty returnvalue (set startIndexToFill = 0)
                startIndexToFill = startIndexToFill < 0 ? 0 : startIndexToFill;

                // fill data to the rest
                for (int i = startIndexToFill; i < 16; i++)
                {
                    returnValue[i] = 0x20;
                }
            }

            // return value
            return returnValue;
        }

        private MifareCard CreateMifareCardInstance(string readerName, byte keySlot, byte[] authenticationKeys)
        {
            try
            {
                // create card context
                var contextFactory = ContextFactory.Instance;
                var context = contextFactory.Establish(SCardScope.System);

                // key slot:
                byte MSB = keySlot;

                // create IsoReader
                var isoReader = new IsoReader(context, readerName, SCardShareMode.Shared, SCardProtocol.Any, false);
                var card = new MifareCard(isoReader);
                var loadKeySuccessful = card.LoadKey(
                    KeyStructure.VolatileMemory,
                    MSB, // first key slot
                    authenticationKeys // key
                );

                if (!loadKeySuccessful)
                {
                    throw new Exception("LOAD KEY failed with key " + BitConverter.ToString(authenticationKeys));
                }

                return card;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateMifareCardInstance exception: " + ex.ToString());
                return null;
            }
        }
    }
}
