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
        public void SendEmail(string toMail, string name, string userName, string password)
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
                    Subject = "Merhaba Dijitals Uygulamasına Hoşgeldin!",
                    IsBodyHtml = true,
                };

                string htmlBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{
                                font-family: Arial, sans-serif;
                            }}
                            .container {{
                                padding: 20px;
                                border: 1px solid #ddd;
                                border-radius: 5px;
                                background-color: #f9f9f9;
                            }}
                            .header {{
                                font-size: 24px;
                                font-weight: bold;
                                color: #333;
                                margin-bottom: 10px;
                            }}
                            .content {{
                                font-size: 16px;
                                color: #555;
                            }}
                            .footer {{
                                font-size: 12px;
                                color: #999;
                                margin-top: 20px;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>Merhaba, {name}!</div>
                            <div class='content'>
                                <p>Dijitals uygulamasına hoşgeldiniz!</p>
                                <p><b>Kullanıcı Adınız:</b> {userName}</p>
                                <p><b>Parolanız:</b> {password}</p>
                            </div>
                            <div class='footer'>
                                Bu e-posta otomatik olarak oluşturulmuştur, lütfen yanıtlamayın.
                            </div>
                        </div>
                    </body>
                    </html>";

                mailMessage.Body = htmlBody;
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
