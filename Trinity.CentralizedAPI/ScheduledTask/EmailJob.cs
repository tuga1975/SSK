using Quartz;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Trinity.BackendAPI.ScheduledTask
{
    public class EmailJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            using (var message = new MailMessage("cnb.trinity.2018@gmail.com", "cnb.trinity.2018@gmail.com"))
            {
                message.Subject = "Test";
                message.Body = "Test at " + DateTime.Now;
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
            //System.IO.File.AppendAllLines(@"C:\Users\thangnv1\Desktop\DrugResults.log", new string[] { DateTime.Now.ToString("dd/MM/yyyy, HH:mm:ss") });
        }
    }
}