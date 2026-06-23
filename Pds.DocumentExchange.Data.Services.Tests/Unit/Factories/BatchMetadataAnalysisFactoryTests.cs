using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.BatchAnalysis;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Exceptions;
using Pds.DocumentExchange.Data.Services.Implementations.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Factories;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.Factories
{
    [TestClass]
    public sealed class BatchMetadataAnalysisFactoryTests :
        MoqTestingTests<BatchMetadataAnalysisFactory, IBatchMetadataAnalysisFactory>
    {
        private const string TestEncryptionKey = "any old encryption key";
        private const string TestBatchID = "690e9f18-4b05-4c6b-ba62-11df98a12f67";
        private readonly Mock<ILoggerAdapter<BatchMetadataAnalysisFactory>> _mockLoggingService = new Mock<ILoggerAdapter<BatchMetadataAnalysisFactory>>(MockBehavior.Loose);

        [TestMethod]
        public void ConstructorFailsWithNullDocumentStore()
        {
            // arrange
            var encryptor = MakeStrictMock<IEncryptionService>();
            var configuration = MakeStrictMock<IConfigurationDataService>();
            var encryptionKey = MakeStrictMock<CosmosDbConfiguration>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new BatchMetadataAnalysisFactory(null, encryptor, configuration, encryptionKey, _mockLoggingService.Object));
        }

        [TestMethod]
        public void ConstructorFailsWithNullEncryptionService()
        {
            // arrange
            var docStore = MakeStrictMock<ICosmosDbService>();
            var configuration = MakeStrictMock<IConfigurationDataService>();
            var encryptionKey = MakeStrictMock<CosmosDbConfiguration>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new BatchMetadataAnalysisFactory(docStore, null, configuration, encryptionKey, _mockLoggingService.Object));
        }

        [TestMethod]
        public void ConstructorFailsWithNullConfigurationDataService()
        {
            // arrange
            var docStore = MakeStrictMock<ICosmosDbService>();
            var encryptor = MakeStrictMock<IEncryptionService>();
            var encryptionKey = MakeStrictMock<CosmosDbConfiguration>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new BatchMetadataAnalysisFactory(docStore, encryptor, null, encryptionKey, _mockLoggingService.Object));
        }

        [TestMethod]
        public void ConstructorFailsWithNullConfigrationSettings()
        {
            // arrange
            var docStore = MakeStrictMock<ICosmosDbService>();
            var encryptor = MakeStrictMock<IEncryptionService>();
            var configuration = MakeStrictMock<IConfigurationDataService>();

            // act / assert
            Assert.ThrowsException<ArgumentNullException>(() => new BatchMetadataAnalysisFactory(docStore, encryptor, configuration, null, _mockLoggingService.Object));
        }

        [TestMethod]
        public void EncryptionKeyMeetsExpectation()
        {
            // arrange
            var sut = BuildTestSystem();

            // act
            var result = sut.EncryptionKey;

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(TestEncryptionKey);
        }

        [TestMethod]
        public async Task CreateAnalysisFromMeetsExpectation()
        {
            // arrange
            var sut = BuildTestSystem();

            var batches = Enumerable.Empty<BatchMetadata>();

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));
            GetMock(sut.DocumentStore)
                .Setup(x => x.GetBatchesByParent(TestBatchID))
                .Returns(Task.FromResult(batches));

            // act
            var result = await sut.AnalyseBatch(TestBatchID);

            // assert
            VerifyAllMocks(sut);

            result.Should().BeAssignableTo<IBatchMetadataAnalysis>();
        }

        [TestMethod]
        public async Task CreateWithNoUploaderThrows()
        {
            // arrange
            var sut = BuildTestSystem();

            var metadata = new FileMetadata { };
            var files = MakeReadonlyItems<FileMetadata>(1, metadata);
            var batches = MakeReadonlyItems<BatchMetadata>(1, new BatchMetadata { Files = files });

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));

            // act / assert
            await Assert.ThrowsExceptionAsync<BatchAnalysisNoUploaderException>(() => sut.Create(batches, TestBatchID));
        }

        [TestMethod]
        public async Task CreateWithEmptyBatchThrows()
        {
            // arrange
            const string fullName = "full name";
            const string emailAddress = "email address";

            var sut = BuildTestSystem();

            var metadataUser = new FileMetadataUser { FullName = fullName, EmailAddress = emailAddress };
            var metadata = new FileMetadata { };
            var files = MakeReadonlyItems<FileMetadata>(1, metadata);
            var batch1 = new BatchMetadata { Files = files, UploadedBy = metadataUser };
            var batches = MakeReadonlyItems<BatchMetadata>(1, batch1);

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(fullName, TestEncryptionKey))
                .Returns(fullName);
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(emailAddress, TestEncryptionKey))
                .Returns(emailAddress);

            // act / assert
            await Assert.ThrowsExceptionAsync<BatchAnalysisBatchEmptyException>(() => sut.Create(batches, TestBatchID));
        }

        [TestMethod]
        public async Task CreateWithFromUKRPNProviderMismatchThrows()
        {
            // arrange
            const string fullName = "full name";
            const string emailAddress = "email address";

            var sut = BuildTestSystem();

            var metadataUser = new FileMetadataUser { FullName = fullName, EmailAddress = emailAddress };
            var metadata1 = new FileMetadata { FromUkprn = 1, Processed = true };
            var metadata2 = new FileMetadata { FromUkprn = 2, Processed = true };
            var files = new FileMetadata[] { metadata1, metadata2 };
            var batch1 = new BatchMetadata { Files = files, UploadedBy = metadataUser };
            var batches = MakeReadonlyItems<BatchMetadata>(1, batch1);

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(fullName, TestEncryptionKey))
                .Returns(fullName);
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(emailAddress, TestEncryptionKey))
                .Returns(emailAddress);

            // act / assert
            await Assert.ThrowsExceptionAsync<BatchAnalysisOrgCardinalityMismatchException>(() => sut.Create(batches, TestBatchID));
        }

        [TestMethod]
        public async Task CreateWithFromUKRPNProviderMismatchAccrossBatchesThrows()
        {
            // arrange
            const string fullName = "full name";
            const string emailAddress = "email address";

            var sut = BuildTestSystem();

            var metadataUser = new FileMetadataUser { FullName = fullName, EmailAddress = emailAddress };
            var metadata1 = new FileMetadata { FromUkprn = 1, Processed = true };
            var metadata2 = new FileMetadata { FromUkprn = 2, Processed = true };
            var files1 = MakeReadonlyItems<FileMetadata>(1, metadata1);
            var files2 = MakeReadonlyItems<FileMetadata>(1, metadata2);
            var batch1 = new BatchMetadata { Files = files1, UploadedBy = metadataUser };
            var batch2 = new BatchMetadata { Files = files2, UploadedBy = metadataUser };
            var batches = new BatchMetadata[] { batch1, batch2 };

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(fullName, TestEncryptionKey))
                .Returns(fullName);
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(emailAddress, TestEncryptionKey))
                .Returns(emailAddress);

            // act / assert
            await Assert.ThrowsExceptionAsync<BatchAnalysisOrgCardinalityMismatchException>(() => sut.Create(batches, TestBatchID));
        }

        [TestMethod]
        [DataRow(102392, false)]
        [DataRow(1, false)]
        [DataRow(0, false)]
        [DataRow(-1, false)]
        [DataRow(-999, true)]
        public async Task CreateMeetsExpectation(int testOrganisation, bool expectedInternal)
        {
            // arrange
            const string fullName = "full name";
            const string emailAddress = "email address";
            DateTime createDate = new DateTime(2019, 06, 01);

            var sut = BuildTestSystem();

            var metadataUser = new FileMetadataUser { FullName = fullName, EmailAddress = emailAddress };
            var theGood = new FileMetadata { FromUkprn = testOrganisation, Processed = true, VirusScanSuccessful = true };
            var theBad = new FileMetadata { FromUkprn = testOrganisation, Processed = true, History = new FileMetadataHistory[] { new FileMetadataHistory { Action = FileAction.FileScanBad } } };
            var theMissing = new FileMetadata { FromUkprn = testOrganisation, Processed = true };
            var files1 = new FileMetadata[] { theGood, theBad, theMissing };
            var batch1 = new BatchMetadata { Files = files1, UploadedBy = metadataUser, CreatedDate = createDate };
            var batches = MakeReadonlyItems<BatchMetadata>(1, batch1);

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(0)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(0)));
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(fullName, TestEncryptionKey))
                .Returns(fullName);
            GetMock(sut.Encryption)
                .Setup(x => x.Decrypt(emailAddress, TestEncryptionKey))
                .Returns(emailAddress);

            // act
            var result = await sut.Create(batches, TestBatchID);

            // assert
            result.Should().BeAssignableTo<IBatchMetadataAnalysis>();
            result.ParentBatchID.Should().Be(TestBatchID);
            result.InitialBatchDate.Should().Be(createDate);
            result.IssuingPerson.Should().Be(fullName);
            result.IssuingEmail.Should().Be(emailAddress);
            result.IssuingOrganisation.Should().Be(testOrganisation);
            result.Batches.Should().BeSameAs(batches);
            result.IsInternal.Should().Be(expectedInternal);
            result.ClearFiles.Count.Should().Be(1);
            result.InfectedFiles.Count.Should().Be(1);
        }

        [TestMethod]
        [DataRow(10392840)]
        [DataRow(8357)]
        [DataRow(28)]
        [DataRow(438343240)]
        public void TargetOrganisationIDMeetsExpectation(int candidate)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            var metadata = new FileMetadata { ToUkprn = candidate };

            // act
            var result = sut.ReceivingOrganisationUkprn(metadata);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(candidate);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void FileIsProcessedMeetsExpectation(bool candidate)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            var metadata = new FileMetadata { Processed = candidate };

            // act
            var result = sut.IsProcessed(metadata);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(candidate);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void FileIsClearMeetsExpectation(bool candidate)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            var metadata = new FileMetadata { VirusScanSuccessful = candidate };

            // act
            var result = sut.IsClear(metadata);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(candidate);
        }

        [TestMethod]
        [DataRow(FileAction.EmailSent, false)]
        [DataRow(FileAction.Error, false)]
        [DataRow(FileAction.ErrorPrePublish, false)]
        [DataRow(FileAction.FileScanGood, false)]
        [DataRow(FileAction.FileScanned, false)]
        [DataRow(FileAction.Moving, false)]
        [DataRow(FileAction.FileScanBad, true)]
        public void FileHasBadScanHistoryMeetsExpectation(FileAction candidate, bool expectedResult)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            var history = new FileMetadataHistory { Action = candidate };
            var metadata = new FileMetadata
            {
                History = MakeEnumerableItems<FileMetadataHistory>(1, history)
            };

            // act
            var result = sut.HasBadScanHistory(metadata);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        [DataRow("14", 14, true)]
        [DataRow("29304", 14, false)]
        [DataRow("4903948", 4903948, true)]
        [DataRow("1", 1, true)]
        [DataRow("2", 3, false)]
        [DataRow("strange alpha's that won't exist", 45723, false)]
        public void FileIsProductMeetsExpectation(string fileProduct, int productProduct, bool expectedResult)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            // and yes on the one side the identifier is a string and on the other an int...
            var product = new BatchAnalysisProduct(new Product { Identifier = productProduct });
            var metadata = new FileMetadata { ProductIdentifier = fileProduct };

            // act
            var result = sut.IsProduct(metadata, product);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        [DataRow("14", 14, true)]
        [DataRow("29304", 14, false)]
        [DataRow("4903948", 4903948, true)]
        [DataRow("1", 1, true)]
        [DataRow("2", 3, false)]
        [DataRow("strange alpha's that won't exist", 45723, false)]
        public void FileThatContainProductsMeetsExpectation(string fileProduct, int productProduct, bool expectedResult)
        {
            // arrange
            var sut = BuildTestSystem();

            // you can't mock these types.
            // and yes on the one side the identifier is a string and on the other an int...
            var product = new BatchAnalysisProduct(new Product { Identifier = productProduct });
            var products = MakeReadonlyItems<BatchAnalysisProduct>(1, product);
            var metadata = new FileMetadata { ProductIdentifier = fileProduct };
            var files = MakeReadonlyItems<FileMetadata>(1, metadata);

            // act
            var result = sut.ContainProducts(files, products);

            // assert
            VerifyAllMocks(sut);

            result.Should().Be(expectedResult);
        }

        [TestMethod]
        [DataRow("14", 14, 1)]
        [DataRow("29304", 14, 0)]
        [DataRow("4903948", 4903948, 1)]
        [DataRow("1", 1, 1)]
        [DataRow("2", 3, 0)]
        [DataRow("strange alpha's that won't exist", 45723, 0)]
        public async Task GetProductTeamsForMeetsExpectation(string fileProduct, int productProduct, int expectedResult)
        {
            // arrange
            const string teamId = "any old agency team";
            const string mailAddress = "any old mail address";

            var sut = BuildTestSystem();

            // you can't mock these types.
            // and yes on the one side the identifier is a string and on the other an int...
            var team = new AgencyTeam { Identifier = teamId, EmailAddress = mailAddress };
            var product = new Product { Identifier = productProduct, AgencyTeams = new string[] { teamId } };
            var metadata = new FileMetadata { ProductIdentifier = fileProduct };
            var files = MakeReadonlyItems<FileMetadata>(1, metadata);
            var batch1 = new BatchMetadata { Files = files };
            var batches = MakeReadonlyItems<BatchMetadata>(1, batch1);

            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetTeams())
                .Returns(Task.FromResult(MakeEnumerableItems<AgencyTeam>(1, team)));
            GetMock(sut.ConfigurationStore)
                .Setup(x => x.GetProducts())
                .Returns(Task.FromResult(MakeEnumerableItems<Product>(1, product)));

            // act
            var result = await sut.GetProductTeamsFor(batches);

            // assert
            VerifyAllMocks(sut);

            result.Should().BeAssignableTo<IReadOnlyCollection<IBatchAnalysisTeam>>();
            result.Count.Should().Be(expectedResult);
        }

        internal override void VerifyAllMocks(BatchMetadataAnalysisFactory sut)
        {
            GetMock(sut.DocumentStore).VerifyAll();
            GetMock(sut.ConfigurationStore).VerifyAll();
            GetMock(sut.Encryption).VerifyAll();
        }

        internal override BatchMetadataAnalysisFactory BuildTestSystem()
        {
            var docStore = MakeStrictMock<ICosmosDbService>();
            var encryptor = MakeStrictMock<IEncryptionService>();
            var configuration = MakeStrictMock<IConfigurationDataService>();
            var encryptionKey = new CosmosDbConfiguration { DataEncryptionKey = TestEncryptionKey };

            return new BatchMetadataAnalysisFactory(docStore, encryptor, configuration, encryptionKey, _mockLoggingService.Object);
        }
    }
}