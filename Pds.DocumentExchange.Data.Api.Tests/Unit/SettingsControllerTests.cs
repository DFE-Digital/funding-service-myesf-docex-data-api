using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Api.Models;
using Pds.DocumentExchange.Data.Services.Interfaces.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class SettingsControllerTests
    {
        private readonly Mock<IAdminSettingsService> _mockAdminSettingsService = new Mock<IAdminSettingsService>(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new Mock<IMapper>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<SettingsController>> _mockLoggingService = new Mock<ILoggerAdapter<SettingsController>>(MockBehavior.Loose);

        private readonly SettingsController _settingsController;

        public SettingsControllerTests()
        {
            _settingsController = new SettingsController(
                _mockAdminSettingsService.Object,
                _mockMapper.Object,
                _mockLoggingService.Object);
        }

        #region ProductsThatOrganisationsCanUpload

        [TestMethod]
        public async Task ProductsThatOrganisationsCanUpload_ReturnsTheAllowedProducts()
        {
            //Arrange
            var allProducts = GetAllProducts();
            var allServiceProducts = GetAllServiceProducts();

            var expectedServiceProducts = allServiceProducts.Where(p => p.CanOrganisationsUpload);
            var expectedProducts = allProducts.Where(p => p.CanOrganisationsUpload);

            _mockAdminSettingsService
                .Setup(c => c.GetProductsThatOrganisationsCanUpload())
                .ReturnsAsync(expectedServiceProducts);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<Product>>(expectedServiceProducts))
                .Returns(expectedProducts);

            // Act
            var result = await _settingsController.ProductsThatOrganisationsCanUpload();

            //Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedProducts);

            VerifyMocks();
        }

        [TestMethod]
        public async Task ProductsThatOrganisationsCanUpload_WhenNoProductsAllowed_ReturnsNoContentResponse()
        {
            //Arrange
            var allServiceProducts = new List<Services.DTOs.Product>
            {
                new Services.DTOs.Product { Identifier = 10036, Name = "Post 16 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10037, Name = "16 to 19 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10038, Name = "Capital CFO assurance statement", PluralName = "Capital CFO assurance statements", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10083, Name = "Alternative completions", PluralName = "Alternative completions", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10084, Name = "Business case audit evidence", PluralName = "Business case audit evidence", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10085, Name = "Business case", PluralName = "Business cases", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = false }
            };

            var expectedServiceProducts = allServiceProducts.Where(p => p.CanOrganisationsUpload);

            _mockAdminSettingsService
                 .Setup(c => c.GetProductsThatOrganisationsCanUpload())
                 .ReturnsAsync(expectedServiceProducts);

            // Act
            var result = await _settingsController.ProductsThatOrganisationsCanUpload();

            //Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NoContentResult>();

            VerifyMocks();
        }

        [TestMethod]
        public async Task ProductsThatOrganisationsCanUpload_WhenAdminSettingsServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetProductsThatOrganisationsCanUpload())
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _settingsController.ProductsThatOrganisationsCanUpload());
        }

        #endregion


        #region Products

        [TestMethod]
        public async Task GetProducts_ReturnsTheProducts()
        {
            // Arrange
            var allProducts = GetAllProducts();
            var allServiceProducts = GetAllServiceProducts();

            _mockAdminSettingsService
                .Setup(c => c.GetProducts())
                .ReturnsAsync(allServiceProducts);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<Product>>(allServiceProducts))
                .Returns(allProducts);

            // Act
            var result = await _settingsController.GetProducts();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(allProducts);

            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProducts_WhenAdminSettingsServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetProducts())
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _settingsController.GetProducts());
        }

        [TestMethod]
        [DataRow(10036)]
        [DataRow(10085)]
        public async Task GetProduct_ByIdentifier_ReturnsTheProduct(int identifier)
        {
            // Arrange
            var serviceProduct = GetServiceProductByIdentifier(identifier);
            var product = GetProductByIdentifier(identifier);

            _mockAdminSettingsService
                .Setup(admin => admin.GetProduct(identifier))
                .ReturnsAsync(serviceProduct);

            _mockMapper
                .Setup(m => m.Map<Product>(serviceProduct))
                .Returns(product);

            // Act
            var result = await _settingsController.GetProduct(identifier);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(product);

            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProduct_ByIdentifier_ReturnsUnknownProduct()
        {
            // Arrange
            int identifier = 0;

            var serviceProduct = new Services.DTOs.UnknownProduct();
            var product = new Product
            {
                Identifier = -1,
                Name = "[Unknown product]",
                PluralName = "[Unknown products]",
                AgencyTeams = Enumerable.Empty<string>(),
                CanOrganisationsUpload = false
            };

            _mockAdminSettingsService
                .Setup(admin => admin.GetProduct(identifier))
                .ReturnsAsync(serviceProduct);

            _mockMapper
                .Setup(m => m.Map<Product>(serviceProduct))
                .Returns(product);

            // Act
            var result = await _settingsController.GetProduct(identifier);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(product);

            VerifyMocks();
        }

        [TestMethod]
        public async Task GetProducts_ByIdentifier_WhenAdminSettingsServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetProduct(It.IsAny<int>()))
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _settingsController.GetProduct(It.IsAny<int>()));
        }

        [TestMethod]
        public async Task AddOrUpdateProduct_AddsOrUpdatesAndReturnsTheProduct()
        {
            // Arrange
            var productId = 10036;
            var product = GetProductByIdentifier(productId);

            var serviceProduct = GetServiceProductByIdentifier(productId);

            _mockMapper
                .Setup(m => m.Map<Services.DTOs.Product>(product))
                .Returns(serviceProduct);

            _mockMapper
                .Setup(m => m.Map<Product>(serviceProduct))
                .Returns(product);

            _mockAdminSettingsService
                .Setup(c => c.AddOrUpdateProduct(productId, serviceProduct))
                .ReturnsAsync(serviceProduct);

            // Act
            var result = await _settingsController.AddOrUpdateProduct(productId, product);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(product);

            VerifyMocks();
        }

        #region Product Helpers

        public List<Product> GetAllProducts()
        {
            return new List<Product>
            {
                new Product { Identifier = 10036, Name = "Post 16 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Product { Identifier = 10037, Name = "16 to 19 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Product { Identifier = 10038, Name = "Capital CFO assurance statement", PluralName = "Capital CFO assurance statements", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Product { Identifier = 10083, Name = "Alternative completions", PluralName = "Alternative completions", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true },
                new Product { Identifier = 10084, Name = "Business case audit evidence", PluralName = "Business case audit evidence", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true },
                new Product { Identifier = 10085, Name = "Business case", PluralName = "Business cases", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true }
            };
        }

        public Product GetProductByIdentifier(int identifier)
            => GetAllProducts().FirstOrDefault(p => p.Identifier == identifier);

        public List<Services.DTOs.Product> GetAllServiceProducts()
        {
            return new List<Services.DTOs.Product>
            {
                new Services.DTOs.Product { Identifier = 10036, Name = "Post 16 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10037, Name = "16 to 19 grant assurance", PluralName = "Post 16 grants assurance", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10038, Name = "Capital CFO assurance statement", PluralName = "Capital CFO assurance statements", AgencyTeams = new[] { "DocumentExchangeAdministratorRiskAssurance" }, CanOrganisationsUpload = false },
                new Services.DTOs.Product { Identifier = 10083, Name = "Alternative completions", PluralName = "Alternative completions", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true },
                new Services.DTOs.Product { Identifier = 10084, Name = "Business case audit evidence", PluralName = "Business case audit evidence", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true },
                new Services.DTOs.Product { Identifier = 10085, Name = "Business case", PluralName = "Business cases", AgencyTeams = new[] { "DocumentExchangeAdministratorFundingCentre" }, CanOrganisationsUpload = true }
            };
        }

        public Services.DTOs.Product GetServiceProductByIdentifier(int identifier)
            => GetAllServiceProducts().FirstOrDefault(p => p.Identifier == identifier);

        #endregion


        #endregion


        #region Teams

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(20)]
        public async Task GetTeams_ReturnsTheTeams(int numberOfTeams)
        {
            // Arrange
            var teams = Enumerable.Range(1, numberOfTeams)
                .Select(i => new AgencyTeam
                {
                    Identifier = $"team{i}",
                    Name = $"team{i}",
                    EmailAddress = $"team{i}@fake.org"
                })
                .ToList();

            var serviceTeams = Enumerable.Range(1, numberOfTeams)
                .Select(i => new Services.DTOs.AgencyTeam
                {
                    Identifier = $"team{i}",
                    Name = $"team{i}",
                    EmailAddress = $"team{i}@fake.org"
                })
                .ToList();

            _mockAdminSettingsService
                .Setup(c => c.GetTeams())
                .ReturnsAsync(serviceTeams);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<AgencyTeam>>(serviceTeams))
                .Returns(teams);

            // Act
            var result = await _settingsController.GetTeams();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(teams);

            VerifyMocks();
        }

        [TestMethod]
        public async Task GetTeams_WhenAdminSettingsServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockMapper
                .Setup(m => m.Map<AgencyTeam>(It.IsAny<Services.DTOs.AgencyTeam>()))
                .Returns(new AgencyTeam
                {
                    Identifier = $"team1",
                    Name = $"team1",
                    EmailAddress = $"team1@fake.org"
                });

            _mockAdminSettingsService
                .Setup(c => c.GetTeams())
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _settingsController.GetTeams());
        }

        [TestMethod]
        public async Task AddOrUpdateTeam_AddsOrUpdatesAndReturnsTheTeam()
        {
            // Arrange
            var team =
                new AgencyTeam
                {
                    Identifier = "team",
                    Name = "team",
                    EmailAddress = "team@fake.org"
                };

            var serviceTeam =
                new Services.DTOs.AgencyTeam
                {
                    Identifier = "team",
                    Name = "team",
                    EmailAddress = "team@fake.org"
                };

            _mockMapper
                .Setup(m => m.Map<Services.DTOs.AgencyTeam>(team))
                .Returns(serviceTeam);

            _mockMapper
                .Setup(m => m.Map<AgencyTeam>(serviceTeam))
                .Returns(team);

            _mockAdminSettingsService
                .Setup(c => c.AddOrUpdateTeam("team", serviceTeam))
                .ReturnsAsync(serviceTeam);

            // Act
            var result = await _settingsController.AddOrUpdateTeam("team", team);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(team);

            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateTeam_WhenAdminSettingsServiceThrowsException_ThrowsException()
        {
            //Arrange
            _mockMapper
                .Setup(m => m.Map<Services.DTOs.AgencyTeam>(It.IsAny<AgencyTeam>()))
                .Returns(new Services.DTOs.AgencyTeam
                {
                    Identifier = $"team1",
                    Name = $"team1",
                    EmailAddress = $"team1@fake.org"
                });

            _mockAdminSettingsService
                .Setup(c => c.AddOrUpdateTeam(It.IsAny<string>(), It.IsAny<Services.DTOs.AgencyTeam>()))
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            //Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _settingsController.AddOrUpdateTeam(It.IsAny<string>(), It.IsAny<AgencyTeam>()));
        }

        #endregion


        #region DocumentExchangeEnabled

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsDocumentExchangeEnabled_ReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.IsDocumentExchangeEnabled())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.IsDocumentExchangeEnabled();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetDocumentExchangeEnabledStatus_UpdatesAndReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetDocumentExchangeEnabledStatus(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetDocumentExchangeEnabledStatus(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion


        #region OrganisationUploadEnabled

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsOrganisationUploadEnabled_ReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.IsOrganisationUploadEnabled())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.IsOrganisationUploadEnabled();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetOrganisationUploadEnabledStatus_UpdatesAndReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetOrganisationUploadEnabledStatus(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetOrganisationUploadEnabledStatus(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion


        #region FileExtensions

        [TestMethod]
        [DataRow(1)]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(20)]
        public async Task GetFileExtensions_ReturnsTheFileExtensions(int numberOfFileExtensions)
        {
            // Arrange
            var fileExtensions = Enumerable.Range(1, numberOfFileExtensions)
                .Select(i => new FileExtensionInfo
                {
                    Identifier = $"ext{i}",
                    Extension = $"ext{i}",
                    ProductIdentifiers = Enumerable.Range(1, numberOfFileExtensions % 3)
                })
                .ToList();

            var serviceFileExtensions = Enumerable.Range(1, numberOfFileExtensions)
                .Select(i => new Services.DTOs.FileExtensionInfo
                {
                    Identifier = $"ext{i}",
                    Extension = $"ext{i}",
                    ProductIdentifiers = Enumerable.Range(1, numberOfFileExtensions % 3)
                })
                .ToList();

            _mockAdminSettingsService
                .Setup(c => c.GetFileExtensions())
                .ReturnsAsync(serviceFileExtensions);

            _mockMapper
                .Setup(m => m.Map<IEnumerable<FileExtensionInfo>>(serviceFileExtensions))
                .Returns(fileExtensions);

            // Act
            var result = await _settingsController.GetFileExtensions();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(fileExtensions);

            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateFileExtension_AddsOrUpdatesAndReturnsTheFileExtension()
        {
            // Arrange
            var extension = "ext";

            var fileExtension =
                new FileExtensionInfo
                {
                    Identifier = extension,
                    Extension = extension,
                    ProductIdentifiers = Enumerable.Range(10001, 5)
                };

            var serviceFileExtension =
                new Services.DTOs.FileExtensionInfo
                {
                    Identifier = extension,
                    Extension = extension,
                    ProductIdentifiers = Enumerable.Range(10001, 5)
                };

            _mockMapper
                .Setup(m => m.Map<Services.DTOs.FileExtensionInfo>(fileExtension))
                .Returns(serviceFileExtension);

            _mockMapper
                .Setup(m => m.Map<FileExtensionInfo>(serviceFileExtension))
                .Returns(fileExtension);

            _mockAdminSettingsService
                .Setup(c => c.AddOrUpdateFileExtension(extension, serviceFileExtension))
                .ReturnsAsync(serviceFileExtension);

            // Act
            var result = await _settingsController.AddOrUpdateFileExtension(extension, fileExtension);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(fileExtension);

            VerifyMocks();
        }

        #endregion


        #region ServiceReplyEmail

        [TestMethod]
        [DataRow("abc@test.org")]
        [DataRow("def@fake.org")]
        public async Task GetServiceReplyEmail_ReturnsTheSettingValue(string expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetServiceReplyEmail())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.GetServiceReplyEmail();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow("abc@test.org")]
        [DataRow("def@fake.org")]
        public async Task SetServiceReplyEmail_UpdatesAndReturnsTheSettingValue(string expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetServiceReplyEmail(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetServiceReplyEmail(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion


        #region ServiceNoReplyEmail

        [TestMethod]
        [DataRow("abc@test.org")]
        [DataRow("def@fake.org")]
        public async Task GetServiceNoReplyEmail_ReturnsTheSettingValue(string expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetServiceNoReplyEmail())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.GetServiceNoReplyEmail();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow("abc@test.org")]
        [DataRow("def@fake.org")]
        public async Task SetServiceNoReplyEmail_UpdatesAndReturnsTheSettingValue(string expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetServiceNoReplyEmail(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetServiceNoReplyEmail(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion


        #region AgencyPublishCompleteNotification

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task IsAgencyPublishCompleteProviderNotificationEnabled_ReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.IsAgencyPublishCompleteProviderNotificationEnabled())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.IsAgencyPublishCompleteProviderNotificationEnabled();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task SetAgencyPublishCompleteProviderNotificationEnabledStatus_UpdatesAndReturnsTheSettingValue(bool expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetAgencyPublishCompleteNotificationsStatus(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetAgencyPublishCompleteNotificationsStatus(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion


        #region GetMaxFileUploadSize

        [DataRow(true)]
        [DataRow(false)]
        public async Task GetMaxFileUploadSize_ReturnsTheSettingValue(int expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.GetMaxFileUploadSize())
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.GetMaxFileUploadSize();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        public async Task GetMaxFileUploadSize_UpdatesAndReturnsTheSettingValue(long expectedValue)
        {
            // Arrange
            _mockAdminSettingsService
                .Setup(c => c.SetMaxFileUploadSize(expectedValue))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await _settingsController.SetMaxFileUploadSize(expectedValue);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>().Which.Value.Should().BeEquivalentTo(expectedValue);

            VerifyMocks();
        }

        #endregion

        #region EmailSettings

        [TestMethod]
        public async Task GetEmailSettings_ReturnsTheSettingValue()
        {
            // Arrange
            var settings = new[]
            {
                new EmailSetting
                {
                    EmailMessageType = "test",
                    LastUpdated = DateTime.Now
                }
            };

            var serviceSettings = new[]
            {
                new Services.DTOs.EmailSetting
                {
                    EmailMessageType = "test",
                    LastUpdated = DateTime.Now
                }
            };

            _mockAdminSettingsService
                .Setup(c => c.GetEmailSettings()).Returns(Task.FromResult((IReadOnlyCollection<Services.DTOs.EmailSetting>)serviceSettings));

            _mockMapper
                .Setup(c => c.Map<IEnumerable<EmailSetting>>(It.IsAny<IEnumerable<Services.DTOs.EmailSetting>>())).Returns(settings);

            // Act
            var result = await _settingsController.GetEmailSettings();

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeEquivalentTo(settings);

            VerifyMocks();
        }

        [TestMethod]
        public async Task AddOrUpdateEmailSetting_ReturnsTheAddedOrUpdatedSettingValue()
        {
            // Arrange
            var setting = new EmailSetting
            {
                    EmailMessageType = "test",
                    LastUpdated = DateTime.Now
            };

            var serviceSettings = new Services.DTOs.EmailSetting
            {
                EmailMessageType = "test",
                LastUpdated = DateTime.Now
            };

            _mockAdminSettingsService
                .Setup(c => c.SetEmailSetting(It.IsAny<string>(), serviceSettings)).Returns(Task.FromResult(serviceSettings));

            _mockMapper
                .Setup(c => c.Map<Services.DTOs.EmailSetting>(It.IsAny<EmailSetting>())).Returns(serviceSettings);

            _mockMapper
                .Setup(c => c.Map<Product>(It.IsAny<Services.DTOs.EmailSetting>())).Returns(new Product());

            // Act
            var result = await _settingsController.AddOrUpdateEmailSetting("old test", setting);

            // Assert
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<AcceptedResult>();

            VerifyMocks();
        }

        #endregion

        private void VerifyMocks()
            => Mock.VerifyAll(
                _mockAdminSettingsService,
                _mockMapper,
                _mockLoggingService);
    }
}