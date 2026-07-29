using Mapster;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.DocumentExchange.Data.Api.Mapster;

namespace Pds.DocumentExchange.Data.Api.Tests.Unit.Mapster
{
    [TestClass]
    [TestCategory("Unit")]
    public class MapsterTests
    {
        [TestMethod]
        public void MapsterTypeAdapterConfigExtensionsMeetsExpectation()
        {
            // arrange
            TypeAdapterConfig config = new TypeAdapterConfig();
            config.Configure();

            // act / assert
            TypeAdapterConfig.GlobalSettings.Compile();
        }
    }
}