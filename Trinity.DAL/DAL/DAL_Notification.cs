using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL.DAL
{
    public class DAL_Notification
    {
        IDatabaseFactory databaseFactory;
        UnitOfWork _unitOfWork = new UnitOfWork();

        public int GetNumberOfUnreadNotification()
        {
            //int returnValue = _unitOfWork.DataContext.Notifications.Where(item => item.IsRead != true).Count();

            return 1;
        }
    }
}
