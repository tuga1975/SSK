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
    public class DAL_UpdateProfile_Requests
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        //public void Rollback(string UserID)
        //{
        //    var dataApprove = _localUnitOfWork.DataContext.UpdateProfile_Requests.Where(d => d.UserId == UserID && d.Status == Enum_UpdateProfile_Requests.Approve).OrderByDescending(d => d.UpdatedTime).FirstOrDefault();
        //    if (dataApprove != null)
        //    {
        //        bool isUpdateUserProfile = false;
        //        BE.UpdateProfile_Requests_Model model = JsonConvert.DeserializeObject<BE.UpdateProfile_Requests_Model>(dataApprove.Current_Content_JSON);
        //        if (model.User_Profiles!=null)
        //        {
        //            DAL.DBContext.User_Profiles userProfile = _localUnitOfWork.DataContext.User_Profiles.FirstOrDefault(d => d.UserId.Equals(UserID));

        //            var sourceProps = model.User_Profiles.GetType().GetProperties().Where(x => x.CanRead).ToList();
        //            var destProps = userProfile.GetType().GetProperties()
        //                    .Where(x => x.CanWrite)
        //                    .ToList();

        //            foreach (var sourceProp in sourceProps)
        //            {
        //                if (destProps.Any(x => x.Name == sourceProp.Name))
        //                {
        //                    var _setProps = destProps.First(x => x.Name == sourceProp.Name);

        //                    object vauleNow = _setProps.GetValue(userProfile);
        //                    object valueOld = sourceProp.GetValue(model.User_Profiles);
        //                    if (_setProps.CanWrite)
        //                    {
        //                        _setProps.SetValue(userProfile, sourceProp.GetValue(entity, null), null);
        //                    }
        //                }
        //            }

        //            //foreach (var item in model["User_Profiles"])
        //            //{
        //            //    PropertyInfo propertyInfo = userProfile.GetType().GetProperty(item.Key);
        //            //    object vauleNow =  propertyInfo.GetValue(userProfile);
        //            //    object valueOld = item.Value;
        //            //    if (vauleNow != valueOld)
        //            //    {
        //            //        var underlyingType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
        //            //        propertyInfo.SetValue(userProfile, item.Value == null ? null : Convert.ChangeType(item.Value, underlyingType ?? propertyInfo.PropertyType), null);
        //            //        isUpdateUserProfile = true;
        //            //    }
        //            //}
        //        }


        //        if (isUpdateUserProfile)
        //        {

        //        }

        //        //foreach (var item in model.UploadedDocuments)
        //        //{
        //        //    _localUnitOfWork.GetRepository<Trinity.DAL>
        //        //}

        //    }
        //}

        public void CreateRequest(BE.UserProfile User_Profiles_Old,
                                  BE.Address Alternate_Addresses_Old,
                                  BE.UserProfile User_Profiles_New,
                                  BE.Address Alternate_Addresses_New,
                                  Guid? DocumentID,
                                  bool isScanDocument,
                                  bool isDoLogin
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
                Dictionary<string, object> dataRequest = new Dictionary<string, object>();
                dataRequest.Add("User_Profiles", User_Profiles_New);
                dataRequest.Add("Alternate_Addresses", Alternate_Addresses_New);
                if (DocumentID.HasValue)
                    dataRequest.Add("UploadedDocuments", new List<Guid>() { DocumentID.Value });
                else
                    dataRequest.Add("UploadedDocuments", new List<Guid>());

                dataPending = new DBContext.UpdateProfile_Requests()
                {
                    Status = isDoLogin || !isScanDocument ? Enum_UpdateProfile_Requests.Approve : Enum_UpdateProfile_Requests.Pending,
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
                    dataPending.Status = Enum_UpdateProfile_Requests.Approve;

                _localUnitOfWork.GetRepository<DBContext.UpdateProfile_Requests>().Update(dataPending);
            }


            _localUnitOfWork.Save();

        }
    }
}
