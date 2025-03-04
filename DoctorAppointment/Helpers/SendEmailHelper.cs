using System.Net.Mail;
using System.Net;
namespace DoctorAppointment.Helpers
{
    public static class SendEmailHelper
    {
        public static void SendEmail(string toEmail, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("vstudio7377@gmail.com", "rdhz ufis jlpk ujsv"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("vstudio7377@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
        }


    }
}
