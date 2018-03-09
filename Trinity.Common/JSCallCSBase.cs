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
public class JSCallCSBase
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
    //private bool ArgumentListMatches(this MethodInfo m, Type[] args)
    //{
    //    // If there are less arguments, then it just doesn't matter.
    //    var pInfo = m.GetParameters();
    //    if (pInfo.Length < args.Length)
    //        return false;

    //    // Now, check compatibility of the first set of arguments.
    //    var commonArgs = args.Zip(pInfo, (margs, pinfo) => Tuple.Create(margs, pinfo.ParameterType));
    //    if (commonArgs.Where(t => !t.Item1.IsAssignableFrom(t.Item2)).Any())
    //        return false;

    //    // And make sure the last set of arguments are actually default!
    //    return pInfo.Skip(args.Length).All(p => p.IsOptional);
    //}
    private void actionThread(object pram)
    {
        try
        {
            var data = (object[])pram;
            var method = data[0].ToString();

            MethodInfo theMethod = _thisType.GetMethod(method);
            var parameterInfo = theMethod.GetParameters();
            List<object> dataParameter = ((object[])data[2]).ToList();
            if(dataParameter.Count< parameterInfo.Length)
            {
                for (int i = 1; i <= parameterInfo.Length-dataParameter.Count; i++)
                {
                    dataParameter.Add(parameterInfo[i].RawDefaultValue);
                }
            }
            var dataReturn = theMethod.Invoke(this, dataParameter.ToArray());
            if (data[1] != null)
            {
                this._web.InvokeScript("callEventCallBack", data[1], JsonConvert.SerializeObject(dataReturn, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
        }
        catch (Exception ex)
        {
            this._web.InvokeScript("ShowMessageBox", ex.InnerException!=null? ex.InnerException.Message:ex.Message);
        }
        this._web.SetLoading(false);
    }

    public void ClientCallServer(string method, string guidEvent, params object[] pram)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, guidEvent, pram });
    }
    public void ExitWaitPopupMessage(string ID)
    {
        Lib.ArrayIDWaitMessage.Remove(ID);
    }
    public void ShowPopupMessage(string title, string content,string id)
    {
        this._web.LoadPopupHtml("PopupMessage.html", new object[] { title, content,id });
    }
}
