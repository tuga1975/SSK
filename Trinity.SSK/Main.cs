﻿using Futronic.SDKHelper;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using PCSC;
using SSK.DeviceMonitor;
using SSK.DriverScan;
using SSK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity.DAL.DAL;

namespace SSK
{
    public partial class Main : Form
    {
        
        private JSCallCS jsCallCS = null;

        private SmartCard smartCard = null;
        
        public Main()
        {
            InitializeComponent();

            APIUtils.LayerWeb = LayerWeb;
            jsCallCS = new JSCallCS(this.LayerWeb);
            smartCard = new SmartCard(this.LayerWeb);
            this.LayerWeb.Url = new Uri(String.Format("file:///{0}/View/html/Layout.html", CSCallJS.curDir));
            this.LayerWeb.ObjectForScripting = jsCallCS;

            //
            // For testing purpose only
            // 
            //Trinity.DAL.DAL.DAL_User dalUser = new Trinity.DAL.DAL.DAL_User();
            //Trinity.BE.User user = dalUser.GetUserBySmartCardId("123456789");

            // StartIdentification
            // FingerprintMonitor.StartIdentification(OnGetBaseTemplateComplete);
            // StartVerification
            // StartVerification();

            // StartCardMonitor
            // StartCardMonitor();
        }

        private void StartCardMonitor()
        {
            DeviceMonitor.SCardMonitor.StartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);

        }

        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("onCardInitialized");
            string cardInfo = DeviceMonitor.SCardMonitor.GetCardUID();
            Debug.WriteLine($"Card UID: {cardInfo}");
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("OnCardInserted");
            string cardInfo = DeviceMonitor.SCardMonitor.GetCardUID();
            Debug.WriteLine($"Card UID: {cardInfo}");
        }
        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("OnCardRemoved");
        }

        private void StartVerification()
        {
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId("9999");
            if (user == null)
            {
                return;
            }

            FingerprintMonitor.StartVerification(OnVerificationComplete, user.Fingerprint_Template);
        }

        private void OnVerificationComplete(bool bSuccess, int nRetCode, bool bVerificationSuccess)
        {
            var result = FingerprintMonitor.VerificationResult(bSuccess, nRetCode, bVerificationSuccess);
        }

        private void OnGetBaseTemplateComplete(bool bSuccess, int nResult)
        {
            DAL_User dAL_User = new DAL_User();
            var user = dAL_User.GetUserBySmartCardId("9999");
            if (user == null)
            {
                return;
            }

            bool identificationResult = FingerprintMonitor.IdentificationResult(bSuccess, nResult, user.Fingerprint_Template);
        }

        private void LayerWeb_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.LayerWeb.InvokeScript("createEvent", JsonConvert.SerializeObject(jsCallCS.GetType().GetMethods().Where(d => d.IsPublic && !d.IsVirtual && !d.IsSecuritySafeCritical).ToArray().Select(d => d.Name)));
            APIUtils.SignalR.CheckNotification();
            //smartCard.Scanning();
            CodeBehind.Authentication.SmartCard smartCard = new CodeBehind.Authentication.SmartCard(this.LayerWeb);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();
            APIUtils.Dispose();
        }
    }
}
