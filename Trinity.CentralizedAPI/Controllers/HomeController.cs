using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Trinity.BackendAPI.Controllers
{

    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            //string html = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/EmailTemplate/EmailTemplate.html"), System.Text.Encoding.UTF8);
            //Attachment oAttachment = new Attachment(HttpContext.Server.MapPath("~/EmailTemplate/logoSendMail.png"));
            //oAttachment.ContentId = Guid.NewGuid().ToString().Trim();
            //MailMessage message = new MailMessage();
            //SmtpClient smtp = new SmtpClient();
            //message.Attachments.Add(oAttachment);
            //message.From = new MailAddress("cnb.trinity.2018@gmail.com");
            //message.To.Add("thangnv1@tinhvan.com");

            //message.Subject = "[CNB-Trinity] Dppointment reminder";
            //message.Body = html.Replace("{IdLogo}", oAttachment.ContentId).Replace("{Subject}", "[CNB-Trinity] Dppointment reminder").Replace("{Body}", "Tomorrow " + DateTime.Today.ToString("dd/MM/yyyy") + " you will have an appointment");
            //message.IsBodyHtml = true;
            //smtp.Port = 587;
            //smtp.Host = "smtp.gmail.com";
            //smtp.EnableSsl = true;
            //smtp.UseDefaultCredentials = false;
            //smtp.Credentials = new NetworkCredential("cnb.trinity.2018@gmail.com", "P@ssw0rd@1234");
            //smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess;
            //smtp.Send(message);

            return View();
        }
    }
}