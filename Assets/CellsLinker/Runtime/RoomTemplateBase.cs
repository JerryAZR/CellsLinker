using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CellsLinker.Runtime {

    [RequireComponent(typeof(Grid))]
    public class RoomTemplateBase : MonoBehaviour {

        [Tooltip("List of doors in this room prefab. Each door should point outward to where the connecting room would be.")]
        public List<RoomDoor> Doors = new();

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private (BoundsInt bounds, HashSet<Vector2Int> walls) GetRoomBounds() {
            BoundsInt combinedBounds = default;
            HashSet<Vector2Int> wallCells = new();

            // Search in immediate children for RoomTileLayers tagged as walls
            foreach (Transform child in transform) {
                var tag = child.GetComponent<RoomTileLayerTag>();
                if (tag != null && tag.LayerIsWall) {
                    // Tilemaps are guaranteed to exist with RoomTileLayerTag
                    var tilemap = child.GetComponent<Tilemap>();
                    // Compress bounds before using
                    tilemap.CompressBounds();
                    if (tilemap.cellBounds.size == Vector3Int.zero) continue;

                    BoundsInt layerBounds = tilemap.cellBounds;
                    combinedBounds = MergeBounds(combinedBounds, layerBounds);

                    foreach (Vector3Int pos in layerBounds.allPositionsWithin) {
                        if (tilemap.HasTile(pos)) {
                            wallCells.Add((Vector2Int)pos);
                        }
                    }
                }
            }

            return (combinedBounds, wallCells);
        }

        private BoundsInt MergeBounds(BoundsInt bounds1, BoundsInt bounds2) {
            if (bounds1.size != Vector3Int.zero && bounds2.size != Vector3Int.zero) {
                // Regular non-zero case, most common
                Vector3Int maxPoint = Vector3Int.Max(bounds1.max, bounds2.max);
                Vector3Int minPoint = Vector3Int.Min(bounds1.min, bounds2.min);
                BoundsInt newBounds = new();
                newBounds.SetMinMax(minPoint, maxPoint);
                return newBounds;
            } else if (bounds1.size == Vector3Int.zero && bounds2.size == Vector3Int.zero) {
                // Both zero, return default
                return default;
            } else if (bounds1.size == Vector3Int.zero) {
                return bounds2;
            } else {
                return bounds1;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            foreach (var door in Doors) {
                DrawDoor(door);
            }
        }

        private void DrawDoor(RoomDoor door) {
            Grid grid = GetComponentInParent<Grid>();
            if (grid == null) {
                Debug.LogWarning("Grid not found for RoomTemplateBase");
                return;
            }

            // Draw each tile in the door area
            Vector3 growDirection = door.Edge switch {
                DoorEdge.East => Vector3.up,
                DoorEdge.West => Vector3.up,
                DoorEdge.North => Vector3.right,
                DoorEdge.South => Vector3.right,
                _ => throw new System.InvalidOperationException($"Unknown DoorEdge enum: {door.Edge}"),
            };
            Vector3 anchorWorldPos = grid.GetCellCenterWorld((Vector3Int)door.LocalPosition);
            Vector3 doorCenter = anchorWorldPos + growDirection * 0.5f * (door.Width - 1);
            Vector3 doorSize = door.Edge == DoorEdge.East || door.Edge == DoorEdge.West ?
                Vector3.Scale(grid.cellSize, new Vector3(1, door.Width, 0)) :
                Vector3.Scale(grid.cellSize, new Vector3(door.Width, 1, 0));

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(doorCenter, doorSize * 0.9f);

            // Draw a circle to highight the anchor cell
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(anchorWorldPos, 0.25f);

            switch (door.Directionality) {
                case DoorDirectionality.Bidirectional:
                    DrawArrow(doorCenter, door.Edge.AsVectorInt());
                    DrawArrow(doorCenter, -door.Edge.AsVectorInt());
                    break;
                case DoorDirectionality.EntranceOnly:
                    DrawArrow(doorCenter, -door.Edge.AsVectorInt());
                    break;
                case DoorDirectionality.ExitOnly:
                    DrawArrow(doorCenter, door.Edge.AsVectorInt());
                    break;
            }
        }

        private void DrawArrow(Vector3 origin, Vector2Int dir) {
            Handles.color = Color.cyan;
            Vector3 dir3 = new(dir.x, dir.y, 0);
            Handles.ArrowHandleCap(0, origin, Quaternion.LookRotation(dir3, Vector3.up), 0.5f, EventType.Repaint);
        }
#endif

    }
    public enum DoorDirectionality {
        Bidirectional,  // Normal door, can go both ways
        EntranceOnly,   // One-way entrance (from this room to the next)
        ExitOnly        // One-way exit (from previous room into this one)
    }

    public enum DoorEdge {
        North = 0, // +Y
        East = 1,  // +X
        South = 2, // -Y
        West = 3   // -X
    }

    public static class DoorUtils {
        private static readonly Vector2Int[] _edgeDirections = new[]
        {
            Vector2Int.up,    // North
            Vector2Int.right, // East
            Vector2Int.down,  // South
            Vector2Int.left   // West
        };

        public static Vector2Int AsVectorInt(this DoorEdge edge) => _edgeDirections[(int)edge];
    }

    /// <summary>
    /// Represents a doorway in a room template. Used during level generation to connect rooms.
    /// </summary>
    [System.Serializable]
    public class RoomDoor {
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
