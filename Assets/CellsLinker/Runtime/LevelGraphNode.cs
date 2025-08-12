using System;
using System.Collections.Generic;
using UnityEngine;

namespace CellsLinker.Runtime {
    /// <summary>
    /// Represents a node in the level graph, corresponding to a logical room in the level layout.
    /// </summary>
    public class LevelGraphNode {
        /// <summary>
        /// Unique identifier for internal use (e.g., lookup or debug).
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The room template collection this node draws from.
        /// </summary>
        public RoomTemplateCollection TemplateCollection { get; }

        /// <summary>
        /// Parent node in the graph (null for root).
        /// </summary>
        public LevelGraphNode Parent { get; private set; }

        /// <summary>
        /// Child nodes in the graph (e.g. connected rooms).
        /// </summary>
        public List<LevelGraphNode> Children { get; } = new();

        public LevelGraphNode(RoomTemplateCollection templateCollection, int id, LevelGraphNode parent) {
            TemplateCollection = templateCollection ?? throw new ArgumentNullException(nameof(templateCollection));
            Id = id;
            Parent = parent;
            Parent.Children.Add(this);
        }

        public override string ToString() =>
            $"Node({TemplateCollection.CollectionName}-{Id}, Children: {Children.Count})";
    }
}
