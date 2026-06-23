using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace Pds.DocumentExchange.Data.Populator.MvcConfiguration
{
    /// <summary>
    /// Transforms route values to strings for use in URIs by slugifying the value.
    /// For example, an action "GetValue" will be represented in the URI as "get-value".
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-3.1#use-a-parameter-transformer-to-customize-token-replacement"/>.
    /// </summary>
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        /// <inheritdoc/>
        public string TransformOutbound(object value)
        {
            if (value == null)
            {
                return null;
            }

            return Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1-$2").ToLower();
        }
    }
}