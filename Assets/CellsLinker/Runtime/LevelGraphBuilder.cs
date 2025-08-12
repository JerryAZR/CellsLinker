using System.Collections.Generic;

namespace CellsLinker.Runtime {

    /// <summary>
    /// A builder class for constructing room-based level graphs.
    /// Nodes are added sequentially and can be labeled or branched into.
    /// Supports scoping and jumping to labeled positions.
    /// </summary>
    /// Must be usable as a dictionary key.</typeparam>
    public class LevelGraphBuilder {

        public LevelGraph Graph { get; private set; }

        private LevelGraphNode _head;
        private Dictionary<string, LevelGraphNode> _labeledNodes = new();
        private Stack<LevelGraphNode> _scopeStack = new();

        /// <summary>
        /// Initializes an empty builder. You must call <see cref="Add"/> before other operations.
        /// </summary>
        public LevelGraphBuilder()  {
            _head = null;
            Graph = null;
        }

        /// <summary>
        /// Initialize a sub-graph from the existing head and graph
        /// </summary>
        /// <param name="head"></param>
        /// <param name="graph"></param>
        public LevelGraphBuilder(LevelGraphNode head, LevelGraph graph) {
            _head = head;
            Graph = graph;
        }

        /// <summary>
        /// Adds a room node of the specified type to the current path of the level graph.
        /// </summary>
        /// <param name="roomType">The type or category of the room node to add.</param>
        /// <returns>The current builder instance for fluent chaining.</returns>
        public LevelGraphBuilder Add(RoomTemplateCollection roomType) {
            if (Graph != null) {
                // Create a new node, add it as a child to head, then make it the new head
                _head = new(roomType, Graph.NewId(), _head);
            } else {
                // Create graph and add the first node as root
                _head = new(roomType, 0, null);
                Graph = new(_head);
            }
            return this;
        }

        /// <summary>
        /// Marks the current node with a string label for future jump references.
        /// This is similar to defining a jump target.
        /// </summary>
        /// <param name="label">A unique string label identifying the current position.</param>
        /// <returns>The current builder instance for fluent chaining.</returns>
        public LevelGraphBuilder Label(string label) {
            _labeledNodes[label] = _head;
            return this;
        }


        /// <summary>
        /// Creates a link from the current node to the node marked with the specified label.
        /// Must only be used with valid labels created via <see cref="Label"/>.
        /// </summary>
        /// <param name="label">The label of the target node to jump to.</param>
        /// <returns>The current builder instance for fluent chaining.</returns>
        public LevelGraphBuilder JumpTo(string label) {
            _head = _labeledNodes[label];
            return this;
        }

        /// <summary>
        /// Begins a new scope by pushing the current node onto an internal stack.
        /// Use <see cref="ExitScope"/> to return to this node later.
        /// </summary>
        /// <returns>The current builder instance for fluent chaining.</returns>
        public LevelGraphBuilder EnterScope() {
            _scopeStack.Push(_head);
            return this;
        }

        /// <summary>
        /// Ends the current scope by returning to the most recently entered scope node.
        /// Matches a prior call to <see cref="EnterScope"/>.
        /// </summary>
        /// <returns>The current builder instance for fluent chaining.</returns>
        public LevelGraphBuilder ExitScope() {
            _head = _scopeStack.Pop();
            return this;
        }

        /// <summary>
        /// Starts a new branch builder from the node labeled by the specified string.
        /// Used to build optional or alternative paths starting from a shared label.
        /// </summary>
        /// <param name="label">The label to branch from.</param>
        /// <returns>A new builder instance rooted at the given label.</returns>
        public LevelGraphBuilder GetBranchBuilder(string label) {
            return new LevelGraphBuilder(_labeledNodes[label], Graph);
        }

        /// <summary>
        /// Starts a new branch builder from the current node.
        /// Useful for diverging paths without needing to label the fork point.
        /// </summary>
        /// <returns>A new builder instance rooted at the current node.</returns>
        public LevelGraphBuilder GetBranchBuilder() {
            return new LevelGraphBuilder(_head, Graph);
        }
    }
}
