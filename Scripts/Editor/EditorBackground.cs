using UnityEngine;
using UnityEditor;

namespace UUtils.Utilities
{
    [System.Serializable]
    public class EditorBackground
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        [SerializeField]
        private Vector2 offset = new Vector2(0,0);
        public Vector2 Offset { get { return offset; } }

        [SerializeField]
        private Color backgroundColor = new Color(0.27f, 0.27f, 0.27f, 1);

        [SerializeField]
        private Color gridSmallColor = new Color(0.8f, 0.8f, 0.8f, 1);

        [SerializeField]
        private Color gridLargeColor = new Color(0.16f, 0.16f, 0.16f, 1);

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Grid

        public void DrawWindowBackground(Rect _windowRect)
        {
            DrawColorBG(_windowRect);
            DrawGrid(10.0001f, 0.1f, gridSmallColor, _windowRect);
            DrawGrid(10, 0.1f, gridSmallColor, _windowRect);
            DrawGrid(100, 0.7f, gridLargeColor, _windowRect);
            DrawGrid(100.01f, 0.7f, gridLargeColor, _windowRect);
        }

        /// <summary>
        /// Background color of the window
        /// </summary>
        private void DrawColorBG(Rect _windowRect)
        {
            EditorGUI.DrawRect(new Rect(0, 0, _windowRect.size.x, _windowRect.size.y), backgroundColor);
        }

        /// <summary>
        /// Lines
        /// </summary>
        private void DrawGrid(float _gridSpacing, float _gridOpacity, Color _gridColor, Rect _windowRect)
        {
            int _widthDivs = Mathf.CeilToInt(_windowRect.width / _gridSpacing);
            int _heightDivs = Mathf.CeilToInt(_windowRect.height / _gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);

            Vector3 _newOffset = new Vector3(offset.x % _gridSpacing, offset.y % _gridSpacing, 0); // TODO fix emptiness at window edges

            // Horizontal lines
            for (int j = 0; j < _heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-_gridSpacing, _gridSpacing * j, 0) + _newOffset, new Vector3(_windowRect.width, _gridSpacing * j, 0f) + _newOffset);
            }

            // Vertical lines
            for (int i = 0; i < _widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(_gridSpacing * i, -_gridSpacing, 0) + _newOffset, new Vector3(_gridSpacing * i, _windowRect.height, 0f) + _newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        #endregion Grid

        ////////////////////////////////////////////////////////////////////////

        #region Resize

        public void OnDrag(Vector2 _drag, EventType _eventType, bool _middleMouseHeldDown)
        {
            if (_middleMouseHeldDown && _eventType == EventType.MouseDrag)
                offset += _drag;
        }

        /// <summary>
        /// Keyboard arrow is held down.
        /// </summary>
        public void OnDrag(Vector2 _drag, bool keyDown)
        {
            if (keyDown)
                offset += _drag;
        }

        public void ResetDrag()
        {
            offset.x = 0;
            offset.y = 0;
        }

        #endregion Resize

        ////////////////////////////////////////////////////////////////////////
    }
}
