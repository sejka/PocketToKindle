using Core;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using Xunit;

namespace Tests
{
    public class UserProcessorTests
    {
        private User[] userMock = new User[]
            {
                new User()
                {
                    ArticleUrls = new string[] { "someurl" },
                    KindleEmail = "somemail@email.com"
                },
                new User()
                {
                    ArticleUrls = new string[] { "someurl2" },
                    KindleEmail = "somemail2@email.com"
                },
                new User()
                {
                    ArticleUrls = new string[] { "someurl3" },
                    KindleEmail = "somemail3@email.com"
                }
            };

        private User[] userMock2 = new User[]
            {
                new User()
                {
                    ArticleUrls = new string[] { "someurl2" },
                    KindleEmail = "somemail2@email.com"
                }
            };

        [Fact]
        public async void GoesThroughEveryUser()
        {
            var userServiceMock = new Mock<IUserService>();
            TableContinuationToken nullToken = null;
            userServiceMock.Setup(x => x.GetContinuationToken())
                .Returns(nullToken);
            userServiceMock.Setup(x => x.GetUserBatch())
                .ReturnsAsync(userMock);

            var sender = new Mock<ISender>();
            sender.Setup(x => x.SendAsync(It.IsAny<User>()))
                .ReturnsAsync(new string[] { "asd.com" });

            var userProcessor = new UserProcessor(userServiceMock.Object, sender.Object);

            await userProcessor.ProcessAsync();

            sender.Verify(x => x.SendAsync(userMock[0]), Times.Once);
            sender.Verify(x => x.SendAsync(userMock[1]), Times.Once);
            sender.Verify(x => x.SendAsync(userMock[2]), Times.Once);
        }

        [Fact]
        public async void GetsMultipleUserBatches()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.SetupSequence(x => x.GetContinuationToken())
                .Returns(new TableContinuationToken())
                .Returns(null);
            userServiceMock.SetupSequence(x => x.GetUserBatch())
                .ReturnsAsync(userMock)
                .ReturnsAsync(userMock2);

            var senderMock = new Mock<ISender>();
            senderMock.Setup(x => x.SendAsync(It.IsAny<User>()))
                .ReturnsAsync(new string[] { "asd.com" });

            var userProcessor = new UserProcessor(userServiceMock.Object, senderMock.Object);
            await userProcessor.ProcessAsync();

            senderMock.Verify(x => x.SendAsync(userMock[0]), Times.Once);
            senderMock.Verify(x => x.SendAsync(userMock[1]), Times.Once);
            senderMock.Verify(x => x.SendAsync(userMock[2]), Times.Once);
            senderMock.Verify(x => x.SendAsync(userMock2[0]), Times.Once);

            userServiceMock.Verify(x => x.GetUserBatch(), Times.Exactly(2));
        }
    }
}