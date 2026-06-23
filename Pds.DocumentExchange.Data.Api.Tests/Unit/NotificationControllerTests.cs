using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Core.BulkJobs.Interfaces;
using Pds.Core.BulkJobs.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Api.Controllers;
using Pds.DocumentExchange.Data.Services.DTOs.Notification;
using Pds.DocumentExchange.Data.Services.Interfaces.Coordinators;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit
{
    [TestClass]
    [TestCategory("Unit")]
    public class NotificationControllerTests
    {
        private readonly Mock<INotifyPublishCompleteCoordinator> _mockNotifyPublishCompleteCoordinator = new Mock<INotifyPublishCompleteCoordinator>(MockBehavior.Strict);
        private readonly Mock<INotifyUploadCompleteCoordinator> _mockNotifyUploadCompleteCoordinator = new Mock<INotifyUploadCompleteCoordinator>(MockBehavior.Strict);
        private readonly Mock<IBulkJobManager> _mockBulkJobManager = new Mock<IBulkJobManager>(MockBehavior.Strict);
        private readonly Mock<ILoggerAdapter<NotificationController>> _mockLogger = new Mock<ILoggerAdapter<NotificationController>>();

        private readonly NotificationController _notificationController;

        public NotificationControllerTests()
        {
            _notificationController = new NotificationController(
                _mockNotifyPublishCompleteCoordinator.Object,
                _mockNotifyUploadCompleteCoordinator.Object,
                _mockBulkJobManager.Object,
                _mockLogger.Object);
        }

        [TestMethod]
        public async Task AgencyPublishComplete_WhenUserInfoIsValid_ReturnsOkResponse()
        {
            // Arrange
            _mockBulkJobManager.Setup(x => x.CreateBulkJob(It.IsAny<IEnumerable<Job<string, BatchNotificationSummary>>>(), It.IsAny<Func<string, Task<BatchNotificationSummary>>>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            // Act
            var result = await _notificationController.AgencyPublishComplete("test");

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [TestMethod]
        public async Task AgencyPublishComplete_WhenUserInfoIsNotValid_ThrowsException()
        {
            // Arrange
            _mockBulkJobManager.Setup(x => x.CreateBulkJob(It.IsAny<IEnumerable<Job<string, BatchNotificationSummary>>>(), It.IsAny<Func<string, Task<BatchNotificationSummary>>>()))
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            // Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _notificationController.AgencyPublishComplete("test"));
        }

        [TestMethod]
        public async Task OrganisationUploadComplete_WhenUserInfoIsValid_ReturnsOkResponse()
        {
            // Arrange
            _mockNotifyUploadCompleteCoordinator.Setup(x => x.NotifyUsers(It.IsAny<string>()))
                .ReturnsAsync(It.IsAny<BatchNotificationSummary>());

            // Act
            var result = await _notificationController.OrganisationUploadComplete("test");

            // Assert
            result.Should().BeOfType<OkResult>();
        }

        [TestMethod]
        public async Task OrganisationUploadComplete_WhenNotifyUploadCompleteCoordinatorThrowsException_ThrowsException()
        {
            // Arrange
            _mockNotifyUploadCompleteCoordinator.Setup(x => x.NotifyUsers(It.IsAny<string>()))
                .ThrowsAsync(It.IsAny<ArgumentNullException>());

            // Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _notificationController.OrganisationUploadComplete("test"));
        }
    }
}
