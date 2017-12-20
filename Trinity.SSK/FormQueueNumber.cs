using System;
using System.Data;
using System.Linq;
using Newtonsoft.Json;
using System.Windows.Forms;
using Trinity.DAL;

namespace SSK
{
    public partial class FormQueueNumber : Form
    {
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

            _jsCallCS = new JSCallCS(this.wbQueueNumber);

            this.wbQueueNumber.Url = new Uri(String.Format("file:///{0}/View/html/Layout_QueueNumber.html", CSCallJS.curDir));
            this.wbQueueNumber.ObjectForScripting = _jsCallCS;
        }

        private void wbQueueNumber_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.wbQueueNumber.InvokeScript("createEvent", JsonConvert.SerializeObject(_jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            this.wbQueueNumber.LoadPageHtml("QueueNumber.html");
            RefreshQueueNumbers();
        }

        public void ShowOnSecondaryScreen()
        {
            if (Screen.AllScreens.Count() > 1)
            {
                if (Screen.AllScreens[0].Primary)
                {
                    this.Location = Screen.AllScreens[1].WorkingArea.Location;
                }
                else
                {
                    this.Location = Screen.AllScreens[0].WorkingArea.Location;
                }
            }
        }
        public void RefreshQueueNumbers()
        {
            DAL_QueueNumber dalQueue = new DAL_QueueNumber();
            var arrayQueue = dalQueue.GetAllQueueNumberByDate(DateTime.Today).Select(d => new Trinity.BE.Queue()
            {
                Status = d.Status,
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
            this.wbQueueNumber.RefreshQueueNumbers(currentQueueNumber, waitingQueueNumbers);
        }
    }
}
