using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using Trinity.DAL.DBContext;
using Trinity.DAL;
using Trinity.BE;

namespace SSK
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class JSCallCS
    {
        private WebBrowser web = null;
        private Type thisType = null;

        public JSCallCS(WebBrowser web)
        {
            this.web = web;
            thisType = this.GetType();
        }

        public void LoadPage(string file)
        {
            web.LoadPageHtml(file);
        }


        public async Task GetQueuAsync(int a)
        {
            //QueueHandler queuHandler = new QueueHandler();
            //var queuValue = queuHandler.GetQueue();
            MessageBox.Show("1");
        }

        public void LoadNotication()
        {
            //var model = sSKCentralizedEntities.Notifications.Where(item => item.Read != true).ToList();
            web.LoadPageHtml("Notication.html", new List<string>());
        }

        public void LoadProfile()
        {
            try
            {
                //profile model 
                var model = new Models.ProfileModel();
                web.LoadPageHtml("Profile.html", model);
            }
            catch (Exception)
            {

                LoadPage("Supervisee.html");
            }
        }

        public void SaveProfile(string param, bool primaryInfoChange)
        {
            try
            {
                if (primaryInfoChange)
                {

                }
                else
                {
                    var data = JsonConvert.DeserializeObject<Models.ProfileModel>(param);
                    //save change to db
                    //using (var unitOfWork = new Trinity.DAL.Repository.UnitOfWork())
                    //{
                    //    var userEntity = unitOfWork.DataContext.Users.FirstOrDefault(u => u.Name == data.ParticularsName);
                    //    if (userEntity != null)
                    //    {
                    //        var userProfileRepo = unitOfWork.GetRepository<User_Profiles>();
                    //        var userProfileEntity = unitOfWork.DataContext.User_Profiles.FirstOrDefault(p => p.UserId == userEntity.UserId);
                    //        if (userProfileEntity != null)
                    //        {

                    //            //particulars optional
                    //            userProfileEntity.Primary_Phone = data.PrimaryContact;
                    //            userProfileEntity.Secondary_Phone = data.SecondaryContact;
                    //            userProfileEntity.Primary_Email = data.PrimaryEmail;
                    //            userProfileEntity.Secondary_Email = data.SecondaryEmail;

                    //            //next of kin
                    //            userProfileEntity.NextOfKin_Name = data.NextOfKinDetails.NextOfKinDetailsName;
                    //            userProfileEntity.NextOfKin_Contact_Number = data.NextOfKinDetails.NextOfKinDetailsContactNumber;
                    //            userProfileEntity.NextOfKin_Relationship = data.NextOfKinDetails.NextOfKinDetailsRelationship;
                    //            userProfileEntity.NextOfKin_BlkHouse_Number = data.NextOfKinDetails.NextOfKinDetailsHouseNumber;
                    //            userProfileEntity.NextOfKin_FlrUnit_Number = data.NextOfKinDetails.NextOfKinDetailsUnitNumber;
                    //            userProfileEntity.NextOfKin_Street_Name = data.NextOfKinDetails.NextOfKinDetailsStreetName;
                    //            userProfileEntity.NextOfKin_Country = data.NextOfKinDetails.NextOfKinDetailsCountry;
                    //            userProfileEntity.NextOfKin_PostalCode = data.NextOfKinDetails.NextOfKinDetailsPostalCode;

                    //            //Employment details
                    //            userProfileEntity.Employment_Name = data.EmployerDetails.EmployerName;
                    //            userProfileEntity.Employment_Contact_Number = data.EmployerDetails.EmployerContact;
                    //            userProfileEntity.Employment_Company_Name = data.EmployerDetails.CompanyName;
                    //            userProfileEntity.Employment_Job_Title = data.EmployerDetails.JobTitle;
                    //            userProfileEntity.Employment_Start_Date = Convert.ToDateTime(data.EmployerDetails.StartDate);
                    //            userProfileEntity.Employment_Remarks = data.EmployerDetails.EndDateAndRemarks;

                    //            userProfileRepo.Update(userProfileEntity);
                    //            unitOfWork.Save();
                    //        }
                    //    }
                    //}
                    DAL_UserProfile dalUserProfile = new DAL_UserProfile();

                    UserProfile userProfile = new UserProfile()
                    {
                        //particulars optional
                        Primary_Phone = data.PrimaryContact,
                        Secondary_Phone = data.SecondaryContact,
                        Primary_Email = data.PrimaryEmail,
                        Secondary_Email = data.SecondaryEmail,

                        //next of kin
                        NextOfKin_Name = data.NextOfKinDetails.NextOfKinDetailsName,
                        NextOfKin_Contact_Number = data.NextOfKinDetails.NextOfKinDetailsContactNumber,
                        NextOfKin_Relationship = data.NextOfKinDetails.NextOfKinDetailsRelationship,
                        NextOfKin_BlkHouse_Number = data.NextOfKinDetails.NextOfKinDetailsHouseNumber,
                        NextOfKin_FlrUnit_Number = data.NextOfKinDetails.NextOfKinDetailsUnitNumber,
                        NextOfKin_Street_Name = data.NextOfKinDetails.NextOfKinDetailsStreetName,
                        NextOfKin_Country = data.NextOfKinDetails.NextOfKinDetailsCountry,
                        NextOfKin_PostalCode = data.NextOfKinDetails.NextOfKinDetailsPostalCode,

                        //Employment details
                        Employment_Name = data.EmployerDetails.EmployerName,
                        Employment_Contact_Number = data.EmployerDetails.EmployerContact,
                        Employment_Company_Name = data.EmployerDetails.CompanyName,
                        Employment_Job_Title = data.EmployerDetails.JobTitle,
                        Employment_Start_Date = Convert.ToDateTime(data.EmployerDetails.StartDate),
                        Employment_Remarks = data.EmployerDetails.EndDateAndRemarks
                    };
                    dalUserProfile.SaveUserProfile(userProfile, true);
                }

                //send notify to case officer
                //load Supervisee page 
                LoadPage("Supervisee.html");
            }
            catch (Exception)
            {
                LoadPage("Supervisee.html");
            }
        }

        private void actionThread(object pram)
        {

            var data = (object[])pram;
            var method = data[0].ToString();
            MethodInfo theMethod = thisType.GetMethod(method);
            theMethod.Invoke(this, (object[])data[1]);
            web.SetLoading(false);
        }
        public void ClientCallServer(string method, params object[] pram)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(actionThread), new object[] { method, pram });
        }
    }
}
