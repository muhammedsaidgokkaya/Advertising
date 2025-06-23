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
        public void SendEmail(string signInUrl, string toMail, string name, string userName, string password)
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
                    <!DOCTYPE html>
                    <html lang=""tr"">
                    <head>
                        <meta charset=""UTF-8"" />
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                        <title>Giriş Bilgileriniz</title>
                    </head>
                    <body style=""margin: 0; padding: 0; font-family: 'Roboto', sans-serif; background-color: #ffffff; color: #ffffff;"">
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; padding: 30px 0;"">
                        <tr>
                            <td align=""center"">
                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width: 600px; background-color: #1e1e1e; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.6);"">
                                <tr>
                                <td style=""background-color: #1976d2; padding: 20px; text-align: center;"">
                                    <h2 style=""margin: 0; color: #ffffff;"">🎉 Dijitals'e Hoş Geldiniz!</h2>
                                </td>
                                </tr>
                                <tr>
                                <td style=""padding: 30px; color: #e0e0e0;"">
                                    <p style=""font-size: 16px; margin-bottom: 20px;"">Sayın <strong>{name}</strong>,</p>
                                    <p style=""font-size: 15px; line-height: 1.6;"">
                                    Dijitals ailesine katıldığınız için teşekkür ederiz. Aşağıda sistemimize giriş yapabilmeniz için gerekli bilgiler yer almaktadır. Lütfen bu bilgileri gizli tutunuz.
                                    </p>
                                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-top: 25px; background-color: #2c2c2c; border-radius: 6px; padding: 20px;"">
                                    <tr>
                                        <td style=""padding: 10px 20px; font-size: 15px;"">📧 <strong>E-posta:</strong></td>
                                        <td style=""padding: 10px 20px; font-size: 15px; color: #90caf9;"">{toMail}</td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 10px 20px; font-size: 15px;"">🔑 <strong>Şifre:</strong></td>
                                        <td style=""padding: 10px 20px; font-size: 15px; color: #90caf9;"">{password}</td>
                                    </tr>
                                    </table>
                                    <div style=""text-align: center; margin: 35px 0 10px;"">
                                    <a href=""{signInUrl}?mail={toMail}&password={password}"" style=""background-color: #1976d2; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; font-weight: 500; display: inline-block;"">Giriş Yap</a>
                                    </div>

                                    <p style=""font-size: 13px; color: #aaaaaa; margin-top: 30px;"">Bu e-posta otomatik olarak oluşturulmuştur. Lütfen yanıtlamayınız.</p>
                                </td>
                                </tr>
                                <tr>
                                <td style=""background-color: #1a1a1a; text-align: center; padding: 20px; font-size: 12px; color: #666;"">
                                    © 2025 Dijitals – Tüm hakları saklıdır.<br />
                                    <a href=""https://dijitals.app"" style=""color: #666; text-decoration: none;"">dijitals.app</a>
                                </td>
                                </tr>
                            </table>
                            </td>
                        </tr>
                        </table>
                    </body>
                    </html>
                ";

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
