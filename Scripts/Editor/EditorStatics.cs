using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace UUtils.Utilities
{
    public static class EditorStatics
    {
        ////////////////////////////////////////////////////////////////////////

        #region GUILayoutOption

        public static GUILayoutOption[] Width_5 = { GUILayout.Width(5) };
        public static GUILayoutOption[] Width_10 = { GUILayout.Width(10) };
        public static GUILayoutOption[] Width_20 = { GUILayout.Width(20) };
        public static GUILayoutOption[] Width_27 = { GUILayout.Width(27) };
        public static GUILayoutOption[] Width_30 = { GUILayout.Width(30) };
        public static GUILayoutOption[] Width_45 = { GUILayout.Width(45) };
        public static GUILayoutOption[] Width_50 = { GUILayout.Width(50) };
        public static GUILayoutOption[] Width_70 = { GUILayout.Width(70) };
        public static GUILayoutOption[] Width_80 = { GUILayout.Width(80) };
        public static GUILayoutOption[] Width_90 = { GUILayout.Width(90) };
        public static GUILayoutOption[] Width_100 = { GUILayout.Width(100) };
        public static GUILayoutOption[] Width_110 = { GUILayout.Width(110) };
        public static GUILayoutOption[] Width_120 = { GUILayout.Width(120) };
        public static GUILayoutOption[] Width_140 = { GUILayout.Width(140) };
        public static GUILayoutOption[] Width_150 = { GUILayout.Width(150) };
        public static GUILayoutOption[] Width_180 = { GUILayout.Width(180) };
        public static GUILayoutOption[] Width_300 = { GUILayout.Width(300) };
        public static GUILayoutOption[] Width_210 = { GUILayout.Width(210) };
        public static GUILayoutOption[] Width_500 = { GUILayout.Width(500) };
        public static GUILayoutOption[] Width_350 = { GUILayout.Width(350) };
        public static GUILayoutOption[] Width_440 = { GUILayout.Width(440) };
        public static GUILayoutOption[] Width_400 = { GUILayout.Width(400) };
        public static GUILayoutOption[] Width_465 = { GUILayout.Width(465) };
        public static GUILayoutOption[] Width_250 = { GUILayout.Width(250) };
        public static GUILayoutOption[] Width_1000 = { GUILayout.Width(1000) };

        #endregion GUILayoutOption

        ////////////////////////////////////////////////////////////////////////

        #region Strings

        /// <summary>
        /// ↳
        /// </summary>
        public static string StringPointMark = "↳ ";
        /// <summary>
        /// ↓
        /// </summary>
        public static string StringArrowDown = "↓";
        /// <summary>
        /// ↑
        /// </summary>
        public static string StringArrowUp = "↑";
        /// <summary>
        /// +
        /// </summary>
        public static string StringAddSign = "+";
        /// <summary>
        /// -
        /// </summary>
        public static string StringRemoveSign = "-";

        public static string StringMiddleMark = "•";

        #endregion Strings

        ////////////////////////////////////////////////////////////////////////

        #region GUIContent

        /// <summary>
        /// Contains text and tooltips for anything that needs a GUIContent
        /// </summary>
        public static GUIContent ContentExplanation = new GUIContent("IE", "");

        /// <summary>
        /// Contains information about a spawn or path point
        /// </summary>
        public static GUIContent TextureContent;

        /// <summary>
        /// Style for TextureContent
        /// </summary>
        public static GUIStyle Style = new GUIStyle();

        public static Color GUIPreColor;

        #endregion GUIContent

        ////////////////////////////////////////////////////////////////////////

        #region InfoAboveSpawn&PathPoint

        // Calculated font sizes
        public static int FontSizeH0;
        public static int FontSizeH1;
        public static int FontSizeH2;
        public static int FontSizeH3;

        // Default font sizes
        private static int fontSizeH0 = 16;
        private static int fontSizeH1 = 15;
        private static int fontSizeH2 = 14;
        private static int fontSizeH3 = 13;

        /// <summary>
        /// distance from a point to the scene view camera position calculated in GetDistance(Vector3 pointPosition)
        /// </summary>
        private static float distance;

        public static void SetGUIStyleTextureBackground(Vector3 _pointPosition, int _width, int _height, int _maxClampWidth, int _maxClampHeight)
        {
            distance = GetDistance(_pointPosition);

            Style.normal.background = Resources.Load<Texture2D>("UUtils/EditorGizmo/T_SpawnPointEditor_Background");

            Style.stretchWidth = false;
            Style.stretchHeight = false;
            Style.fixedWidth = Mathf.Clamp(_width - distance, 1, _maxClampWidth);
            Style.fixedHeight = Mathf.Clamp(_height - distance, 1, _maxClampHeight);
        }

        public static void CalculateFontSizes(Vector3 _pointPosition)
        {
            int _fontSize = (int)((300 - (int)GetDistance(_pointPosition)) * 0.07f);

            FontSizeH0 = Mathf.Clamp(_fontSize, 1, fontSizeH0);
            FontSizeH1 = Mathf.Clamp(_fontSize, 1, fontSizeH1);
            FontSizeH2 = Mathf.Clamp(_fontSize, 1, fontSizeH2);
            FontSizeH3 = Mathf.Clamp(_fontSize, 1, fontSizeH3);
        }

        /// <summary>
        /// Return the distance from a point to the scene view camera position
        /// </summary>
        public static float GetDistance(Vector3 _pointPosition)
        {
            Vector3 _sceneCamPosition = SceneView.lastActiveSceneView.camera.transform.position;
            return Vector3.Distance(_sceneCamPosition, _pointPosition);
        }

        #endregion InfoAboveSpawn&PathPoint

        ////////////////////////////////////////////////////////////////////////

        #region GUIStyle

        /// <summary>
        /// Create a GUI style which can be used as a background.
        /// If width > 0, box will have the specified width.
        /// If height > 0, box will have the specified height.
        /// </summary>
        public static GUIStyle GetBoxStyle(int _margLeft, int _margRight, int _margTop, int _margBottom, int _padLeft, int _padRight, int _padTop, int _padBottom, int _width = 0, int _height = 0)
        {
            GUIStyle _style = new GUIStyle(EditorStyles.helpBox);
            _style.margin = new RectOffset(_margLeft, _margRight, _margTop, _margBottom);
            _style.padding = new RectOffset(_padLeft, _padRight, _padTop, _padBottom);

            if(_width > 0)
            {
                _style.fixedWidth = _width;
            }

            if (_height > 0)
            {
                _style.fixedHeight = _height;
            }

            return _style;
        }

        #endregion GUIStyle

        ////////////////////////////////////////////////////////////////////////

        #region CreateFields

        public static void FillGUIEmptyContent(string _text, string _tooltip)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;
        }

        /// <summary>
        /// Normal label field
        /// </summary>
        public static void CreateLabelField(string _text, string _tooltip, GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;
            EditorGUILayout.LabelField(ContentExplanation, _width);
        }

        /// <summary>
        /// Bold label field
        /// </summary>
        public static void CreateLabelField(string _text, GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = string.Empty;
            EditorGUILayout.LabelField(ContentExplanation, EditorStyles.boldLabel, _width);
        }

        /// <summary>
        /// Bold label field
        /// </summary>
        public static void CreateLabelField(string _text, GUILayoutOption[] _width, GUIStyle _style)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = string.Empty;
            EditorGUILayout.LabelField(ContentExplanation, _style, _width);
        }

        public static void CreateLabelField(string _text, GUILayoutOption[] _width, Color _color)
        {
            GUIStyle _style = new GUIStyle(EditorStyles.boldLabel);
            ContentExplanation.text = _text;
            // New color
            _style.normal.textColor = _color;
            EditorGUILayout.LabelField(ContentExplanation, _style, _width);
        }

        public static string CreateTextField(string _text, string _tooltip, ref string _field, GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            return EditorGUILayout.TextField(ContentExplanation, _field, _width);
        }

        public static string CreateTextField(string _text, string _tooltip, string _field, GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            return EditorGUILayout.TextField(ContentExplanation, _field, _width);
        }

        /// <summary>
        /// Create an editor IntPopup field
        /// </summary>
        /// <returns>Field value.</returns>
        /// <param name="_text">Text.</param>
        /// <param name="_currentValue">Current value.</param>
        /// <param name="_stringNames">String names.</param>
        /// <param name="_indexes">Indexes.</param>
        /// <param name="width">Width.</param>
        public static int CreateIntPopup(string _text, int _currentValue, List<string> _stringNames, List<int> _indexes, GUILayoutOption[] width)
        {
            return EditorGUILayout.IntPopup(
                _text,
                _currentValue,
                _stringNames.ToArray(),
                _indexes.ToArray(),
                width
            );
        }

        public static int CreateIntPopup(string _text, int _currentValue, string[] _stringNames, int[] _indexes, GUILayoutOption[] width)
        {
            return EditorGUILayout.IntPopup(
                _text,
                _currentValue,
                _stringNames,
                _indexes,
                width
            );
        }

        public static int CreateIntField(string _text, string _tooltip, ref int _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.IntField(ContentExplanation, _field, _width);
        }

        public static int CreateIntField(string _text, string _tooltip, int _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.IntField(ContentExplanation, _field, _width);
        }

        public static float CreateFloatField(string _text, string _tooltip, ref float _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.FloatField(ContentExplanation, _field, _width);
        }

        public static float CreateFloatField(string _text, string _tooltip, float _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.FloatField(ContentExplanation, _field, _width);
        }

        public static bool CreateBoolField(string _text, string _tooltip, ref bool _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.Toggle(ContentExplanation, _field, _width);
        }

        public static bool CreateBoolField(string _text, string _tooltip, bool _field, params GUILayoutOption[] _width)
        {
            ContentExplanation.text = _text;
            ContentExplanation.tooltip = _tooltip;

            if (_width.Length == 0)
                _width = Width_210;

            return EditorGUILayout.Toggle(ContentExplanation, _field, _width);
        }

        public static Vector3 CreateVector3AxisFields(string _preText, Vector3 _targetPosition, GUILayoutOption[] _width)
        {
            Vector3 _newPosition = _targetPosition;

            _newPosition.x = CreateFloatField(
                _preText + "X",
                "",
                ref _newPosition.x,
                _width
            );

            _newPosition.y = CreateFloatField(
                _preText + "Y",
                "",
                ref _newPosition.y,
                _width
            );

            _newPosition.z = CreateFloatField(
                _preText + "Z",
                "",
                ref _newPosition.z,
                _width
            );

            return _newPosition;
        }

        #endregion CreateFields

        ////////////////////////////////////////////////////////////////////////

        #region Tools

        /// <summary>
        /// Hide transform bar in the inspector and transforms handles
        /// </summary>
        public static void HideTools(Transform _transf)
        {
            // Hide transform editing
            _transf.hideFlags = HideFlags.HideInInspector;
            Tools.current = Tool.None;
        }

        #endregion Tools

        ////////////////////////////////////////////////////////////////////////
    }
}
