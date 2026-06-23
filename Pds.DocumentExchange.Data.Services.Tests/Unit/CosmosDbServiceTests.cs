using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Pds.Core.Common.Organisation.Enums;
using Pds.Core.Common.Organisation.Models;
using Pds.Core.Logging;
using Pds.DocumentExchange.Data.Core.Common;
using Pds.DocumentExchange.Data.Repository.Interfaces;
using Pds.DocumentExchange.Data.Services.DTOs;
using Pds.DocumentExchange.Data.Services.Enums;
using Pds.DocumentExchange.Data.Services.Implementations.CosmosDb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit
{
    [TestClass]
    public class CosmosDbServiceTests
    {
        private readonly Mock<IDocumentsRepository> _documentsRepository = new Mock<IDocumentsRepository>();
        private readonly Mock<IFileRepository> _mockFileRepository = new Mock<IFileRepository>();
        private readonly Mock<ILoggerAdapter<CosmosDbServiceBase>> _mockLoggingService = new Mock<ILoggerAdapter<CosmosDbServiceBase>>();

        private readonly CosmosDbService _cosmosDbService;

        public CosmosDbServiceTests()
        {
            var cosmosDbRepository = _documentsRepository.Object;
            var fileRepository = _mockFileRepository.Object;
            var loggingRepository = _mockLoggingService.Object;

            _cosmosDbService = new CosmosDbService(cosmosDbRepository, fileRepository, loggingRepository);
        }

        /// <summary>Tests one of the methods (GetCountOfUnprocessedFiles) for the base method. passing in dynamic params array and returning a generic Type.</summary>
        /// <param name="spExists">if set to <c>true</c> then the test will pretend (for setting up the mocks) the sp exists.</param>
        /// <param name="readFileContentsSucceeds">if set to <c>true</c> then the test will pretend (mocks) the read file contents succeeds].</param>
        /// <param name="spCreationSucceeds">if set to <c>true</c> then the test will pretend (mocks) the sp creation succeeds.</param>
        /// <returns>An awaitable Task.</returns>
        [TestMethod, TestCategory("Unit")]
        [DataRow(true, true, true)]
        [DataRow(true, true, false)]
        [DataRow(true, false, true)]
        [DataRow(true, false, false)]
        [DataRow(false, true, true)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, false, false)]
        public async Task GetCountOfUnprocessedFilesTest(bool spExists, bool readFileContentsSucceeds, bool spCreationSucceeds)
        {
            var spName = CosmosDbStoredProcedureNames.GetCountOfUnprocessedFiles;

            // Arrange
            var spFileName = $"{spName}.js";
            const int invalidValue = -999, expectedSuccessResult = 1;
            const string parentBatchId = "parentBatchId1234", storedProcedureBody = "Body of the SP";
            var readFileException = new Exception("Reading file fails!");
            var createFileException = new Exception("Creating file fails!");
            var actualResult = invalidValue;
            var actualExceptionOccurred = false;

            _mockLoggingService.Setup(l => l.LogInformation(It.IsAny<string>())).Verifiable();
            var runSpSetupSequence = _documentsRepository.SetupSequence(c => c.RunStoredProcedure<int>(It.IsAny<string>(), It.IsAny<string>()));
            var createSpSetup = _documentsRepository.Setup(c => c.CreateStoredProcedure(It.IsAny<string>(), It.IsAny<string>()));
            var readFileSetup = _mockFileRepository.Setup(f => f.ReadSingleFileContents(It.IsAny<string>()));

            var exceptionExpected = false;
            if (spExists)
            {
                runSpSetupSequence.Returns(async () => await Task.Run(() => expectedSuccessResult));
            }
            else
            {
                // We setup the first call to RunStoredProcedure to fail, saying SP NotFound.
                runSpSetupSequence.Throws(new CosmosDbException { StatusCode = HttpStatusCode.NotFound });

                if (readFileContentsSucceeds)
                {
                    readFileSetup.Returns(storedProcedureBody);

                    // We need the second call to conditionally succeed/fail depending on input variables.
                    if (spCreationSucceeds)
                    {
                        runSpSetupSequence.Returns(async () => await Task.Run(() => expectedSuccessResult));
                    }
                    else
                    {
                        createSpSetup.Throws(createFileException);
                        exceptionExpected = true;
                    }
                }
                else
                {
                    readFileSetup.Throws(readFileException);
                    exceptionExpected = true;
                }
            }

            // Act
            try
            {
                actualResult = await _cosmosDbService.GetCountOfUnprocessedFiles(parentBatchId);
            }
            catch (Exception)
            {
                actualExceptionOccurred = true;
            }

            // Assert
            actualResult.Should().Be(!exceptionExpected ? expectedSuccessResult : invalidValue);
            actualExceptionOccurred.Should().Be(exceptionExpected);

            _mockFileRepository.Verify(f => f.ReadSingleFileContents(spFileName), !spExists ? Times.Once() : Times.Never());
            _documentsRepository.Verify(c => c.CreateStoredProcedure(spName, storedProcedureBody), (!spExists && readFileContentsSucceeds) ? Times.Once() : Times.Never());
        }

        [TestMethod, TestCategory("Unit")]
        [DataRow("SP01,SP02,SP03", "SP11,SP12,SP13", "SP21,SP22")]
        [DataRow("SP01-FailsReading,SP02,SP03", "SP11-FailsReading,SP12-FailsDelete,SP13", "SP21,SP22-FailsCreate")]
        [DataRow("SP01,SP02,SP03", "", "SP21,SP22")]
        [DataRow("SP01,SP02,SP03", "SP11,SP12,SP13", "")]
        [DataRow("", "SP11,SP12,SP13", "SP21,SP22")]
        [DataRow("", "", "SP21,SP22")]
        public async Task EnsureAllStoredProceduresUpToDateTest(string spsMatching, string spsNotMatch, string spsNotExist)
        {
            /*
             * An example of the input storedProcedureName names:
             * MatchingStoredProcedures     : SP01-FailsReading,            SP02,               SP03
             * NonMatchingStoredProcedures  : SP11-FailsReading,            SP12-FailsDelete,   SP13
             * MissingStoredProcedures      : SP21,                         SP22-FailsCreate
             *
             * The stored procedures also contain in their name if they are meant to throw exception:
             * on ReadStoredProcedure/DeleteStoredProcedure/CreateStoredProcedure.
             */

            // Arrange
            const string spBodyMatching = "/* Stored Procedure Body Matching. */";
            const string spBodyNotMatching = "/* Stored Procedure Body Not Matching. */";
            var fileNamesAndContents = new Dictionary<string, string>();

            var matchingArray = spsMatching.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var notMatchingArray = spsNotMatch.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
            var notExistingArray = spsNotExist.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            var (matchingSPs, notMatchingSPs, notExistSPs) = (new List<string>(), new List<string>(), new List<string>());
            var (spsFailReading, spsFailDelete, spsFailCreate) = (new List<string>(), new List<string>(), new List<string>());
            var (deleteException, createException, readException) = (new Exception("Delete SP fails"), new Exception("Create SP fails"), new Exception("Reading SP fails"));

            matchingArray.ForEach(sp => { AddToNormalOrExceptionLists(sp, spsFailReading, spsFailDelete, spsFailCreate, matchingSPs, fileNamesAndContents, spBodyMatching); });
            notMatchingArray.ForEach(sp => { AddToNormalOrExceptionLists(sp, spsFailReading, spsFailDelete, spsFailCreate, notMatchingSPs, fileNamesAndContents, spBodyMatching); });
            notExistingArray.ForEach(sp => { AddToNormalOrExceptionLists(sp, spsFailReading, spsFailDelete, spsFailCreate, matchingSPs, fileNamesAndContents, spBodyMatching); });

            _mockLoggingService.Setup(l => l.LogInformation(It.IsAny<string>())).Verifiable();
            _mockFileRepository.Setup(f => f.ReadMultipleFilesMatchingAPattern(It.IsAny<string>())).Returns(fileNamesAndContents);
            _documentsRepository.Setup(db => db.ReadStoredProcedure(It.Is<string>(sp => matchingSPs.Contains(sp)))).Returns(async () => await Task.Run(() => spBodyMatching));
            _documentsRepository.Setup(db => db.ReadStoredProcedure(It.Is<string>(sp => notMatchingSPs.Contains(sp)))).Returns(async () => await Task.Run(() => spBodyNotMatching));
            _documentsRepository.Setup(db => db.ReadStoredProcedure(It.Is<string>(sp => notExistSPs.Contains(sp)))).Throws(new CosmosDbException { StatusCode = HttpStatusCode.NotFound });
            _documentsRepository.Setup(db => db.ReadStoredProcedure(It.Is<string>(sp => spsFailReading.Contains(sp)))).Throws(readException);
            _documentsRepository.Setup(db => db.DeleteStoredProcedure(It.Is<string>(sp => spsFailDelete.Contains(sp)))).Throws(deleteException);
            _documentsRepository.Setup(db => db.CreateStoredProcedure(It.Is<string>(sp => spsFailCreate.Contains(sp)), It.IsAny<string>())).Throws(createException);

            // Act
            var expectedExceptionOccurred = spsFailReading.Any() || spsFailDelete.Any() || spsFailCreate.Any();
            var actualExceptionOccurred = false;

            string actualExceptionMessage = null;

            try
            {
                await _cosmosDbService.EnsureAllStoredProceduresUpToDate();
            }
            catch (Exception exc)
            {
                actualExceptionOccurred = true;
                actualExceptionMessage = exc.Message;
            }

            // Assert
            actualExceptionOccurred.Should().Be(expectedExceptionOccurred);
            if (expectedExceptionOccurred)
            {
                actualExceptionMessage.Should().NotBeNullOrEmpty();
                actualExceptionMessage.Should().ContainAll(spsFailReading);
                actualExceptionMessage.Should().ContainAll(spsFailDelete);
                actualExceptionMessage.Should().ContainAll(spsFailCreate);
            }

            spsFailReading.ForEach(sp => _mockLoggingService.Verify(l => l.LogError(readException, It.Is<string>(msg => msg.Contains($"Reading {sp} failed")))));
            spsFailDelete.ForEach(sp => _mockLoggingService.Verify(l => l.LogError(deleteException, It.Is<string>(msg => msg.Contains($"Deleting {sp} failed")))));
            spsFailCreate.ForEach(sp => _mockLoggingService.Verify(l => l.LogError(createException, It.Is<string>(msg => msg.Contains($"Creating {sp} failed")))));

            matchingSPs.ForEach(sp => _documentsRepository.Verify(db => db.CreateStoredProcedure(sp, It.IsAny<string>()), Times.Never()));
            notMatchingSPs.ForEach(sp =>
            {
                _documentsRepository.Verify(db => db.DeleteStoredProcedure(sp), Times.Once());
                _documentsRepository.Verify(db => db.CreateStoredProcedure(sp, It.IsAny<string>()), Times.Once());
            });
            notExistSPs.ForEach(sp =>
            {
                _documentsRepository.Verify(db => db.DeleteStoredProcedure(sp), Times.Never());
                _documentsRepository.Verify(db => db.CreateStoredProcedure(sp, It.IsAny<string>()), Times.Once);
            });
        }

        [TestMethod, TestCategory("Unit")]
        public void TestThatAllMethodsAreCalledWithAppropriateStoredProcedureName()
        {
            var methodNamesToTest = GetMethodNamesToTestForAzureCosmosDbStoredProcedures();
            var typeCosmosDbService = typeof(CosmosDbService);
            var setupRunProcedureMethodInfo = typeof(CosmosDbServiceTests).GetMethod("SetupRunStoredProcedure");
            var methodsWithJsonSerialisedParameters = new List<string> { "AddHistoryToFileMetadata", "UpdateDocumentMetadata" };
            List<string> errors = new List<string>();

            foreach (var serviceMethodToTest in typeCosmosDbService.GetMethods())
            {
                if (methodNamesToTest.Contains(serviceMethodToTest.Name))
                {
                    var objects = serviceMethodToTest.GetParameters().Select(parameter => FabricateAnObject(parameter.ParameterType)).ToArray();

                    try
                    {
                        if (!serviceMethodToTest.ContainsGenericParameters)
                        {
                            var methodResultPropertyInfo = serviceMethodToTest.ReturnType.GetProperty("Result");

                            if (methodResultPropertyInfo != null)
                            {
                                //Arrange
                                var resultType = methodResultPropertyInfo.PropertyType;
                                var returnTypeInstance = FabricateAnObject(resultType);

                                if (setupRunProcedureMethodInfo != null)
                                {
                                    var setupForRunStoredProcedure = setupRunProcedureMethodInfo.MakeGenericMethod(resultType);
                                    setupForRunStoredProcedure?.Invoke(
                                        this,
                                        new[] { serviceMethodToTest.Name, returnTypeInstance }); //Invoking SetupRunStoredProcedure<T> this way will ensure T is the actual type, and not object.

                                    //Act
                                    serviceMethodToTest.Invoke(_cosmosDbService, objects.ToArray());

                                    //Assert

                                    //Ensure the method was called with appropriate stored procedure name.
                                    _documentsRepository.Verify(r => r.RunStoredProcedure<It.IsAnyType>(serviceMethodToTest.Name, It.IsAny<dynamic[]>()));

                                    //If the method was wrapping all parameters into a JSON string, then ensure each parameter was present in the JSON serialised string.
                                    if (methodsWithJsonSerialisedParameters.Contains(serviceMethodToTest.Name))
                                    {
                                        objects.ToList().ForEach(
                                            parameterObj =>
                                            {
                                                if (parameterObj.GetType().IsPrimitive || "string".Equals(parameterObj.GetType().Name, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    _documentsRepository.Verify(dbRepo =>
                                                        dbRepo.RunStoredProcedure<It.IsAnyType>(serviceMethodToTest.Name, It.Is<string>(str => str.Contains($"{parameterObj}"))));
                                                }
                                                else
                                                {
                                                    var serializedObject = JsonConvert.SerializeObject(parameterObj);
                                                    _documentsRepository.Verify(dbRepo =>
                                                        dbRepo.RunStoredProcedure<It.IsAnyType>(serviceMethodToTest.Name, It.Is<string>(str => str.Contains($"{serializedObject}"))));
                                                }
                                            });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        //For failed methods, add error to list and continue testing the rest of the methods. Show all errors at the end.
                        errors.Add($"{serviceMethodToTest.Name} failed: with exception message {exc.Message}. Stacktrace: {exc.StackTrace}");
                    }
                }
            }

            errors.ForEach(Assert.Fail);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetConfiguration_RunsTheSprocAndReturnsTheValue()
        {
            //Arrange
            const int expected = 1234;
            _documentsRepository.Setup(
                db => db.RunStoredProcedure<int>("GetConfiguration", It.IsAny<dynamic[]>())).Returns(async () => await Task.Run(() => expected));

            //Act
            int actual = await _cosmosDbService.GetConfiguration<int>(ConfigurationSection.Teams);

            //Assert
            _documentsRepository.Verify(
                dbRepo => dbRepo.RunStoredProcedure<It.IsAnyType>("GetConfiguration", It.Is<string>(str => str.Contains($"{ConfigurationSection.Teams}"))));

            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task UpdateConfiguration_RunsTheSprocAndReturnsTheValue()
        {
            //Arrange
            const int expected = 1234;
            _documentsRepository.Setup(
                db => db.RunStoredProcedure<int>("UpdateConfiguration", It.IsAny<dynamic[]>())).Returns(async () => await Task.Run(() => expected));

            //Act
            int actual = await _cosmosDbService.UpdateConfiguration<int>(ConfigurationSection.Teams, expected);

            //Assert
            _documentsRepository.Verify(
                dbRepo => dbRepo.RunStoredProcedure<It.IsAnyType>("UpdateConfiguration", It.Is<string>(str => str.Contains($"{ConfigurationSection.Teams}")), expected.ToString()));

            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetListConfiguration_RunsTheSprocAndReturnsTheListOfValues()
        {
            //Arrange
            var expected = Enumerable.Range(1, 300).Where(i => i % 3 == 1).ToList();

            _documentsRepository.Setup(
                db => db.RunStoredProcedure<IEnumerable<int>>("GetListConfiguration", It.IsAny<dynamic[]>())).Returns(async () => await Task.Run(() => expected));

            //Act
            IEnumerable<int> actual = await _cosmosDbService.GetListConfiguration<int>(ConfigurationSection.Teams);

            //Assert
            _documentsRepository.Verify(
                dbRepo => dbRepo.RunStoredProcedure<It.IsAnyType>("GetListConfiguration", It.Is<string>(str => str.Contains($"{ConfigurationSection.Teams}"))));

            actual.Should().BeEquivalentTo(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task AddOrUpdateListConfiguration_RunsTheSprocAndReturnsTheValue()
        {
            //Arrange
            const int expected = 1234;
            _documentsRepository.Setup(
                db => db.RunStoredProcedure<int>("AddOrUpdateListConfiguration", It.IsAny<dynamic[]>())).Returns(async () => await Task.Run(() => expected));

            //Act
            int actual = await _cosmosDbService.AddOrUpdateListConfiguration<int, string>(ConfigurationSection.Teams, "identifier", expected);

            //Assert
            _documentsRepository.Verify(
                dbRepo => dbRepo.RunStoredProcedure<It.IsAnyType>("AddOrUpdateListConfiguration", It.Is<string>(str => str.Contains($"{ConfigurationSection.Teams}")), "identifier", expected.ToString()));

            actual.Should().Be(expected);
        }

        [TestMethod, TestCategory("Unit")]
        public async Task GetMIReport_RunsTheSprocAndReturnsTheValue()
        {
            //Arrange
            IEnumerable<MIReport> expected = new List<MIReport>().AsEnumerable();

            _documentsRepository.Setup(
                db => db.RunStoredProcedureWithContinuationToken<MIReport>("GetMIReport", It.IsAny<dynamic[]>())).Returns(async () => await Task.Run(() => expected));

            //Act
            var actual = await _cosmosDbService.GetMIReport(It.IsAny<DateTime>(), It.IsAny<DateTime>());

            //Assert
            _documentsRepository.Verify(
                dbRepo => dbRepo.RunStoredProcedureWithContinuationToken<It.IsAnyType>("GetMIReport", It.IsAny<dynamic[]>()));

            actual.Should().BeEquivalentTo(expected);
        }

        private static List<string> GetMethodNamesToTestForAzureCosmosDbStoredProcedures()
        {
            List<string> methodNamesToTest = new List<string>
            {
                "AddHistoryToFileMetadata",
                "MetadataOfDocumentsReceivedByOrganisation",
                "MetadataOfDocumentsSentByOrganisation",
                "GetBatchesByParent",
                "GetCountOfUnprocessedFiles",
                "GetCurrentVersionNumber",
                "GetReportOfDocumentsViewed",
                "GetReportOfDocumentsViewedSummary",
                "GetReportOfFileScanBad",
                "GetReportOfInitialUploadErrors",
                "GetReportOfFilesSentFromAgency",
                "TryAndSetAnExclusiveEmailLockIdOnTheBatches",
                "UpdateDocumentMetadata",
                "GetCountOfUnseenReceivedFiles",
                "GetMIReport"
            };
            return methodNamesToTest;
        }

        private static void AddToNormalOrExceptionLists(
            string storedProcedureName,
            List<string> spsFailReading,
            List<string> spsFailDelete,
            List<string> spsFailCreate,
            List<string> normalRequiredList,
            Dictionary<string, string> fileNamesAndContents,
            string storedProcedureBody)
        {
            if (storedProcedureName.Contains("FailsReading"))
            {
                spsFailReading.Add(storedProcedureName);
            }
            else if (storedProcedureName.Contains("FailsDelete"))
            {
                spsFailDelete.Add(storedProcedureName);
            }
            else if (storedProcedureName.Contains("FailsCreate"))
            {
                spsFailCreate.Add(storedProcedureName);
            }
            else
            {
                normalRequiredList.Add(storedProcedureName);
            }

            //Put all files into the files and contents dictionary, since each one needs to be tried.
            fileNamesAndContents.Add(storedProcedureName, storedProcedureBody);
        }

        private static object FabricateAnObject(Type parameterType)
        {
            string typeName = parameterType.Name, typeFullName = parameterType.FullName;

            object fabricatedObject = null;
            switch (typeName.ToLower())
            {
                case "configurationsection":
                    fabricatedObject = ConfigurationSection.DocumentExchangeOrganisationUploadEnabled;
                    break;
                case "datetime":
                    fabricatedObject = DateTime.Now;
                    break;
                case "filemetadata":
                    fabricatedObject = new FileMetadata { Metadata = new Dictionary<string, string>(), FileName = "File name 1", FromUkprn = 1234, ToUkprn = 5678, Version = 21, };
                    break;
                case "ienumerable`1":
                    if (!string.IsNullOrEmpty(typeFullName))
                    {
                        if (typeFullName.Contains("BatchMetadata", StringComparison.OrdinalIgnoreCase))
                        {
                            fabricatedObject = new[] { new BatchMetadata { Id = "1002", ParentBatchIdentifier = "P1003" } };
                        }
                        else if (typeFullName.Contains("ClickSummary", StringComparison.OrdinalIgnoreCase))
                        {
                            fabricatedObject = new[] { new ClickSummary { ClickCount = 1005, OriginalFileName = "F1006" } };
                        }
                        else if (typeFullName.Contains("ClickDetail", StringComparison.OrdinalIgnoreCase))
                        {
                            fabricatedObject = new[] { new ClickDetail { OriginalFileName = "F1007", Version = "V1008" } };
                        }
                        else if (typeFullName.Contains("ErrorDetail", StringComparison.OrdinalIgnoreCase))
                        {
                            fabricatedObject = new[] { new ErrorDetail { InitialErrors = "No Errors" } };
                        }
                        else if (typeFullName.Contains("organisationidentifier", StringComparison.OrdinalIgnoreCase))
                        {
                            fabricatedObject = new[] { new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" } };
                        }
                    }

                    break;
                case "int32":
                    fabricatedObject = 1001;
                    break;
                case "keyvaluepair`2":
                    // Currently there's only one method that needs returns key value pair, so the following code should be enough.
                    var dict = new Dictionary<string, Dictionary<string, string>>();
                    dict.Add("Key1", new Dictionary<string, string> { { "Key2", "Value2" } });
                    fabricatedObject = dict.FirstOrDefault();

                    break;
                case "organisationidentifier":
                    fabricatedObject = new OrganisationIdentifier { Type = OrganisationIdentifierType.Ukprn, Value = "12345678" };
                    break;
                case "string":
                    fabricatedObject = "str1001";
                    break;
            }

            return fabricatedObject;
        }
    }
}