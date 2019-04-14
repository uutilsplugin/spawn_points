using UnityEngine;
using System.Collections.Generic;

namespace UUtils.Utilities
{
    public class SelectionBox
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        private Rect selectionBoxRect;

        private GUIStyle selectionBoxStyle;

        public bool IsSelecting { get; private set; }

        private Color colorSelection = new Color(0.8f, 0.8f, 0.8f, 0.5f);

        private List<Identity> selectedList = new List<Identity>();

        public int CountSelected { get { return selectedList.Count; } }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public SelectionBox()
        {
            Texture2D _texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

            _texture.SetPixel(0, 0, colorSelection);
            _texture.SetPixel(1, 1, colorSelection);
            _texture.Apply();

            selectionBoxStyle = new GUIStyle();
            selectionBoxStyle.normal.background = _texture;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Selection Box

        public void ProcessSelectionBox(Event _event, bool _isMouseOverNode, bool _isLeftMouseHeldDown, bool _isDraggingNode, bool _isSearchBoxActive)
        {
            if (_isDraggingNode || _isSearchBoxActive) return;

            if (_isMouseOverNode && !IsSelecting) return;

            // Create the box with left mouse click
            CreateSelectionBox(_event.mousePosition, _isLeftMouseHeldDown);

            // Drag selection box with left mouse button            
            OnDragSelectBox(_event.type, _event.mousePosition, _isLeftMouseHeldDown);

            // Reset if let go of left mouse button
            ResetSelectionBox(_isLeftMouseHeldDown);

            // Draw on screen
            DrawSelectionBox(_isLeftMouseHeldDown);
        }

        private void CreateSelectionBox(Vector2 _mousePosition, bool _isLeftMouseHeldDown)
        {
            if (_isLeftMouseHeldDown && !IsSelecting)
            {
                IsSelecting = true;
                selectionBoxRect.Set(_mousePosition.x, _mousePosition.y, 0, 0);
                GUI.changed = true;
            }
        }

        private void OnDragSelectBox(EventType _eventType, Vector2 _mousePosition, bool _isLeftMouseHeldDown)
        {
            if (_isLeftMouseHeldDown && IsSelecting && _eventType == EventType.MouseDrag)
            {
                selectionBoxRect.size = _mousePosition - selectionBoxRect.position;
                GUI.changed = true;
            }
        }

        private void ResetSelectionBox(bool _isLeftMouseHeldDown)
        {
            if(IsSelecting && !_isLeftMouseHeldDown)
            {
                IsSelecting = false;
                selectionBoxRect.Set(0, 0, 0, 0);

                GUI.changed = true;
            }
        }

        private void DrawSelectionBox(bool _isLeftMouseHeldDown)
        {
            if (IsSelecting && _isLeftMouseHeldDown)
            {
                GUI.Box(selectionBoxRect, "", selectionBoxStyle);
            }
        }

        #endregion Selection Box

        ////////////////////////////////////////////////////////////////////////

        #region Selection

        public Identity GetSelected(int _index)
        {
            int _count = CountSelected;
            if (_index >= 0 && _index < _count)
            {
                return selectedList[_index];
            }

            return null;
        }

        public void AddSelected(Identity _identity)
        {
            selectedList.Add(_identity);
            _identity.InSelectionBox = true;
        }

        public void ClearSelected()
        {
            ClearHover();
            selectedList.Clear();
        }

        public void ClearHover()
        {
            int _count = CountSelected;
            for (int _i = 0; _i < _count; _i++)
            {
                if(selectedList[_i] != null)
                {
                    selectedList[_i].InSelectionBox = false;
                }
            }
        }

        public bool SelectionBoxContains(Vector2 _position)
        {
            return selectionBoxRect.Contains(_position, true);
        }

        #endregion Selection

        ////////////////////////////////////////////////////////////////////////
    }
}
