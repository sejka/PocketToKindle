using Core;
using Microsoft.Azure.WebJobs;
using Moq;
using Xunit;

namespace Tests
{
    public class UserProcessorTests
    {
        private readonly User[] userMock = new User[]
            {
                new User()
                {
                    KindleEmail = "somemail@email.com"
                },
                new User()
                {
                    KindleEmail = "somemail2@email.com"
                },
                new User()
                {
                    KindleEmail = "somemail3@email.com"
                }
            };

        private readonly User[] userMock2 = new User[]
            {
                new User()
                {
                    KindleEmail = "somemail2@email.com"
                }
            };

        [Fact]
        public async void GoesThroughEveryUser()
        {
            var userServiceMock = new Mock<IUserService>();
            var cloudQueueMock = new Mock<ICollector<User>>();
            userServiceMock.Setup(x => x.GetUserBatch())
                .ReturnsAsync(userMock);

            var userProcessor = new UserProcessor(userServiceMock.Object, cloudQueueMock.Object);

            await userProcessor.EnqueueAllUsersAsync();

            cloudQueueMock.Verify(x => x.Add(userMock[0]), Times.Once);
            cloudQueueMock.Verify(x => x.Add(userMock[1]), Times.Once);
            cloudQueueMock.Verify(x => x.Add(userMock[2]), Times.Once);
        }

        [Fact]
        public async void GetsMultipleUserBatches()
        {
            var userServiceMock = new Mock<IUserService>();
            userServiceMock.SetupSequence(x => x.GetUserBatch())
                .ReturnsAsync(userMock)
                .ReturnsAsync(userMock2);

            var queue = new Mock<ICollector<User>>();

            var userProcessor = new UserProcessor(userServiceMock.Object, queue.Object);
            await userProcessor.EnqueueAllUsersAsync();

            queue.Verify(x => x.Add(userMock[0]), Times.Once);
            queue.Verify(x => x.Add(userMock[1]), Times.Once);
            queue.Verify(x => x.Add(userMock[2]), Times.Once);
            queue.Verify(x => x.Add(userMock2[0]), Times.Once);

            userServiceMock.Verify(x => x.GetUserBatch(), Times.Exactly(2));
        }
    }
}