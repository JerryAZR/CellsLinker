using UnityEngine;

namespace CellsLinker.Runtime.ScriptableObjects
{
    [CreateAssetMenu(fileName = "GizmoResource", menuName = "CellsLinker/GizmoResource")]
    public class GizmoResourceSO : ScriptableObject
    {
        [Header("Arrows")]
        public Texture2D arrowUp;
        public Texture2D arrowRight;
        public Texture2D arrowDown;
        public Texture2D arrowLeft;

        [Header("Bidirectional Arrows")]
        public Texture2D arrowHorizontal; // left-right
        public Texture2D arrowVertical;   // up-down

        [Header("Symbols")]
        public Texture2D door;
        public Texture2D warning; // e.g. yellow triangle
    }
}
