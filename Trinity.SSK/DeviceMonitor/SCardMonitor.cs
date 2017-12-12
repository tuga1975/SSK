using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.DeviceMonitor
{
    class SCardMonitor
    {
        static List<string> _lstCardReaders = new List<string>();
        string _currentCardReader = string.Empty;
        PCSC.SCardMonitor _sCardMonitor;
        public SCardMonitor()
        {
        }
        internal void Start()
        {
            StartDeviceMonitor();
        }

        private void StartDeviceMonitor()
        {
            var factory = DeviceMonitorFactory.Instance;
            using (var deviceMonitor = factory.Create(SCardScope.System))
            {
                deviceMonitor.Initialized += OnInitialized;
                deviceMonitor.StatusChanged += OnStatusChanged;
                deviceMonitor.MonitorException += OnMonitorException;

                deviceMonitor.Start();
                //MethodInvoker test = new MethodInvoker(deviceMonitor.Start);
                //test.Invoke();
                //MethodInvoker((MethodInvoker)(() =>
                //{
                //    deviceMonitor.Start();
                //}));
                WaitUntilSpacebarPressed();

                deviceMonitor.Initialized -= OnInitialized;
                deviceMonitor.StatusChanged -= OnStatusChanged;
                deviceMonitor.MonitorException -= OnMonitorException;
            }
        }

        private void WaitUntilSpacebarPressed()
        {
            while (true) { }
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
                StartCardMonitor();
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
            StartCardMonitor();
        }

        private void StartCardMonitor()
        {
            if (_currentCardReader == string.Empty && _lstCardReaders.Count > 0)
            {
                var contextFactory = ContextFactory.Instance;
                _sCardMonitor = new PCSC.SCardMonitor(contextFactory, SCardScope.System);
                _sCardMonitor.Initialized += OnCardInitialized;
                _sCardMonitor.CardInserted += OnCardInserted;
                _sCardMonitor.CardRemoved += OnCardRemoved;

                _sCardMonitor.Start(_lstCardReaders[0]);
                _currentCardReader = _lstCardReaders[0];
            }
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("Card removed");
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            string cardInfo = GetCardUID();
            Debug.WriteLine($"Card UID: {cardInfo}");
        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            string cardInfo = GetCardUID();
            Debug.WriteLine($"Card UID: {cardInfo}");
        }
        private string GetCardUID()
        {
            if (_currentCardReader == string.Empty)
            {
                return string.Empty;
            }

            var contextFactory = ContextFactory.Instance;
            using (var context = contextFactory.Establish(SCardScope.System))
            {
                //var readerNames = context.GetReaders();
                //if (NoReaderFound(readerNames))
                //{
                //    Debug.WriteLine("GetCardUID: You need at least one reader in order to run this feature.");
                //    return string.Empty;
                //}

                //// always get reader _currentCardReader
                //var readerName = readerNames.Count() > 0 ? readerNames[0] : null;
                //if (readerName == null)
                //{
                //    return "You need at least one reader in order to run this feature.";
                //}
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
    }
}
