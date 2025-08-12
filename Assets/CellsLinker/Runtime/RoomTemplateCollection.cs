using System.Collections.Generic;
using UnityEngine;

namespace CellsLinker.Runtime
{
    /// <summary>
    /// A ScriptableObject that holds a collection of room templates.
    /// </summary>
    [CreateAssetMenu(fileName = "RoomTemplateCollection", menuName = "CellsLinker/Room Template Collection")]
    public class RoomTemplateCollection : ScriptableObject
    {
        [Tooltip("Name of this template collection, used for logging or debugging.")]
        public string CollectionName;

        [Tooltip("List of room template prefabs available for this room type.")]
        public List<RoomTemplateBase> Templates = new();
    }
}
