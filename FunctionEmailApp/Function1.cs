using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Net.Mail;

namespace FunctionEmailApp
{
    public static class Function1
    {
        [FunctionName("SendEmail")]
        public static async Task<IActionResult> Run(
                    [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
                    ILogger log)
        {
            log.LogInformation("Received a request to send an email.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);

                string name=data?.contactName;
                string email = data?.contactEmail;
                string taskText = data?.contactMessage;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(taskText))
                {
                    log.LogError("Email address or task text is missing in the request.");
                    return new BadRequestObjectResult("Email address and task text are required.");
                }

                var subject = "Task DeadLine Reminder";
                var message = $"The deadline for task '{taskText}' is tomorrow. Please complete it on time.";
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(name, "mehdizadeorxan2000@gmail.com"));
                emailMessage.To.Add(new MailboxAddress("Recipient", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("plain") { Text = message };

                using var client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("mehdizadeorxan2000@gmail.com", "mnjmalhjvloxihcr");
                await client.SendAsync(emailMessage);
                client.Disconnect(true);

                log.LogInformation("Email sent successfully.");
                return new OkResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to send email.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
