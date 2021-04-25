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

        [TestCase(1L, 5, 3)]
        [TestCase(1L, 5, 5)]
        [TestCase(6L, 5, 6)]
        [TestCase(5_461_512L, 5, 60)]
        public void GetAllCombinationCount(long expectedResult, int count, int itemsCount)
        {
            // Act
            var result = this.subject.GetAllCombinationsCount(itemsCount, count);

            // Assert
            Assert.AreEqual(expectedResult, result);
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
        public void BuildOrientedGraph()
        {
            var testCases = BuildOrientedGraph_TestCases();

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
                var result = this.subject.BuildOrientedGraph(data, comparer).ToArray();
            }
        }

        [Test]
        // [TestCaseSource(nameof(BuildOrderedClusters_TestCases))] It doesn't work
        public void BuildClusteredData()
        {
            var testCases = BuildOrientedGraph_TestCases();

            var comparer = new Func<(int, int), (int, int), CompareResult>(((int x, int y) a, (int x, int y) b) =>
            {
                if (a.x >= b.x && a.y >= b.y) return CompareResult.GreaterOrEqual;
                if (a.x <= b.x && a.y <= b.y) return CompareResult.Less;
                return CompareResult.NotComparable;
            });

            foreach (var data in testCases)
            {
                var nodes = this.subject.BuildOrientedGraph(data, comparer).ToArray();

                // Act
                var result = this.subject.BuildClusteredData(nodes, 5);
            }
        }

        [Test]
        // [TestCaseSource(nameof(BuildOrderedClusters_TestCases))] It doesn't work
        public void GetAllOptimizedCombinations_2()
        {
            // Arrange
            //var data = new (int x, int y)[] { (6, 4), (5, 5), (5, 5), (4, 6), (4, 6), (4, 6), (3, 8), (3, 8), (3, 8), (3, 7), (2, 8) };
            var data = BuildOrientedGraph_TestCases().First();

            var comparer = new Func<(int, int), (int, int), CompareResult>(((int x, int y) a, (int x, int y) b) =>
            {
                if (a.x >= b.x && a.y >= b.y) return CompareResult.GreaterOrEqual;
                if (a.x <= b.x && a.y <= b.y) return CompareResult.Less;
                return CompareResult.NotComparable;
            });

            var nodes = this.subject.BuildOrientedGraph(data, comparer).ToArray();
            var clusteredData = this.subject.BuildClusteredData(nodes, 5);
            var result = this.subject.GetAllOptimizedCombinations(clusteredData).ToArray();
        }


        [Test]
        // [TestCaseSource(nameof(BuildOrderedClusters_TestCases))] It doesn't work
        public void GetAllOptimizedCombinations_3()
        {
            var func = (Func<int, List<int[]>[]>)(i => new List<int[]>[]
                {
                    new List<int[]> { new int[] { i } },
                    new List<int[]>(),
                    new List<int[]>(),
                    new List<int[]>(),
                    new List<int[]>(),
                });

            var clusteredData = new List<int[]>[][]
            {
                func(1),
                func(2),
                func(3),
                func(4),
                func(5),
                func(6),
                func(7),
            };

            var result = this.subject.GetAllOptimizedCombinations(clusteredData).ToArray();
        }


        static (int, int)[][] BuildOrientedGraph_TestCases()
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