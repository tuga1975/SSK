﻿using System;
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
    public static readonly string curDir = Directory.GetCurrentDirectory().ToLower();

    private async static Task<bool> _WaitPopupMessage(string ID)
    {
        Lib.ArrayIDWaitMessage.Add(ID, null);

        while (!Lib.ArrayIDWaitMessage[ID].HasValue)
        {
            await Task.Delay(1000);
        }
        bool value = Lib.ArrayIDWaitMessage[ID].Value;
        Lib.ArrayIDWaitMessage.Remove(ID);
        return value;
    }

    public static bool ShowMessageConfirm(this WebBrowser web, string title, string content)
    {
        string ID = Guid.NewGuid().ToString().Trim();
        web.InvokeScript("ShowMessageConfirm", title, content, "", ID);
        return Task.Run(async () => await _WaitPopupMessage(ID)).Result;
    }
    public static bool ShowMessageConfirm(this WebBrowser web, string content)
    {
        return ShowMessageConfirm(web, string.Empty, content);
    }

    public static void ShowMessage(this WebBrowser web, string title, string content)
    {
        string ID = Guid.NewGuid().ToString().Trim();
        web.InvokeScript("ShowMessage", title, content, ID);
        bool status = Task.Run(async () => await _WaitPopupMessage(ID)).Result;
    }
    public static void ShowMessage(this WebBrowser web, string content)
    {
        ShowMessage(web, string.Empty, content);
    }
    public static void ShowMessageAsync(this WebBrowser web, string title, string content)
    {
        web.InvokeScript("ShowMessage", title, content, string.Empty);
    }
    public static void ShowMessageAsync(this WebBrowser web, string content)
    {
        ShowMessageAsync(web, string.Empty, content);
    }
    public static void LoadPopupHtml(this WebBrowser web, string file)
    {
        web.InvokeScript("AddContentPopup", "<div id=\"" + file + "\">" + File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8) + "</div>", null, file);
    }
    public static void LoadPopupHtml(this WebBrowser web, string file, object model)
    {
        web.InvokeScript("AddContentPopup", "<div id=\"" + file + "\">" + File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8) + "</div>", JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), file);
    }
    public static void LoadPageHtml(this WebBrowser web, string file)
    {
        web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8), null, file);
    }
    public static void LoadPageHtml(this WebBrowser web, string file, object model)
    {
        web.InvokeScript("AddContentPage", File.ReadAllText(String.Format("{1}/View/html/{0}", file, CSCallJS.curDir), Encoding.UTF8), JsonConvert.SerializeObject(model, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), file);
    }


    private static object BeginInvoke(WebBrowser web, string function, params object[] pram)
    {
        return web.Document.InvokeScript(function, pram);
    }
    private delegate object StringArgReturningVoidDelegate(WebBrowser web, string function, params object[] pram);
    public static object InvokeScript(this WebBrowser web, string function, params object[] pram)
    {
        object value = null;
        try
        {
            if (web.InvokeRequired)
            {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(BeginInvoke);
                value = web.Invoke(d, web,function,pram);
            }
            else
            {
                value = BeginInvoke(web,function, pram);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return value;
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

    public static void UpdateLoginNRICTextValue(WebBrowser web, string value)
    {
        web.InvokeScript("updateLoginNRICTextValue", value);
    }

    public static void DisplayNRICLogin(WebBrowser web)
    {
        web.InvokeScript("displayNRICLogin");
    }

    public static void SetNRICFocus(WebBrowser web)
    {
        web.InvokeScript("setNRICFocus");
    }

    public static void DisplayLogoutButton(WebBrowser web, bool display)
    {
        web.InvokeScript("displayLogoutButton", display);
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

    #region Queue Number
    public static void RefreshQueueNumbers(this WebBrowser web, string servingQueueNumber, string currentQueueNumber, string[] nextQueueNumberList, string[] holdingList)
    {
        var nextQueue = JsonConvert.SerializeObject(nextQueueNumberList, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        var holdList = JsonConvert.SerializeObject(holdingList, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        web.InvokeScript("refreshQueueNumbers", servingQueueNumber, currentQueueNumber, nextQueue, holdList);
    }

    public static void ScanDocumentCompleted(this WebBrowser web, string frontPath)
    {
        web.InvokeScript("scanDocumentCompleted", frontPath);
    }

    #endregion
}