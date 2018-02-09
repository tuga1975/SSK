using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Enrolment
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        [STAThread]
        static void Main()
        {

        //    Application.ThreadException +=
        //new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

        //    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        //    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        //    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);



        //    AppDomain.CurrentDomain.UnhandledException += LogUnhandledExceptions;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            Application.Run(new Main());
        }
        //static void Application_ThreadException
        //(object sender, System.Threading.ThreadExceptionEventArgs e)
        //{// All exceptions thrown by the main thread are handled over this method

        //    try
        //    {
        //        Exception ex = e.Exception;
        //        Lib.LayerWeb.InvokeScript("ShowMessageBox", ex.InnerException.Message);
        //    }
        //    catch (Exception)
        //    {
                
        //    }
            
        //}
        // static void LogUnhandledExceptions(object sender, UnhandledExceptionEventArgs e)
        //{
        //    try
        //    {
        //        Exception ex = (Exception)e.ExceptionObject;
        //        Lib.LayerWeb.InvokeScript("ShowMessageBox", ex.InnerException.Message);
        //    }
        //    catch (Exception exc)
        //    {
        //        try
        //        {
        //            MessageBox.Show("Fatal Non-UI Error",
        //                "Fatal Non-UI Error. Could not write the error to the event log. Reason: "
        //                + exc.Message, MessageBoxButtons.OK, MessageBoxIcon.Stop);
        //        }
        //        finally
        //        {
                    
        //        }
        //    }
        //}
    }
}
