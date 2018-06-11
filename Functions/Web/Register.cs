using Core;
using Core.EmailSenders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using PocketSharp;
using PocketSharp.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Functions.Web
{
    public static class Register
    {
        [FunctionName("Register")]
        public static async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req,
            TraceWriter log,
            ExecutionContext context)
        {
            var _config = new ConfigBuilder(context.FunctionAppDirectory).Build();
            var _emailSender = new MailgunSender(_config.MailGunSenderOptions.ApiKey, _config.MailGunSenderOptions.HostEmail);
            var _client = new PocketClient(_config.PocketConsumerKey, callbackUri: _config.PocketRedirectUri);

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            RegisterRequest request = JsonConvert.DeserializeObject<RegisterRequest>(requestBody);

            if (!IsValidEmail(request.KindleEmail))
            {
                log.Error($"Not valid email: {request.KindleEmail}.");
                return new BadRequestObjectResult("email provided is not valid");
            }

            PocketUser pocketUser = await _client.GetUser(request.RequestCode);

            IUserService userService = UserService.BuildUserService(_config.StorageConnectionString);
            await userService.AddUserAsync(new User
            {
                AccessCode = pocketUser.Code,
                PocketUsername = pocketUser.Username,
                KindleEmail = request.KindleEmail,
                LastProcessingDate = DateTime.UtcNow
            });

            await SendWelcomeEmail(_emailSender, request.KindleEmail);
            log.Info($"Successfully registered user: {request.KindleEmail}.");

            return new OkObjectResult("yasss");
        }

        private static async Task SendWelcomeEmail(IEmailSender _emailSender, string email)
        {
            await _emailSender.SendEmailWithHtmlAttachmentAsync(email, "Thanks for registering in PocketToKindle!",
                @"<html>
                    <body>
                        <h1>Thanks for registering in PocketToKindle!</h1>
                        From now on your newly added pocket articles should appear in kindle library.
                        If there's anything wrong with your article there should be a link at last page which will let me know about the issue.
                    </body>
                </html>");
        }

        private class RegisterRequest
        {
            public string RequestCode { get; set; }
            public string KindleEmail { get; set; }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}