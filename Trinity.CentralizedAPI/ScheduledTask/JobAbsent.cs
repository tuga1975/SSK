using Quartz;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;
using System.Configuration;

namespace Trinity.BackendAPI.ScheduledTask
{
    public class JobAbsent : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                
                var arrayUser = new DAL.DAL_User().GetFromAbsent(DateTime.Today);
                string html = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/EmailTemplate.html"), System.Text.Encoding.UTF8);
                Attachment oAttachment = new Attachment(System.Web.Hosting.HostingEnvironment.MapPath("~/EmailTemplate/logoSendMail.png"));
                oAttachment.ContentId = Guid.NewGuid().ToString().Trim();

                SmtpClient client = new SmtpClient();
                client.Host = ConfigurationManager.AppSettings["Host"];
                client.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                client.UseDefaultCredentials = false;
                client.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSsl"]);
                client.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["User-NetworkCredential"], ConfigurationManager.AppSettings["Password-NetworkCredential"]);
                MailMessage mailMessage = new MailMessage();
                mailMessage.Attachments.Add(oAttachment);
                mailMessage.From = new MailAddress(ConfigurationManager.AppSettings["User-NetworkCredential"]);

                foreach (var user in arrayUser.Where(d => !string.IsNullOrEmpty(d.Email)))
                {
                    mailMessage.To.Add(user.Email);
                }
                mailMessage.Subject = "[CNB-Trinity] Reminder absent";
                mailMessage.Body = html.Replace("{IdLogo}", oAttachment.ContentId).Replace("{Subject}", "[CNB-Trinity] Reminder absent").Replace("{Body}", "Today you have an appointment but are absent");
                mailMessage.IsBodyHtml = true;
                client.Send(mailMessage);

                DAL.DAL_Messages dal_message = new DAL.DAL_Messages();
                dal_message.Insert(arrayUser, mailMessage.Subject, mailMessage.Body, true);
                dal_message.Insert(arrayUser, mailMessage.Subject, "Today you have an appointment but are absent", false);
            }
            catch (Exception)
            {

            }
        }
    }
}