using Microsoft.AspNetCore.Mvc;
using Pds.DocumentExchange.Data.Populator.Scenarios;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pds.DocumentExchange.Data.Populator.Controllers
{
    /// <summary>
    /// Populator for "documents uploaded from external" scenario.
    /// </summary>
    public partial class PopulatorController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> PopulateExample()
        {
            var stopwatch = Stopwatch.StartNew();

            var populator = new ExampleScenario();
            var result = await populator.PopulateScenarioData(
                new ExampleScenario.Configuration(),
                new ExampleScenario.Parameters());

            stopwatch.Stop();
            var took = stopwatch.Elapsed;
            return Ok($"Result was \"{result}\"; took: {took}");
        }

        [HttpGet]
        public async Task<IActionResult> TearDownExample()
        {
            var stopwatch = Stopwatch.StartNew();

            var populator = new ExampleScenario();
            var result = await populator.TearDownScenarioData(
                new ExampleScenario.Configuration());

            stopwatch.Stop();
            var took = stopwatch.Elapsed;
            return Ok($"Result was \"{result}\"; took: {took}");
        }
    }
}