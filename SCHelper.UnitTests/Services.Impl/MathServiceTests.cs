using NUnit.Framework;
using SCHelper.Services;
using SCHelper.Services.Impl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.UnitTests.Services.Impl
{
    public class MathServiceTests
    {
        private MathService subject;

        [SetUp]
        public void Setup()
        {
            this.subject = new MathService();
        }

        [Test]
        public void GetAllPermutations()
        {
            // Arrange
            var sequence = new int[] { 1, 2, 3 };

            // Act
            var result = this.subject.GetAllPermutations(sequence).ToArray();

            // Assert
            Assert.AreEqual(6, result.Length);
            CollectionAssert.AreEquivalent(new int[] { 1, 2, 3 }, result[0]);
            CollectionAssert.AreEquivalent(new int[] { 1, 3, 2 }, result[1]);
            CollectionAssert.AreEquivalent(new int[] { 2, 1, 3 }, result[2]);
            CollectionAssert.AreEquivalent(new int[] { 2, 3, 1 }, result[3]);
            CollectionAssert.AreEquivalent(new int[] { 3, 1, 2 }, result[4]);
            CollectionAssert.AreEquivalent(new int[] { 3, 2, 1 }, result[5]);
        }

        [Test]
        // [TestCaseSource(nameof(BuildOrderedClusters_TestCases))] It doesn't work
        public void BuildOrderedClusters()
        {
            var testCases = BuildOrderedClusters_TestCases();

            // Arrange
            var expectedResults = new (int x, int y)[][]
            {
                    new (int x, int y)[] { (7, 5), (5, 5), (4, 2) },
                    new (int x, int y)[] { (7, 5), (5, 5), (3, 3), (2, 3) },
                    new (int x, int y)[] { (7, 5), (5, 5), (2, 4), (2, 3) },
                    new (int x, int y)[] { (5, 9), (3, 8), (3, 8), (2, 4), (2, 3) },
                    new (int x, int y)[] { (5, 9), (3, 8), (3, 8), (3, 3), (2, 3) },
                    new (int x, int y)[] { (5, 9), (5, 7), (5, 5), (4, 2) },
                    new (int x, int y)[] { (5, 9), (5, 7), (5, 5), (3, 3), (2, 3) },
                    new (int x, int y)[] { (5, 9), (5, 7), (5, 5), (2, 4), (2, 3) },
            };

            var comparer = new Func<(int, int), (int, int), CompareResult>(((int x, int y) a, (int x, int y) b) =>
            {
                if (a.x >= b.x && a.y >= b.y) return CompareResult.GreaterOrEqual;
                if (a.x <= b.x && a.y <= b.y) return CompareResult.Less;
                return CompareResult.NotComparable;
            });

            foreach (var data in testCases)
            {
                // Act
                var result = this.subject.BuildOrderedClusters(data, comparer).ToArray();

                // Assert
                Assert.AreEqual(expectedResults.Length, result.Length);
                Assert.IsTrue(expectedResults.All(x => result.Any(x => x.SequenceEqual(x))));
            }
        }

        static (int, int)[][] BuildOrderedClusters_TestCases()
            => new (int x, int y)[][]
            {
                new(int x, int y)[] { (5, 7), (3, 8), (2, 3), (3, 3), (2, 4), (4, 2), (7, 5), (5, 5), (3, 8), (5, 9), },
                new(int x, int y)[] { (5, 9), (7, 5), (5, 7), (3, 8), (5, 5), (3, 8), (4, 2), (3, 3), (2, 4), (2, 3), },
                new(int x, int y)[] { (2, 3), (4, 2), (3, 3), (2, 4), (5, 5), (3, 8), (7, 5), (5, 7), (3, 8), (5, 9), },
                new(int x, int y)[] { (5, 5), (3, 8), (4, 2), (3, 3), (2, 4), (7, 5), (5, 7), (3, 8), (2, 3), (5, 9), },
                new(int x, int y)[] { (2, 3), (5, 9), (4, 2), (3, 3), (2, 4), (7, 5), (5, 7), (3, 8), (5, 5), (3, 8), },
                new(int x, int y)[] { (2, 3), (5, 9), (7, 5), (5, 7), (3, 8), (4, 2), (3, 3), (2, 4), (5, 5), (3, 8), },
                new(int x, int y)[] { (3, 8), (3, 8), (2, 3), (5, 9), (7, 5), (5, 7), (4, 2), (3, 3), (2, 4), (5, 5), },
                new(int x, int y)[] { (2, 3), (5, 9), (7, 5), (5, 7), (4, 2), (3, 3), (2, 4), (5, 5), (3, 8), (3, 8), },
            };
    }
}