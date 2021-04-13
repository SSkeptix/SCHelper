using System;
using System.Collections.Generic;
using System.Linq;

namespace SCHelper.Services.Impl
{
    public class MathService : IMathService
    {
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

        public IEnumerable<T[]> BuildOrderedClusters<T>(T[] data, Func<T, T, CompareResult> compare)
        {
            var compares = new int[data.Length, data.Length];
            for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
            {
                for (int columnIndex = 0; columnIndex < data.Length; columnIndex++)
                {
                    if (rowIndex == columnIndex)
                        compares[rowIndex, columnIndex] = 0;
                    else if (rowIndex > columnIndex)
                        compares[rowIndex, columnIndex] = -compares[columnIndex, rowIndex];
                    else
                    {
                        var compareResult = compare(data[rowIndex], data[columnIndex]);
                        compares[rowIndex, columnIndex] = compareResult switch
                        {
                            CompareResult.NotComparable => 0,
                            CompareResult.GreaterOrEqual => 1,
                            CompareResult.Less => -1,
                        };
                    }
                }
            }

            var countOfChildNodes = new (int NodeIndex, int ChildCount)[data.Length];
            for (int rowIndex = 0; rowIndex < data.Length; rowIndex++)
            {
                var node = (NodeIndex: rowIndex, ChildCount: 0);
                for (int columnIndex = 0; columnIndex < data.Length; columnIndex++)
                    if (compares[rowIndex, columnIndex] == 1)
                        node.ChildCount++;
                countOfChildNodes[rowIndex] = node;
            }
            var orderedNodeIndexes = countOfChildNodes.OrderByDescending(x => x.ChildCount).Select(x => x.NodeIndex).ToArray();

            var nodes = data.Select(x => new OrientedGraphNode<T>(x)).ToArray();
            foreach (var rowIndex in orderedNodeIndexes)
            {
                var childNodeIndexes = new List<int>(data.Length);
                foreach (var columnIndex in orderedNodeIndexes)
                    if (compares[rowIndex, columnIndex] == 1)
                        childNodeIndexes.Add(columnIndex);

                for (int i = childNodeIndexes.Count - 2; i >= 0; i--)
                    for (int j = childNodeIndexes.Count - 1; j > i; j--)
                        if (compares[childNodeIndexes[i], childNodeIndexes[j]] == 1)
                            childNodeIndexes.RemoveAt(j);

                nodes[rowIndex].Children.AddRange(childNodeIndexes.Select(x => nodes[x]));
                foreach (var childNodeIndex in childNodeIndexes)
                    nodes[childNodeIndex].Parents.Add(nodes[rowIndex]);
            }

            var currentResult = new T[data.Length];
            var queues = new Queue<OrientedGraphNode<T>>[data.Length];
            int currentIndex = 0;
            queues[0] = new Queue<OrientedGraphNode<T>>(nodes.Where(x => !x.Parents.Any()));
            while(currentIndex >= 0)
            {
                if (queues[currentIndex].Count > 0)
                {
                    var node = queues[currentIndex].Peek();
                    currentResult[currentIndex] = node.Item;

                    if (node.Children.Any())
                    {
                        currentIndex++;
                        queues[currentIndex] = new Queue<OrientedGraphNode<T>>(node.Children);
                    }
                    else
                    {
                        yield return currentResult.Take(currentIndex + 1).ToArray();
                        queues[currentIndex].Dequeue();
                    }
                }
                else
                {
                    currentIndex--;
                    if (currentIndex >= 0)
                        queues[currentIndex].Dequeue();
                }
            }
        }
    }

    public class OrientedGraphNode<T>
    {
        public T Item { get; }

        public List<OrientedGraphNode<T>> Parents { get; } = new List<OrientedGraphNode<T>>();

        public List<OrientedGraphNode<T>> Children { get; } = new List<OrientedGraphNode<T>>();

        public OrientedGraphNode(T item)
        {
            this.Item = item;
        }
    }
}
