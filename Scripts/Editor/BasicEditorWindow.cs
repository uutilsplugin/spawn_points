using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace UUtils.Utilities
{
    [Serializable]
    public abstract class BasicEditorWindow : EditorWindow
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// Is left mouse button currently held down
        /// </summary>
        protected bool isLeftMouseHeldDown { get; private set; }

        /// <summary>
        /// Is middle mouse button currently held down
        /// </summary>
        protected bool isMiddleMouseHeldDown { get; private set; }

        private static Vector2 dragArrowLeftDown = new Vector2(1, -1);
        private static Vector2 dragArrowLeftUp = new Vector2(1, 1);
        private static Vector2 dragArrowLeft = new Vector2(1, 0);
        private bool isArrowLeftHeldDown;

        private static Vector2 dragArrowRightDown = new Vector2(-1, -1);
        private static Vector2 dragArrowRightUp = new Vector2(-1, 1);
        private static Vector2 dragArrowRight = new Vector2(-1, 0);
        private bool isArrowRightHeldDown;

        private static Vector2 dragArrowUp = new Vector2(0, 1);
        private bool isArrowUpHeldDown;

        private static Vector2 dragArrowDown = new Vector2(0, -1);
        private bool isArrowDownHeldDown;

        protected bool isShiftHeldDown;

        protected static Vector2 buttonSize = new Vector2(40, 30);

        private static List<KeyValuePair<string, Vector2>> buttons = new List<KeyValuePair<string, Vector2>>
        {
            new KeyValuePair<string, Vector2>("↖", dragArrowLeftUp),
            new KeyValuePair<string, Vector2>("↑", dragArrowUp),
            new KeyValuePair<string, Vector2>("↗", dragArrowRightUp),

            new KeyValuePair<string, Vector2>("←", dragArrowLeft),
            new KeyValuePair<string, Vector2>(EditorStatics.StringMiddleMark, new Vector2(0,0)),
            new KeyValuePair<string, Vector2>("→", dragArrowRight),

            new KeyValuePair<string, Vector2>("↙", dragArrowLeftDown),
            new KeyValuePair<string, Vector2>("↓", dragArrowDown),
            new KeyValuePair<string, Vector2>("↘", dragArrowRightDown),
        };

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////
        
        #region Rect Interface

        private static Rect rectInterfaceControl = new Rect(new Vector2(0, 0), new Vector2(0, 0));
        /// <summary>
        /// Controls interface size. Changing its height will affect y position
        /// of every interface drawn after (below).
        /// </summary>
        private static Vector2 rectInterfaceSize = new Vector2(280, 240);
        private static Vector2 rectInterfaceOffset = new Vector2(2, 2);
        protected float _RectInterfaceControlSize { get { return rectInterfaceControl.height; } }

        private Vector2 rectInterfaceStartSize;
        private float rectInterfaceMaxHeight = 240;

        #endregion Rect Interface

        ////////////////////////////////////////////////////////////////////////

        #region Toolbar 

        /// <summary>
        /// Toolbar is scrollable
        /// </summary>
        private Vector2 toolbarScrollPosition;

        private GUIStyle scrollbarStyle = new GUIStyle();

        /// <summary>
        /// String => button text,
        /// Func => check if button can be displayed,
        /// Action => execute action on button click
        /// </summary>
        private List<Tuple<GUIContent, Func<bool>, Action>> toolbar = new List<Tuple<GUIContent, Func<bool>, Action>>();

        #endregion Toolbar

        ////////////////////////////////////////////////////////////////////////

        #region Zoom

        protected EditorZoom editorZoom = new EditorZoom();

        /// <summary>
        /// Center position of the window, content center not the actual editor window
        /// </summary>
        protected Vector2 windowCenter;

        #endregion Zoom

        ////////////////////////////////////////////////////////////////////////

        #region SelectionBox

        protected SelectionBox selectionBox { get; private set; }

        #endregion SelectionBox

        ////////////////////////////////////////////////////////////////////////

        #region Skin

        private EditorSettings editorSettings;

        #endregion Skin

        ////////////////////////////////////////////////////////////////////////

        #region EditorBackground

        protected EditorBackground editorBackground;

        #endregion EditorBackground

        ////////////////////////////////////////////////////////////////////////

        #region BasicUnity

        private void OnEnable()
        {
            CheckNullables();
            rectInterfaceStartSize = position.size;
        }

        protected virtual void OnGUI()
        {
            CheckNullables();

            editorBackground.DrawWindowBackground(position);

            ProcessMouse(Event.current);

            ProcessKeyboardArrowHeldDown(Event.current);

            ProcessKeyBoardInput(Event.current);

            ProcessKeyboardEvents(Event.current);

            ProcessEvents(Event.current);

            OnBeforeSelectionBox(Event.current);

            if (!IsOverControlInterface(Event.current.mousePosition) && !IsMouseOverPriorityUI(Event.current.mousePosition) && !isShiftHeldDown)
            {
                selectionBox.ProcessSelectionBox(Event.current, false, isLeftMouseHeldDown, false, false);

                OnDragMultiSelected(Event.current);

                OnDragWithSelectionBox(Event.current);
            }

            // Called after so axis does not get drawn over this
            DrawInterfaceControls();
        }

        /// <summary>
        /// OnGUI doesn't update as often as needed so we force it with Repaint()
        /// </summary>
        private void Update()
        {
            Repaint();
        }

        private void CheckNullables()
        {
            // TODO
            // if (editorSettings == null) editorSettings = new EditorSettings();

            if (selectionBox == null) selectionBox = new SelectionBox();

            if (editorBackground == null) editorBackground = new EditorBackground();
        }

        #endregion BasicUnity

        ////////////////////////////////////////////////////////////////////////

        #region Process

        /// <summary>
        /// Process events which depend on selection box not selecting
        /// </summary>
        private void ProcessEvents(Event _event)
        {
            if (selectionBox.IsSelecting)
            {
                return;
            }

            editorBackground.OnDrag(_event.delta, _event.type, isMiddleMouseHeldDown);

            editorBackground.OnDrag(dragArrowLeft, isArrowLeftHeldDown);
            editorBackground.OnDrag(dragArrowRight, isArrowRightHeldDown);
            editorBackground.OnDrag(dragArrowUp, isArrowUpHeldDown);
            editorBackground.OnDrag(dragArrowDown, isArrowDownHeldDown);
        }

        /// <summary>
        /// TODO
        /// Drag multiple nodes at once
        /// </summary>
        private void OnDragMultiSelected(Event _event)
        {
            if (selectionBox.IsSelecting)
                return;

            if (_event.type == EventType.MouseDrag && isLeftMouseHeldDown)
            {
                _event.Use();
            }
        }

        /// <summary>
        /// Call before processing selection box so priority elements can be 
        /// processed in the required order.
        /// </summary>
        protected abstract void OnBeforeSelectionBox(Event _event);

        /// <summary>
        /// Content should be centered
        /// </summary>
        protected abstract void OnCenterContent();

        #endregion Process

        ////////////////////////////////////////////////////////////////////////

        #region Input

        private void ProcessMouse(Event _event)
        {
            if (IsOverControlInterface(_event.mousePosition))
            {
                return;
            }

            if (_event.type == EventType.MouseDown && _event.button == 0)
            {
                isLeftMouseHeldDown = true;
                OnLeftMouseButtonDown(_event.mousePosition);
            }

            // MUST NOT BE ELSE IF
            if ((_event.type == EventType.MouseUp || _event.rawType == EventType.MouseUp) && _event.button == 0)
            {
                isLeftMouseHeldDown = false;
                OnLeftMouseButtonReleased(_event.mousePosition);
            }

            if (_event.type == EventType.MouseDown && _event.button == 2)
            {
                isMiddleMouseHeldDown = true;
            }

            // MUST NOT BE ELSE IF
            if (_event.type == EventType.MouseUp && _event.button == 2)
            {
                isMiddleMouseHeldDown = false;
            }
            if (_event.type == EventType.MouseUp && _event.button == 1)
            {
                OnRightMouseButtonReleased(_event.mousePosition);
            }

            if (_event.type == EventType.ScrollWheel && _event.isScrollWheel)
            {
                OnMouseScroll(_event.mousePosition, _event.delta.y);
            }

            isShiftHeldDown = _event.shift;
        }

        /// <summary>
        /// Left mouse button was pressed down at position.
        /// </summary>
        /// <param name="_mousePosition">Mouse position in editor window</param>
        protected virtual void OnLeftMouseButtonDown(Vector2 _mousePosition)
        {
            
        }

        /// <summary>
        /// Left mouse button was released at position
        /// </summary>
        /// <param name="_mousePosition"></param>
        protected virtual void OnLeftMouseButtonReleased(Vector2 _mousePosition)
        {
            //
        }

        /// <summary>
        /// Right mouse button was released at position
        /// </summary>
        /// <param name="_mousePosition"></param>
        protected virtual void OnRightMouseButtonReleased(Vector2 _mousePosition)
        {
            //
        }

        /// <summary>
        /// Mouse is scrolling.
        /// </summary>
        /// <param name="_mousePosition">Mouse position in window</param>
        /// <param name="_scroll">Scroll amount and direction.</param>
        protected virtual void OnMouseScroll(Vector2 _mousePosition, float _scroll)
        {
            int _sign = (int)Mathf.Sign(_scroll);

            if(_sign > 0)
            {
                if (!editorZoom.CanZoomOut())
                {
                    return;
                }
            }

            else
            {
                if (!editorZoom.CanZoomIn())
                {
                    return;
                }
            }

            editorZoom.OnMouseScrollZoom(_sign);

            // Mouse position if measured from window center
            Vector2 _mousePositionCentered = new Vector2(
                _mousePosition.x - position.width / 2,
                _mousePosition.y - position.height / 2
            );

            // Even out scroll strength
            float _ratio = position.width / position.height;
            float _zoomMaxRatio = (float)editorZoom.ZoomTotalMax / editorZoom.ZoomTotal;
            _mousePositionCentered *= _ratio * _zoomMaxRatio * _sign;

            // Drag position depending on current mouse position
            Vector2 _drag = _mousePositionCentered * editorZoom.ScrollWheelZoom;
            editorBackground.OnDrag(_drag, true);
        }

        /// <summary>
        /// Checks if keyboard arrow keys are being held down
        /// </summary>
        /// <param name="_event"></param>
        private void ProcessKeyboardArrowHeldDown(Event _event)
        {
            if (IsOverControlInterface(_event.mousePosition))
            {
                return;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.LeftArrow)
            {
                _event.Use();
                isArrowLeftHeldDown = true;
            }

            // MUST NOT BE ELSE IF
            if ((_event.type == EventType.KeyUp || _event.rawType == EventType.KeyUp) && _event.keyCode == KeyCode.LeftArrow)
            {
                _event.Use();
                isArrowLeftHeldDown = false;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.RightArrow)
            {
                _event.Use();
                isArrowRightHeldDown = true;
            }

            // MUST NOT BE ELSE IF
            if ((_event.type == EventType.KeyUp || _event.rawType == EventType.KeyUp) && _event.keyCode == KeyCode.RightArrow)
            {
                _event.Use();
                isArrowRightHeldDown = false;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.UpArrow)
            {
                _event.Use();
                isArrowUpHeldDown = true;
            }

            // MUST NOT BE ELSE IF
            if ((_event.type == EventType.KeyUp || _event.rawType == EventType.KeyUp) && _event.keyCode == KeyCode.UpArrow)
            {
                _event.Use();
                isArrowUpHeldDown = false;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.DownArrow)
            {
                _event.Use();
                isArrowDownHeldDown = true;
            }

            // MUST NOT BE ELSE IF
            if ((_event.type == EventType.KeyUp || _event.rawType == EventType.KeyUp) && _event.keyCode == KeyCode.DownArrow)
            {
                _event.Use();
                isArrowDownHeldDown = false;
            }
        }

        /// <summary>
        /// Override to insert methods which check for keyboard events
        /// </summary>
        /// <param name="_event"></param>
        protected virtual void ProcessKeyboardEvents(Event _event)
        {

        }

        /// <summary>
        /// Processes keyboard input to check when any keyboard key was released and then passes
        /// that key to OnKeyboardKeyReleased() along with current mouse position
        /// </summary>
        /// <param name="_event"></param>
        private void ProcessKeyBoardInput(Event _event)
        {
            if ((_event.type == EventType.KeyUp || _event.rawType == EventType.KeyUp))
            {
                OnKeyboardKeyReleased(_event.mousePosition, _event.keyCode);
            }
        }

        protected virtual void OnKeyboardKeyReleased(Vector2 _mousePosition, KeyCode _keyCode)
        {
            //
        }

        /// <summary>
        /// Selection box has started
        /// </summary>
        /// <param name="_event"></param>
        private void OnDragWithSelectionBox(Event _event)
        {
            // Mouse must not be over this window, selection box enabled and have a dragging event
            if (mouseOverWindow && mouseOverWindow != this && selectionBox.IsSelecting && _event.type == EventType.MouseDrag)
            {
                editorBackground.OnDrag(-_event.delta, _event.type, true);
            }
        }

        #endregion Input

        ////////////////////////////////////////////////////////////////////////

        #region Interface

        /// <summary>
        /// Is the mouse over a priority UI element.
        /// Used to determine if selection box is eligible for drawing.
        /// </summary>
        protected virtual bool IsMouseOverPriorityUI(Vector2 _mousePosition)
        {
            return false;
        }

        protected bool IsOverControlInterface(Vector2 _mousePosition)
        {
            return rectInterfaceControl.Contains(_mousePosition, true);
        }

        /// <summary>
        /// Draw whatever needs to be drawn before interface
        /// </summary>
        protected virtual void OnDrawBeforeInterface()
        {
            //
        }

        protected void DrawInterfaceControls()
        {
            OnDrawBeforeInterface();

            // Must clear so there wouldn't be an infinite amount of items
            toolbar.Clear();
            OnBeforeDrawToolbar();

            SetInterfaceSize();
            rectInterfaceControl.Set(rectInterfaceOffset.x, rectInterfaceOffset.y, rectInterfaceSize.x, rectInterfaceSize.y);

            GUILayout.BeginArea(rectInterfaceControl, "CONTROLS", GUI.skin.GetStyle("Window"));

            EditorGUILayout.BeginHorizontal(); // *
            float _toolbarHeight = rectInterfaceControl.height - 25;
            toolbarScrollPosition = EditorGUILayout.BeginScrollView(
                toolbarScrollPosition, 
                false, 
                false,
                scrollbarStyle,
                scrollbarStyle,
                GUI.skin.GetStyle("Box"), 
                new GUILayoutOption[] { GUILayout.Width(35), GUILayout.Height(_toolbarHeight) }
            );
            DrawToolbarItems();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginVertical(); // **

            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"), new GUILayoutOption[] { GUILayout.Width(100) });
            DrawInterfaceMove();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("Box"), new GUILayoutOption[] { GUILayout.Width(180) });
            OnDrawAfterInterfaceMoveLine();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical(); // **

            EditorGUILayout.EndHorizontal(); // *

            GUILayout.EndArea();

            OnDrawInterface();
        }

        /// <summary>
        /// Override to start drawing any interface
        /// </summary>
        protected virtual void OnDrawInterface()
        {
            //
        }

        /// <summary>
        /// Sets control interface height relative to editor window height
        /// </summary>
        private void SetInterfaceSize()
        {
            if(rectInterfaceSize.y > position.height)
            {
                rectInterfaceSize.y = position.height - 10;
            }

            else
            {
                rectInterfaceSize.y = position.height - 10;
                rectInterfaceSize.y = Mathf.Clamp(rectInterfaceSize.y, 0, rectInterfaceMaxHeight);
            }

        }

        /// <summary>
        /// Draw interface for moving around the widnow with arrow buttons
        /// </summary>
        private void DrawInterfaceMove()
        {
            int _count = 0;
            foreach (var _item in buttons)
            {
                if (_count == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }

                if (!string.IsNullOrEmpty(_item.Key))
                {
                    if (string.Equals(_item.Key, EditorStatics.StringMiddleMark))
                    {
                        if (GUILayout.Button(_item.Key, EditorStatics.Width_30))
                        {
                            editorBackground.ResetDrag();
                            OnCenterContent();
                        }
                    }

                    else if (GUILayout.Button(_item.Key, EditorStatics.Width_30))
                    {
                        editorBackground.OnDrag(_item.Value * editorZoom.ZoomValue, true);
                    }
                }

                _count++;
                if (_count == 3)
                {
                    EditorGUILayout.EndHorizontal();
                    _count = 0;
                }
            }
        }

        /// <summary>
        /// Override to draw after the move interface
        /// </summary>
        protected virtual void OnDrawAfterInterfaceMoveLine()
        {
            //
        }

        /// <summary>
        /// Override to add buttons to the toolbar. 
        /// Use AddToolbarItem(string _text, Action _action) to add buttons.
        /// </summary>
        protected virtual void OnBeforeDrawToolbar()
        {
            //
        }

        /// <summary>
        /// Add a an item to toolbar
        /// </summary>
        /// <param name="_content">Display text and tooltip</param>
        /// <param name="_func">Condition which should be checked to see if the button should be displayed</param>
        /// <param name="_action">Invoked when clicked on the button</param>
        protected void AddToolbarItem(GUIContent _content, Func<bool> _func, Action _action)
        {
            toolbar.Add(new Tuple<GUIContent, Func<bool>, Action>(_content, _func, _action));
        }

        /// <summary>
        /// Draw every item in the toolbar
        /// </summary>
        private void DrawToolbarItems()
        {
            try
            {
                int _count = toolbar.Count;
                for (int _i = 0; _i < _count; _i++)
                {
                    if (toolbar[_i].Item2 != null && toolbar[_i].Item2())
                    {
                        if (GUILayout.Button(toolbar[_i].Item1, EditorStatics.Width_27))
                        {
                            toolbar[_i]?.Item3();
                        }
                    }

                    else if (toolbar[_i].Item2 == null)
                    {
                        if (GUILayout.Button(toolbar[_i].Item1, EditorStatics.Width_27))
                        {
                            toolbar[_i]?.Item3();
                        }
                    }
                }
            }
            catch (Exception _ex)
            {
                // Avoid Abort getting control n position error
                // Error explanation
                // https://forum.unity.com/threads/argumentexception-getting-control-0s-position-in-a-group-with-only-0-controls-when.135021/
            }
        }

        #endregion Interface

        ////////////////////////////////////////////////////////////////////////
    }
}
