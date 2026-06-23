using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Services.AutoMapper;

namespace Pds.DocumentExchange.Data.Services.Tests.Unit.AutoMapperProfiles
{
    [TestClass]
    [TestCategory("Unit")]
    public class AutomapperTests
    {
        [TestMethod]
        public void AutoMapperProfileMeetsExpectation()
        {
            // arrange
            var profile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));

            // act / assert
            configuration.AssertConfigurationIsValid();
        }
    }
}
