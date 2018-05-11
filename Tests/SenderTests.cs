using Core;
using Core.EmailSenders;
using Moq;
using PocketSharp;
using PocketToKindle.Models;
using PocketToKindle.Parsers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class SenderTests
    {
        //[Fact]
        //public async Task Test()
        //{
        //    var pocketClientMock = new Mock<IPocketClient>();
        //    var parserMock = new Mock<IParser>()
        //        .Setup(x => x.ParseAsync(It.IsAny<string>()))
        //        .ReturnsAsync(new MercuryArticle
        //        {
        //            Content = nameof(MercuryArticle.Content),
        //            Title = nameof(MercuryArticle.Title),
        //        });

        //    var emailSenderMock = new Mock<IEmailSender>();

        //    var sender = new Sender(pocketClientMock.Object, parserMock, emailSenderMock.Object);

        //    await sender.SendAsync(new User
        //    {
        //        PocketUsername = nameof(User.PocketUsername),
        //        LastProcessingDate = DateTime.UtcNow.AddDays(-1),
        //        AccessCode = nameof(User.AccessCode),
        //        KindleEmail = nameof(User.KindleEmail)
        //    });

        //    //pocketClientMock.Verify(x => x.Get(since: DateTime.UtcNow.AddDays(-1), count: 5), Times.Once);
        //    pocketClientMock.Verify(x => x.AccessCode == nameof(User.AccessCode));
        //    emailSenderMock.Verify(x => x.SendEmailWithHtmlAttachmentAsync(
        //        nameof(User.KindleEmail),
        //        nameof(MercuryArticle.Title),
        //        nameof(MercuryArticle.Content)),
        //        Times.Once);
        //}
    }
}