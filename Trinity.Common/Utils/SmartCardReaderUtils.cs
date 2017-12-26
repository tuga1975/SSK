using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.Common.Common;
using Trinity.Common.Utils;

namespace Trinity.Common
{
    class SmartCardReaderUtils : DeviceUtils
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

            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(_cardReaders[0], SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        Debug.WriteLine(string.Format("GetCardUID: Could not connect to reader {0}:\n{1}",
                            _cardReaders[0],
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
    }
    class SmartCardReaderUtils_Old
    {
        IDeviceMonitor _smartCardReaderMonitor;
        static List<string> _lstCardReaders = new List<string>();
        string _currentCardReader = string.Empty;
        bool _smartCardReaderMonitorStarted;
        PCSC.SCardMonitor _sCardMonitor;
        CardInitializedEvent _onCardInitialized;
        CardInsertedEvent _onCardInserted;
        CardRemovedEvent _onCardRemoved;
        public bool StartSmartCardReaderMonitor()
        {
            try
            {
                var factory = DeviceMonitorFactory.Instance;
                _smartCardReaderMonitor = factory.Create(SCardScope.System);

                _smartCardReaderMonitor.Initialized += OnInitialized;
                _smartCardReaderMonitor.StatusChanged += OnStatusChanged;
                _smartCardReaderMonitor.MonitorException += OnMonitorException;

                _smartCardReaderMonitor.Start();

                return _smartCardReaderMonitorStarted = true;
            }
            catch
            {
                return _smartCardReaderMonitorStarted = false;
            }
        }

        private void OnMonitorException(object sender, DeviceMonitorExceptionEventArgs args)
        {
            Debug.WriteLine($"Exception: {args.Exception}");
        }

        private void OnStatusChanged(object sender, DeviceChangeEventArgs e)
        {
            foreach (var removed in e.DetachedReaders)
            {
                Debug.WriteLine($"Reader detached: {removed}");
                _lstCardReaders.Remove(removed);
                if (_lstCardReaders == null || _lstCardReaders.Count == 0)
                {
                    _currentCardReader = string.Empty;
                    _sCardMonitor.Cancel();
                }
            }

            foreach (var added in e.AttachedReaders)
            {
                Debug.WriteLine($"New reader attached: {added}");
                _lstCardReaders.Add(added);
            }
        }

        private void OnInitialized(object sender, DeviceChangeEventArgs e)
        {
            Debug.WriteLine("Current connected readers:");
            foreach (var name in e.AllReaders)
            {
                Debug.WriteLine(name);
            }
            _lstCardReaders.AddRange(e.AllReaders);
        }

        public bool StartCardMonitor(CardInitializedEvent onCardInitialized, CardInsertedEvent onCardInserted, CardRemovedEvent onCardRemoved)
        {
            try
            {
                if (!_smartCardReaderMonitorStarted)
                {
                    Debug.WriteLine("StartSmartCardReaderMonitor first please...");
                    return false;
                }

                if (_currentCardReader == string.Empty && _lstCardReaders.Count > 0)
                {
                    _onCardInitialized = onCardInitialized;
                    _onCardInserted = onCardInserted;
                    _onCardRemoved = onCardRemoved;

                    var contextFactory = ContextFactory.Instance;
                    _sCardMonitor = new PCSC.SCardMonitor(contextFactory, SCardScope.System);
                    _sCardMonitor.Initialized += _onCardInitialized;
                    _sCardMonitor.CardInserted += _onCardInserted;
                    _sCardMonitor.CardRemoved += _onCardRemoved;

                    _sCardMonitor.Start(_lstCardReaders[0]);
                    _currentCardReader = _lstCardReaders[0];

                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetCardUID()
        {
            if (_currentCardReader == string.Empty)
            {
                return string.Empty;
            }

            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                using (var rfidReader = new SCardReader(context))
                {
                    var sc = rfidReader.Connect(_currentCardReader, SCardShareMode.Shared, SCardProtocol.Any);
                    if (sc != SCardError.Success)
                    {
                        Debug.WriteLine(string.Format("GetCardUID: Could not connect to reader {0}:\n{1}",
                            _currentCardReader,
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

        public void Dispose()
        {
            if (_smartCardReaderMonitor != null)
            {
                _smartCardReaderMonitor.Initialized -= OnInitialized;
                _smartCardReaderMonitor.StatusChanged -= OnStatusChanged;
                _smartCardReaderMonitor.MonitorException -= OnMonitorException;
                _smartCardReaderMonitor.Cancel();

                _currentCardReader = string.Empty;

                if (_sCardMonitor != null)
                {
                    _sCardMonitor.Initialized -= _onCardInitialized;
                    _sCardMonitor.CardInserted -= _onCardInserted;
                    _sCardMonitor.CardRemoved -= _onCardRemoved;
                    _sCardMonitor.Cancel();
                }
            }
        }

        public void StopSCardMonitor()
        {
            _sCardMonitor.Initialized -= _onCardInitialized;
            _sCardMonitor.CardInserted -= _onCardInserted;
            _sCardMonitor.CardRemoved -= _onCardRemoved;
            _sCardMonitor.Cancel();
        }
    }
}
