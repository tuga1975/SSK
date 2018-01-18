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
        // monitor - need to be refactor
        List<string> _cardReaders;
        PCSC.SCardMonitor _sCardMonitor;

        // 
        string _readerName;

        // authentication keys
        byte[] _authenticationKeys = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        string _passphrase;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SmartCardReaderUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardReaderUtils()
        {
            _cardReaders = new List<string>();

            _readerName = EnumDeviceNames.SmartCardContactlessReader;

            _passphrase = "TVO@2018";
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
            string cardReaderName = EnumDeviceNames.SmartCardContactlessReader;

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

        public bool ReadAllData_MifareClassic(ref SmartCardData_Original smartCardData_Original)
        {
            try
            {
                Debug.WriteLine("starting ReadAllData_MifareClassic...");

                string readerName = EnumDeviceNames.SmartCardContactlessReader;

                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);

                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // key slot
                byte MSB = 0x00;

                // create mifare card
                MifareCard card = CreateMifareCardInstance(MSB);

                if (card == null)
                {
                    throw new Exception("Can not create card instance with keys: " + BitConverter.ToString(_authenticationKeys));
                }

                // loop all blocks read/write to read binary data
                StringBuilder sbData = new StringBuilder();
                int currentSector = -1; // authenticate each sector, not block
                foreach (var block in lstBlocks.Where(block => block.BlockType == EnumBlockTypes.Data))
                {
                    if (currentSector != block.Sector_Index)
                    {
                        currentSector = block.Sector_Index;

                        // authentication
                        // typeB
                        bool authSuccessful = card.Authenticate(MSB, block.Block_Index_Hex, KeyType.KeyA, 0x00);
                        if (!authSuccessful)
                        {
                            throw new Exception("AUTHENTICATE failed at block " + block.Block_Index_Dec + " with key " + BitConverter.ToString(_authenticationKeys));
                        }
                    }

                    // get data 
                    byte[] result = card.ReadBinary(MSB, block.Block_Index_Hex, 16);

                    // display data writen
                    string dataString = (result != null) ? Encoding.UTF8.GetString(result) : string.Empty;
                    string dateBit = (result != null) ? BitConverter.ToString(result) : string.Empty;
                    //sbData.AppendLine($"Block {block.Block_Index_Dec}: " + dateBit);
                    sbData.Append(dataString);
                }

                string encryptedData = sbData.ToString().Trim();
                //encryptedData = encryptedData.Replace('-', '+');
                //encryptedData = encryptedData.Replace('_', '/');

                //// decrypting result
                //var decrytedtData = CommonUtil.DecryptString(encryptedData, _passphrase);
                // set card data
                //_cardData = JsonConvert.DeserializeObject<SmartCardData_Original>(encryptedData);
                smartCardData_Original = JsonConvert.DeserializeObject<SmartCardData_Original>(encryptedData);

                // return value
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadAllData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        public bool ReadAllData_MifareClassic_ZXPSeries9(ref string data)
        {
            try
            {
                Debug.WriteLine("starting ReadAllData_MifareClassic_ZXPSeries9...");

                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);

                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // MSB
                byte MSB = 0x60;

                // Create card
                MifareCard card = CreateMifareCardInstance(MSB);

                if (card == null)
                {
                    throw new Exception("Can not create card instance with keys: " + BitConverter.ToString(_authenticationKeys));
                }

                // loop all blocks read/write to read binary data
                StringBuilder sbData = new StringBuilder();
                foreach (var block in lstBlocks)
                {
                    // authentication
                    // typeB
                    bool authSuccessful = card.Authenticate(MSB, block.Block_Index_Hex, KeyType.KeyA, 0x00);
                    if (!authSuccessful)
                    {
                        throw new Exception("AUTHENTICATE failed at block " + block.Block_Index_Dec + " with key " + BitConverter.ToString(_authenticationKeys));
                        //Debug.WriteLine("AUTHENTICATE failed at block " + block.Block_Index_Sector_Dec + " with key " + BitConverter.ToString(authenticationKeys));
                        //continue;
                    }

                    // get data 
                    byte[] result = card.ReadBinary(MSB, block.Block_Index_Hex, 16);

                    // display data writen
                    string datastring = (result != null) ? Encoding.UTF8.GetString(result) : string.Empty;
                    string dateBit = (result != null) ? BitConverter.ToString(result) : string.Empty;
                    sbData.AppendLine($"Block {block.Block_Index_Hex}: " + dateBit);
                }

                // return value
                data = sbData.ToString();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReadAllData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        private MifareCard CreateMifareCardInstance(byte MSB, string readerName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(readerName))
                {
                    readerName = _readerName;
                }

                // create card context
                var contextFactory = ContextFactory.Instance;
                var context = contextFactory.Establish(SCardScope.System);

                // create IsoReader
                var isoReader = new IsoReader(context, readerName, SCardShareMode.Shared, SCardProtocol.Any, false);

                // create mifare card and load authentication keys
                MifareCard card = new MifareCard(isoReader);
                bool loadKeySuccessful = card.LoadKey(
                    KeyStructure.VolatileMemory,
                    MSB, 
                    _authenticationKeys // key
                );

                // load authentication keys
                if (!loadKeySuccessful)
                {
                    throw new Exception("LOAD KEY failed: " + BitConverter.ToString(_authenticationKeys));
                }

                return card;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CreateMifareCardInstance_Printer" + ex.ToString());
                return null;
            }
        }

        public List<MifareCard_Block> GetAll_Blocks_MifareClassic(EnumSmartCardTypes cardType)
        {
            try
            {
                // get list blocks will be read/write (decimal index)
                List<MifareCard_Block> lstBlocks = new List<MifareCard_Block>();
                MifareCard_Block block;

                if (cardType == EnumSmartCardTypes.MifareClassic1K)
                {
                    // classic 1k has 16 sectors
                    // sector 0: block 0: manufactory block, block 1,2: data, block 3: sector trailer
                    // sector 1 to sector 16: 4 blocks/sector (Sector trailer is at block 3)
                    for (int sectorIndex = 0; sectorIndex < 16; sectorIndex++)
                    {
                        // block 0 at sector 0: keys stored, must not be read/write
                        // 4 blocks per sector, read/write at blocks 0,1,2, trailer block (authentication keys stored) at block 3
                        for (int blockIndex = 0; blockIndex < 4; blockIndex++)
                        {
                            // calculate block to add to lstBlocks
                            block = new MifareCard_Block();
                            block.Sector_Index = sectorIndex;
                            block.Block_Index_Dec = (sectorIndex * 4) + blockIndex;

                            // calculate Block_Hex_Index_Card
                            block.Block_Index_Hex = GetBlockIndex_Card_Hex(block.Block_Index_Dec);

                            // EnumBlockTypes
                            if (block.Sector_Index == 0 && blockIndex == 0)
                            {
                                block.BlockType = EnumBlockTypes.ManufacturerBlock;
                            }
                            else if (blockIndex == 3)
                            {
                                block.BlockType = EnumBlockTypes.SectorTrailer;
                            }
                            else
                            {
                                block.BlockType = EnumBlockTypes.Data;
                            }

                            // add block to list
                            lstBlocks.Add(block);
                        }
                    }
                }
                else if (cardType == EnumSmartCardTypes.MifareClassic4K)
                {
                    // sectors structure:
                    // refer: https://www.nxp.com/docs/en/data-sheet/MF1S70YYX_V1.pdf
                    // classic 4k has 39 sectors
                    // sector 0: block 0: manufactory block, block 1,2: data, block 3: sector trailer
                    // sector 1 to sector 31: 4 blocks/sector (Sector trailer is at block 3)
                    // sector 32 to sector 39: 16 blocks/sector (Sector trailer is at block 15)

                    // get list block from sector 0 to 31 (32 sectors)
                    for (int sectorIndex = 0; sectorIndex < 32; sectorIndex++)
                    {
                        // block 0 at sector 0: keys stored, must not be read/write
                        // 4 blocks per sector, read/write at blocks 0,1,2, trailer block (authentication keys stored) at block 3
                        for (int blockIndex = 0; blockIndex < 4; blockIndex++)
                        {
                            // calculate block to add to lstBlocks
                            block = new MifareCard_Block();
                            block.Sector_Index = sectorIndex;
                            block.Block_Index_Dec = (sectorIndex * 4) + blockIndex;

                            // calculate Block_Hex_Index_Card
                            block.Block_Index_Hex = GetBlockIndex_Card_Hex(block.Block_Index_Dec);

                            // EnumBlockTypes
                            if (block.Sector_Index == 0 && blockIndex == 0)
                            {
                                block.BlockType = EnumBlockTypes.ManufacturerBlock;
                            }
                            else if (blockIndex == 3)
                            {
                                block.BlockType = EnumBlockTypes.SectorTrailer;
                            }
                            else
                            {
                                block.BlockType = EnumBlockTypes.Data;
                            }

                            // add block to list
                            lstBlocks.Add(block);
                        }
                    }

                    // get list block from sector 32 to 39 (8 blocks)
                    // start from block 128 = lstBlocks_Decimal.Length
                    int nextSectorIndex = lstBlocks.Max(m => m.Sector_Index) + 1;
                    int nextBlockIndex_Dec = lstBlocks.Count();
                    for (int sectorIndex = 0; sectorIndex < 8; sectorIndex++)
                    {
                        // block 0 at sector 0: keys stored, must not be read/write
                        // 16 blocks per sector, read/write at blocks 0,1,2...14 trailer block (authentication keys stored) at block 15
                        for (int blockIndex = 0; blockIndex < 16; blockIndex++)
                        {
                            // calculate block to add to lstBlocks
                            block = new MifareCard_Block();
                            block.Sector_Index = nextSectorIndex + sectorIndex;
                            block.Block_Index_Dec = nextBlockIndex_Dec + (sectorIndex * 16) + blockIndex;

                            // calculate Block_Hex_Index_Card
                            block.Block_Index_Hex = GetBlockIndex_Card_Hex(block.Block_Index_Dec);

                            // EnumBlockTypes
                            if (blockIndex == 15)
                            {
                                block.BlockType = EnumBlockTypes.SectorTrailer;
                            }
                            else
                            {
                                block.BlockType = EnumBlockTypes.Data;
                            }

                            // add block to list
                            lstBlocks.Add(block);
                        }
                    }
                }

                return lstBlocks;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetAll_Blocks_MifareClassic exception: " + ex.ToString());
                return null;
            }
        }

        private byte GetBlockIndex_Card_Hex(int block_Index_Sector_Dec)
        {
            try
            {
                string hexOutput = String.Format("{0:X}", block_Index_Sector_Dec);
                byte block_Index_Card_Hex = Convert.ToByte(hexOutput, 16);

                return block_Index_Card_Hex;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetBlockIndex_Card_Hex exception: " + ex.ToString());
                return new byte();
            }
        }

        public bool WriteDutyOfficerData(DutyOfficerData data, string readerName = null)
        {
            try
            {
                if (readerName == null)
                {
                    readerName = _readerName;
                }

                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);
                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // calculate storage size of smartcard
                int totalBytes = lstBlocks.Count(block => block.BlockType == EnumBlockTypes.Data) * 16;

                SmartCardData_Original cardData = new SmartCardData_Original()
                {
                    DutyOfficerData = data
                };
                
                // convert object to json
                var jsonData = JsonConvert.SerializeObject(cardData);

                // encrypting
                var encryptedString = CommonUtil.EncryptString(jsonData, _passphrase);

                // check length of encryptContent, remove last HistoricalRecord
                while (encryptedString.Length > totalBytes)
                {
                    throw new Exception("Data length of DutyOfficerData is overflow");
                }

                // data to write
                byte[] byteData = Encoding.ASCII.GetBytes(jsonData);

                bool result = WriteData_MifareClassic(byteData, readerName);

                // return value
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        public bool WriteSuperviseeBiodata(SuperviseeBiodata data, string readerName = null)
        {
            try
            {
                if (readerName == null)
                {
                    readerName = _readerName;
                }

                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);
                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // calculate storage size of smartcard
                int totalBytes = lstBlocks.Count(block => block.BlockType == EnumBlockTypes.Data) * 16;

                // create SmartCardData object
                SmartCardData_Original cardData = new SmartCardData_Original()
                {
                    SuperviseeBiodata = data
                };

                // convert object to json
                var jsonData = JsonConvert.SerializeObject(cardData);

                // encrypting
                var encryptedString = CommonUtil.EncryptString(jsonData, _passphrase);

                // check length of encryptContent, remove last HistoricalRecord
                while (encryptedString.Length > totalBytes)
                {
                    // remove last activities
                    if (cardData.HistoricalRecords != null || cardData.HistoricalRecords.Count() > 0)
                    {
                        cardData.HistoricalRecords.Remove(cardData.HistoricalRecords.Last());
                    }

                    // convert new data to json
                    jsonData = JsonConvert.SerializeObject(cardData);

                    // encrypting
                    encryptedString = CommonUtil.EncryptString(jsonData, _passphrase);
                }

                // data to write
                byte[] byteData = Encoding.ASCII.GetBytes(jsonData);

                bool result = WriteData_MifareClassic(byteData, readerName);

                // return value
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        public bool AppendHistoricalRecord(SmartCardData_Original cardData, HistoricalRecord record)
        {
            try
            {
                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);
                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // calculate storage size of smartcard
                int totalBytes = lstBlocks.Count(block => block.BlockType == EnumBlockTypes.Data) * 16;

                // create SmartCardData object
                if (cardData == null)
                {
                    throw new Exception("SuperviseeBiodata in the card can not be null");
                }
                else
                {
                    if (cardData.HistoricalRecords == null || cardData.HistoricalRecords.Count == 0)
                    {
                        List<HistoricalRecord> historicalRecords = new List<HistoricalRecord>();
                        historicalRecords.Add(record);

                        cardData.HistoricalRecords = historicalRecords;
                    }
                    else
                    {
                        cardData.HistoricalRecords.Insert(0, record);
                    }
                }

                // convert object to json
                var jsonData = JsonConvert.SerializeObject(cardData);

                // encrypting
                var encryptedString = CommonUtil.EncryptString(jsonData, _passphrase);

                // check length of encryptContent, remove last HistoricalRecord
                while (encryptedString.Length > totalBytes)
                {
                    // remove last record
                    if (cardData.HistoricalRecords != null || cardData.HistoricalRecords.Count() > 0)
                    {
                        cardData.HistoricalRecords.Remove(cardData.HistoricalRecords.Last());
                    }

                    // convert new data to json
                    jsonData = JsonConvert.SerializeObject(cardData);

                    // encrypting
                    encryptedString = CommonUtil.EncryptString(jsonData, _passphrase);
                }

                // data to write
                byte[] byteData = Encoding.ASCII.GetBytes(jsonData);

                bool result = WriteData_MifareClassic(byteData);

                // return value
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
        private bool WriteData_MifareClassic(byte[] data, string readerName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(readerName))
                {
                    readerName = _readerName;
                }

                // get list blocks will be read/write
                List<MifareCard_Block> lstBlocks = GetAll_Blocks_MifareClassic(EnumSmartCardTypes.MifareClassic4K);
                lstBlocks = lstBlocks?.Where(block => block.BlockType == EnumBlockTypes.Data).ToList();

                if (lstBlocks == null || lstBlocks.Count() == 0)
                {
                    throw new Exception("Can not get card blocks");
                }

                // MSB, default for ACS ACR122 0
                byte MSB = 0x00;
                if (readerName.Equals(EnumDeviceNames.SmartCardPrinterContactlessReader))
                {
                    MSB = 0x60;
                }

                // create mifare card
                MifareCard card = CreateMifareCardInstance(MSB, readerName);

                if (card == null)
                {
                    throw new Exception("Can not create card instance with keys: " + BitConverter.ToString(_authenticationKeys));
                }

                // verify card
                //bool verifycationResult = VerifyCardAuthentication(card, MSB, lstBlocks);

                //if (!verifycationResult)
                //{
                //    throw new Exception("Authentication failed");
                //}

                // loop to write binary data to mifare card
                int writtenBlock = 0;
                int currentSector = -1;
                foreach (var block in lstBlocks)
                {
                    if (currentSector != block.Sector_Index)
                    {
                        currentSector = block.Sector_Index;

                        // authentication
                        // typeB
                        bool authSuccessful = card.Authenticate(MSB, block.Block_Index_Hex, KeyType.KeyA, 0x00);
                        if (!authSuccessful)
                        {
                            throw new Exception("AUTHENTICATE failed at block " + block.Block_Index_Dec + " with key " + BitConverter.ToString(_authenticationKeys));
                        }
                    }

                    // starting write binary data
                    byte[] dataToWrite = GetDataToWrite(data, writtenBlock);
                    var updateSuccessful = card.UpdateBinary(MSB, block.Block_Index_Hex, dataToWrite);

                    if (!updateSuccessful)
                    {
                        throw new Exception("UPDATE BINARY failed at block " + block.Block_Index_Dec + " with value " + BitConverter.ToString(dataToWrite));
                    }

                    writtenBlock++;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteBinaryData_MifareClassic exception: " + ex.ToString());
                return false;
            }
        }

        private bool VerifyCardAuthentication(MifareCard card, byte MSB, List<MifareCard_Block> lstBlocks)
        {
            try
            {
                bool authSuccessful = true;
                foreach (var block in lstBlocks)
                {
                    // authentication, typeA for classic 4K
                    authSuccessful = card.Authenticate(MSB, block.Block_Index_Hex, KeyType.KeyB, 0x00);

                    if (!authSuccessful)
                    {
                        throw new Exception("AUTHENTICATE failed at block " + block.Block_Index_Dec + " with key " + BitConverter.ToString(_authenticationKeys));
                    }
                }
                return authSuccessful;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("VerifyCardAuthentication exception: " + ex.ToString());
                return false;
            }
        }

        private byte[] GetDataToWrite(byte[] sourceData, int writtenBlock)
        {
            // calculate starting index of source data will be written into this block
            int startIndex = writtenBlock * 16;

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
                    returnValue[i] = 0x00;
                }
            }

            // return value
            return returnValue;
        }
    }
}
