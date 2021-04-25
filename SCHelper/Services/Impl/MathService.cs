using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class MathService : IMathService
    {
        private readonly ILogger<MathService> logger;

        public MathService(ILogger<MathService> logger)
        {
            this.logger = logger;
        }

        public IEnumerable<T[]> GetAllCombinations<T>(T[] data, int count)
        {
            if (count >= data.Length)
                yield return data;
            else
            {
                var items = Enumerable.Range(0, count).ToArray();
                yield return items.Select(x => data[x]).ToArray();

                int index = count - 1;
                while (items[0] < data.Length - count)
                {
                    if (items[index] < data.Length - count + index)
                    {
                        items[index]++;
                        while (index + 1 < count)
                        {
                            index++;
                            items[index] = items[index - 1] + 1;
                        }
                        yield return items.Select(x => data[x]).ToArray();
                    }
                    else
                    {
                        while (index < 0 || items[index] == data.Length - count + index)
                        {
                            index--;
                        }
                    }
                }
            }
        }

        public long GetAllCombinationsCount(int itemsCount, int count)
        {
            if (itemsCount <= count) return 1;

            long result = 1;
            for (int i = 0; i < count; i++)
                result *= itemsCount - i;

            for (int i = 0; i < count; i++)
                result /= count - i;

            return result;
        }

        public IEnumerable<T[]> GetAllPermutations<T>(T[] data)
        {
            void SwapItems(int[] sequence, int index_0, int index_1)
            {
                var item = sequence[index_0];
                sequence[index_0] = sequence[index_1];
                sequence[index_1] = item;
            }

            bool NextPermutation(int[] sequence)
            {
                var i = sequence.Length;
                do
                {
                    if (i < 2) { return false; }
                    --i;
                } while (sequence[i - 1] > sequence[i]);

                var j = sequence.Length;
                while (i < j && sequence[i - 1] > sequence[--j]) ;
                SwapItems(sequence, i - 1, j);

                j = sequence.Length;
                while (i < --j) { SwapItems(sequence, i++, j); }
                return true;
            }

            T[] ToData(int[] indexes) => indexes.Select(x => data[x]).ToArray();

            var s = Enumerable.Range(0, data.Length).ToArray();
            yield return ToData(s);

            while (NextPermutation(s))
                yield return ToData(s);
        }


        public IEnumerable<T[]> GetAllOptimizedCombinations_Test<T>(T[] data, int count, Func<T, T, CompareResult> compare)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var graph = BuildOrientedGraph(data, compare);
            var clusteredData = BuildClusteredData(graph, count);

            stopWatch.Stop();
            this.logger.LogInformation($"Cluster time Calculation: {stopWatch.Elapsed}");

            return GetAllOptimizedCombinations(clusteredData);
        }

        public OrientedGraphNode<T>[] BuildOrientedGraph<T>(T[] data, Func<T, T, CompareResult> compare)
        {
            // Build compare matrix.
            var compareMatrix = new int[data.Length, data.Length];
            for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < data.Length; columnIndex++)
                {
                    if (rowIndex == columnIndex)
                        compareMatrix[rowIndex, columnIndex] = 0;
                    else if (rowIndex > columnIndex)
                        compareMatrix[rowIndex, columnIndex] = -compareMatrix[columnIndex, rowIndex];
                    else
                    {
                        var compareResult = compare(data[rowIndex], data[columnIndex]);
                        compareMatrix[rowIndex, columnIndex] = compareResult switch
                        {
                            CompareResult.NotComparable => 0,
                            CompareResult.GreaterOrEqual => 1,
                            CompareResult.Less => -1,
                            _ => throw new NotImplementedException(),
                        };
                    }
                }
            }

            // Sort items in order to have less children.
            var countOfChildNodes = new (int NodeIndex, int ChildCount)[data.Length];
            for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
            {
                var node = (NodeIndex: rowIndex, ChildCount: 0);
                for (int columnIndex = 0; columnIndex < data.Length; columnIndex++)
                    if (compareMatrix[rowIndex, columnIndex] == 1)
                        node.ChildCount++;
                countOfChildNodes[rowIndex] = node;
            }          
            var orderedNodeIndexes = countOfChildNodes
                .OrderByDescending(x => x.ChildCount)
                .Select(x => x.NodeIndex)
                .ToArray();

            // Iterate nodes. From most bigger parrent to leafs.
            var nodes = data.Select(x => new OrientedGraphNode<T>(x)).ToArray();
            foreach (var rowIndex in orderedNodeIndexes)
            {
                var childNodeIndexes = new List<int>(data.Length);
                foreach (var columnIndex in orderedNodeIndexes)
                    if (compareMatrix[rowIndex, columnIndex] == 1)
                        childNodeIndexes.Add(columnIndex);

                // Remove a child from the node, if it has another child that already has this node.
                for (int i = childNodeIndexes.Count - 2; i >= 0; i--)
                    for (int j = childNodeIndexes.Count - 1; j > i; j--)
                        if (compareMatrix[childNodeIndexes[i], childNodeIndexes[j]] == 1)
                            childNodeIndexes.RemoveAt(j);

                nodes[rowIndex].Children.AddRange(childNodeIndexes.Select(x => nodes[x]));
                foreach (var childNodeIndex in childNodeIndexes)
                    nodes[childNodeIndex].Parents.Add(nodes[rowIndex]);
            }

            // Return root (parent) nodes.
            return nodes
                .Where(x => !x.Parents.Any())
                .ToArray();
        }

        public List<T[]>[][] BuildClusteredData<T>(OrientedGraphNode<T>[] rootNodes, int maxCountOfItems)
        {
            var currentResult = new T[maxCountOfItems];
            var queues = new Queue<OrientedGraphNode<T>>[maxCountOfItems];
            queues[0] = new Queue<OrientedGraphNode<T>>(rootNodes);

            var result = new List<T[]>[rootNodes.Length][];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new List<T[]>[maxCountOfItems];
                for (int j = 0; j < maxCountOfItems; j++)
                    result[i][j] = new List<T[]>();
            }

            var rootNodeIndex = 0;
            int nodeDepthIndex = 0;
            while (nodeDepthIndex >= 0)
            {
                if (queues[nodeDepthIndex].Count > 0)
                {
                    var node = queues[nodeDepthIndex].Peek();
                    currentResult[nodeDepthIndex] = node.Item;

                    result[rootNodeIndex][nodeDepthIndex].Add(currentResult.Take(nodeDepthIndex + 1).ToArray());

                    if (node.Children.Any() && nodeDepthIndex < maxCountOfItems - 1)
                    {
                        nodeDepthIndex++;
                        queues[nodeDepthIndex] = new Queue<OrientedGraphNode<T>>(node.Children);
                    }
                    else
                    {
                        queues[nodeDepthIndex].Dequeue();
                        if (nodeDepthIndex == 0)
                            rootNodeIndex++;
                    }
                }
                else
                {
                    nodeDepthIndex--;
                    if (nodeDepthIndex >= 0)
                        queues[nodeDepthIndex].Dequeue();
                    if (nodeDepthIndex == 0)
                        rootNodeIndex++;
                }
            }

            return result;
        }

        public IEnumerable<T[]> GetAllOptimizedCombinations<T>(List<T[]>[][] clusteredData)
        {
            var maxCountOfItems = clusteredData[0].Length;
            var clusterMaxLength = clusteredData
                .Select(x =>
                {
                    for (int i = 1; i < maxCountOfItems; i++)
                        if (x[i].Count == 0)
                            return i;
                    return maxCountOfItems;
                })
                .ToArray();

            var currentResult = Enumerable.Repeat(-1, maxCountOfItems).ToArray();
            
            var index = 0;
            while (true)
            {
                begin:
                var currentCluster = ++currentResult[index];
                if (currentResult[index] >= clusteredData.Length)
                {
                    index--;
                    if (index < 0) { break; }
                    continue;
                }

                var currentClusterUsage = 1;
                while (++index < maxCountOfItems)
                {
                    if (currentClusterUsage >= clusterMaxLength[currentCluster])
                    {
                        currentCluster++;
                        if (currentCluster >= clusteredData.Length)
                            goto begin;

                        currentClusterUsage = 1;
                    }
                    else
                        currentClusterUsage++;
                    currentResult[index] = currentCluster;
                }
                index--;

                // ====== output result

                var dict = new Dictionary<int, int>(maxCountOfItems);
                for (int i = 0; i < maxCountOfItems; i++)
                    dict[currentResult[i]] = dict.ContainsKey(currentResult[i])
                        ? dict[currentResult[i]] + 1
                        : 1;

                var sources = dict.Select(x => clusteredData[x.Key][x.Value - 1]).ToArray();
                var sourceIndex = 0;
                var sourceIndexes = Enumerable.Repeat(-1, sources.Length).ToArray();

                while (true)
                {
                    sourceIndexes[sourceIndex]++;

                    if (sourceIndexes[sourceIndex] >= sources[sourceIndex].Count)
                    {
                        sourceIndex--;
                        if (sourceIndex < 0) { break; }
                        continue;
                    }

                    while (++sourceIndex < sources.Length)
                        sourceIndexes[sourceIndex] = 0;
                    sourceIndex--;

                    yield return sources.SelectMany((source, i) => sources[i][sourceIndexes[i]]).ToArray();                  
                }
            }
        }
    }

    public class OrientedGraphNode<T>
    {
        public T Item { get; }

        public List<OrientedGraphNode<T>> Parents { get; set; } = new List<OrientedGraphNode<T>>();

        public List<OrientedGraphNode<T>> Children { get; set; } = new List<OrientedGraphNode<T>>();

        public OrientedGraphNode(T item)
        {
            this.Item = item;
        }
    }
}
