using SSK.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;

namespace SSK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private DbContext.SSKCentralizedEntities sSKCentralizedEntities = new DbContext.SSKCentralizedEntities();
        private WebBrowser web = null;
        private Type thisType = null;

        public JSCallCS(WebBrowser web)
        {
            this.web = web;
            thisType = this.GetType();
        }

        public void LoadPage(string file)
        {
            web.LoadPageHtml(file);
        }


        public async Task GetQueuAsync(int a)
        {
            QueueHandler queuHandler = new QueueHandler();
            var queuValue = queuHandler.GetQueue();
            MessageBox.Show(queuValue);
        }
        
        public void LoadNotication()
        {
            //var model = sSKCentralizedEntities.Notifications.Where(item => item.Read != true).ToList();
            web.LoadPageHtml("Notication.html", new List<string>());
        }

        public void LoadProfile()
        {
            try
            {
                //profile model 
                var model = new Models.ProfileModel();
                web.LoadPageHtml("Profile.html", model);
            }
            catch (Exception)
            {

                LoadPage("Supervisee.html");
            }
        }

        public void SaveProfile(string param)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<Models.ProfileModel>(param);
                //save change to db
                //load profile page
                //send notify to case officer
                LoadProfile();
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        private void actionThread(object pram)
        {
            
            var data = (object[])pram;
            var method = data[0].ToString();
            MethodInfo theMethod = thisType.GetMethod(method);
            theMethod.Invoke(this, (object[])data[1]);
            web.SetLoading(false);
        }
        public void ClientCallServer(string method,params object[] pram)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, pram });
        }
    }
}
