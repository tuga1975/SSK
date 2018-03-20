using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_UploadedDocuments
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();
        
        public Guid Insert(byte[] content,string UserID)
        {
            Trinity.DAL.DBContext.UploadedDocument document = new DBContext.UploadedDocument() {
                UploadedDate = DateTime.Now,
                UploadedBy = UserID,
                Document_ID = Guid.NewGuid(),
                //DocumentContent = content,
                Note = "--"
            };
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.UploadedDocument>().Add(document);
            _localUnitOfWork.Save();
            return document.Document_ID;
        }
    }
}
