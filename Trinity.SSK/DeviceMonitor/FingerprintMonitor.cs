using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSK.DeviceMonitor
{
    class FingerprintMonitor
    {
        static void Start()
        {
            //Debug.WriteLine("Programme is loading database, please wait ...");
            //SSKEntities sskEntities = new SSKEntities();
            //_users = sskEntities.Fingerprints.ToList();
            //if (_users.Count == 0)
            //{
            //    Console.WriteLine("Users not found. Please, run enrollment process first.");
            //    return;
            //}

            //_futronicIdentification = new FutronicIdentification();

            //// Set control property
            //_futronicIdentification.FakeDetection = true;
            //_futronicIdentification.FFDControl = true;
            //_futronicIdentification.FARN = 200;
            //_futronicIdentification.Version = VersionCompatible.ftr_version_compatible;
            //_futronicIdentification.FastMode = true;
            //_futronicIdentification.MinMinuitaeLevel = 3;
            //_futronicIdentification.MinOverlappedLevel = 3;

            //// register events
            //_futronicIdentification.OnPutOn += OnPutOn;
            //_futronicIdentification.OnTakeOff += OnTakeOff;
            ////_futronicIdentification.UpdateScreenImage += new UpdateScreenImageHandler(this.UpdateScreenImage);
            //_futronicIdentification.OnFakeSource += OnFakeSource;
            //_futronicIdentification.OnGetBaseTemplateComplete += OnGetBaseTemplateComplete;

            //// start identification process
            //_futronicIdentification.GetBaseTemplate()
        }
    }
}
