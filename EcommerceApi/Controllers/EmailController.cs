using System.Net;
using System.Net.Mail;
using EcommerceApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EcommerceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailSettings _emailSettings;

        public EmailController(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] Contact contact)
        {
            try
            {
                var smtpServer = _emailSettings.SmtpServer;
                var smtpPort = _emailSettings.SmtpPort;
                var username = _emailSettings.SmtpUsername;
                var password = _emailSettings.SmtpPassword;
                var enableSsl = _emailSettings.EnableSsl;

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.Credentials = new NetworkCredential(username, password);
                    client.EnableSsl = enableSsl;
                    client.Timeout = 10000; // 10 seconds timeout

                    var message = new MailMessage
                    {
                        From = new MailAddress(username),
                        Subject = "Question from Ecommerce Application",
                        Body =
                            $"A new question has been submitted:<br><br>"
                            + $"<strong>Name:</strong> {contact.FirstName} {contact.LastName}<br>"
                            + $"<strong>Email:</strong> {contact.Email}<br>"
                            + $"<strong>Phone:</strong> {contact.PhoneNumber}<br>"
                            + $"<strong>Question:</strong> {contact.Questions}",
                        IsBodyHtml = true,
                    };
                    message.To.Add("jjkst.dev@gmail.com");

                    await client.SendMailAsync(message);
                }

                return Ok(new { message = "Email sent successfully" });
            }
            catch (SmtpException smtpEx)
            {
                return BadRequest(
                    new
                    {
                        error = $"SMTP Error: {smtpEx.Message}",
                        statusCode = smtpEx.StatusCode,
                        details = "Check your SMTP credentials and settings",
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new { error = $"Error sending email: {ex.Message}", details = ex.StackTrace }
                );
            }
        }

        [HttpGet("settings")]
        public IActionResult GetEmailSettings()
        {
            return Ok(
                new
                {
                    SmtpServer = _emailSettings.SmtpServer,
                    SmtpPort = _emailSettings.SmtpPort,
                    EnableSsl = _emailSettings.EnableSsl,
                    Username = _emailSettings.SmtpUsername,
                }
            );
        }
    }
}
