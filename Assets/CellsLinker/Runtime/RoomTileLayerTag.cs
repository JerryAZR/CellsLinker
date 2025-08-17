using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Tag component to mark Tilemap GameObjects in a room template prefab for
/// special processing during level generation.
///
/// Attach this to Tilemap-containing GameObjects to indicate that their tiles
/// should be copied to the global map,
/// and whether they represent wall layers (which can be cleared for door
/// placement).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Tilemap))]
public class RoomTileLayerTag : MonoBehaviour {
    /// <summary>
    /// Indicates whether this tile layer is a solid layer.
    /// Blocking tiles like walls or floors are treated as solid and will be cleared in overlapping
    /// regions where doors are placed.
    /// Non-solid layers (e.g. background, decorations) are left untouched, even if a door overlaps them.
    /// </summary>
    [Tooltip("If true, this layer is considered a wall/floor and will be cleared in door regions to open passages.\n" +
             "If false, the layer is left untouched even if it overlaps with doors.")]
    public bool LayerIsSolid;
}
