using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;

namespace Trinity.DAL
{
    public class DAL_Messages
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();
        public void Insert(List<Trinity.DAL.DBContext.Membership_Users> arrayUser, string Subject, string Body, bool isEmail)
        {
            using (var transaction = _localUnitOfWork.DataContext.Database.BeginTransaction())
            {
                try
                {
                    List<Trinity.DAL.DBContext.Message> arrayInsert = new List<Message>();
                    foreach (var user in arrayUser)
                    {
                        arrayInsert.Add(new Message()
                        {
                            Email = isEmail,
                            SMS = isEmail ? false : true,
                            MsgContent = Body,
                            Subject = Subject,
                            UserId = user.UserId
                        });
                    }
                    _localUnitOfWork.GetRepository<DAL.DBContext.Message>().AddRange(arrayInsert);
                    _localUnitOfWork.DataContext.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }

        }
    }
}
