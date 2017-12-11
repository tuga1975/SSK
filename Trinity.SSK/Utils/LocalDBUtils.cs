using SSK.Client.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SSK.DbContext;

namespace SSK.Utils
{
    class LocalDBUtils
    {
        UnitOfWork _unitOfWork = new UnitOfWork();
        public Queue GetQueue()
        {
            try
            {
                var queueEntity = new Queue()
                {
                    AppId = "AppClient",
                    Date = DateTime.Now
                };

                var queueRepo = _unitOfWork.GetRepository<Queue>().Add(queueEntity);

                //_unitOfWork.GetRepository<Queue>().Add(queueEntity);
                if (_unitOfWork.Save() > 0)
                {
                    return queueEntity;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
