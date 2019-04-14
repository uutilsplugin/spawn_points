using UnityEngine;

namespace UUtils.Utilities
{
    [System.Serializable]
    public enum IdentityType
    {
        None,
        SpawnPoint,
        Path,
        PathPoint
    }

    [System.Serializable]
    public class Identity
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        [SerializeField]
        protected int id;
        public int ID { get { return id; } }

        public string Name;

        [SerializeField]
        protected IdentityType identityType = IdentityType.None;
        public IdentityType IdentityType { get { return identityType; } }

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// Allows moving of an identity in the graph editor
        /// </summary>
        private Rect handleRect = new Rect();

        public Vector2 RectPosition { get { return handleRect.position; } }

        /// <summary>
        /// Default rect size
        /// </summary>
        private static Vector2 rectSize = new Vector2(10, 10);

        /// <summary>
        /// Identity is selected in graph window.
        /// </summary>
        public bool IsSelected;

        /// <summary>
        /// Was this identity added to the selection box
        /// </summary>
        public bool InSelectionBox { get; set; }

        /// <summary>
        /// Default handle color in graph window when identity isn't selected
        /// </summary>
        private static Color colorDefault = new Color(1, 1, 1, 1);

        /// <summary>
        /// Should this point be showing in the scene view
        /// </summary>
        public bool IsDisplayingHandles = true;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor Methods

        #if UNITY_EDITOR

        /// <summary>
        /// Update this points handle rect with a position and return it.
        /// </summary>
        /// <param name="_position">Position of the rect with zoom and view applied.</param>
        public Rect GetHandleRect(Vector2 _position)
        {
            Vector2 _size = GetHandleRectSize();

            // Center rect
            float _x = _position.x - _size.x / 2;
            float _y = _position.y - _size.y / 2;

            handleRect.Set(_x, _y, _size.x, _size.y);
            return handleRect;
        }

        public bool MouseOverRect(Vector2 _position)
        {
            if(!IsDisplayingHandles)
            {
                return false;
            }

            return handleRect.Contains(_position);
        }

        /// <summary>
        /// Each object can override its rects size
        /// </summary>
        public virtual Vector2 GetHandleRectSize()
        {
            return rectSize;
        }

        public virtual Color GetDefaultColor()
        {
            return colorDefault;
        }

        /// <summary>
        /// Default identity color with alpha = 100
        /// </summary>
        public Color GetColorSelection()
        {
            Color color = GetDefaultColor();
            color.a = 0.2f;

            return color;
        }

        #endif

        #endregion Editor Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
