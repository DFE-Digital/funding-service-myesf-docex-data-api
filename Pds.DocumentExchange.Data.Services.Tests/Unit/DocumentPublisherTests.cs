using AutoMapper;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Logging;
using Pds.Core.Utils;
using Pds.Core.Utils.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.DTOs.Configuration;
using Pds.DocumentExchange.Data.Services.DTOs.Filters;
using Pds.DocumentExchange.Data.Services.DTOs.User;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations;
using Pds.DocumentExchange.Data.Services.Interfaces;
using Pds.DocumentExchange.Data.Services.Interfaces.Lookup;
using Pds.DocumentExchange.Data.Services.Interfaces.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    [TestCategory("Unit")]
    public class DocumentPublisherTests
    {
        private readonly Mock<IDirectoriesManager> _mockDirectoriesManager =
            new Mock<IDirectoriesManager>(MockBehavior.Strict);

        private readonly Mock<IServiceBusQueueManager> _mockServiceQueueManager =
            new Mock<IServiceBusQueueManager>(MockBehavior.Strict);

        private readonly Mock<ICosmosDbService> _mockCosmosDbService = new Mock<ICosmosDbService>(MockBehavior.Strict);

        private readonly Mock<IFileNameProvider> _mockFileNameProvider =
            new Mock<IFileNameProvider>(MockBehavior.Strict);

        private readonly Mock<IFileMetadataUserEncryptor> _mockFileMetadataUserEncryptor =
            new Mock<IFileMetadataUserEncryptor>(MockBehavior.Strict);

        private readonly Mock<ISystemProvider> _mockSystemProvider = new Mock<ISystemProvider>(MockBehavior.Strict);
        private readonly Mock<IProductsLookup> _mockProductsLookup = new Mock<IProductsLookup>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>(MockBehavior.Strict);

        private readonly Mock<ILoggerAdapter<DocumentPublisher>> _mockLogger =
            new Mock<ILoggerAdapter<DocumentPublisher>>(MockBehavior.Strict);

        private readonly Mock<IAgencyService> _mockAgencyService = new Mock<IAgencyService>(MockBehavior.Strict);

        private readonly Mock<IDirectory> _documentSourceMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);
        private readonly Mock<IDirectory> _documentDestinationMockDirectory = new Mock<IDirectory>(MockBehavior.Strict);

        private readonly Mock<IJsonMessageServiceBusQueue> _virusScanRequiredMockQueue =
            new Mock<IJsonMessageServiceBusQueue>(MockBehavior.Strict);

        private readonly Mock<IBulkJobManager> _bulkJobManager = new Mock<IBulkJobManager>(MockBehavior.Strict);

        private readonly Mock<IRetryMechanism> _retry = new Mock<IRetryMechanism>(MockBehavior.Loose);

        public DocumentPublisher GetTestPublisher(DocumentPublisherConfiguration configuration)
        {
            return new DocumentPublisher(
                _mockDirectoriesManager.Object,
                _mockServiceQueueManager.Object,
                _mockCosmosDbService.Object,
                _mockFileNameProvider.Object,
                _mockProductsLookup.Object,
                _mockFileMetadataUserEncryptor.Object,
                _mockSystemProvider.Object,
                _mockMapper.Object,
                configuration,
                _mockLogger.Object,
                _mockAgencyService.Object,
                _bulkJobManager.Object,
                _retry.Object);
        }

        [TestMethod]
        public async Task PublishDocument_WhenProductIsAllowed_MovesDocumentAndPushesMessageToQueue()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "Principal",
                FullName = "FullName",
                EmailAddress = "Email address"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = userInfo,
                ProductId = 1002
            };

            var fileNames = new List<string>
            {
                "123456_1002_abc.pdf",
                "123456_1005_abc.pdf",
                "123456_1005_def.pdf"
            };

            var fileNamesForProduct = fileNames
                .Where(f => f.Contains($"_{agencyPublishRequest.ProductId}_"))
                .ToList();

            var team = "team";
            var utcNow = new DateTime(2020, 1, 1);

            var parentBatchIdentifier = new Guid("ea5e2974-8cf9-495d-8659-b1a32d42f0e3");

            var organisationIdValue = 123456;

            var mappedUser = new FileMetadataUser
            {
                Principal = userInfo.Principal,
                FullName = userInfo.FullName,
                EmailAddress = userInfo.EmailAddress,
                IsEncrypted = false
            };

            var encryptedUser = new FileMetadataUser
            {
                Principal = "encrypted-principal",
                FullName = "encrypted-full-name",
                EmailAddress = "encrypted-email-address",
                IsEncrypted = true
            };

            var product = new Product
            {
                Identifier = agencyPublishRequest.ProductId,
                Name = "Product 1",
                PluralName = "Products 1",
                AgencyTeams = new List<string> { team }
            };

            var agencyDocuments = new ListResult<AgencyDocument>
            {
                Items = fileNamesForProduct
                            .Select(
                                f => new AgencyDocument
                                {
                                    FileName = f,
                                    Product = product
                                })
            };

            var documentPublisherConfiguration = new DocumentPublisherConfiguration
            {
                AgencyDestinationDirectoryKey = "processing",
                VirusScanRequiredQueueName = "virus-scan-required-queue"
            };

            _mockProductsLookup
                .Setup(productsLookup => productsLookup.Get(agencyPublishRequest.ProductId))
                .ReturnsAsync(product);

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(team))
                .ReturnsAsync(_documentSourceMockDirectory.Object);

            _mockDirectoriesManager
                .Setup(manager => manager.GetDirectory(documentPublisherConfiguration.AgencyDestinationDirectoryKey))
                .ReturnsAsync(_documentDestinationMockDirectory.Object);

            var mockNewGuidSequence = _mockSystemProvider
                .SetupSequence(systemProvider => systemProvider.Guid.NewGuid())
                .Returns(parentBatchIdentifier);

            _mockAgencyService
                .Setup(
                    agencyService => agencyService.GetDocuments(
                        It.Is<IEnumerable<string>>(
                            teams => teams.Single() == team),
                        It.Is<AgencyListDocumentOptions>(
                            opt =>
                                opt.PageNumber == 1 &&
                                opt.PageSize == int.MaxValue &&
                                opt.Validity == AgencyDocumentValidity.Valid &&
                                opt.FilterOptions.Any(
                                    f => f is RadioFilterOption &&
                                         ((RadioFilterOption)f).Key == FilterKey.ProductIdRadio.ToString() &&
                                         ((RadioFilterOption)f).Value ==
                                         agencyPublishRequest.ProductId.ToString()))))
                .ReturnsAsync(agencyDocuments);

            _mockMapper
                .Setup(m => m.Map<UserInfo, FileMetadataUser>(userInfo))
                .Returns(mappedUser);

            _mockFileMetadataUserEncryptor
                .Setup(encryptor => encryptor.Encrypt(mappedUser))
                .Returns(encryptedUser);

            _mockSystemProvider
                .Setup(systemProvider => systemProvider.DateTime.UtcNow())
                .Returns(utcNow);

            _mockLogger
               .Setup(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()));

            _mockLogger
                 .Setup(
                     l => l.LogInformation(
                         It.Is<string>(
                             s => s.Contains(
                                 $"Starting publish of {agencyDocuments.Items.Count()} documents with product ID {agencyPublishRequest.ProductId} and team:{team}."))));
            _mockLogger
                 .Setup(
                     l => l.LogInformation(
                         It.Is<string>(
                             s => s.Contains(
                                 $"Starting PublishDocuments with team:{team} and product ID {agencyPublishRequest.ProductId}."))));

            _mockLogger
                 .Setup(
                     l => l.LogInformation(
                         It.Is<string>(
                             s => s.Contains(
                                 $"Finished PublishDocuments with product ID {agencyPublishRequest.ProductId} and team:{team}."))));

            _documentSourceMockDirectory
                .Setup(src => src.GetFiles())
                .Returns(
                    CreateIAsyncEnumerable(
                        fileNames.Select(f => new FileReferenceInfo { FileName = f })));

            foreach (var fileName in fileNamesForProduct)
            {
                var batchId = Guid.NewGuid();
                mockNewGuidSequence.Returns(batchId);

                _documentSourceMockDirectory
                    .Setup(dir => dir.Copy(_documentDestinationMockDirectory.Object, fileName))
                    .ReturnsAsync(fileName);

                _mockLogger
                    .Setup(logger => logger.LogInformation(It.Is<string>(s => s.Contains($"Moved file {fileName}"))));

                _mockFileNameProvider
                    .Setup(fileNameProvider => fileNameProvider.GetComponents(fileName, true))
                    .Returns(
                        new FileNameComponents
                        {
                            ProductIdentifier = agencyPublishRequest.ProductId.ToString(),
                            AcademicYear = 202021,
                            OrganisationIdentifier = organisationIdValue
                        });

                _mockCosmosDbService
                    .Setup(
                        db => db.AddBatch(
                            It.Is<BatchMetadata>(
                                batch =>
                                    batch.ParentBatchIdentifier == parentBatchIdentifier.ToString() &&
                                    batch.Id == batchId.ToString() &&
                                    batch.CreatedDate == utcNow &&
                                    batch.UploadedBy == encryptedUser &&
                                    batch.Files.Single().FileName == fileName &&
                                    batch.Files.Single().OriginalFileName == fileName &&
                                    batch.Files.Single().ProductIdentifier ==
                                    agencyPublishRequest.ProductId.ToString() &&
                                    batch.Files.Single().Metadata["Year"] == "202021" &&
                                    batch.Files.Single().ToUkprn == organisationIdValue)))
                    .Returns(Task.CompletedTask);

                _mockLogger
                    .Setup(
                        l => l.LogInformation(
                            It.Is<string>(
                                s => s.Contains($"{batchId} metadata saved"))));
            }

            _mockLogger
                .Setup(
                    l => l.LogInformation(
                        It.Is<string>(
                            s => s.Contains(
                                $"{agencyDocuments.Items.Count()} files moved"))));

            _mockServiceQueueManager
                .Setup(manager => manager.GetQueue(documentPublisherConfiguration.VirusScanRequiredQueueName))
                .Returns(_virusScanRequiredMockQueue.Object);

            _virusScanRequiredMockQueue
                .Setup(
                    queue => queue.PushMessage(
                        It.Is<VirusScanRequestMessage>(
                            msg =>
                                msg.ParentBatchIdentifier == parentBatchIdentifier.ToString() &&
                                msg.DocumentDirection == ExchangeDocumentDirection.PublishedByAgency)))
                .Returns(Task.CompletedTask);

            _mockLogger
                .Setup(
                    l => l.LogInformation(
                        It.Is<string>(
                            s => s.Contains(
                                $"{parentBatchIdentifier} sent to queue {documentPublisherConfiguration.VirusScanRequiredQueueName}"))));

            var documentPublisher = GetTestPublisher(documentPublisherConfiguration);

            string documentPublisherResult = string.Empty;

            _bulkJobManager
                .Setup(manager => manager.CreateBulkJob(
                    It.IsAny<IEnumerable<Job<string, string>>>(),
                    It.IsAny<Func<string, Task<string>>>()))
                .ReturnsAsync(
                    (IEnumerable<Job<string, string>> _, Func<string, Task<string>> publishFunction) =>
                    {
                        documentPublisherResult = publishFunction(string.Empty).GetAwaiter().GetResult();
                        return Guid.NewGuid();
                    });

            // Act
            var result = await documentPublisher.PublishDocuments(team, agencyPublishRequest);

            // Assert
            result.Should()
                .BeEquivalentTo(
                    new KeyValuePair<Product, int>(product, fileNamesForProduct.Count));

            documentPublisherResult.Should().Be($"{fileNamesForProduct.Count} documents published.");

            Mock.VerifyAll(
                _mockProductsLookup,
                _mockDirectoriesManager,
                _mockAgencyService,
                _mockFileMetadataUserEncryptor,
                _documentSourceMockDirectory,
                _mockCosmosDbService,
                _virusScanRequiredMockQueue);
        }

        [TestMethod]
        public void PublishDocument_WhenFilterReturnsAnotherProduct_ThrowsException()
        {
            // Arrange
            var userInfo = new UserInfo
            {
                Principal = "Principal",
                FullName = "FullName",
                EmailAddress = "Email address"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = userInfo,
                ProductId = 1002
            };

            var fileNames = new List<string>
            {
                "123456_1002_abc.pdf",
                "123456_1005_abc.pdf",
                "123456_1005_def.pdf"
            };

            var fileNamesForProduct = fileNames
                .Where(f => f.Contains($"_{agencyPublishRequest.ProductId}_"))
                .ToList();

            var team = "team";

            var product = new Product
            {
                Identifier = agencyPublishRequest.ProductId,
                Name = "Product 1",
                PluralName = "Products 1",
                AgencyTeams = new List<string> { team }
            };

            var unexpectedProduct = new Product
            {
                Identifier = agencyPublishRequest.ProductId + 1,
                Name = "Product 2",
                PluralName = "Products 2",
                AgencyTeams = new List<string> { team }
            };
            _mockLogger
                .Setup(
                    l => l.LogInformation(
                        It.Is<string>(
                            s => s.Contains(
                                $"Starting PublishDocuments with team:{team} and product ID {agencyPublishRequest.ProductId}."))));

            _mockLogger
                 .Setup(
                     l => l.LogInformation(
                         It.Is<string>(
                             s => s.Contains(
                                 $"Finished PublishDocuments with product ID {agencyPublishRequest.ProductId} and team:{team}."))));

            _mockLogger
                .Setup(
                    l => l.LogInformation(
                        It.Is<string>(
                            s => s.Contains(
                                $"Starting PublishDocuments with team:{team} and product ID {agencyPublishRequest.ProductId}."))));

            _mockLogger
               .Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            _mockProductsLookup
                .Setup(productsLookup => productsLookup.Get(agencyPublishRequest.ProductId))
                .ReturnsAsync(product);

            _mockAgencyService
                .Setup(
                    agencyService => agencyService.GetDocuments(
                        It.Is<IEnumerable<string>>(
                            teams => teams.Single() == team),
                        It.Is<AgencyListDocumentOptions>(
                            opt =>
                                opt.PageNumber == 1 &&
                                opt.PageSize == int.MaxValue &&
                                opt.Validity == AgencyDocumentValidity.Valid &&
                                opt.FilterOptions.Any(
                                    f => f is RadioFilterOption &&
                                         ((RadioFilterOption)f).Key == FilterKey.ProductIdRadio.ToString() &&
                                         ((RadioFilterOption)f).Value ==
                                         agencyPublishRequest.ProductId.ToString()))))
                .ReturnsAsync(
                    new ListResult<AgencyDocument>
                    {
                        Items = fileNamesForProduct
                            .Select(
                                f => new AgencyDocument
                                {
                                    FileName = f,
                                    Product = unexpectedProduct
                                })
                    });

            // Act
            Func<Task> act = async () => await GetTestPublisher(new DocumentPublisherConfiguration())
                .PublishDocuments(team, agencyPublishRequest);

            // Assert
            act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage($"*aborting document publish operation*");
        }

        [TestMethod]
        public void PublishDocument_WhenProductNotFound_ThrowsException()
        {
            // Arrange
            var team = "team";

            var userInfo = new UserInfo
            {
                Principal = "Principal",
                FullName = "FullName",
                EmailAddress = "Email address"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = userInfo,
                ProductId = 1002
            };

            _mockProductsLookup
                .Setup(productsLookup => productsLookup.Get(agencyPublishRequest.ProductId))
                .ReturnsAsync(new UnknownProduct());

            _mockLogger
              .Setup(
                  l => l.LogInformation(
                      It.Is<string>(
                          s => s.Contains(
                              $"Starting PublishDocuments with team:{team} and product ID {agencyPublishRequest.ProductId}."))));

            _mockLogger
              .Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            // Act
            Func<Task> act = async () => await GetTestPublisher(new DocumentPublisherConfiguration())
                .PublishDocuments(team, agencyPublishRequest);

            // Assert
            act.Should()
                .ThrowAsync<InvalidDataException>()
                .WithMessage($"*identifier {agencyPublishRequest.ProductId} not found*");
        }

        [TestMethod]
        public void PublishDocument_WhenTeamIsNotAllowedToPublishProduct_ThrowsException()
        {
            // Arrange
            var team = "team";

            var userInfo = new UserInfo
            {
                Principal = "Principal",
                FullName = "FullName",
                EmailAddress = "Email address"
            };

            var agencyPublishRequest = new AgencyPublishRequest
            {
                UserInfo = userInfo,
                ProductId = 1002
            };

            _mockProductsLookup
                .Setup(productsLookup => productsLookup.Get(agencyPublishRequest.ProductId))
                .ReturnsAsync(
                    new Product
                    {
                        Identifier = agencyPublishRequest.ProductId,
                        Name = $"Product {agencyPublishRequest.ProductId}",
                        AgencyTeams = new[] { "different team" }
                    });

            _mockLogger
              .Setup(
                  l => l.LogInformation(
                      It.Is<string>(
                          s => s.Contains(
                              $"Starting PublishDocuments with team:{team} and product ID {agencyPublishRequest.ProductId}."))));

            _mockLogger
              .Setup(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()));

            // Act
            Func<Task> act = async () => await GetTestPublisher(new DocumentPublisherConfiguration())
                .PublishDocuments(team, agencyPublishRequest);

            // Assert
            act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*not authorised to publish*");
        }

        private async IAsyncEnumerable<T> CreateIAsyncEnumerable<T>(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                yield return item;
            }

            await Task.CompletedTask;
        }
    }
}