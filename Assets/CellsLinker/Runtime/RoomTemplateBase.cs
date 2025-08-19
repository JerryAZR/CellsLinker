using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.VisualScripting;

namespace CellsLinker.Runtime
{

    [RequireComponent(typeof(Grid))]
    public class RoomTemplateBase : MonoBehaviour
    {

        [Tooltip("List of doors in this room prefab. Each door should point outward to where the connecting room would be.")]
        public List<RoomDoor> Doors = new();
        [Tooltip("If this room can be mirrored/flipped horizontally")]
        public bool AllowMirror;
        public RectInt RoomRect => _roomRect;
        [SerializeField, HideInInspector]
        private RectInt _roomRect;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private RectInt GetRoomRect()
        {
            BoundsInt combinedBounds = default;

            // Search in immediate children for RoomTileLayers tagged as walls
            foreach (Transform child in transform)
            {
                var tag = child.GetComponent<RoomTileLayerTag>();
                if (tag != null && tag.LayerIsSolid)
                {
                    // Tilemaps are guaranteed to exist with RoomTileLayerTag
                    var tilemap = child.GetComponent<Tilemap>();
                    // Compress bounds before using
                    tilemap.CompressBounds();
                    if (tilemap.cellBounds.size == Vector3Int.zero) continue;

                    BoundsInt layerBounds = tilemap.cellBounds;
                    combinedBounds = MergeBounds(combinedBounds, layerBounds);
                }
            }

            Vector2Int roomMax, roomMin;
            if (combinedBounds.size != Vector3Int.zero)
            {
                // Offset roomMax by one to "exclude" max row and column
                roomMax = (Vector2Int)combinedBounds.max - Vector2Int.one;
                roomMin = (Vector2Int)combinedBounds.min;
            }
            else
            {
                roomMax = new Vector2Int(int.MinValue, int.MinValue);
                roomMin = new Vector2Int(int.MaxValue, int.MaxValue);
            }

            foreach (RoomDoor door in Doors)
            {
                Vector2Int doorMax = door.LocalPosition + (door.Width - 1) * (
                    (door.Edge == DoorEdge.East || door.Edge == DoorEdge.West) ?
                    Vector2Int.up : Vector2Int.right);
                roomMin = Vector2Int.Min(roomMin, door.LocalPosition);
                roomMax = Vector2Int.Max(roomMax, doorMax);
            }

            RectInt rect = new();
            rect.SetMinMax(roomMin, roomMax);

            return rect;
        }

        private BoundsInt MergeBounds(BoundsInt bounds1, BoundsInt bounds2)
        {
            if (bounds1.size != Vector3Int.zero && bounds2.size != Vector3Int.zero)
            {
                // Regular non-zero case, most common
                Vector3Int maxPoint = Vector3Int.Max(bounds1.max, bounds2.max);
                Vector3Int minPoint = Vector3Int.Min(bounds1.min, bounds2.min);
                BoundsInt newBounds = new();
                newBounds.SetMinMax(minPoint, maxPoint);
                return newBounds;
            }
            else if (bounds1.size == Vector3Int.zero && bounds2.size == Vector3Int.zero)
            {
                // Both zero, return default
                return default;
            }
            else if (bounds1.size == Vector3Int.zero)
            {
                return bounds2;
            }
            else
            {
                return bounds1;
            }
        }

        public bool IsDoorValid(RoomDoor door)
        {
            return door.Edge switch
            {
                DoorEdge.North => door.LocalPosition.y == RoomRect.yMax,
                DoorEdge.East => door.LocalPosition.x == RoomRect.xMax,
                DoorEdge.South => door.LocalPosition.y == RoomRect.yMin,
                DoorEdge.West => door.LocalPosition.x == RoomRect.xMin,
                _ => throw new UnexpectedEnumValueException<DoorEdge>(door.Edge),
            };
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _roomRect = GetRoomRect();
        }
#endif

    }
    public enum DoorDirectionality
    {
        Bidirectional,  // Normal door, can go both ways
        EntranceOnly,   // One-way entrance (from this room to the next)
        ExitOnly        // One-way exit (from previous room into this one)
    }

    /// <summary>
    /// Note that (3 - edge) gives you the opposite edge
    /// </summary>
    public enum DoorEdge
    {
        North = 0, // +Y
        East = 1,  // +X
        West = 2,   // -X
        South = 3, // -Y
    }

    public static class DoorUtils
    {
        private static readonly Vector2Int[] _edgeDirections = new[]
        {
            Vector2Int.up,    // North
            Vector2Int.right, // East
            Vector2Int.left,  // West
            Vector2Int.down,  // South
        };

        public static Vector2Int AsVectorInt(this DoorEdge edge) => _edgeDirections[(int)edge];
        public static DoorEdge Opposite(this DoorEdge edge) => (DoorEdge)(3 - (int)edge);
    }

    /// <summary>
    /// Represents a doorway in a room template. Used during level generation to connect rooms.
    /// </summary>
    [System.Serializable]
    public class RoomDoor
    {
        /// <summary>
        /// Position of the door in local prefab space (tile coordinates).
        /// </summary>
        [Tooltip("Door anchor position in grid coordinates relative to the room origin.")]
        public Vector2Int LocalPosition;

        /// <summary>
        /// Direction the door faces, used to align and connect to other rooms.
        /// </summary>
        [Tooltip("Cardinal edge this door is on (points outward to the adjacent room).")]
        public DoorEdge Edge;

        [Tooltip("Specifies whether the door is bidirectional, entrance-only, or exit-only.")]
        public DoorDirectionality Directionality = DoorDirectionality.Bidirectional;

        /// <summary>
        /// Optional: Width of the door opening, in tiles.
        /// Only applies perpendicular to Direction. Default is 2.
        /// </summary>
        [Min(1)]
        public int Width = 2;
    }
}
