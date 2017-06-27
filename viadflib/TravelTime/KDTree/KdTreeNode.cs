
namespace viadflib.TravelTime.KDTree
{
    /// <summary>
    /// Represents a node in kd-tree"/>
    /// </summary>
    /// <typeparam name="T">The type of the value of the node.</typeparam>
    public class KdTreeNode<T>
    {
        // Value of node.
        private T value;

        // Pair of child nodes.
        private KdTreeNode<T> leftChild;
        private KdTreeNode<T> rightChild;

        /// <summary>
        /// Initializes a new instance of the <see cref="KdTreeNode{T}"/> class.
        /// </summary>
        /// <param name="value">The value of the node.</param>
        public KdTreeNode(T value)
        {
            this.value = value;
        }

        /// <summary>
        /// Gets the value of the node.
        /// </summary>
        /// <value>The value of the node.</value>
        public T Value
        {
            get { return this.value; }
            internal set { this.value = value; }
        }

        /// <summary>
        /// Gets the left child node of the current node.
        /// </summary>
        /// <value>The left child node.</value>
        public KdTreeNode<T> LeftChild
        {
            get { return this.leftChild; }
            internal set { this.leftChild = value; }
        }

        /// <summary>
        /// Gets the right child node of the current node.
        /// </summary>
        /// <value>The right child node.</value>
        public KdTreeNode<T> RightChild
        {
            get { return this.rightChild; }
            internal set { this.rightChild = value; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            const string nullNodeString = "-";

            return string.Format("{0} -> {1} ; {2}", this.value,
                this.leftChild == null ? nullNodeString : this.leftChild.value.ToString(),
                this.rightChild == null ? nullNodeString : this.rightChild.value.ToString());
        }
    }
}
