using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Factories
{
    [TestClass]
    public sealed class NotificationMessageHeaderFactoryTests :
        MoqTestingTests<NotificationMessageHeaderFactory, ICreateNotificationMessageHeaders>
    {
        [TestMethod]
        [DataRow("about this", "from me", "to one", "called")]
        public async Task CreateMeetsExpectations(string expectedSubject, string expectedSender, string toAddress, string toName)
        {
            // arrange
            var sut = BuildTestSystem();

            // act
            var result = await sut.Create(expectedSubject, expectedSender, toAddress, toName);

            //assert
            result.Subject.Should().Be(expectedSubject);
            result.FromAddress.Should().Be(expectedSender);
            result.ToAddresses.Should().Contain(toAddress);
            result.CopyAddresses.Should().BeEmpty();
            result.Usernames.Should().Contain(toName);
        }

        [TestMethod]
        [DataRow("about this", "from me", "to one", "and two", "and three", "and four", "called one", "called two", "called three", "called four")]
        public async Task CreateMeetsExpectations(string expectedSubject, string expectedSender, params string[] toAddressesAndNames)
        {
            // arrange
            var sut = BuildTestSystem();
            var toAddresses = toAddressesAndNames.Take(4).ToArray();
            var toNames = toAddressesAndNames.Skip(4).ToArray();

            // act
            var result = await sut.Create(expectedSubject, expectedSender, toAddresses, toNames);

            //assert
            result.Subject.Should().Be(expectedSubject);
            result.FromAddress.Should().Be(expectedSender);
            result.ToAddresses.Should().Contain(toAddresses);
            result.CopyAddresses.Should().BeEmpty();
            result.Usernames.Should().Contain(toNames);
        }

        internal override NotificationMessageHeaderFactory BuildTestSystem() =>
            new NotificationMessageHeaderFactory();

        internal override void VerifyAllMocks(NotificationMessageHeaderFactory sut)
        {
            // nothing to do...
        }
    }
}