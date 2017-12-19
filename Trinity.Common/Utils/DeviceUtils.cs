﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;

namespace Trinity.Common.Utils
{
    public abstract class DeviceUtils
    {
        public abstract void Start();
        public abstract void Stop();
        public abstract DeviceStatus GetDeviceStatus();
    }
}