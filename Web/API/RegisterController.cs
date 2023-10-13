using Core;
using Core.EmailSenders;
using Microsoft.AspNetCore.Mvc;
using PocketSharp;
using PocketSharp.Models;
using Web.Database;

namespace Web.API
{
    [Route("api/register")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly PocketClient _pocketClient;
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<RegisterController> _log;

        public RegisterController(PocketClient pocketClient, ILogger<RegisterController> log, IUserService userService, IEmailSender emailSender)
        {
            _pocketClient = pocketClient;
            _userService = userService;
            _emailSender = emailSender;
            _log = log;
        }

        [HttpGet("getinfo")]
        public async Task<IActionResult> GetInfo()
        {
            string requestCode = await _pocketClient.GetRequestCode();

            var result = new
            {
                RequestCode = requestCode,
                RegistrationLink = _pocketClient.GenerateRegistrationUri(requestCode)
            };
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!IsValidEmail(request.KindleEmail))
            {
                _log.LogError($"Not valid email: {request.KindleEmail}.");
                return new BadRequestObjectResult("email provided is not valid");
            }

            PocketUser pocketUser = new();

            try
            {
                pocketUser = await _pocketClient.GetUser(request.RequestCode);
            }
            catch (PocketException pocketException)
            {
                _log.LogError($"Something went wrong: {pocketException.Message}.");
                return new BadRequestObjectResult(pocketException.Message);
            }

            await _userService.AddUserAsync(new User
            {
                AccessCode = pocketUser.Code,
                PocketUsername = pocketUser.Username,
                KindleEmail = request.KindleEmail,
                LastProcessingDate = DateTime.UtcNow,
                Token = Guid.NewGuid().ToString()
            });

            await SendWelcomeEmail(_emailSender, request.KindleEmail);
            _log.LogInformation($"Successfully registered user: {request.KindleEmail}.");

            return new OkObjectResult("Registration successful");
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

        private static async Task SendWelcomeEmail(IEmailSender _emailSender, string email)
        {
            await _emailSender.SendEmailWithHtmlAttachmentAsync(email, "Thanks for registering in PocketToKindle!",
                @"<html>
                    <body>
                        <h1>Thanks for registering in PocketToKindle!</h1>
                        From now on your newly added pocket articles should appear in kindle library.
                        If there's anything wrong with your article there should be a link at last page which will let me know about the issue.
                        Also you can contact me on twitter @karolsejka.
                    </body>
                </html>");
        }
    }

    public class RegisterRequest
    {
        public string KindleEmail { get; set; }
        public string RequestCode { get; set; }
    }
}