using System;
using System.Collections.Generic;
using System.Linq;
namespace viadflib.TravelTime.KDTree
{
    /// <summary>
    /// KD tree structure for doing proximity geo searches
    /// Once the index is created it can't be changed
    /// </summary>
    public class KdTree<TValue>
    {
        private readonly int _nrOfDimensions;
        private readonly KdTreeNode<TValue> _root;

        private readonly Func<TValue, double>[] _valueSelectors;
        private readonly ValueLocationComparer _valueComparer;

        public KdTree(IEnumerable<TValue> elements, params Func<TValue, double>[] valueSelectors)
        {
            _nrOfDimensions = valueSelectors.Length;
            _valueSelectors = valueSelectors;
            _valueComparer = new ValueLocationComparer(_valueSelectors);
            
            var elementsArray = elements.ToArray();

            _root = Build(elementsArray, 0, elementsArray.Length - 1, 0);
        }

        private KdTreeNode<TValue> Build(TValue[] elementsArray, int startIndex, int endIndex, int depth)
        {
            var length = endIndex - startIndex + 1;

            if (length == 0) return null;

            // Sort array of elements by component of chosen dimension, in ascending magnitude.
            _valueComparer.Dimension = depth % _nrOfDimensions;

            Array.Sort(elementsArray, startIndex, length, _valueComparer);

            // Select median element as pivot.
            var medianIndex = startIndex + length / 2;
            var medianElement = elementsArray[medianIndex];

            // Create node and construct sub-trees around pivot element.
            var node = new KdTreeNode<TValue>(medianElement);
            node.LeftChild = Build(elementsArray, startIndex, medianIndex - 1, depth + 1);
            node.RightChild = Build(elementsArray, medianIndex + 1, endIndex, depth + 1);

            return node;
        }
     
        /// <summary>
        /// Finds nodes in the tree that lie within the specified range of a location.
        /// </summary>
        /// <param name="location">The location for which to find the nearest node.</param>
        /// <param name="range">The range in which to search for nodes.</param>
        /// <returns>A collection of values with distance from <paramref name="location"/> less than
        /// <paramref name="range"/>.</returns>
        public IEnumerable<TValue> FindInRange(Vector location, double range)
        {
            var nodesList = new List<TValue>();
            FindInRange(_root, location, range, nodesList, 0);
            return nodesList.AsReadOnly();
        }

        private Vector CreateVector(TValue value)
        {
            double[] values = new double[_valueSelectors.Length];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = _valueSelectors[i](value);
            }
            return new Vector(values);
        }

        private void FindInRange(KdTreeNode<TValue> node, Vector location, double range, IList<TValue> valuesList, int depth)
        {
            if (node == null) return;

            var dimension = depth % _nrOfDimensions;

            var nodeLocation = CreateVector(node.Value);

            var distance = (nodeLocation - location).Abs();

            // add to list if its in range
            if (distance < range)
            {
                valuesList.Add(node.Value);
            }

            var nearChildNode = location[dimension] < nodeLocation[dimension] ? node.LeftChild : node.RightChild;

            if (nearChildNode != null)
            {
                FindInRange(nearChildNode, location, range, valuesList, depth + 1);
            }

            // also other half needs to be checked?
            if (range > Math.Abs(nodeLocation[dimension] - location[dimension]))
            {
                // Check for nodes in sub-tree of far child.
                var farChildNode = nearChildNode == node.LeftChild ? node.RightChild : node.LeftChild;

                if (farChildNode != null)
                {
                    FindInRange(farChildNode, location, range, valuesList, depth + 1);
                }
            }
        }
       
        private class ValueLocationComparer : Comparer<TValue>
        {
            private readonly Func<TValue, double>[] _valueSelectors;

            public ValueLocationComparer(Func<TValue, double>[] valueSelectors)
            {
                _valueSelectors = valueSelectors;
            }

            public int Dimension
            {
                private get;
                set;
            }

            public override int Compare(TValue x, TValue y)
            {
                return _valueSelectors[Dimension](x).CompareTo(_valueSelectors[Dimension](y));
            }
        }
    }
}
