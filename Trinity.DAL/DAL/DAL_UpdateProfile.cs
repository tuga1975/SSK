using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.Repository;
using Newtonsoft.Json;
using System.Reflection;

namespace Trinity.DAL
{
    public class DAL_UpdateProfile
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void Approve(string userId)
        {
            DAL.DBContext.UpdateProfile_Requests dataApprove = _localUnitOfWork.DataContext.UpdateProfile_Requests.FirstOrDefault(d => d.UserId == userId && d.Status == Enum_UpdateProfile.Pending);
            if (dataApprove != null)
            {
                dataApprove.Status = Enum_UpdateProfile.Approve;
                _localUnitOfWork.GetRepository<DAL.DBContext.UpdateProfile_Requests>().Update(dataApprove);
                _localUnitOfWork.Save();
            }
        }

        public void Reject(string userId)
        {
            DAL.DBContext.UpdateProfile_Requests dataReject = _localUnitOfWork.DataContext.UpdateProfile_Requests.FirstOrDefault(d => d.UserId == userId && d.Status == Enum_UpdateProfile.Pending);
            if (dataReject != null)
            {
                dataReject.Status = Enum_UpdateProfile.Reject;
                _localUnitOfWork.GetRepository<DAL.DBContext.UpdateProfile_Requests>().Update(dataReject);
                _localUnitOfWork.Save();
                Rollback(userId);
            }
        }

        public void Rollback(string userId)
        {
            var dataApprove = _localUnitOfWork.DataContext.UpdateProfile_Requests.Where(d => d.UserId == userId && d.Status == Enum_UpdateProfile.Approve).OrderByDescending(d => d.UpdatedTime).FirstOrDefault();
            if (dataApprove != null)
            {
                bool isUpdateUserProfile = false;
                BE.UpdateProfile_Request_Model model = JsonConvert.DeserializeObject<BE.UpdateProfile_Request_Model>(dataApprove.Current_Content_JSON);
                if (model.User_Profiles != null)
                {
                    List<string> ignore = new List<string>() { "LeftThumb_Photo", "RightThumb_Photo", "User_Photo2", "User_Photo1" };
                    DAL.DBContext.User_Profiles userProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId.Equals(userId));

                    var sourceProps = model.User_Profiles.GetType().GetProperties().Where(x => x.CanRead && !ignore.Contains(x.Name)).ToList();
                    var destProps = userProfile.GetType().GetProperties()
                            .Where(x => x.CanWrite && !ignore.Contains(x.Name))
                            .ToList();
                    foreach (var sourceProp in sourceProps)
                    {
                        if (destProps.Any(x => x.Name == sourceProp.Name))
                        {
                            var _setProps = destProps.First(x => x.Name == sourceProp.Name);
                            var vauleNow = _setProps.GetValue(userProfile);
                            var valueOld = sourceProp.GetValue(model.User_Profiles);
                            if (_setProps.CanWrite && ((vauleNow == null && valueOld != null) || (vauleNow != null && valueOld == null) || (vauleNow != null && valueOld != null && !vauleNow.Equals(valueOld))))
                            {
                                _setProps.SetValue(userProfile, valueOld, null);
                                isUpdateUserProfile = true;
                            }
                        }
                    }
                    if (isUpdateUserProfile)
                        _localUnitOfWork.GetRepository<DAL.DBContext.User_Profiles>().Update(userProfile);
                }
                bool isUpdateAlternate_Addresses = false;
                if (model.Alternate_Addresses != null && !string.IsNullOrEmpty(model.Alternate_Addresses.Address_ID))
                {
                    DAL.DBContext.Address address = _localUnitOfWork.DataContext.Addresses.FirstOrDefault(d => d.Address_ID == model.Alternate_Addresses.Address_ID);
                    if (address != null)
                    {
                        var sourceProps = model.Alternate_Addresses.GetType().GetProperties().Where(x => x.CanRead).ToList();
                        var destProps = address.GetType().GetProperties()
                                .Where(x => x.CanWrite)
                                .ToList();
                        foreach (var sourceProp in sourceProps)
                        {
                            if (destProps.Any(x => x.Name == sourceProp.Name))
                            {
                                var _setProps = destProps.First(x => x.Name == sourceProp.Name);
                                var vauleNow = _setProps.GetValue(address);
                                var valueOld = sourceProp.GetValue(model.Alternate_Addresses);
                                if (_setProps.CanWrite && ((vauleNow == null && valueOld != null) || (vauleNow != null && valueOld == null) || (vauleNow != null && valueOld != null && !vauleNow.Equals(valueOld))))
                                {
                                    _setProps.SetValue(address, valueOld, null);
                                    isUpdateAlternate_Addresses = true;
                                }
                            }
                        }
                        if (isUpdateAlternate_Addresses)
                            _localUnitOfWork.GetRepository<DAL.DBContext.Address>().Update(address);
                    }
                }
                if (isUpdateUserProfile || isUpdateAlternate_Addresses)
                {
                    _localUnitOfWork.Save();
                }
            }
        }

        public void CreateRequest(BE.UserProfile User_Profiles_Old,
                                  BE.Address Alternate_Addresses_Old,
                                  BE.UserProfile User_Profiles_New,
                                  BE.Address Alternate_Addresses_New,
                                  Guid? DocumentID,
                                  bool isScanDocument,
                                  bool isDoLogin
                                 )
        {
            var dataApprove = _localUnitOfWork.DataContext.UpdateProfile_Requests.Where(d => d.UserId == User_Profiles_Old.UserId && d.Status == Enum_UpdateProfile.Approve).OrderByDescending(d => d.UpdatedTime).FirstOrDefault();
            var dataPending = _localUnitOfWork.DataContext.UpdateProfile_Requests.FirstOrDefault(d => d.UserId == User_Profiles_Old.UserId && d.Status == Enum_UpdateProfile.Pending);
            if (dataApprove == null)
            {

                Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                dataRequest.Add("User_Profiles", User_Profiles_Old);
                dataRequest.Add("Alternate_Addresses", Alternate_Addresses_Old);
                dataRequest.Add("UploadedDocuments", new List<Guid>());

                dataApprove = new DBContext.UpdateProfile_Requests()
                {
                    Status = Enum_UpdateProfile.Approve,
                    UpdatedTime = DateTime.Now,
                    UserId = User_Profiles_Old.UserId,
                    VersionId = Guid.NewGuid().ToString().Trim(),
                    Current_Content_JSON = JsonConvert.SerializeObject(dataRequest)
                };
                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Add(dataApprove);
            }
            if (dataPending == null)
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
                    Status = isDoLogin || !isScanDocument ? Enum_UpdateProfile.Approve : Enum_UpdateProfile.Pending,
                    UpdatedTime = DateTime.Now,
                    UserId = User_Profiles_Old.UserId,
                    VersionId = Guid.NewGuid().ToString().Trim(),
                    Current_Content_JSON = JsonConvert.SerializeObject(dataRequest)
                };
                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Add(dataPending);
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
                if (isDoLogin)
                    dataPending.Status = Enum_UpdateProfile.Approve;

                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Update(dataPending);
            }
            _localUnitOfWork.Save();
        }
    }
}
