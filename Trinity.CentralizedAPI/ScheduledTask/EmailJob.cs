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
            foreach (var user in arrayUser.Where(d=>!string.IsNullOrEmpty(d.Email)))
            {
                try
                {
                    using (var message = new MailMessage("cnb.trinity.2018@gmail.com", user.Email))
                    {
                        message.Subject = "[CNB-Trinity] Dppointment reminder";
                        message.Body = "Tomorrow "+ dateNext .ToString("dd/MM/yyyy")+ " you will have an appointment";
                        using (SmtpClient client = new SmtpClient
                        {
                            EnableSsl = true,
                            Host = "smtp.gmail.com",
                            Port = 587,
                            Credentials = new NetworkCredential("cnb.trinity.2018@gmail.com", "P@ssw0rd@1234")
                        })
                        {
                            client.Send(message);
                        }
                    }
                }
                catch (Exception)
                {

                    
                }
            }
            
            //System.IO.File.AppendAllLines(@"C:\Users\thangnv1\Desktop\DrugResults.log", new string[] { DateTime.Now.ToString("dd/MM/yyyy, HH:mm:ss") });
        }
    }
}