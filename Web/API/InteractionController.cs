using Core;
using Core.EmailSenders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocketSharp;
using Web.Database;

namespace Web.API
{
    [Route("api")]
    [Controller]
    public class InteractionController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly Config _config;
        private readonly P2kDbContext _context;
        private readonly IParser _parser;
        private readonly IEmailSender _emailSender;

        public InteractionController(Config config,
                                     IUserService userService,
                                     P2kDbContext context,
                                     IParser parser,
                                     IEmailSender emailSender)
        {
            _userService = userService;
            _config = config;
            _context = context;
            _parser = parser;
            _emailSender = emailSender;
        }

        [HttpGet("archive")]
        public async Task<IActionResult> Archive(string articleId, string token)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(articleId))
            {
                return BadRequest("no token or articleId provided");
            }

            var user = await _userService.FindUserWithToken(token);
            if (user == null)
            {
                return Unauthorized("incorrect token or no such user");
            }

            var pocketClient = new PocketClient(_config.PocketConsumerKey, user.AccessCode);
            var article = await pocketClient.Get(articleId);

            if (article == null)
            {
                return NotFound("no such articleId");
            }

            var success = await pocketClient.Archive(article);

            return success
                ? Ok("Archiving successful")
                : BadRequest("Something went wrong");
        }

        [HttpGet("report")]
        public async Task<IActionResult> Report(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("specify url");
            }

            if (await _context.ReportedArticles.AnyAsync(x => x.Url == url))
            {
                return Ok($"Url {url} was already reported, thanks");
            }

            await _context.ReportedArticles.AddAsync(new ReportedArticle
            {
                Url = url,
                DateAdded = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            return Ok($"Thank you for submitting that article. We'll investigate {url} soon.");
        }

        [HttpGet("star")]
        public async Task<IActionResult> Star(string token, string articleId)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(articleId))
            {
                return BadRequest("no token or articleId provided");
            }

            var user = await _userService.FindUserWithToken(token);
            if (user == null)
            {
                return Unauthorized("incorrect token or no such user");
            }

            var pocketClient = new PocketClient(_config.PocketConsumerKey, user.AccessCode);
            var article = await pocketClient.Get(articleId);

            if (article == null)
            {
                return NotFound("no such articleId");
            }

            var success = await pocketClient.Favorite(article);

            return success
                ? Ok("Starring successful")
                : BadRequest("Something went wrong");
        }

        [HttpGet("preview")]
        public async Task<IActionResult> Preview(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("specify url");
            }

            var article = await _parser.ParseAsync(url);

            return new ContentResult
            {
                Content = article.Content,
                ContentType = "text/html",
                StatusCode = 200
            };
        }

        [HttpGet("send")]
        public async Task<IActionResult> Send(string url, string token)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("specify url");
            }

            var user = await _userService.FindUserWithToken(token);
            if (user == null)
            {
                return Unauthorized("incorrect token or no such user");
            }

            var article = await _parser.ParseAsync(url);

            await _emailSender.SendEmailWithHtmlAttachmentAsync(user.KindleEmail, article.Title, article.Content);

            return new ContentResult
            {
                Content = article.Content,
                ContentType = "text/html",
                StatusCode = 200
            };
        }
    }
}