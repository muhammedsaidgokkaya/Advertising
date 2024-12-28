using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net;

namespace Utilities.Helper
{
    public class EmailHelper
    {
        public void SendEmail(string toMail, string userName, string password)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("gokkayasaid.o@gmail.com", "dsns jkkj ksue zpxl"),//google hesabından Uygulama Şifreleri
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(toMail),
                    Subject = "Merhaba Dijital Uygulamasına Hoşgeldin!",
                    Body = "Kullanıcı adınız:" + userName + " Parolanız:" + password,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toMail);

                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }
    }
}
