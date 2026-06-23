using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Scenarios
{
    public class ExampleScenario : IPopulatorScenario<
        ExampleScenario.Configuration,
        ExampleScenario.Parameters,
        string,
        string>
    {
        public async Task<string> PopulateScenarioData(
            Configuration configuration,
            Parameters parameters)
        {
            return await Task.FromResult("Hello, world!");
        }

        public async Task<string> TearDownScenarioData(
            Configuration configuration)
        {
            return await Task.FromResult("Goodbye, world!");
        }

        public class Configuration
        {
        }

        public class Parameters
        {
        }
    }
}