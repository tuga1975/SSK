using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Dynamic;

public static class CSCallJS
{
    public static readonly string curDir = Directory.GetCurrentDirectory().ToLower().Replace("\\bin\\debug", string.Empty);
    

    public static void LoadPageHtml(this WebBrowser web, string file)
    {
        web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8));
    }
    public static void LoadPageHtml(this WebBrowser web, string file,object model)
    {
        web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8),JsonConvert.SerializeObject(model));
    }

    public static void InvokeScript(this WebBrowser web, string function,params object[] pram)
    {
        web.Invoke((MethodInvoker)(() =>
        {
            web.Document.InvokeScript(function, pram);
        }));
        
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

    /// <summary>
    /// Run script javascript
    /// </summary>
    /// <param name="web"></param>
    /// <param name="script">Example: alert(); $('element').text('A') ...</param>
    public static void RunScript(this WebBrowser web, string script)
    {
        web.InvokeScript("RunScript", script);
    }
}