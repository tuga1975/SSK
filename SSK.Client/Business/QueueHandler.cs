using SSK.Client.Repository;
using SSK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SSK.DbContext;
using SSK.Utils;

namespace SSK.Business
{
    class QueueHandler
    {
        UnitOfWork _unitOfWork = new UnitOfWork();
        public string GetQueue()
        {
            try
            {
                LocalDBUtils localDBUtils = new LocalDBUtils();
                var queue = localDBUtils.GetQueue();

                if (queue != null)
                {
                    return "Your QueueValue is " + queue.Id;
                }

                return "Oops!. Cannot get the Queue.";
            }
            catch (Exception ex)
            {
                return "Oops!. Something went wrong.";
            }
        }
    }
}
