using System;
using UnityEngine;
using UnityEditor;

namespace UUtils.Utilities.Graphs
{
    [Serializable]
    public enum ViewType
    {
        Intersection,
        Ruler
    }

    [Serializable]
    public enum PerspectiveType
    {
        /// <summary>
        /// X => horizontal (x), Z => vertical (y)
        /// </summary>
        Top_XZ,
        /// <summary>
        /// Z => horizontal (x), Y => vertical (y)
        /// </summary>
        Right_ZY
    }

    [Serializable]
    public abstract class GraphEditorWindow : BasicEditorWindow
    {
        ////////////////////////////////////////////////////////////////////////

        #region View

        [SerializeField]
        private ViewType viewType = ViewType.Ruler;
        public ViewType ViewType { get { return viewType; } }

        [SerializeField]
        private PerspectiveType perspectiveType;
        public PerspectiveType PerspectiveType { get { return perspectiveType; } }

        #endregion View

        ////////////////////////////////////////////////////////////////////////

        #region Axis

        /// <summary>
        /// Center of the window on creation
        /// </summary>
        [SerializeField]
        private Vector3 intersection;
        public Vector3 Intersection { get { return intersection; } }

        [SerializeField]
        private Vector2 windowStartSize;

        [SerializeField]
        private Vector3 xAxisIntersectionLineStart = new Vector3(0, 0, 0);
        [SerializeField]
        private Vector3 xAxisIntersectionLineEnd = new Vector3(0, 0, 0);

        [SerializeField]
        private Vector3 xAxisRulerLineStart = new Vector3(0, 0, 0);
        [SerializeField]
        private Vector3 xAxisRulerLineEnd = new Vector3(0, 0, 0);

        [SerializeField]
        private Vector3 yAxisIntersectionLineStart = new Vector3(0, 0, 0);
        [SerializeField]
        private Vector3 yAxisIntersectionLineEnd = new Vector3(0, 0, 0);

        [SerializeField]
        private Vector3 yAxisRulerLineStart = new Vector3(0, 0, 0);
        [SerializeField]
        private Vector3 yAxisRulerLineEnd = new Vector3(0, 0, 0);

        [NonSerialized]
        private Rect rulerViewHorizontalBackground;
        [NonSerialized]
        private Rect rulerViewVerticalBackground;

        #endregion Axis

        ////////////////////////////////////////////////////////////////////////

        #region Line Size

        [NonSerialized]
        private float lineLengthLarge = 10;
        [NonSerialized]
        private float lineLengthSmall = 4;

        /// <summary>
        /// Must be used as a multiplier when drawing any user line to the graph.
        /// Large distance between each unit line.
        /// </summary>
        [NonSerialized]
        private int unitOffset = 50;
        /// <summary>
        /// Small distance between each unit line.
        /// Keep at 5 to have 10 small units between two unitOffsets
        /// </summary>
        [NonSerialized]
        private int unitOffsetSmall = 5;

        #endregion Line Size

        ////////////////////////////////////////////////////////////////////////

        #region View Position

        // This is done for editor window optimization to reduce new Vector3 calls
        // when drawing lines

        /// <summary>
        /// Start position of a view when using Handles.DrawLine.
        /// Both intersection and side view use this
        /// </summary>
        private Vector3 viewStartPosition = new Vector3(0, 0, 0);
        /// <summary>
        /// End position of a view when using Handles.DrawLine
        /// Both intersection and side view use this
        /// </summary>
        private Vector3 viewEndPosition = new Vector3(0, 0, 0);

        #endregion View Position

        ////////////////////////////////////////////////////////////////////////

        #region Label

        [SerializeField]
        private GUIStyle styleUnits = new GUIStyle();

        [NonSerialized]
        private Vector2 labelPositionXLeft = new Vector2(-10, -20);
        [NonSerialized]
        private Vector2 labelPositionXRight = new Vector2(-5, -20);

        [NonSerialized]
        private Vector2 labelPositionYUpIntersectionView = new Vector2(20, -8);
        [NonSerialized]
        private Vector2 labelPositionYDownIntersectionView = new Vector2(20, -8);

        [NonSerialized]
        private Vector2 labelPositionYUpSideView = new Vector2(-40, -5);

        #endregion Label

        ////////////////////////////////////////////////////////////////////////

        #region Draw

        /// <summary>
        /// Used to draw rotation handles
        /// </summary>
        private Quaternion quaternion = new Quaternion();

        #endregion Draw

        ////////////////////////////////////////////////////////////////////////

        #region Drag

        /// <summary>
        /// Distance between mouse and identity when identity drag has started.
        /// This will normalize identity dragging and prevent it from snapping
        /// into mouse center position
        /// </summary>
        protected Vector2 mouseDistanceToSelected;

        #endregion Drag

        ////////////////////////////////////////////////////////////////////////

        #region Mouse Position Information

        private static Rect rectMouseBox = new Rect(new Vector2(0, 0), new Vector2(0, 0));
        private static Vector2 rectMouseBoxSize = new Vector2(280, 40);
        private static Vector2 rectMouseBoxOffset = new Vector2(2, 3);

        #endregion Mouse Position Information

        ////////////////////////////////////////////////////////////////////////

        #region Mouse Helper

        private static Color colorMouseHelper = new Color(1, 1, 1, 0.4f);

        [NonSerialized]
        private Vector2 mouseHelperStartHorizontal;

        [NonSerialized]
        private Vector2 mouseHelperEndHorizontal;

        [NonSerialized]
        private Vector2 mouseHelperStartVertical;

        [NonSerialized]
        private Vector2 mouseHelperEndVertical;

        [SerializeField]
        private bool displayMouseHelper;

        private static GUIContent contentMouseHelper = new GUIContent("H", "Display mouse helper.");

        #endregion Mouse Helper

        ////////////////////////////////////////////////////////////////////////

        #region Variables

        protected void Setup()
        {
            position.Set(position.x, position.y, 500, 500);

            ResetIntersectionLines();

            rulerViewHorizontalBackground = new Rect(0, 0, position.width, 50);
            rulerViewVerticalBackground = new Rect(position.width - 50, 0, 50, position.height);

            SetFontOnEnable();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            OnWindowResize();

            ResizeFont();
        }

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Resizing

        private float GetWindowSizeDifferenceX()
        {
            return Mathf.Abs(windowStartSize.x - position.size.x);
        }

        private float GetWindowSizeDifferenceY()
        {
            return Mathf.Abs(windowStartSize.y - position.size.y);
        }

        /// <summary>
        /// TODO
        /// </summary>
        private void ResizeFont()
        {
            styleUnits.fontSize = 10;
        }

        private void SetFontOnEnable()
        {
            styleUnits.fontSize = 10;
            styleUnits.fontStyle = FontStyle.Bold;
            styleUnits.normal.textColor = Color.grey;
        }

        protected override void OnCenterContent()
        {
            ResetIntersectionLines();
        }

        private void OnWindowResize()
        {
            if (!IsResizing())
            {
                return;
            }

            ResetIntersectionLines();
        }

        private void ResetIntersectionLines()
        {
            intersection = new Vector3(position.width / 2, position.height / 2, 0);

            xAxisIntersectionLineStart.y = intersection.y;
            xAxisIntersectionLineEnd.y = intersection.y;

            yAxisIntersectionLineStart.x = intersection.x;
            yAxisIntersectionLineEnd.x = intersection.x;

            windowStartSize = position.size;
        }

        private bool IsResizing()
        {
            return windowStartSize != position.size;
        }

        #endregion Resizing

        ////////////////////////////////////////////////////////////////////////

        #region Switch View

        /// <summary>
        /// Editor window which inherits from this one should override this method
        /// to draw, and not use OnGUI(), so anything that has to be drawn,
        /// doesn't get drawn over this editor window UI
        /// </summary>
        protected abstract void OnBeforeDrawSelectView();

        protected override void OnDrawBeforeInterface()
        {
            base.OnDrawBeforeInterface();

            SetWindowCenter();

            DrawMouseHelper(Event.current.mousePosition);
            OnBeforeDrawSelectView();
            DrawView(0.5f, Color.green, Color.white, position);
            DrawMousePositionUI(GetRealPosition(Event.current.mousePosition));
        }

        protected override void OnDrawAfterInterfaceMoveLine()
        {
            base.OnDrawAfterInterfaceMoveLine();

            EditorGUILayout.BeginHorizontal();

            EditorStatics.CreateLabelField("Display", string.Empty, EditorStatics.Width_70);

            viewType = (ViewType)EditorGUILayout.EnumPopup(
                string.Empty,
                viewType,
                EditorStatics.Width_90
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorStatics.CreateLabelField("Perspective", string.Empty, EditorStatics.Width_70);

            perspectiveType = (PerspectiveType)EditorGUILayout.EnumPopup(
                string.Empty,
                perspectiveType,
                EditorStatics.Width_90
            );

            EditorGUILayout.EndHorizontal();
        }

        #endregion View

        ////////////////////////////////////////////////////////////////////////

        #region Intersection View

        private void DrawView(float _gridOpacity, Color _gridColor, Color _lineColor, Rect _window)
        {
            Handles.BeginGUI();

            SetIntersectionViewLines(_window);
            SetRulerViewLines(_window);

            // Y axis start end point with window start and current size difference
            float _ySizeDifference = GetWindowSizeDifferenceY();

            float _center = GetCenterOffset(false);

            // Update horizontal position
            RefViewStartPosition(ref viewStartPosition, false, _center);
            RefViewEndPosition(ref viewEndPosition, false, _center);

            if (viewType == ViewType.Ruler)
            {
                rulerViewVerticalBackground = new Rect(position.width - 35, 0, 35, position.height);
                GUI.Label(rulerViewVerticalBackground, string.Empty, EditorStyles.textField);
            }

            // Y Main line
            Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);
            Handles.DrawLine(viewStartPosition, viewEndPosition);
            Handles.color = Color.white;

            SetupDrawAxisLinesVertical(_gridOpacity, _lineColor, _window, _center, _ySizeDifference);

            if (viewType == ViewType.Ruler)
            {
                rulerViewHorizontalBackground = new Rect(0, position.height - 40, position.width, 40);
                GUI.Label(rulerViewHorizontalBackground, string.Empty, EditorStyles.textField);
            }

            // X axis start end point with window start and current size difference
            float _xSizeDifference = GetWindowSizeDifferenceX();

            _center = GetCenterOffset(true);

            // Update horizontal position
            RefViewStartPosition(ref viewStartPosition, true, _center);
            RefViewEndPosition(ref viewEndPosition, true, _center);

            // X Main line
            Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);
            Handles.DrawLine(viewStartPosition, viewEndPosition);
            Handles.color = Color.white;

            SetupDrawAxisLinesHorizontal(_gridOpacity, _lineColor, _window, _center, _xSizeDifference);

            Handles.EndGUI();
        }

        /// <summary>
        /// Define how should horizontal axis lines be drawn
        /// </summary>
        private void SetupDrawAxisLinesHorizontal(float _gridOpacity, Color _lineColor, Rect _window, float _center, float xSizeDifference)
        {
            if (viewType == ViewType.Intersection)
            {
                // Right side, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, false, true, editorBackground.Offset.x, intersection.x, 0, xSizeDifference, true, labelPositionXRight, _window.width);
                // Right side, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, false, true, editorBackground.Offset.x, intersection.x, 0, xSizeDifference, false, Vector2.zero, _window.width);
                // Left side, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, true, true, editorBackground.Offset.x, intersection.x, _window.width, xSizeDifference, true, labelPositionXLeft, _window.width);
                // Left side, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, true, true, editorBackground.Offset.x, intersection.x, _window.width, xSizeDifference, false, Vector2.zero, _window.width);
            }

            else
            {
                // Right side, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, false, true, editorBackground.Offset.x, intersection.x, xAxisRulerLineEnd.x, xSizeDifference, true, labelPositionXRight, _window.width);
                // Right side, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, false, true, editorBackground.Offset.x, intersection.x, xAxisRulerLineEnd.x, xSizeDifference, false, Vector2.zero, _window.width);
                // Left side, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, true, true, editorBackground.Offset.x, intersection.x, xAxisRulerLineStart.x, xSizeDifference, true, labelPositionXLeft, _window.width);
                // Left side, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, true, true, editorBackground.Offset.x, intersection.x, xAxisRulerLineStart.x, xSizeDifference, false, Vector2.zero, _window.width);
            }
        }

        /// <summary>
        /// Define how should vertical axis lines be drawn
        /// </summary>
        /// <param name="_gridOpacity">Grid opacity.</param>
        /// <param name="_lineColor">Line color.</param>
        /// <param name="_window">Window.</param>
        /// <param name="_center">Center.</param>
        /// <param name="_ySizeDifference">Y size difference.</param>
        private void SetupDrawAxisLinesVertical(float _gridOpacity, Color _lineColor, Rect _window, float _center, float _ySizeDifference)
        {
            if (viewType == ViewType.Intersection)
            {
                // Up, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, true, false, editorBackground.Offset.y, intersection.y, _window.height, _ySizeDifference, true, labelPositionYUpIntersectionView, _window.height);
                // Up, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, true, false, editorBackground.Offset.y, intersection.y, _window.height, _ySizeDifference, false, Vector2.zero, _window.height);
                // Down, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, false, false, editorBackground.Offset.y, intersection.y, 0, _ySizeDifference, true, labelPositionYDownIntersectionView, _window.height);
                // Down, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, false, false, editorBackground.Offset.y, intersection.y, 0, _ySizeDifference, false, Vector2.zero, _window.height);

            }

            else
            {
                // Up, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, true, false, editorBackground.Offset.y, intersection.y, yAxisRulerLineStart.y, _ySizeDifference, true, labelPositionYUpSideView + labelPositionYUpSideView * editorZoom.ZoomTotal / editorZoom.ZoomTotalMax, _window.height);
                // Up, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, true, false, editorBackground.Offset.y, intersection.y, yAxisRulerLineStart.y, _ySizeDifference, false, Vector2.zero, _window.height);
                // Down, large length
                CalculateLines(unitOffset, lineLengthLarge, _gridOpacity, _lineColor, _center, false, false, editorBackground.Offset.y, intersection.y, yAxisRulerLineEnd.y, _ySizeDifference, true, labelPositionYUpSideView + labelPositionYUpSideView * editorZoom.ZoomTotal / editorZoom.ZoomTotalMax, _window.height);
                // Down, small length, don't display units
                CalculateLines(unitOffsetSmall, lineLengthSmall, _gridOpacity, _lineColor, _center, false, false, editorBackground.Offset.y, intersection.y, yAxisRulerLineEnd.y, _ySizeDifference, false, Vector2.zero, _window.height);
            }
        }

        /// <summary>
        /// Setup values for the main line of the intersection view
        /// </summary>
        private void SetIntersectionViewLines(Rect _window)
        {
            xAxisIntersectionLineStart.x = 0;
            xAxisIntersectionLineEnd.x = _window.width;

            yAxisIntersectionLineStart.y = 0;
            yAxisIntersectionLineEnd.y = _window.height;
        }

        /// <summary>
        /// Setup values for the main line of the side view
        /// </summary>
        private void SetRulerViewLines(Rect _window)
        {
            // Numbers are decided appearance wise
            xAxisRulerLineStart.x = 0;
            xAxisRulerLineStart.y = _window.height - 15;
            xAxisRulerLineEnd.x = _window.width;
            xAxisRulerLineEnd.y = _window.height - 15;

            yAxisRulerLineStart.x = _window.width - 15;
            yAxisRulerLineStart.y = 0;
            yAxisRulerLineEnd.x = _window.width - 15;
            yAxisRulerLineEnd.y = _window.height;
        }

        /// <summary>
        /// Update start position of a chosen vector to intersection or side view
        /// </summary>
        /// <param name="_position">Vector which will be updated.</param>
        /// <param name="_horizontal">Horizontal or vertical axis</param>
        /// <param name="_center">Constant Y position for horizontal axis. Constant X position for vertical axis.</param>
        private void RefViewStartPosition(ref Vector3 _position, bool _horizontal, float _center)
        {
            if (viewType == ViewType.Intersection)
            {
                if (_horizontal)
                {
                    _position.x = xAxisIntersectionLineStart.x;
                    _position.y = _center;
                }

                else
                {
                    _position.x = _center;
                    _position.y = yAxisIntersectionLineStart.y;
                }
            }

            else
            {
                if (_horizontal)
                {
                    _position.x = xAxisRulerLineStart.x;
                    _position.y = _center;
                }

                else
                {
                    _position.x = _center;
                    _position.y = yAxisRulerLineStart.y;
                }
            }
        }

        private void RefViewEndPosition(ref Vector3 _position, bool _horizontal, float _center)
        {
            if (viewType == ViewType.Intersection)
            {
                if (_horizontal)
                {
                    _position.x = xAxisIntersectionLineEnd.x;
                    _position.y = _center;
                }

                else
                {
                    _position.x = _center;
                    _position.y = yAxisIntersectionLineEnd.y;
                }
            }

            else
            {
                if (_horizontal)
                {
                    _position.x = xAxisRulerLineEnd.x;
                    _position.y = _center;
                }

                else
                {
                    _position.x = _center;
                    _position.y = yAxisRulerLineEnd.y;
                }
            }
        }

        private float GetCenterOffset(bool _horizontal)
        {
            if (viewType == ViewType.Intersection)
            {
                if (_horizontal)
                {
                    return xAxisIntersectionLineStart.y + editorBackground.Offset.y;
                }

                else
                {
                    return yAxisIntersectionLineStart.x + editorBackground.Offset.x;
                }
            }

            // Side view doesn't use editorBackground.Offset as its supposed to be stuck at its position
            else
            {
                if (_horizontal)
                {
                    return xAxisRulerLineStart.y;
                }

                else
                {
                    return yAxisRulerLineStart.x;
                }
            }
        }

        private void SetWindowCenter()
        {
            if (viewType == ViewType.Intersection)
            {
                    windowCenter.x = yAxisIntersectionLineStart.x + editorBackground.Offset.x;
                    windowCenter.y = xAxisIntersectionLineStart.y + editorBackground.Offset.y;
            }

            // Side view doesn't use editorBackground.Offset as its supposed to be stuck at its position
            else
            {
                windowCenter.x = yAxisRulerLineStart.x;
                windowCenter.y = xAxisRulerLineStart.y;
            }
        }

        #endregion View

        ////////////////////////////////////////////////////////////////////////

        #region Small Lines

        /// <summary>
        /// Calculate positions of small lines along axis which indicate current coordinates
        /// </summary>
        /// <param name="_units">Distance between each line.</param>
        /// <param name="_lineLength">Line length.</param>
        /// <param name="_gridOpacity">Grid opacity.</param>
        /// <param name="_gridColor">Grid color.</param>
        /// <param name="_center">Line origin middle point on axis.</param>
        /// <param name="_invertDirection">Left/Up or Right/Down.</param>
        /// <param name="_horizontal">Draw horizontally or vertically.</param>
        /// <param name="_backgroundOffsetAxis">Background offset axis. X for horizontal, Y for vertical.</param>
        /// <param name="_positionStart">Start center point axis. X for horizontal, Y for vertical.</param>
        /// <param name="_positionEnd">Axis end point. X for horizontal, Y for vertical.</param>
        /// <param name="_windowSizeDifference">Difference in window size if the window was resized after opening.</param>
        /// <param name="_displayUnits">Display lines current location</param>
        /// <param name="_labelPosition">Label position.</param>
        /// <param name="_windowSize">Window size - width (X) or height (Y).</param>
        private void CalculateLines(int _units, float _lineLength, float _gridOpacity, Color _gridColor, float _center, bool _invertDirection, bool _horizontal, float _backgroundOffsetAxis, float _positionStart, float _positionEnd, float _windowSizeDifference, bool _displayUnits, Vector2 _labelPosition, float _windowSize)
        {
            // Left/Up or Right/Down
            int _direction = _invertDirection ? -1 : 1;

            // Distance from axis center intersection to edge of the editor window
            float _centerToWindowEdgeDistance = _positionStart - (_backgroundOffsetAxis * _direction) + _windowSizeDifference;

            // How many lines from center to edge.
            // How many lines is decided by _units(distance) between each line
            int _lines = Mathf.FloorToInt(_centerToWindowEdgeDistance / _units);

            // How many lines can be seen in the window
            int _totalWindowLines = Mathf.FloorToInt(_windowSize / _units);

            // How many should be displayed on screen
            // example: if Y axis goes let off screen, to draw lines along X use total available lines in the window,
            // otherwise, use how many lines from center point to the edge of the screen, IE: _lines
            int _onscreenLines = _totalWindowLines >= _lines ? _lines : _totalWindowLines + 1;

            Handles.BeginGUI();
            Handles.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, _gridOpacity);

            // Axis start always depends on current scroll position
            // Axis end is always the same and never has to be calculated
            float _positionStartOffset = _backgroundOffsetAxis + _positionStart;

            DrawLines(_lines, _positionStartOffset, _positionEnd, _units, _lineLength, _center, _invertDirection, _horizontal, _displayUnits, _labelPosition);

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void DrawLines(int _lines, float _startPosition, float _endPosition, int _units, float _lineLength, float _center, bool _invertDirection, bool _horizontal, bool _displayUnits, Vector2 _labelPosition)
        {
            // Left/Up or Right/Down
            int _direction = _invertDirection ? -1 : 1;

            // Line from middle to top or middle to right
            float _position0 = _center + _lineLength;
            // Line from middle to bottom or middle to left
            float _position1 = _center - _lineLength;

            // If _horizontal, line y position on x axis
            // else, line x position on y axis
            float _position2;

            Vector3 _lineStart = new Vector3(0, 0, 0);
            Vector3 _lineEnd = new Vector3(0, 0, 0);

            for (int i = _lines; i >= 0; i--)
            {
                _position2 = _startPosition + _units * i * _direction;

                // Line has reached the axis line end point
                if (IsOutOfAxisView(_invertDirection, _position2, _endPosition))
                {
                    break;
                }

                string _text;
                Rect _rect;

                if (_horizontal)
                {
                    GetLineStartAndEnd(_position2, _position1, _position2, _position0, ref _lineStart, ref _lineEnd);
                    _text = (i * editorZoom.ZoomValue * _direction).ToString("#");
                    _rect = new Rect(_position2 + _labelPosition.x - editorZoom.ZoomTotal * 2, _center + _labelPosition.y, 100, 100);
                }

                else
                {
                    GetLineStartAndEnd(_position0, _position2, _position1, _position2, ref _lineStart, ref _lineEnd);
                    _text = (-i * editorZoom.ZoomValue * _direction).ToString("#");
                    _rect = new Rect(_center + _labelPosition.x, _position2 + _labelPosition.y, 100, 100);
                }

                if (_displayUnits && i != 0)
                {
                    DrawDisplayUnits(_rect, _text, i);
                }

                Handles.DrawLine(_lineStart, _lineEnd);
            }
        }

        private void GetLineStartAndEnd(float _startX, float _startY, float _endX, float _endY, ref Vector3 _lineStart, ref Vector3 _lineEnd)
        {
            _lineStart.x = _startX;
            _lineStart.y = _startY;
            _lineEnd.x = _endX;
            _lineEnd.y = _endY;
        }

        private void DrawDisplayUnits(Rect _rect, string _text, int _index)
        {
            // Skip every other because zoom in is too big
            if (editorZoom.ZoomTotal >= editorZoom.ZoomLabelDisplaySkip)
            {
                if (_index % 2 == 0)
                {
                    GUI.Label(_rect, _text, styleUnits);
                }
            }

            else
            {
                GUI.Label(_rect, _text, styleUnits);
            }
        }

        private bool IsOutOfAxisView(bool _invertDirection, float _currentPosition, float _endPosition)
        {
            if (viewType == ViewType.Intersection)
            {
                return _invertDirection ? _currentPosition > _endPosition : _currentPosition < _endPosition;
            }

            // Side view
            return _invertDirection ? _currentPosition < _endPosition : _currentPosition > _endPosition;
        }

        #endregion Small Lines

        ////////////////////////////////////////////////////////////////////////

        #region Helper

        /// <summary>
        /// Apply current zoom depending on view.
        /// </summary>
        /// <returns>The zoom.</returns>
        /// <param name="val">Value.</param>
        /// <param name="isGraph">True if converting real scene to graph editor window position, false if graph mouse position to real scene position.</param>
        protected float ApplyZoom(float val, bool isGraph)
        {
            return isGraph ? val / editorZoom.ZoomValue : val * editorZoom.ZoomValue;
        }

        /// <summary>
        /// Apply distance units.
        /// </summary>
        /// <returns>The zoom.</returns>
        /// <param name="val">Value.</param>
        /// <param name="isGraph">True if converting real scene to graph editor window position, false if graph mouse position to real scene position.</param>
        protected float ApplyUnits(float val, bool isGraph)
        {
            return isGraph ? val * unitOffset : val / unitOffset;
        }

        /// <summary>
        /// Get real scene position based of current mouse position in graph window
        /// </summary>
        /// <returns>The real position.</returns>
        /// <param name="_mousePosition">Mouse position.</param>
        /// <param name="identityPosition">PerspectiveType changes 2 of 3 axis values, 
        /// 3rd must remain the same otherwise identity is moved which should not happen</param>
        protected Vector3 GetRealPosition(Vector2 _mousePosition, Vector3? identityPosition = null)
        {
            Vector3 _position = new Vector3(0, 0, 0);

            if(PerspectiveType == PerspectiveType.Top_XZ)
            {
                _position.x = ApplyUnits(ApplyZoom(_mousePosition.x - intersection.x - editorBackground.Offset.x, false), false);
                _position.z = -ApplyUnits(ApplyZoom(_mousePosition.y - intersection.y - editorBackground.Offset.y, false), false);
                if(identityPosition != null)
                {
                    _position.y = ((Vector3)identityPosition).y;
                }
            }

            else
            {
                if (identityPosition != null)
                {
                    _position.x = ((Vector3)identityPosition).x;
                }
                _position.z = ApplyUnits(ApplyZoom(_mousePosition.x - intersection.x - editorBackground.Offset.x, false), false);
                _position.y = -ApplyUnits(ApplyZoom(_mousePosition.y - intersection.y - editorBackground.Offset.y, false), false);
            }

            return _position;
        }

        protected void RefGetPosition(ref Vector2 _vector, Vector3 _position)
        {
            /// X => horizontal (x), Z => vertical (y)
            if (perspectiveType == PerspectiveType.Top_XZ)
            {
                // Apply zoom to current point, unit offset distance and current move offset to get correct distance from intersection
                _vector.x = ApplyUnits(ApplyZoom(_position.x, true), true) + intersection.x + editorBackground.Offset.x;
                // Value will be inverted, that's why - at start
                _vector.y = -ApplyUnits(ApplyZoom(_position.z, true), true) + intersection.y + editorBackground.Offset.y;
            }

            /// Z => horizontal (x), Y => vertical (y)
            else
            {
                // Apply zoom to current point, unit offset distance and current move offset to get correct distance from intersection
                _vector.x = ApplyUnits(ApplyZoom(_position.z, true), true) + intersection.x + editorBackground.Offset.x;
                // Value will be inverted, that's why - at start
                _vector.y = -ApplyUnits(ApplyZoom(_position.y, true), true) + intersection.y + editorBackground.Offset.y;
            }
        }

        /// <summary>
        /// Converts objects vector3 position into one of current SideCurrentView's
        /// and draws it in the correct location applying intersection start
        /// and zoom.
        /// </summary>
        /// <param name="_vectorStart">Reusable vector for line start</param>
        /// <param name="_vectorEnd">Reusable vector for line end.</param>
        /// <param name="_startPosition">Object position.</param>
        protected void RefDrawAtRealPosition(ref Vector2 _vectorStart, ref Vector2 _vectorEnd, Vector3 _startPosition, Vector3 _endPosition, Color _lineColor)
        {
            RefGetPosition(ref _vectorStart, _startPosition);
            RefGetPosition(ref _vectorEnd, _endPosition);

            Handles.BeginGUI();
            Handles.color = _lineColor;
            Handles.DrawLine(_vectorStart, _vectorEnd);
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        protected void RefDrawHandles(ref Vector2 _vector, Vector3 _position, Identity _identity , ref Vector3 _rotation, bool _mouseOverRect, bool _draggingOtherRect)
        {
            Color32 color;

            // This order must be followed or colors aren't toggled properly
            if(_draggingOtherRect)
            {
                // Other rect is dragged, use default color
                color = _identity.GetDefaultColor();
            }

            else
            {
                // Mouse is over rect
                if(_mouseOverRect)
                {
                    color = _identity.GetColorSelection();
                }

                else
                {
                    // Rect is selected
                    color = (_identity.IsSelected || _identity.InSelectionBox) ?
                            _identity.GetColorSelection() :
                            _identity.GetDefaultColor();
                }
            }

            RefGetPosition(ref _vector, _position);

            Handles.BeginGUI();
            EditorGUI.DrawRect(_identity.GetHandleRect(_vector), color);
            Handles.EndGUI();
        }

        protected void SetMouseDistanceToSelected(Vector2 _selectedRectPosition, Vector2 _mousePosition, Vector2 _size)
        {
            mouseDistanceToSelected = _mousePosition - _selectedRectPosition - _size / 2;
        }

        #endregion Helper

        ////////////////////////////////////////////////////////////////////////

        #region Mouse Position Information

        private void DrawMousePositionUI(Vector3 _mousePosition)
        {
            float _y = _RectInterfaceControlSize + rectMouseBoxOffset.y;
            rectMouseBox.Set(rectMouseBoxOffset.x, _y, rectMouseBoxSize.x, rectMouseBoxSize.y);

            GUILayout.BeginArea(rectMouseBox, "MOUSE POSITION",GUI.skin.GetStyle("Window"));

            if (PerspectiveType == PerspectiveType.Top_XZ)
            {
                string _text = string.Format(
                    "X: ({0})   Z: ({1})",
                    _mousePosition.x,
                    _mousePosition.z
                );

                EditorStatics.CreateLabelField(_text, EditorStatics.Width_350);
            }

            else
            {
                string _text = string.Format(
                    "Z: ({0})   Y: ({1})",
                    _mousePosition.z,
                    _mousePosition.y
                );

                EditorStatics.CreateLabelField(_text, EditorStatics.Width_350);
            }

            GUILayout.EndArea();
        }

        #endregion Mouse Position Information

        ////////////////////////////////////////////////////////////////////////

        #region Mouse Helper

        private void DrawMouseHelper(Vector2 _mousePosition)
        {
            if(!displayMouseHelper)
            {
                return;
            }

            Color _startColor;
            Handles.BeginGUI();
            _startColor = Handles.color;
            Handles.color = colorMouseHelper;

            mouseHelperStartHorizontal.x = 0;
            mouseHelperStartHorizontal.y = _mousePosition.y;
            mouseHelperEndHorizontal.x = position.width;
            mouseHelperEndHorizontal.y = _mousePosition.y;

            mouseHelperStartVertical.x = _mousePosition.x;
            mouseHelperStartVertical.y = 0;
            mouseHelperEndVertical.x = _mousePosition.x;
            mouseHelperEndVertical.y = position.height;

            Handles.DrawLine(mouseHelperStartHorizontal, mouseHelperEndHorizontal);
            Handles.DrawLine(mouseHelperStartVertical, mouseHelperEndVertical);

            Handles.color = _startColor;
            Handles.EndGUI();
        }

        #endregion Mouse Helper

        ////////////////////////////////////////////////////////////////////////

        #region Toolbar

        protected override void OnBeforeDrawToolbar()
        {
            base.OnBeforeDrawToolbar();

            // Mouse helper
            AddToolbarItem(
                contentMouseHelper,
                null,
                () => { displayMouseHelper = !displayMouseHelper; }
            );

            // Zoom in
            AddToolbarItem(
                EditorZoom.ContentZoomIn,
                null,
                editorZoom.ZoomIn
            );

            // Zoom out
            AddToolbarItem(
                EditorZoom.ContentZoomOut,
                null,
                editorZoom.ZoomOut
            );
        }

        #endregion Toolbar

        ////////////////////////////////////////////////////////////////////////
    }
}
