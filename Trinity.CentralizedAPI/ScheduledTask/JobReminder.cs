﻿using Quartz;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Drawing;

namespace Trinity.BackendAPI.ScheduledTask
{
    public class JobReminder : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                DAL.DAL_Notification dal_NoTi = new DAL.DAL_Notification();
                
                DateTime dateNext = DateTime.Today.AddDays(1);
                var arrayUser = new DAL.DAL_User().GetFromAppointmentDate(dateNext);
                string html = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/EmailTemplate.html"), System.Text.Encoding.UTF8);
                Attachment oAttachment = new Attachment(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/logoSendMail.png"));
                
                oAttachment.ContentId = Guid.NewGuid().ToString().Trim();
                DAL.DAL_Messages dal_message = new DAL.DAL_Messages();

                string strBase64 = string.Empty;

                using (Image image = Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/logoSendMail.png")))
                {
                    using (MemoryStream m = new MemoryStream())
                    {
                        image.Save(m, image.RawFormat);
                        byte[] imageBytes = m.ToArray();
                        strBase64 = "data:image/png;base64,"+Convert.ToBase64String(imageBytes);
                    }
                }

                foreach (var recepient in arrayUser)
                {
                    MailMessage message = new MailMessage();
                    message.Attachments.Add(oAttachment);
                    message.From = new MailAddress(ConfigurationManager.AppSettings["User-NetworkCredential"]);
                    message.To.Add(recepient.Email);
                    message.Subject = "Reminder of Appointment on " + dateNext.ToString("dd/MM/yyyy");
                    message.Body = html.Replace("{IdLogo}", oAttachment.ContentId).Replace("{Subject}", "Reminder of Appointment on " + dateNext.ToString("dd/MM/yyyy")).Replace("{Body}", "NRIC "+ Trinity.Common.CommonUtil.GetQueueNumber(recepient.NRIC) + ", you have been scheduled for urine reporting at CNB ENF A on " + DateTime.Today.ToString("dd/MM/yyyy"));
                    message.IsBodyHtml = true;
                    // Send the email async to avoid blocking the main thread
                    SmtpClient client = new SmtpClient();
                    client.Host = ConfigurationManager.AppSettings["Host"];
                    client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                    client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["User-NetworkCredential"], ConfigurationManager.AppSettings["Password-NetworkCredential"]);

                    client.SendCompleted += (se, ea) =>
                    {
                        client.Dispose();
                        message.Dispose();
                        dal_message.Insert(recepient, message.Subject, message.Body, true);
                        dal_message.Insert(recepient, message.Subject, "NRIC " + Trinity.Common.CommonUtil.GetQueueNumber(recepient.NRIC) + ", you have been scheduled for urine reporting at CNB ENF A on " + DateTime.Today.ToString("dd/MM/yyyy"), false);
                    };

                    client.SendAsync(message, null);
                    //Send notification
                    dal_NoTi.InsertNotification(null, recepient.UserId, message.Subject, message.Body.Replace("cid:" + oAttachment.ContentId, strBase64), false, DateTime.Now, null, EnumNotificationTypes.Notification, EnumStation.DUTYOFFICER);
                }
            }
            catch (Exception)
            {
                
            }
            
        }
    }
}