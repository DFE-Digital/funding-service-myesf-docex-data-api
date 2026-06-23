using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Scenarios
{
    public interface IPopulatorScenario<TConfiguration, TPopulateParameters, TPopulateResult, TTearDownResult>
    {
        Task<TPopulateResult> PopulateScenarioData(
            TConfiguration configuration,
            TPopulateParameters parameters);

        Task<TTearDownResult> TearDownScenarioData(
            TConfiguration configuration);
    }
}