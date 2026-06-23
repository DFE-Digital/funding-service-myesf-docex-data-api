using Moq.Language.Flow;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests
{
    /// <summary>
    /// a moq helper.
    /// </summary>
    public static class MoqHelper
    {
        /// <summary>
        /// moq returns in order extension.
        /// </summary>
        /// <typeparam name="T">the incoming type.</typeparam>
        /// <typeparam name="TResult">the result type.</typeparam>
        /// <param name="setup">the moq setup class.</param>
        /// <param name="results">the set of results being returned in order.</param>
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup, params TResult[] results)
            where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
        }
    }
}
