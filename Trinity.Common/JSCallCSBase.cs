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
            if (dataParameter.Count < parameterInfo.Length)
            {
                for (int i = 1; i <= parameterInfo.Length - dataParameter.Count; i++)
                {
                    dataParameter.Add(parameterInfo[i].RawDefaultValue);
                }
            }
            var dataReturn = theMethod.Invoke(this, dataParameter.ToArray());
            if (data[1] != null)
            {
                this._web.InvokeScript("callEventCallBack", data[1], JsonConvert.SerializeObject(dataReturn, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));
            }
            this._web.SetLoading(false);
        }
        catch (Exception ex)
        {
            this._web.SetLoading(false);
            ex = ex.InnerException != null ? ex.InnerException : ex;
            if (ex is Trinity.Common.ExceptionArgs)
            {
                this._web.ShowMessage(((Trinity.Common.ExceptionArgs)ex).ErrorMessage);
            }
            else
            {
                if (ex.Source.Equals(".Net SqlClient Data Provider") || (ex.Source.Equals("EntityFramework") && ex.Message.Equals("The underlying provider failed on Open.")))
                {
                    this._web.ShowMessage("Can not connect to the database.<br/>Please check the connection.");
                }
                else
                {
                    string ID = Guid.NewGuid().ToString().Trim();
                    this._web.ShowMessage("An error occurred.<br/>Please contact the administrator.<br/>Error code: " + ID);
                    //this._web.ShowMessage("Error:" + ex.Message);
                    Trinity.Common.Utils.LogManager.Error("["+ ID + "]  Error in Trinity.Common.JSCallCSBase.actionThread. Details:" + ex.Message);
                }
            }
        }
    }

    public void ClientCallServer(string method, string guidEvent, params object[] pram)
    {
        ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, guidEvent, pram });
    }
    public void ExitWaitPopupMessage(string ID, bool status)
    {
        if (Lib.ArrayIDWaitMessage.ContainsKey(ID))
            Lib.ArrayIDWaitMessage[ID] = status;
    }
    public void LoadPopupHtml(string file, string model)
    {
        this._web.LoadPopupHtml(file, model);
    }
    public void LogErrScript(string message)
    {
        //string file = String.Format("{0}/LogErr/Log.txt", CSCallJS.curDir);
        //string directoryName = new System.IO.FileInfo(file).DirectoryName;
        //if (!System.IO.Directory.Exists(directoryName))
        //{
        //    System.IO.DirectoryInfo di = System.IO.Directory.CreateDirectory(directoryName);
        //}
        //System.IO.File.AppendAllLines(file, new string[] { message });

        //string ID = Guid.NewGuid().ToString().Trim();
        //this._web.ShowMessage("An error occurred.<br/>Please contact the administrator.<br/>Error code: " + ID);
        this._web.ShowMessage("Error:" + message);
        Trinity.Common.Utils.LogManager.Error("Error in Trinity.Common.JSCallCSBase.LogErrScript. Details:" + message);
    }
}
