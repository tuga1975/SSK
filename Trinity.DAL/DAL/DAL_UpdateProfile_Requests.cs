using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.Repository;
using Newtonsoft.Json;

namespace Trinity.DAL
{
    public class DAL_UpdateProfile_Requests
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public void CreateRequest(BE.UserProfile User_Profiles_Old,
                                  BE.Address Alternate_Addresses_Old,
                                  BE.UserProfile User_Profiles_New,
                                  BE.Address Alternate_Addresses_New,
                                  Guid? DocumentID,
                                  bool isScanDocument
                                 )
        {
            var dataApprove = _localUnitOfWork.DataContext.UpdateProfile_Requests.Where(d => d.UserId == User_Profiles_Old.UserId && d.Status == Enum_UpdateProfile_Requests.Approve).OrderByDescending(d => d.UpdatedTime).FirstOrDefault();
            var dataPending = _localUnitOfWork.DataContext.UpdateProfile_Requests.FirstOrDefault(d => d.UserId == User_Profiles_Old.UserId && d.Status == Enum_UpdateProfile_Requests.Pending);
            if (dataApprove == null)
            {

                Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                dataRequest.Add("User_Profiles", User_Profiles_Old);
                dataRequest.Add("Alternate_Addresses", Alternate_Addresses_Old);
                dataRequest.Add("UploadedDocuments", new List<Guid>());

                dataApprove = new DBContext.UpdateProfile_Requests()
                {
                    Status = Enum_UpdateProfile_Requests.Approve,
                    UpdatedTime = DateTime.Now,
                    UserId = User_Profiles_Old.UserId,
                    VersionId = Guid.NewGuid().ToString().Trim(),
                    Current_Content_JSON = JsonConvert.SerializeObject(dataRequest)
                };

                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Add(dataApprove);
            }
            if (dataPending == null)
            {
                if (isScanDocument)
                {
                    Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                    dataRequest.Add("User_Profiles", User_Profiles_New);
                    dataRequest.Add("Alternate_Addresses", Alternate_Addresses_New);
                    if (DocumentID.HasValue)
                        dataRequest.Add("UploadedDocuments", new List<Guid>() { DocumentID.Value });
                    else
                        dataRequest.Add("UploadedDocuments", new List<Guid>());

                    dataPending = new DBContext.UpdateProfile_Requests()
                    {
                        Status = Enum_UpdateProfile_Requests.Pending,
                        UpdatedTime = DateTime.Now,
                        UserId = User_Profiles_Old.UserId,
                        VersionId = Guid.NewGuid().ToString().Trim(),
                        Current_Content_JSON = JsonConvert.SerializeObject(dataRequest)
                    };
                    _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Add(dataPending);
                }
                else
                {
                    Dictionary<string, object> dataRowRequest = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataApprove.Current_Content_JSON);
                    List<Guid> arrayDocument = JsonConvert.DeserializeObject<List<Guid>>(JsonConvert.SerializeObject(dataRowRequest["UploadedDocuments"]));
                    if (DocumentID.HasValue)
                    {
                        arrayDocument.Add(DocumentID.Value);
                    }
                    Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                    dataRequest.Add("User_Profiles", User_Profiles_New);
                    dataRequest.Add("Alternate_Addresses", Alternate_Addresses_New);
                    dataRequest.Add("UploadedDocuments", arrayDocument);

                    dataApprove.Current_Content_JSON = JsonConvert.SerializeObject(dataRequest);
                    dataApprove.UpdatedTime = DateTime.Now;
                    _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Update(dataApprove);

                }
            }
            else
            {
                Dictionary<string, object> dataRowRequest = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataPending.Current_Content_JSON);
                List<Guid> arrayDocument = JsonConvert.DeserializeObject<List<Guid>>(JsonConvert.SerializeObject(dataRowRequest["UploadedDocuments"]));
                if (DocumentID.HasValue)
                {
                    arrayDocument.Add(DocumentID.Value);
                }
                Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                dataRequest.Add("User_Profiles", User_Profiles_New);
                dataRequest.Add("Alternate_Addresses", Alternate_Addresses_New);
                dataRequest.Add("UploadedDocuments", arrayDocument);
                dataPending.Current_Content_JSON = JsonConvert.SerializeObject(dataRequest);
                dataPending.UpdatedTime = DateTime.Now;
                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Update(dataPending);
            }

            
            _localUnitOfWork.Save();

        }
    }
}
