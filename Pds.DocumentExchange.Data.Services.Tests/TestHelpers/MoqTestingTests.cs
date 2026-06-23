using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Pds.DocumentExchange.Data.Services.Tests
{
    public abstract class MoqTestingTests<TTestSystem, TSystemSupports>
    {
        /// <summary>
        /// make strict mock.
        /// </summary>
        /// <typeparam name="TEntity">for this type.</typeparam>
        /// <returns>a strict behaviour mock.</returns>
        public TEntity MakeStrictMock<TEntity>()
            where TEntity : class =>
            new Mock<TEntity>(MockBehavior.Strict).Object;

        /// <summary>
        /// make loose mock.
        /// </summary>
        /// <typeparam name="TEntity">for this type.</typeparam>
        /// <returns>a loose behaviour mock.</returns>
        public TEntity MakeLooseMock<TEntity>()
            where TEntity : class =>
            new Mock<TEntity>(MockBehavior.Loose).Object;

        /// <summary>
        /// get mock.
        /// </summary>
        /// <typeparam name="TEntity">the type.</typeparam>
        /// <param name="forItem">for this instance of <typeparamref name="TEntity"/>the type.</param>
        /// <returns>the mock.</returns>
        public Mock<TEntity> GetMock<TEntity>(TEntity forItem)
            where TEntity : class =>
            Mock.Get(forItem);

        /// <summary>
        /// SUT supports registration contract.
        /// </summary>
        [TestMethod]
        public void TestSystemSupportsRegistrationContract() =>
            TestSystemSupportsGivenContract<TSystemSupports>();

        /// <summary>
        /// the system under test must support the given contract.
        /// </summary>
        /// <typeparam name="TContract">the given contract type.</typeparam>
        public void TestSystemSupportsGivenContract<TContract>() =>
            BuildTestSystem().Should().BeAssignableTo<TContract>();

        /// <summary>
        /// very all mocks.
        /// </summary>
        /// <param name="sut">the system under test.</param>
        internal abstract void VerifyAllMocks(TTestSystem sut);

        /// <summary>
        /// make the 'system under test'.
        /// </summary>
        /// <returns>the system under test.</returns>
        internal abstract TTestSystem BuildTestSystem();

        /// <summary>
        /// make readonly items,  helper function to create list of things.
        /// </summary>
        /// <typeparam name="TItem">the type of item.</typeparam>
        /// <param name="count">the number of items required.</param>
        /// <param name="candidate">a candidate item for the list.</param>
        /// <returns>a collection of items for the test.</returns>
        internal IReadOnlyCollection<TItem> MakeReadonlyItems<TItem>(int count, TItem candidate = null)
            where TItem : class
        {
            var items = new TItem[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = candidate;
            }

            return items;
        }

        /// <summary>
        /// make readonly items,  helper function to create list of things.
        /// </summary>
        /// <typeparam name="TItem">the type of item.</typeparam>
        /// <param name="count">the number of items required.</param>
        /// <param name="candidate">a candidate item for the list.</param>
        /// <returns>a collection of items for the test.</returns>
        internal IReadOnlyCollection<TItem> MakeReadonlyItems<TItem>(int count, TItem? candidate = null)
            where TItem : struct
        {
            var items = new TItem[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = candidate.GetValueOrDefault();
            }

            return items;
        }

        /// <summary>
        /// make enumerable items,  helper function to create list of things.
        /// </summary>
        /// <typeparam name="TItem">the type of item.</typeparam>
        /// <param name="count">the number of items rquired.</param>
        /// <param name="candidate">a candidate item for the list.</param>
        /// <returns>a collection of items for the test.</returns>
        internal IReadOnlyCollection<TItem> MakeEnumerableItems<TItem>(int count, TItem candidate = null)
            where TItem : class
        {
            var items = new TItem[count];
            for (var i = 0; i < count; i++)
            {
                items[i] = candidate;
            }

            return items;
        }
    }
}