using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    public class DBSpawnPointsEditorWindow : EditorWindow, ISerializationCallbackReceiver
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        [SerializeField]
        private SpawnPointCollectionSO so;

        [SerializeField]
        private Vector2 scrollPosition;

        /// <summary>
        /// Does window have delegate OnSceneGUI
        /// </summary>
        [SerializeField]
        private bool hasOnSceneGUI;

        /// <summary>
        /// If true, raycasting a point will create a new spawnpoint/path/pathpoint at the center of the object.
        /// Else, it'll create it at the exact ray hit location
        /// </summary>
        [SerializeField]
        private bool useRaycastObjectPosition;

        /// <summary>
        /// Line which connects _so.Collection.Points
        /// </summary>
        [SerializeField]
        private Vector3 pathLineOffset = new Vector3(0, 5, 0);

        /// <summary>
        /// Offset for SpawnPoints info.
        /// </summary>
        [SerializeField]
        private Vector3 gizmoSpawnPointOffset = new Vector3(-10, 75, 0);

        /// <summary>
        /// Offset for PathPoints info
        /// </summary>
        [SerializeField]
        private Vector3 gizmoPathPointOffset = new Vector3(-10, 55, 0);

        [SerializeField]
        private string pointContentString;

        [SerializeField]
        private Color handlesLineStartColor;

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Preview

        [SerializeField]
        private Mesh previewMesh;

        [SerializeField]
        private Vector3 previewMeshScale = Vector3.one;

        [SerializeField]
        private Material previewMaterial;

        /// <summary>
        /// Used to create a quaternion from spawn points or points rotation
        /// </summary>
        private Quaternion quaternion = new Quaternion();

        #endregion Preview

        ////////////////////////////////////////////////////////////////////////

        #region Points

        /// <summary>
        /// Position of the new path point
        /// </summary>
        private Vector3 newPathPointPosition = new Vector3(0, 0, 0);

        private Vector3 newPathPointPositionOffset = new Vector3(50, 0, 50);

        #endregion Points

        ////////////////////////////////////////////////////////////////////////

        #region Keyboard

        /// <summary>
        /// Current path with keyboard controls enabled
        /// </summary>
        [SerializeField]
        private int currentSpawnPointKeyboard = -1;
        [SerializeField]
        private int currentPathKeyboard = -1;

        /// <summary>
        /// Cannot change a list from OnInspectorGUI and OnGUI at the same time. So this way, this is toggled on in OnGUI and toggled off in OnInspectorGUI if it was toggled on.
        /// </summary>
        [SerializeField]
        private bool addSpawnPointToList;
        /// <summary>
        /// Physics ray hit target position. Used to set a new spawn _so.Collection.Points position
        /// </summary>
        [SerializeField]
        private Vector3 hitTargetSpawnPoint;

        /// <summary>
        /// Cannot change a list from OnInspectorGUI and OnGUI at the same time. So this way, this is toggled on in OnGUI and toggled off in OnInspectorGUI if it was toggled on.
        /// </summary>
        [SerializeField]
        private bool _addPathToList;
        /// <summary>
        /// Physics ray hit target position. Used to set a new path _so.Collection.Points position
        /// </summary>
        [SerializeField]
        private Vector3 hitTargetPath;

        [SerializeField]
        private bool addPathPointToList;
        /// <summary>
        /// Physics ray hit target position. Used to set a new path _so.Collection.Points position
        /// </summary>
        [SerializeField]
        private Vector3 hitTargetPathPoint;

        #endregion Keyboard

        ////////////////////////////////////////////////////////////////////////

        #region GUIContent 

        private GUIStyle pointStyle;

        public static GUIContent ContentFocusButton = new GUIContent("F", "Focus scene camera to this point");

        public static GUIContent ContentSelf = new GUIContent("S", "Move spawn point without affecting path point positions");

        public static GUIContent ContentHandles = new GUIContent("H", "Show/Hide gizmo handles for moving points");

        public static GUIContent ContentGizmos = new GUIContent("G", "Show/Hide connection lines between points, preview object and labels");

        public static GUIContent ContentLabels = new GUIContent("L", "Show/Hide labels. Will not be shown if gizmos are not showing");

        public static GUIContent ContentMesh = new GUIContent("M", "Show/Hide preview mesh at this point. Will not be shown if gizmos are not showing. Only shown in scene view.");

        private static GUIContent contentKeyboard = new GUIContent("K", "Toggle keyboard controls for adding and removing points and paths. Only works in scene view.");

        #endregion GUIContent

        ////////////////////////////////////////////////////////////////////////

        #region Style

        [SerializeField]
        private GUIStyle styleInterface;

        #endregion Style

        ////////////////////////////////////////////////////////////////////////

        #region Window

        [MenuItem("Tools/UUtils/Spawn Points")]
        public static void ShowWindow()
        {
            GetWindow<DBSpawnPointsEditorWindow>("Spawn Points");
        }

        /// <summary>
        /// Window is closed
        /// </summary>
        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            // Remove handles from scene
            SceneView.RepaintAll();
        }

        private void Awake()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            styleInterface = EditorStatics.GetBoxStyle(20, 0, 0, 10, 15, 15, 15, 15, 485);
        }

        public void OnBeforeSerialize()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        public void OnAfterDeserialize()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        #endregion Window

        ////////////////////////////////////////////////////////////////////////

        #region GUI

        [SerializeField]
        private GraphSpawnPointEditorWindow graph;

        private void OnGUI()
        {
            SelectSO();

            if(!so)
            {
                return;
            }

            Undo.RecordObject(so, "Undo Spawn Point");

            EditorGUILayout.BeginHorizontal();

            EditorStatics.CreateLabelField(string.Empty, EditorStatics.Width_10);

            if (GUILayout.Button("Graph", EditorStatics.Width_70))
            {
                graph = new GraphSpawnPointEditorWindow();
                graph.Enable(so);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false);

            InterfaceControls();

            InterfacePreview();

            InterfaceSpawnPoints();

            EditorGUILayout.Space();

            GUILayout.EndScrollView();

            // If not called, changes to scriptable object are lost when Unity is restarted
            EditorUtility.SetDirty(so);
        }

        private void Update()
        {
            Repaint();
            SceneView.RepaintAll();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if(so == null)
            {
                return;
            }

            Undo.RecordObject(so, "Undo Spawn Point");
            
            KeyboardRaycast();
            KeyboardAddSpawnPoint();
            KeyboardAddPath();
            KeyboardAddPathPoint();

            if (GetCount() > 0)
            {
                EditSpawnPoints();
                DrawGizmos(sceneView.camera);
                sceneView.Repaint();
            }

            EditorUtility.SetDirty(so);
        }

        #endregion GUI

        ////////////////////////////////////////////////////////////////////////

        #region Controls

        /// <summary>
        /// Layout for keyboard controls.
        /// Creates fields for selecting buttons which control adding points if keyboard control is enabled.    
        /// </summary>
        private void InterfaceControls()
        {
            EditorGUILayout.BeginVertical(styleInterface);

            EditorGUILayout.LabelField("Press a selected key to add spawn point, path or path point", EditorStatics.Width_500);

            so.KeyCodeAddSpawnPoint = (KeyCode)EditorGUILayout.EnumPopup(
                "Spawn Point",
                so.KeyCodeAddSpawnPoint,
                EditorStatics.Width_300
            );

            so.KeyCodeAddPath = (KeyCode)EditorGUILayout.EnumPopup(
                "Path",
                so.KeyCodeAddPath,
                EditorStatics.Width_300
            );

            so.KeyCodeAddPathPoint = (KeyCode)EditorGUILayout.EnumPopup(
                "Path Point",
                so.KeyCodeAddPathPoint,
                EditorStatics.Width_300
            );

            so.KeyCodeDelete = (KeyCode)EditorGUILayout.EnumPopup(
                "Delete",
                so.KeyCodeDelete,
                EditorStatics.Width_300
            );

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Select mesh and material which will be used to display a preview mesh
        /// at spawn point and/or point position with their respective rotations
        /// </summary>
        private void InterfacePreview()
        {
            EditorGUILayout.BeginVertical(styleInterface);

            EditorGUILayout.LabelField("Preview:", EditorStatics.Width_500);

            previewMesh = (Mesh)EditorGUILayout.ObjectField(
                "Preview Mesh",
                previewMesh,
                typeof(Mesh),
                true
            );

            previewMaterial = (Material)EditorGUILayout.ObjectField(
                "Preview Material",
                previewMaterial,
                typeof(Material),
                true
            );

            EditorGUILayout.BeginHorizontal();

            // Spacing
            EditorGUILayout.LabelField("Scale", EditorStatics.Width_150);
            previewMeshScale = EditorGUILayout.Vector3Field(
                string.Empty,
                previewMeshScale
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        #endregion Controls

        ////////////////////////////////////////////////////////////////////////

        #region Select SO

        /// <summary>
        /// Create a field for selecting a SpawnPointCollectionSO asset
        /// </summary>
        private void SelectSO()
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(20, 0, 20, 10, 15, 15, 15, 15, 485);
            EditorGUILayout.BeginVertical(_style);

            so = (SpawnPointCollectionSO)EditorGUILayout.ObjectField(
                "Collection",
                so,
                typeof(SpawnPointCollectionSO),
                true
            );

            EditorGUILayout.EndVertical();
        }

        #endregion Select SO

        ////////////////////////////////////////////////////////////////////////

        #region Count

        private int GetCount()
        {
            return so.Collection.Points.Count;
        }

        private int GetPathPointCount(int i, int j)
        {
            return so.Collection.Points[i].Paths[j].Points.Count;
        }

        #endregion Count

        ////////////////////////////////////////////////////////////////////////

        #region SpawnPoints

        /// <summary>
        /// Must be in OnSceneGUI().
        /// Allows changing spawn point and path point position and rotation.
        /// </summary>
        private void EditSpawnPoints()
        {
            int _spawnPointCount = GetCount();
            for (int _i = 0; _i < _spawnPointCount; _i++)
            {
                SpawnPoint _point = so.Collection.Points[_i];

                // This controls displaying of handles for all path points under this spawn point
                if (_point.IsDisplayingHandles)
                {
                    // Draw rotation handles
                    EditorGUI.BeginChangeCheck();
                    quaternion.eulerAngles = _point.Rotation;
                    quaternion = Handles.RotationHandle(quaternion, _point.Position);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _point.Rotation = quaternion.eulerAngles;
                    }

                    // Draw position handles
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(_point.Position, _point.Quaternion);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if(!_point.MoveOnlySelf)
                        {
                            _point.OnSpawnPointUpdatePosition(newPosition);
                        }

                        else
                        {
                            _point.UpdatePosition(newPosition);
                        }
                    }

                    EditPathPointsPositionAndRotation(_point);
                }
            }
        }

        /// <summary>
        /// Edit every path points position and rotation in scene view.
        /// </summary>
        /// <param name="_spawnPoint">Parent spawn point</param>
        private void EditPathPointsPositionAndRotation(SpawnPoint _spawnPoint)
        {
            int _pathCount = _spawnPoint.Paths.Count;

            // All paths in a spawn point
            for (int _i = 0; _i < _pathCount; _i++)
            {
                Path _path = _spawnPoint.Paths[_i];
                // Path in a spawn point is toggled
                if (_path.IsDisplayingHandles)
                {
                    // All path _so.Collection.Points in a path
                    int _pathPointCount = _path.Count;
                    for (int _j = 0; _j < _pathPointCount; _j++)
                    {
                        PathPoint _point = _path.Points[_j];
                        // Draw rotation handles
                        EditorGUI.BeginChangeCheck();
                        quaternion.eulerAngles = _point.Rotation;
                        quaternion = Handles.RotationHandle(quaternion, _point.Position);
                        if (EditorGUI.EndChangeCheck())
                        {
                            _point.Rotation = quaternion.eulerAngles;
                        }

                        _point.Position = Handles.PositionHandle(
                            _point.Position,
                            _point.Quaternion
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Starts drawing the spawn point interface.
        /// </summary>
        private void InterfaceSpawnPoints()
        {
            EditorGUILayout.BeginVertical(styleInterface);

            useRaycastObjectPosition = EditorStatics.CreateBoolField(
                "Raycast Position",
                "If true, raycasting a point will create a new spawnpoint/path/pathpoint at the center of the object. Else, it'll create it at the exact ray hit location",
                ref useRaycastObjectPosition
            );

            pathLineOffset = EditorStatics.CreateVector3AxisFields(
                "Gizmo Line ",
                pathLineOffset,
                EditorStatics.Width_300
            );

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Spawn Points", EditorStatics.Width_150);

           if (GUILayout.Button(EditorStatics.StringAddSign, EditorStatics.Width_30))
            {
                so.Collection.AddSpawnPoint();
            }

            // Remove a spawn point
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                so.Collection.Remove();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            // Foldout toggle
            if (GetCount() > 0)
            {
                SpawnPointsDraw();
            }
        }

        /// <summary>
        /// Draws interface for every spawn point.
        /// </summary>
        private void SpawnPointsDraw()
        {
            int spawnPointCount = GetCount();

            GUIStyle _style = EditorStatics.GetBoxStyle(20, 0, 0, 0, 15, 15, 15, 15, 485);

            for (int _i = 0; _i < spawnPointCount; _i++)
            {
                SpawnPoint _point = so.Collection.Points[_i];

                GUILayout.BeginVertical(_style);

                if (pointStyle == null)
                {
                    pointStyle = new GUIStyle(EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginHorizontal();

                EditorStatics.GUIPreColor = EditorStyles.label.normal.textColor;
                pointStyle.normal.textColor = Color.white;

                EditorGUILayout.LabelField(EditorStatics.StringMiddleMark, pointStyle, GUILayout.Width(20));

                pointStyle.normal.textColor = EditorStatics.GUIPreColor;

                string _displayName = so.Collection.Points[_i].Name + "      ID: " + _point.ID;
                _point.Foldout = EditorGUILayout.Foldout(_point.Foldout, _displayName);

                SpawnPointListOrder(GetCount(), _i);

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();

                // Remove, focus, gizmos, keyboard
                if (EditSpawnPointHeaderButtons(_point, _i))
                {
                    EditorGUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                // Do not show the spawn point
                if (!_point.Foldout)
                {
                    GUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.Space();

                EditSpawnPointInfo(_point);

                PathInterface(_point);

                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Edit color, name, position of a spawn point
        /// </summary>
        private void EditSpawnPointInfo(SpawnPoint _point)
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(0, 0, 5, 0, 15, 15, 15, 15, 455);

            GUILayout.BeginVertical(_style);

            _point.Name = EditorStatics.CreateTextField(
                "Name", 
                "Spawn point name - appears in header", 
                ref _point.Name, 
                EditorStatics.Width_300
            );

            _point.ColorGizmo = EditorGUILayout.ColorField(
                "Color",
                 _point.ColorGizmo,
                EditorStatics.Width_300
            );

            EditorGUILayout.Space();

            // Draw position field on the next line
            EditorGUILayout.BeginHorizontal();

            // Spacing
            EditorGUILayout.LabelField("Position", EditorStatics.Width_150);
            _point.Position = EditorGUILayout.Vector3Field(string.Empty, _point.Position, EditorStatics.Width_210);

            EditorGUILayout.EndHorizontal();

            // Draw rotation field on the next line
            EditorGUILayout.BeginHorizontal();

            // Spacing
            EditorGUILayout.LabelField("Rotation", EditorStatics.Width_150);
            _point.Rotation = EditorGUILayout.Vector3Field(string.Empty, _point.Rotation, EditorStatics.Width_210);

            EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Create buttons for removing a spawnpoint, focusing, gizmos, keyboard controls
        /// </summary>
        /// <returns>True if deleting the spawn point when clicked on remove button</returns>
        /// <param name="_index">Index in list.</param>
        private bool EditSpawnPointHeaderButtons(SpawnPoint _point, int _index)
        {
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                so.Collection.RemoveWithID(_point.ID);
                return true;
            }

            // Move scene camera to this spawn point
            if (GUILayout.Button(ContentFocusButton, EditorStatics.Width_30))
            {
                TeleportSceneCamera(_point.Position);
            }

            EditDisplay(ContentSelf, ref _point.MoveOnlySelf);

            EditDisplay(ContentHandles, ref _point.IsDisplayingHandles);

            EditDisplay(ContentGizmos, ref _point.IsDisplayingGizmo);

            EditDisplay(ContentLabels, ref _point.IsDisplayingLabel);

            EditDisplay(ContentMesh, ref _point.IsDisplayingMesh);

            EditDisplayKeyboardControls(_index);

            return false;
        }

        public static void EditDisplay(GUIContent _content, ref bool _point)
        {
            EditorStatics.GUIPreColor = GUI.backgroundColor;
            GUI.backgroundColor = _point ? Color.green : Color.red;
            if (GUILayout.Button(_content, EditorStatics.Width_30))
            {
                _point = !_point;
            }
            GUI.backgroundColor = EditorStatics.GUIPreColor;
        }

        /// <summary>
        /// Create buttons for enabling keyboard controls.
        /// </summary>
        /// <param name="_index">Spawn point index in spawn point list</param>
        private void EditDisplayKeyboardControls(int _index)
        {
            EditorStatics.GUIPreColor = GUI.backgroundColor;

            // Keyboard enable/disable
            if (currentSpawnPointKeyboard == _index)
            {
                // Enable adding pathpoints to this path with keyboard
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(contentKeyboard, EditorStatics.Width_30))
                {
                    // Reset
                    currentSpawnPointKeyboard = -1;
                    currentPathKeyboard = -1;
                }
                GUI.backgroundColor = EditorStatics.GUIPreColor;
            }
            else
            {
                // Enable adding pathpoints to this path with keyboard
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(contentKeyboard, EditorStatics.Width_30))
                {
                    currentSpawnPointKeyboard = _index;
                    currentPathKeyboard = 0;
                }
                GUI.backgroundColor = EditorStatics.GUIPreColor;
            }
        }

        /// <summary>
        /// Switches order of the spawn _so.Collection.Points list
        /// </summary>
        private void SpawnPointListOrder(int _count, int _i)
        {
            // Move down
            if (_i >= 0 && _i < _count - 1)
            {
                // Remove spawn point
                if (GUILayout.Button(EditorStatics.StringArrowDown, EditorStatics.Width_30))
                {
                    SpawnPoint _current = so.Collection.Points[_i];
                    SpawnPoint _next = so.Collection.Points[_i + 1];

                    so.Collection.Points.Insert(_i, _next);
                    so.Collection.Points.RemoveAt(_i + 1);

                    so.Collection.Points.Insert(_i + 1, _current);
                    so.Collection.Points.RemoveAt(_i + 2);
                }
            }

            // Move up
            if (_i > 0 && _i < _count)
            {
                // Remove spawn point
                if (GUILayout.Button(EditorStatics.StringArrowUp, EditorStatics.Width_30))
                {
                    SpawnPoint _current = so.Collection.Points[_i];
                    SpawnPoint _next = so.Collection.Points[_i - 1];

                    so.Collection.Points.Insert(_i, _next);
                    so.Collection.Points.RemoveAt(_i + 1);

                    so.Collection.Points.Insert(_i - 1, _current);
                    so.Collection.Points.RemoveAt(_i);
                }
            }
        }

        #endregion SpawnPoints

        ////////////////////////////////////////////////////////////////////////

        #region Path

        /// <summary>
        /// Create interface for PathPoints
        /// </summary>
        /// <param name="_spawnPointIndex">Current SpawnPoint index</param>
        private void PathInterface(SpawnPoint _point)
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(0, 0, 5, 0, 15, 15, 15, 15, 455);
            EditorGUILayout.BeginVertical(_style);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Paths", pointStyle, GUILayout.Width(35));

            // Push to the right
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(EditorStatics.StringAddSign, EditorStatics.Width_30))
            {
                // Add a path with a starting position same as the spawnpoint current position
                _point.Add(_point.Position);
            }

            int _count = _point.Paths.Count;

            // Remove a path
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                if (_count > 0)
                {
                    _point.Paths.RemoveAt(_count - 1);
                    _count--;
                }
            }

            EditorGUILayout.EndHorizontal();

            // Dont show toggle if no paths added
            if(_count > 0)
            {
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();

                _point.FoldoutPaths = EditorGUILayout.Foldout(
                    _point.FoldoutPaths,
                    "Toggle"
                );

                GUILayout.FlexibleSpace();

                EditorGUILayout.EndHorizontal();
            }

            // Create a foldout for the paths
            if (_point.FoldoutPaths)
            {
                EditorGUILayout.Space();

                PathsDraw(_point);
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw all Paths in the list to the inspector interface
        /// </summary>
        /// <param name="_spawnPointIndex">The index.</param>
        private void PathsDraw(SpawnPoint _point)
        {
            GUIStyle _style = EditorStatics.GetBoxStyle(0, 0, 5, 0, 15, 15, 15, 15, 425);

            int _pathCount = _point.Paths.Count;
            for (int _i = 0; _i < _pathCount; _i++)
            {
                Path _path = _point.Paths[_i];
                EditorGUILayout.BeginVertical(_style);

                if (pointStyle == null)
                {
                    pointStyle = new GUIStyle(EditorStyles.foldout);
                }

                EditorGUILayout.BeginHorizontal();

                // Create a foldout for the path _so.Collection.Points
                _path.Foldout = EditorGUILayout.Foldout(_path.Foldout, _path.Name + " ID:" + _path.ID);

                if(EditPathHeader(_point, _path, _i))
                {
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.EndHorizontal();

                EditPathInfo(_path);

                if (!_path.Foldout)
                {
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.Space();

                /// DRAWN INSPECTOR INTERFACE PATH POINTS
                PathPointsList(_path);

                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Edits the path header.
        /// </summary>
        /// <returns>True if path was removed from the list</returns>
        private bool EditPathHeader(SpawnPoint _point, Path _path, int _pathIndex)
        {
            EditorStatics.CreateLabelField("", EditorStatics.Width_27);

            // Remove spawn point
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                _point.RemoveWithID(_path.ID);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return true;
            }

            EditDisplay(ContentHandles, ref _path.IsDisplayingHandles);

            EditDisplay(ContentGizmos, ref _path.IsDisplayingGizmo);

            EditDisplay(ContentLabels, ref _path.IsDisplayingLabel);

            // Keyboard enable/disable
            if (currentPathKeyboard == _pathIndex)
            {
                // Enable adding pathpoints to this path with keyboard
                EditorStatics.GUIPreColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(contentKeyboard, EditorStatics.Width_30))
                {
                    currentPathKeyboard = -1;
                }
                GUI.backgroundColor = EditorStatics.GUIPreColor;
            }

            else
            {
                // Enable adding pathpoints to this path with keyboard
                EditorStatics.GUIPreColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(contentKeyboard, EditorStatics.Width_30))
                {
                    currentPathKeyboard = _pathIndex;
                }
                GUI.backgroundColor = EditorStatics.GUIPreColor;
            }

            return false;
        }

        private void EditPathInfo(Path _path)
        {
            if(!_path.Foldout)
            {
                return;
            }

            EditorGUILayout.Space();

            _path.Name = EditorStatics.CreateTextField(
                "Name",
                "Path name - appears in header",
                ref _path.Name,
                EditorStatics.Width_300
            );

            _path.PathColor = EditorGUILayout.ColorField("Color", _path.PathColor, EditorStatics.Width_300);
        }

        #endregion Path

        ////////////////////////////////////////////////////////////////////////

        #region PathPoint

        /// <summary>
        /// Create inspector interface for path _so.Collection.Points
        /// </summary>
        private void PathPointsList(Path _path)
        {
            // Add buttons for adding and removing a point in the path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Points", EditorStatics.Width_100);

            // Position of a new point is the position of the previous point in the list + offset
            int _count = _path.Count;

            if (GUILayout.Button(EditorStatics.StringAddSign, EditorStatics.Width_30))
            {
                _path.Add(newPathPointPosition);
                _count++;
            }

            // Remove a spawn point
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                // Path should always have at least one path point
                if (_count > 1)
                {
                    _path.Points.RemoveAt(_count - 1);
                    _count--;
                }
            }

            EditorGUILayout.EndHorizontal();

            DrawPathPoints(_path);
        }

        /// <summary>
        /// Create interface for a PathPoint which allows editing its position and rotation.
        /// You can also remove the point or focus scene camera on it.
        /// </summary>
        private void DrawPathPoints(Path _path)
        {
            EditorGUILayout.Space();
            GUIStyle style = EditorStatics.GetBoxStyle(0, 0, 5, 0, 5, 5, 5, 5, 400);

            int _count = _path.Points.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                PathPoint _point = _path.Points[_i];

                EditorGUILayout.BeginVertical(style);

                DrawPathPointsTitle(_point, _i);

                DrawPathPointsPositionAndRemove(_point, _i, _path);

                DrawPathPointsRotationAndDisplay(_point);

                DrawPathPointName(_point);

                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Title bar which displays points index in path list and points ID
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_index"></param>
        private void DrawPathPointsTitle(PathPoint _point, int _index)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(
                EditorStatics.StringPointMark + _index + ":",
                pointStyle,
                EditorStatics.Width_30
            );

            EditorGUILayout.LabelField("ID: " + _point.ID, EditorStatics.Width_50);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Field for changing path points position and removing the point from path
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_index"></param>
        /// <param name="_path"></param>
        private void DrawPathPointsPositionAndRemove(PathPoint _point, int _index, Path _path)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(string.Empty, EditorStatics.Width_30);
            EditorGUILayout.LabelField("Position", EditorStatics.Width_50);

            _point.Position = EditorGUILayout.Vector3Field(string.Empty, _point.Position, EditorStatics.Width_210);

            // Spacing
            EditorGUILayout.LabelField(string.Empty, EditorStatics.Width_10);

            // Remove spawn point
            if (GUILayout.Button(EditorStatics.StringRemoveSign, EditorStatics.Width_30))
            {
                _path.Points.RemoveAt(_index);
                // Delete, focus
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                return;
            }

            // Move scene camera to this spawn point
            if (GUILayout.Button(ContentFocusButton, EditorStatics.Width_30))
            {
                TeleportSceneCamera(_point.Position);
            }

            // Delete, focus
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Field for changing path points rotation and display options
        /// </summary>
        /// <param name="_point"></param>
        private void DrawPathPointsRotationAndDisplay(PathPoint _point)
        {
            // Draw rotation field on the next line
            EditorGUILayout.BeginHorizontal();

            // Spacing
            EditorGUILayout.LabelField(string.Empty, EditorStatics.Width_30);
            EditorGUILayout.LabelField("Rotation", EditorStatics.Width_50);

            _point.Rotation = EditorGUILayout.Vector3Field(string.Empty, _point.Rotation, EditorStatics.Width_210);

            // Spacing
            EditorGUILayout.LabelField(string.Empty, EditorStatics.Width_10);

            EditDisplay(ContentMesh, ref _point.IsDisplayingMesh);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Field for changing path points name
        /// </summary>
        /// <param name="_point"></param>
        private void DrawPathPointName(PathPoint _point)
        {
            EditorGUILayout.BeginHorizontal();

            // Spacing
            EditorGUILayout.LabelField(string.Empty, EditorStatics.Width_30);

            EditorGUILayout.LabelField("Name", EditorStatics.Width_50);

            _point.Name = EditorStatics.CreateTextField(
                "",
                "Point name - appears in header",
                ref _point.Name,
                EditorStatics.Width_210
            );

            EditorGUILayout.EndHorizontal();
        }

        #endregion PathPoint

        ////////////////////////////////////////////////////////////////////////

        #region KeyboardControls

        /// <summary>
        /// Raycast with keyboard anywhere in scene (on an object with a collider) to create a spawn point, path or path point
        /// </summary>
        private void KeyboardRaycast()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == so.KeyCodeAddPathPoint)
                {
                    KeyboardRaycastPathPoint();
                }

                if (Event.current.keyCode == so.KeyCodeAddSpawnPoint)
                {
                    KeyboardRaycastSpawnPoint();
                }

                if (Event.current.keyCode == so.KeyCodeAddPath)
                {
                    KeyboardRaycastPath();
                }
            }
        }

        private void KeyboardRaycastPathPoint()
        {
            if (GetCount() > 0)
            {
                if (currentSpawnPointKeyboard < 0 || currentPathKeyboard < 0)
                {
                    addPathPointToList = false;
                }

                else
                {
                    // GUIPointToWorldRay because it's editor
                    Ray _ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    RaycastHit _hit;

                    if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
                    {
                        addPathPointToList = true;

                        if (useRaycastObjectPosition)
                        {
                            Debug.Log("Hit point: " + _hit.transform.position);
                            hitTargetPathPoint = _hit.transform.position;
                        }
                        else
                        {
                            Debug.Log("Hit point: " + _hit.point);
                            hitTargetPathPoint = _hit.point;
                        }
                    }
                }
            }

            Event.current.Use();
        }

        private void KeyboardRaycastSpawnPoint()
        {
            // GUIPointToWorldRay because it's editor
            Ray _ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit _hit;

            if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
            {
                addSpawnPointToList = true;

                if (useRaycastObjectPosition)
                {
                    Debug.Log("Hit point: " + _hit.transform.position);
                    hitTargetSpawnPoint = _hit.transform.position;
                }
                else
                {
                    Debug.Log("Hit point: " + _hit.point);
                    hitTargetSpawnPoint = _hit.point;
                }
            }

            Event.current.Use();
        }

        private void KeyboardRaycastPath()
        {
            if (currentSpawnPointKeyboard < 0)
            {
                _addPathToList = false;
            }

            else
            {
                // GUIPointToWorldRay because it's editor
                Ray _ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit _hit;

                if (Physics.Raycast(_ray, out _hit, Mathf.Infinity))
                {
                    _addPathToList = true;

                    if (useRaycastObjectPosition)
                    {
                        Debug.Log("Hit point: " + _hit.transform.position);
                        hitTargetPath = _hit.transform.position;
                    }
                    else
                    {
                        Debug.Log("Hit point: " + _hit.point);
                        hitTargetPath = _hit.point;
                    }
                }
            }

            Event.current.Use();
        }

        /// <summary>
        /// Add a path to the list with keyboard shortcut.
        /// </summary>
        private void KeyboardAddPath()
        {
            // Add a path to the list if you pressed the set key
            if (_addPathToList && currentSpawnPointKeyboard > -1)
            {
                so.Collection.Points[currentSpawnPointKeyboard].Add(hitTargetPath);
                _addPathToList = false;
            }
        }

        /// <summary>
        /// Add a path point to the list with keyboard shortcut.
        /// </summary>
        private void KeyboardAddPathPoint()
        {
            // Add a path point to the list if you pressed the set key
            if (addPathPointToList && currentSpawnPointKeyboard > -1 && currentPathKeyboard > -1)
            {
                int count = GetPathPointCount(currentSpawnPointKeyboard, currentPathKeyboard);
                so.Collection.Points[currentSpawnPointKeyboard].Paths[currentPathKeyboard].Add(hitTargetPathPoint);

                addPathPointToList = false;
            }
        }

        /// <summary>
        /// Add a path point to the list with keyboard shortcut.
        /// </summary>
        private void KeyboardAddSpawnPoint()
        {
            // Add a path point to the list if you pressed the set key
            if (addSpawnPointToList)
            {
                so.Collection.AddSpawnPoint(hitTargetSpawnPoint);

                addSpawnPointToList = false;
            }
        }

        #endregion KeyboardControls

        ////////////////////////////////////////////////////////////////////////

        #region Gizmo

        private void DrawGizmos(Camera camera)
        {
            if (so.Collection == null || so.Collection.Points == null)
            {
                return;
            }

            List<SpawnPoint> _points = so.Collection.Points;
            int _count = GetCount();
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _spawnPoint = _points[_i];

                // Spawn point is toggled
                if (_spawnPoint.IsDisplayingGizmo)
                {
                    if(_spawnPoint.IsDisplayingLabel)
                    {
                        DrawTextureInfo(_i, _spawnPoint.Position);
                    }

                    if(_spawnPoint.IsDisplayingMesh)
                    {
                        DrawObject(
                            camera,
                            _spawnPoint.Position,
                            _spawnPoint.Rotation
                        );
                    }

                    // All paths in a spawn point
                    int _countPaths = _spawnPoint.Paths.Count;
                    for (int _j = 0; _j < _countPaths; _j++)
                    {
                        Path _path = _spawnPoint.Paths[_j];
                        
                        int _pathPointsCount = _path.Points.Count;
                        // Draw line from spawn point to path position
                        if (_pathPointsCount > 0)
                        {
                            handlesLineStartColor = Handles.color;
                            Handles.color = _spawnPoint.ColorGizmo;
                            Handles.DrawLine(
                                _path.GetZeroPointPosition() + pathLineOffset,
                                _spawnPoint.Position + pathLineOffset
                            );
                            Handles.color = handlesLineStartColor;
                        }

                        // Path in a spawn point is toggled
                        if (_path.IsDisplayingGizmo)
                        {
                            // All path _so.Collection.Points in a path
                            for (int _k = 0; _k < _pathPointsCount; _k++)
                            {
                                PathPoint _pathPoint = _path.Points[_k];

                                if(_pathPoint.IsDisplayingMesh)
                                {
                                    DrawObject(
                                        camera,
                                        _pathPoint.Position,
                                        _pathPoint.Rotation
                                    );
                                }

                                if(_path.IsDisplayingLabel)
                                {
                                    // Path point info
                                    if (_k != 0)
                                    {
                                        DrawTextureInfo(_j, _k, _pathPoint.Position);
                                    }
                                }

                                // Draw a line from start to end point
                                if (_k < _pathPointsCount - 1)
                                {
                                    handlesLineStartColor = Handles.color;
                                    Handles.color = _path.PathColor;

                                    Handles.DrawLine(
                                        _pathPoint.Position + pathLineOffset,
                                        _path.Points[_k + 1].Position + pathLineOffset
                                    );

                                    Handles.color = handlesLineStartColor;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the preview object in scene view
        /// </summary>
        /// <param name="_camera">Scene view camera.</param>
        /// <param name="_position">Position.</param>
        /// <param name="_rotation">Rotation.</param>
        private void DrawObject(Camera _camera, Vector3 _position, Vector3 _rotation)
        {
            if(previewMesh != null && previewMaterial != null && _camera != null)
            {
                Quaternion _previewRotation = Quaternion.identity;
                _previewRotation.eulerAngles = _rotation;
                Matrix4x4 _matrix = Matrix4x4.TRS(_position, _previewRotation, previewMeshScale);
                Graphics.DrawMesh(previewMesh, _matrix, previewMaterial, 0, _camera);
            }
        }

        /// <summary>
        /// Draw a texture which displays _so.Collection.Points and its info
        /// </summary>
        private void DrawTextureInfo(int _index, Vector3 _position)
        {
            EditorStatics.CalculateFontSizes(_position);

            if (EditorStatics.GetDistance(_position) > 250)
            {
                return;
            }

            int _multiplier = 2;

            EditorStatics.SetGUIStyleTextureBackground(_position, 320, 140 * _multiplier, 1000, 5000);

            pointContentString = string.Format(
                "\n<b><color=white><size={0}> {1}Spawn Point: {2}\r\n</size></color></b><color=white><size={3}><b> position:</b> {4} \r\n\n</size></color>",
                EditorStatics.FontSizeH0,
                EditorStatics.StringPointMark,
                _index,
                EditorStatics.FontSizeH3,
                _position
            );

            int _count = so.Collection.Points[_index].Paths.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                string _extra = string.Format(
                    "<color=grey><size={0}><b> {1}Path:</b> " + _i + " \n</size></color><color=white><size={2}><b>\t{1}Path Points:</b> {3} \n\n</size></color>",
                    EditorStatics.FontSizeH1,
                    EditorStatics.StringPointMark,
                    EditorStatics.FontSizeH3,
                    GetPathPointCount(_index, _i)
                );

                pointContentString += _extra;
            }

            EditorStatics.TextureContent = new GUIContent(pointContentString);

            Handles.Label(_position + gizmoSpawnPointOffset, EditorStatics.TextureContent, EditorStatics.Style);
        }

        /// <summary>
        /// Draw a texture which displays path _so.Collection.Points and their info
        /// </summary>
        private void DrawTextureInfo(int _pathIndex, int _pointIndex, Vector3 _position)
        {
            EditorStatics.CalculateFontSizes(_position);

            if (EditorStatics.GetDistance(_position) > 200)
            {
                return;
            }

            EditorStatics.SetGUIStyleTextureBackground(_position, 340, 220, 1000, 50);

            EditorStatics.TextureContent = new GUIContent(
                "<b><color=magenta><size=" + EditorStatics.FontSizeH0 + "> " + EditorStatics.StringPointMark + "Path Point: " + _pointIndex + "\r\n</size></color></b>" +
                "<color=white><size=" + EditorStatics.FontSizeH3 + "><b> " + EditorStatics.StringPointMark + "Path:</b> " + _pathIndex + "\r\n</size></color>" +
                "<color=white><size=" + EditorStatics.FontSizeH3 + "><b> " + EditorStatics.StringPointMark + "Position:</b> " + _position + "\r\n</size></color>"
            );

            Handles.Label(_position + gizmoPathPointOffset, EditorStatics.TextureContent, EditorStatics.Style);
        }

        #endregion Gizmo

        ////////////////////////////////////////////////////////////////////////

        #region Camera

        /// <summary>
        /// Teleports scene camera to the desired position
        /// </summary>
        public static void TeleportSceneCamera(Vector3 _cameraPosition)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            sceneView.LookAt(_cameraPosition);
        }

        #endregion Camera

        ////////////////////////////////////////////////////////////////////////
    }
}
