using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Husky.Internal.Shared.Tests.ProgressHandlerTests
{
    public class SequentialBlockingProgressHandlerTests
    {
        [Test]
        [Category("UnitTest")]
        public async ValueTask Handler_consumes_all_produced_events()
        {
            // Arrange
            var itemsToReport = new[] { "Hello", "World", "!" };
            var consumedItems = new List<string>(itemsToReport.Length);
            var sut = new SequentialBlockingProgressHandler<string>(consumedItems.Add);

            // Act
            foreach (var item in itemsToReport)
                sut.Report(item);

            await sut.DisposeAsync();

            // Assert
            CollectionAssert.AreEquivalent(itemsToReport, consumedItems);
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Handler_consumes_all_produced_events_in_the_produced_order()
        {
            // Arrange
            var itemsToReport = new[] { "Hello", "World", "!" };
            var consumedItems = new List<string>(itemsToReport.Length);
            var sut = new SequentialBlockingProgressHandler<string>(s =>
            {
                if (s == itemsToReport[1])
                    Thread.Sleep(50);

                consumedItems.Add(s);
            });

            // Act
            foreach (var item in itemsToReport)
                sut.Report(item);

            await sut.DisposeAsync();

            // Assert
            CollectionAssert.AreEqual(itemsToReport, consumedItems);
        }

        [Test]
        [Category("UnitTest")]
        public async ValueTask Handler_ensures_all_events_are_consumed_when_disposing()
        {
            // Arrange
            var itemsToReport = new[] { "Solo" };
            var consumedItems = new List<string>(itemsToReport.Length);
            var sut = new SequentialBlockingProgressHandler<string>(s =>
            {
                Thread.Sleep(50);
                consumedItems.Add(s);
            });

            // Act
            foreach (var item in itemsToReport)
                sut.Report(item);

            // Assert
            CollectionAssert.IsEmpty(consumedItems);
            await sut.DisposeAsync();
            CollectionAssert.IsNotEmpty(consumedItems);
        }
    }
}