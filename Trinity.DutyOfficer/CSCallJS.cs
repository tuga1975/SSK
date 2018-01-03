using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DutyOfficer
{
    public static class CSCallJS
    {
        public static readonly string curDir = Directory.GetCurrentDirectory().ToLower().Replace("\\bin\\debug", string.Empty);


        public static void LoadPageHtml(this WebBrowser web, string file)
        {
            web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8));
        }
        public static void LoadPageHtml(this WebBrowser web, string file, object model)
        {
            web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8), JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
        }

        public static void InvokeScript(this WebBrowser web, string function, params object[] pram)
        {
            web.Invoke((MethodInvoker)(() =>
            {
                web.Document.InvokeScript(function, pram);
            }));

        }

        public static void RunScript(this WebBrowser web, string script)
        {
            web.InvokeScript("RunScript", script);
        }

        public static void DisplayLogoutButton(WebBrowser web, bool display)
        {
            web.InvokeScript("displayLogoutButton", display);
        }

        public static void DisplayNRICLogin(WebBrowser web)
        {
            web.InvokeScript("displayNRICLogin");
        }

        public static void PushNoti(this WebBrowser web, int count)
        {
            web.InvokeScript("pushNoti", count);
        }

        public static void SetLoading(this WebBrowser web, bool status)
        {
            web.InvokeScript("setLoading", status);
        }

        public static void UpdateNRICTextValue(WebBrowser web, string value)
        {
            web.InvokeScript("updateNRICTextValue", value);
        }

        public static void SetNRICFocus(WebBrowser web)
        {
            web.InvokeScript("setNRICFocus");
        }

        #region Queue Number
        public static void RefreshQueueNumbers(this WebBrowser web, string currentQueueNumber, string[] nextQueueNumberList)
        {
            var model = JsonConvert.SerializeObject(nextQueueNumberList, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            web.InvokeScript("refreshQueueNumbers", currentQueueNumber, model);
        }

        #endregion
    }
}
