using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.AutoMapperProfiles;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.AutoMapperProfiles
{
    [TestClass]
    [TestCategory("Unit")]
    public class AutomapperTests
    {
        // after considerable investigation this is the only test that can
        // be performed against an auto mapper configuration / setup.
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
