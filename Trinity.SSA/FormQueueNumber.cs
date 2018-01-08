using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using Trinity.DAL;

namespace SSA
{
    public partial class FormQueueNumber : Form
    {
        private WebBrowser wbQueueNumber = null;
        private static FormQueueNumber _instance = null;
        private JSCallCS _jsCallCS = null;

        public static FormQueueNumber GetInstance()
        {
            if (_instance == null)
            {
                _instance = new FormQueueNumber();
            }
            return _instance;
        }
        public FormQueueNumber()
        {
            InitializeComponent();
        }

        private void InitializeWebBrowser()
        {
            wbQueueNumber = new WebBrowser();
            this.Controls.Add(wbQueueNumber);
            wbQueueNumber.Dock = DockStyle.Fill;
            wbQueueNumber.DocumentCompleted += wbQueueNumber_DocumentCompleted;

            _jsCallCS = new JSCallCS(wbQueueNumber);
            wbQueueNumber.Url = new Uri(String.Format("file:///{0}/View/html/Layout_QueueNumber.html", CSCallJS.curDir));
            wbQueueNumber.ObjectForScripting = _jsCallCS;
        }

        private void wbQueueNumber_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            wbQueueNumber.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            wbQueueNumber.LoadPageHtml("QueueNumber.html");
            RefreshQueueNumbers();
        }
        public void ShowOnSecondaryScreen()
        {
            if (Screen.AllScreens.Count() > 1)
            {
                if (Screen.AllScreens[0].Primary)
                {

                    this.DesktopLocation = Screen.AllScreens[1].WorkingArea.Location;
                }
                else
                {
                    this.DesktopLocation = Screen.AllScreens[0].WorkingArea.Location;
                }
            }
            InitializeWebBrowser();
        }
        public void RefreshQueueNumbers()
        {
            DAL_QueueNumber dalQueue = new DAL_QueueNumber();
            var arrayQueue = dalQueue.GetAllQueueNumberByDate(DateTime.Today, EnumStations.SSK).Select(d => new Trinity.BE.Queue()
            {
                Status = d.QueueDetails.FirstOrDefault(qd => qd.Queue_ID == d.Queue_ID && qd.Station == EnumStations.SSK).Status,
                QueueNumber = d.QueuedNumber
            }).ToArray();

            string currentQueueNumber = string.Empty;
            for (int i = 0; i < arrayQueue.Length; i++)
            {
                if (arrayQueue[i].Status == EnumQueueStatuses.Processing)
                {
                    currentQueueNumber = arrayQueue[i].QueueNumber;
                    break;
                }
            }
            string[] waitingQueueNumbers = arrayQueue.Where(q => q.Status == EnumQueueStatuses.Waiting).OrderByDescending(d => d.Time).Select(d => d.QueueNumber).ToArray();
            wbQueueNumber.RefreshQueueNumbers(currentQueueNumber, waitingQueueNumbers);
        }
    }
}
