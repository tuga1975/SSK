using Quartz;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Linq;
namespace Trinity.BackendAPI.ScheduledTask
{
    public class EmailJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            DateTime dateNext = DateTime.Today.AddDays(1);
            var arrayUser = new DAL.DAL_User().GetFromAppointmentDate(dateNext);

            MailMessage message = new MailMessage();
            SmtpClient smtp = new SmtpClient();

            message.From = new MailAddress("cnb.trinity.2018@gmail.com");
            foreach (var user in arrayUser.Where(d => !string.IsNullOrEmpty(d.Email)))
            {
                message.To.Add(user.Email);
            }
            message.To.Add("thangxoan91@gmail.com");
            message.Subject = "[CNB-Trinity] Dppointment reminder";
            message.Body = "Tomorrow " + dateNext.ToString("dd/MM/yyyy") + " you will have an appointment";

            smtp.Port = 587;
            smtp.Host = "smtp.gmail.com";
            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("cnb.trinity.2018@gmail.com", "P@ssw0rd@1234");
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Send(message);

            //System.IO.File.AppendAllLines(@"C:\Users\thangnv1\Desktop\DrugResults.log", new string[] { DateTime.Now.ToString("dd/MM/yyyy, HH:mm:ss") });
        }
    }
}