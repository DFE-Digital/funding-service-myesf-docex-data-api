using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Factories
{
    [TestClass]
    public sealed class NotificationMessageFactoryTests :
        MoqTestingTests<NotificationMessageFactory, ICreateNotificationMessages>
    {
        [TestMethod]
        [DataRow("about this", "from me", "with content", true, "to one", "and two", "and three", "and four")]
        public async Task CreateMeetsExpectations(string expectedSubject, string expectedSender, string expectedContent, bool expectedTemplate, string parentBatchId, params string[] toAddresses)
        {
            // arrange
            var sut = BuildTestSystem();
            var header = MakeStrictMock<INotificationMessageHeader>();
            GetMock(header)
                .SetupGet(x => x.Subject)
                .Returns(expectedSubject);
            GetMock(header)
                .SetupGet(x => x.FromAddress)
                .Returns(expectedSender);
            GetMock(header)
                .SetupGet(x => x.ToAddresses)
                .Returns(toAddresses);
            GetMock(header)
                .SetupGet(x => x.Usernames)
                .Returns(MakeReadonlyItems<string>(0));
            GetMock(header)
                .SetupGet(x => x.CopyAddresses)
                .Returns(MakeReadonlyItems<string>(0));

            // act
            var result = await sut.Create(header, expectedContent, expectedTemplate, parentBatchId, 12345678);

            //assert
            result.Subject.Should().Be(expectedSubject);
            result.Content.Should().Be(expectedContent);
            result.UseStandardEmailTemplate.Should().Be(expectedTemplate);
            result.FromAddress.Should().Be(expectedSender);
            result.ToAddresses.Should().Contain(toAddresses);
            result.CopyAddresses.Should().BeEmpty();
            result.Usernames.Should().BeEmpty();
            result.ParentBatchId.Should().Be(parentBatchId);
        }

        internal override NotificationMessageFactory BuildTestSystem() =>
            new NotificationMessageFactory();

        internal override void VerifyAllMocks(NotificationMessageFactory sut)
        {
            // nothing to do...
        }
    }
}