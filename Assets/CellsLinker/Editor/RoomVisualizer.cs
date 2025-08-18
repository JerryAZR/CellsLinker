#if UNITY_EDITOR

using CellsLinker.Runtime;
using CellsLinker.Runtime.ScriptableObjects;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class RoomVisualizer
{
    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    public static void DrawRoomGizmos(RoomTemplateBase room, GizmoType gizmoType)
    {
        foreach (var door in room.Doors)
        {
            DrawDoor(room, door);
        }
        // Draw the RoomRect
        Gizmos.color = Color.white;
        Vector3 roomSize = new(room.RoomRect.size.x, room.RoomRect.size.y, 0);
        Gizmos.DrawWireCube(room.RoomRect.center + Vector2.one / 2, roomSize);
        Handles.Label(room.RoomRect.center, $"Effective Size:\n{room.RoomRect.size}");
    }

    private static void DrawDoor(RoomTemplateBase room, RoomDoor door)
    {
        Grid grid = room.GetComponentInParent<Grid>();
        if (grid == null)
        {
            Debug.LogWarning("Grid not found for RoomTemplateBase");
            return;
        }

        // Draw each tile in the door area
        Vector3 growDirection = door.Edge switch
        {
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

        // Grab the GizmoResource for icons
        GizmoResourceSO resource = GizmoResourceLoader.Get();
        if (resource == null) return;

        // Draw a door label to highight the anchor cell
        Handles.Label(anchorWorldPos, resource.door);
        // Draw arrows to indicate door direction
        if (room.IsDoorValid(door))
        {
            switch (door.Edge)
            {
                case DoorEdge.North:
                    switch (door.Directionality)
                    {
                        case DoorDirectionality.Bidirectional: Handles.Label(doorCenter, resource.arrowVertical); break;
                        case DoorDirectionality.EntranceOnly: Handles.Label(doorCenter, resource.arrowDown); break;
                        case DoorDirectionality.ExitOnly: Handles.Label(doorCenter, resource.arrowUp); break;
                        default: throw new UnexpectedEnumValueException<DoorDirectionality>(door.Directionality);
                    }
                    break;
                case DoorEdge.East:
                    switch (door.Directionality)
                    {
                        case DoorDirectionality.Bidirectional: Handles.Label(doorCenter, resource.arrowHorizontal); break;
                        case DoorDirectionality.EntranceOnly: Handles.Label(doorCenter, resource.arrowLeft); break;
                        case DoorDirectionality.ExitOnly: Handles.Label(doorCenter, resource.arrowRight); break;
                        default: throw new UnexpectedEnumValueException<DoorDirectionality>(door.Directionality);
                    }
                    break;
                case DoorEdge.South:
                    switch (door.Directionality)
                    {
                        case DoorDirectionality.Bidirectional: Handles.Label(doorCenter, resource.arrowVertical); break;
                        case DoorDirectionality.EntranceOnly: Handles.Label(doorCenter, resource.arrowUp); break;
                        case DoorDirectionality.ExitOnly: Handles.Label(doorCenter, resource.arrowDown); break;
                        default: throw new UnexpectedEnumValueException<DoorDirectionality>(door.Directionality);
                    }
                    break;
                case DoorEdge.West:
                    switch (door.Directionality)
                    {
                        case DoorDirectionality.Bidirectional: Handles.Label(doorCenter, resource.arrowHorizontal); break;
                        case DoorDirectionality.EntranceOnly: Handles.Label(doorCenter, resource.arrowRight); break;
                        case DoorDirectionality.ExitOnly: Handles.Label(doorCenter, resource.arrowLeft); break;
                        default: throw new UnexpectedEnumValueException<DoorDirectionality>(door.Directionality);
                    }
                    break;
                default: throw new UnexpectedEnumValueException<DoorEdge>(door.Edge);
            }
        }
        else
        {
            // Draw a warning sign for invalid door
            Handles.Label(doorCenter, resource.warning);
        }
    }

}

#endif
