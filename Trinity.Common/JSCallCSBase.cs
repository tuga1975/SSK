using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;


[ComVisible(true)]
[ClassInterface(ClassInterfaceType.None)]
public class JSCallCSBase: Exception
{
    public WebBrowser _web = null;
    public Type _thisType = null;
    public JSCallCSBase()
    {

    }

    public void LoadPage(string file)
    {
        _web.LoadPageHtml(file);
    }

    private void actionThread(object pram)
    {
        try
        {
            var data = (object[])pram;
            var method = data[0].ToString();

            MethodInfo theMethod = _thisType.GetMethod(method);
            var dataReturn = theMethod.Invoke(this, (object[])data[2]);
            if (data[1] != null)
            {
                this._web.InvokeScript("callEventCallBack", data[1], JsonConvert.SerializeObject(dataReturn, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
        }
        catch (Exception ex)
        {
            this._web.InvokeScript("ShowMessageBox", ex.InnerException.Message);
        }
        this._web.SetLoading(false);
    }

    public void ClientCallServer(string method, string guidEvent, params object[] pram)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, guidEvent, pram });
    }
}
