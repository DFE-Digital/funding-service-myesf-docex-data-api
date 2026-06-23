using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class EncryptionServiceTests
    {
        private readonly IEncryptionService _encryptionService;

        public EncryptionServiceTests()
        {
            _encryptionService = new EncryptionService();
        }

        [TestMethod, TestCategory("Unit")]
        public void EncryptAndDecrypt_ShortInput_ShouldBeReversible()
        {
            //Arrange
            const string testInput = "I am Iron Man.";
            string topSecret = @"1 4m T0p S3creT";

            //Act
            var encrypted = _encryptionService.Encrypt(testInput, topSecret);
            var decrypted = _encryptionService.Decrypt(encrypted, topSecret);

            //Assert
            encrypted.Should().NotBeEquivalentTo(decrypted);
            decrypted.Should().Be(testInput);
        }

        [TestMethod, TestCategory("Unit")]
        public void EncryptAndDecrypt_MediumInput_ShouldBeReversible()
        {
            //Arrange
            const string testInput = "a.very.long.email.address+1234567890@fake.com";
            string topSecret = @"1 4m T0p S3creT";

            //Act
            var encrypted = _encryptionService.Encrypt(testInput, topSecret);
            var decrypted = _encryptionService.Decrypt(encrypted, topSecret);

            //Assert
            encrypted.Should().NotBeEquivalentTo(decrypted);
            decrypted.Should().Be(testInput);
        }

        [TestMethod, TestCategory("Unit")]
        public void EncryptAndDecrypt_LongInput_ShouldBeReversible()
        {
            //Arrange
            const string testInput =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec sagittis nec ipsum vel fermentum. " +
                "Vivamus convallis ut nunc ut bibendum. Sed placerat, massa vitae ultrices molestie, risus orci el" +
                "eifend enim, id mattis mauris neque vel arcu. Etiam id lorem elit. Sed eu mauris leo. Aenean nec " +
                "tristique erat. Interdum et malesuada fames ac ante ipsum primis in faucibus. Etiam ac sem sit am" +
                "et dui rutrum luctus dictum vitae dolor. Morbi ligula turpis, faucibus nec scelerisque sed, aliqu" +
                "et et orci. Mauris congue ex vel accumsan bibendum. Nam ac euismod ante. Mauris varius, dolor sed" +
                " posuere cursus, magna nisi mattis eros, eu tincidunt ligula orci a tortor.Morbi eu gravida velit" +
                ". Maecenas rutrum, libero a viverra ornare, ex turpis rhoncus est, vitae viverra orci nisi at nib" +
                "h.Curabitur efficitur libero sapien, congue condimentum nunc tristique vulputate. Suspendisse sit" +
                " amet tincidunt tortor, eu cursus nisl.Etiam nisl eros, sollicitudin et tortor sit amet, viverra " +
                "volutpat lacus.Cras et ex turpis. Orci varius orci.";

            string topSecret = @"1 4m T0p S3creT";

            //Act
            var encrypted = _encryptionService.Encrypt(testInput, topSecret);
            var decrypted = _encryptionService.Decrypt(encrypted, topSecret);

            //Assert
            encrypted.Should().NotBeEquivalentTo(decrypted);
            decrypted.Should().Be(testInput);
        }

        [TestMethod, TestCategory("Unit")]
        public void Encrypt_NullInput_ShouldBeNull()
        {
            //Arrange
            const string testInput = null;
            string topSecret = @"53creT";

            //Act
            var act = _encryptionService.Encrypt(testInput, topSecret);

            //Assert
            act.Should().BeNull();
        }

        [TestMethod, TestCategory("Unit")]
        public void Decrypt_NullInput_ShouldBeNull()
        {
            //Arrange
            const string testInput = null;
            string topSecret = @"53creT";

            //Act
            var act = _encryptionService.Decrypt(testInput, topSecret);

            //Assert
            act.Should().BeNull();
        }
    }
}