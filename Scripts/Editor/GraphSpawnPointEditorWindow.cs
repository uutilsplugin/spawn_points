using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UUtils.Utilities.Graphs;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    enum IdentityUIState
    {
        None,
        AddSpawnPoint,
        AddPath,
        AddPathPoint,
        Remove,
        MeasureDistance
    }

    /// <summary>
    /// Which axis to lock point when moving
    /// </summary>
    enum Lock
    {
        None,
        X,
        Y,
        Z
    }

    public class GraphSpawnPointEditorWindow : GraphEditorWindow, ISerializationCallbackReceiver
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        /// <summary>
        /// SO which is being edited
        /// </summary>
        [SerializeField]
        private SpawnPointCollectionSO so;

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Lines

        [NonSerialized]
        private Vector2 lineStart = new Vector2(0, 0);

        [NonSerialized]
        private Vector2 lineEnd = new Vector2(0, 0);

        #endregion Lines

        ////////////////////////////////////////////////////////////////////////

        #region Drag

        /// <summary>
        /// Currently dragged point.
        /// </summary>
        private int spawnpointID = -1;

        /// <summary>
        /// Currently dragged path.
        /// </summary>
        private int pathID = -1;

        /// <summary>
        /// Currently dragged point.
        /// </summary>
        private int pathpointID = -1;

        /// <summary>
        /// Used when changing a dragging points position
        /// </summary>
        private Vector3 refPositionVector;

        #endregion Drag

        ////////////////////////////////////////////////////////////////////////

        #region Lock

        [SerializeField]
        private Lock lockAxis = Lock.None;

        #endregion Lock

        ////////////////////////////////////////////////////////////////////////

        #region Buttons UI

        private static GUIContent contentUnHide = new GUIContent("U", "Unhide all hidden identities handles(Ones with red \"H\" button.");

        private static GUIContent contentMeasureDistance = new GUIContent("M", "Measure a distance between two identities. Right click them to select.");

        private static GUIContent contentRemoveIdentity = new GUIContent("R", "Remove an identity with right click while hovering over one.");

        private static GUIContent contentCreateSpawnPoint = new GUIContent("C", "Create a spawn point with right mouse click anywhere in this editor window.");

        private static GUIContent contentCreatePath = new GUIContent("P", "Create a path with right mouse click anywhere in this editor window.");

        private static GUIContent contentCreatePathPoint = new GUIContent("PP", "Create a path point with right mouse click anywhere in this editor window.");

        private static GUIContent contentSelectionDelete = new GUIContent("D", "Delete everything in selection box. First, select items and then click this button.");

        [SerializeField]
        private IdentityUIState identityUIState = IdentityUIState.None;

        #endregion Buttons UI

        ////////////////////////////////////////////////////////////////////////

        #region Distance

        /// <summary>
        /// First click happened after measuring was enabled 
        /// </summary>
        private bool isMeasuring;

        /// <summary>
        /// Position from where measuring starts
        /// </summary>
        private Vector3 measureStartPosition;

        /// <summary>
        /// Name of the identity which is being measured
        /// </summary>
        private string measureStartIdentity;

        /// <summary>
        /// Position where measuring ends
        /// </summary>
        private Vector3 measureEndPosition;

        /// <summary>
        /// Name of the identity which is being measured
        /// </summary>
        private string measureEndIdentity;

        private static Rect rectDistanceBox = new Rect(new Vector2(0, 0), new Vector2(0, 0));
        private static Vector2 rectDistanceBoxSize = new Vector2(280, 90);
        private static Vector2 rectDistanceBoxOffset = new Vector2(2, 210);

        /// <summary>
        /// Measure start position. For handles utility.
        /// </summary>
        [SerializeField]
        private Vector2 distancePositionStart;

        private static Color colorDistance = new Color(0.5f, 1f, 1f, 0.8f);

        #endregion Distance

        ////////////////////////////////////////////////////////////////////////

        #region Selected Identity

        private static Rect rectSelectedBox = new Rect(new Vector2(0, 0), new Vector2(0, 0));
        private static Vector2 rectSelectedBoxSize = new Vector2(280, 120);
        private static Vector2 rectSelectedBoxOffset = new Vector2(2, 88);

        #endregion Selected Identity

        ////////////////////////////////////////////////////////////////////////

        #region Hovered Identity

        private static Rect rectHoveredBox = new Rect(new Vector2(0, 0), new Vector2(0, 0));
        private static Vector2 rectHoveredBoxSize = new Vector2(280, 40);
        private static Vector2 rectHoveredBoxOffset = new Vector2(2, 45);

        #endregion Hovered Identity

        ////////////////////////////////////////////////////////////////////////

        #region Enable And Override Draw

        public void Enable(SpawnPointCollectionSO _so)
        {
            GetWindow<GraphSpawnPointEditorWindow>("Graph");
            Setup();

            so = _so;
        }

        protected override void OnBeforeDrawSelectView()
        {
            if (so == null)
            {
                return;
            }

            Undo.RecordObject(so, "Undo Graph");

            DrawLines();
            DrawHandles(Event.current.mousePosition);
            ProcessHandles(Event.current);
            ResetDraggingPoint();
            ProcessSelectionBox();
            DrawMeasureLine(Event.current.mousePosition);

            EditorUtility.SetDirty(so);
        }

        protected override void OnDrawInterface()
        {
            DrawMeasureDistanceUI();
            DrawSelectedIdentityUI();
            DrawHoveredIdentityUI(Event.current.mousePosition);
        }

        #endregion Enable And Override Draw

        ////////////////////////////////////////////////////////////////////////

        #region Draw

        private void DrawLines()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> _points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                // Spawn point is toggled
                if (so.Collection.Points[_i].IsDisplayingGizmo)
                {
                    int _countPaths = _points[_i].Paths.Count;

                    // All paths in a spawn point
                    for (int _j = 0; _j < _countPaths; _j++)
                    {
                        // Path in a spawn point is toggled
                        if (_points[_i].Paths[_j].IsDisplayingGizmo)
                        {
                            int _countPoints = _points[_i].Paths[_j].Points.Count;
                            // All path so.Collection.Points in a path
                            for (int _k = 0; _k < _countPoints; _k++)
                            {
                                // First point, line from spawn point to the first point
                                if (_k == 0)
                                {
                                    RefDrawAtRealPosition(
                                        ref lineStart, 
                                        ref lineEnd, 
                                        _points[_i].Paths[_j].Points[_k].Position,
                                        _points[_i].Position,
                                        _points[_i].ColorGizmo
                                    );
                                }

                                if (_points[_i].Paths[_j].IsDisplayingLabel)
                                {
                                    // Path point info
                                    if (_k != 0)
                                    {
                                        // DrawTextureInfo(j, k, points[i].Paths[j].Points[k].Position);
                                    }
                                }

                                // Draw a line from start to end point
                                if (_k < _points[_i].Paths[_j].Points.Count - 1)
                                {
                                    RefDrawAtRealPosition(
                                        ref lineStart,
                                        ref lineEnd,
                                        _points[_i].Paths[_j].Points[_k].Position,
                                        _points[_i].Paths[_j].Points[_k + 1].Position,
                                        _points[_i].Paths[_j].PathColor
                                    );
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DrawHandles(Vector2 _mousePosition)
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                // Draw handle in scene view for toggled spawn point
                if (points[_i].IsDisplayingHandles)
                {
                    // Draw handles for a spawn point
                    RefDrawHandles(
                        ref lineStart,
                        points[_i].Position,
                        points[_i], 
                        ref points[_i].Rotation, 
                        points[_i].MouseOverRect(_mousePosition),
                        IsOtherIdentityDragged(points[_i])
                    );

                    // All paths in a spawn point
                    int _pathCount = points[_i].Paths.Count;
                    for (int _j = 0; _j < _pathCount; _j++)
                    {
                        // Path in a spawn point is toggled
                        if (points[_i].Paths[_j].IsDisplayingHandles)
                        {
                            // All path so.Collection.Points in a path
                            int _pathPointCount = points[_i].Paths[_j].Points.Count;
                            for (int _k = 0; _k < _pathPointCount; _k++)
                            {
                                // Draw path rect at first path point, ALWAYS
                                if(_k == 0)
                                {
                                    // Draw handles for a path point
                                    RefDrawHandles(
                                        ref lineStart,
                                        points[_i].Paths[_j].Points[_k].Position,
                                        points[_i].Paths[_j],
                                        ref points[_i].Paths[_j].Points[_k].Rotation,
                                        points[_i].Paths[_j].MouseOverRect(_mousePosition),
                                        IsOtherIdentityDragged(points[_i].Paths[_j])
                                    );
                                }

                                // Draw handles for a path point
                                RefDrawHandles(
                                    ref lineStart,
                                    points[_i].Paths[_j].Points[_k].Position,
                                    points[_i].Paths[_j].Points[_k], 
                                    ref points[_i].Paths[_j].Points[_k].Rotation,
                                    points[_i].Paths[_j].Points[_k].MouseOverRect(_mousePosition),
                                    IsOtherIdentityDragged(points[_i].Paths[_j].Points[_k])
                                );
                            }
                        }
                    }
                }
            }
        }

        #endregion Draw

        ////////////////////////////////////////////////////////////////////////

        #region Lock Axis

        /// <summary>
        /// Add lock axis dropdown after view selection
        /// </summary>
        protected override void OnDrawAfterInterfaceMoveLine()
        {
            base.OnDrawAfterInterfaceMoveLine();

            EditorGUILayout.BeginHorizontal();

            EditorStatics.CreateLabelField(
                "Lock Axis", 
                "Dragged point will only be moved on the selected axis.", 
                EditorStatics.Width_70
            );

            lockAxis = (Lock)EditorGUILayout.EnumPopup(
                string.Empty,
                lockAxis,
                EditorStatics.Width_90
            );

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Get new point position with axis locked to old position value
        /// </summary>
        /// <param name="_oldPosition">Point position before dragging</param>
        /// <param name="_newPosition">Locked axis will be modified to return _oldPosition axis value</param>
        /// <returns></returns>
        private Vector3 GetAxisLockedPosition(Vector3 _oldPosition, Vector3 _newPosition)
        {
            switch (lockAxis)
            {
                case Lock.None:
                    return _newPosition;
                case Lock.X:
                    _newPosition.y = _oldPosition.y;
                    _newPosition.z = _oldPosition.z;
                    return _newPosition;
                case Lock.Y:
                    _newPosition.x = _oldPosition.x;
                    _newPosition.z = _oldPosition.z;
                    return _newPosition;
                case Lock.Z:
                    _newPosition.x = _oldPosition.x;
                    _newPosition.y = _oldPosition.y;
                    return _newPosition;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Process keyboard down event to enable/disable lock axis
        /// </summary>
        /// <param name="_event"></param>
        private void ProcessKeyboardLockAxis(Event _event)
        {
            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.X)
            {
                _event.Use();
                lockAxis = lockAxis == Lock.X ? Lock.None : Lock.X;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.Y)
            {
                _event.Use();
                lockAxis = lockAxis == Lock.Y ? Lock.None : Lock.Y;
            }

            if (_event.type == EventType.KeyDown && _event.keyCode == KeyCode.Z)
            {
                _event.Use();
                lockAxis = lockAxis == Lock.Z ? Lock.None : Lock.Z;
            }
        }

        #endregion Lock Axis

        ////////////////////////////////////////////////////////////////////////

        #region Process

        protected override void ProcessKeyboardEvents(Event _event)
        {
            base.ProcessKeyboardEvents(_event);
            ProcessKeyboardLockAxis(_event);
        }

        protected override void OnBeforeSelectionBox(Event _event)
        {
            ProcessHandles(_event);
        }

        private void ProcessHandles(Event _event)
        {
            // Selection box was previously initialized, can't move any points while active
            // Can't move if using selection box to move
            if(selectionBox.IsSelecting)
            {
                return;
            }

            if(!isShiftHeldDown)
            {
                ProcessHandlesInactiveSelectionBox(_event);
            }

            else
            {
                ProcessHandlesActiveSelectionBoxDrag(_event.mousePosition);
            }

        }

        private void ProcessHandlesInactiveSelectionBox(Event _event)
        {
            if(isShiftHeldDown)
            {
                return;
            }

            // Everything is looped backwards because of the order
            // in which points were drawn is forward
            // This will prevent dragging a bottom point if two points overlap.
            // First thing which needs to bne processed are path points, then paths, then spawn points.
            int _count = so.Collection.GetCount();
            List<SpawnPoint> _points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = _points[_i];

                // Draw handle in scene view for toggled spawn point
                if (_point.IsDisplayingHandles)
                {
                    // All paths in a spawn point
                    int _pathCount = _point.Paths.Count;
                    for (int _j = _pathCount - 1; _j >= 0; _j--)
                    {
                        Path _path = _point.Paths[_j];

                        // Path in a spawn point is toggled
                        if (_path.IsDisplayingHandles)
                        {
                            // All path so.Collection.Points in a path
                            int _pathPointCount = _path.Points.Count;
                            for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                            {
                                // Path points are processed 1st
                                if (ProcessHandlesPathPoint(_point, _path, _path.Points[_k], _event))
                                {
                                    if(_path.Points[_k].InSelectionBox)
                                    {
                                        selectionBox.ClearSelected();
                                    }

                                    return;
                                }
                            }

                            // Paths are processed 2nd
                            if (ProcessHandlePath(_point, _path, _event))
                            {
                                if (_path.InSelectionBox)
                                {
                                    selectionBox.ClearSelected();
                                }

                                return;
                            }
                        }
                    }

                    // Spawn points are processed 3rd
                    if (ProcessHandlesSpawnPoint(_point, _event))
                    {
                        if (_point.InSelectionBox)
                        {
                            selectionBox.ClearSelected();
                        }

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true
        /// </summary>
        /// <returns><c>true</c>, if handles spawn point was processed, <c>false</c> otherwise.</returns>
        /// <param name="_point">Point.</param>
        /// <param name="_event">Event.</param>
        private bool ProcessHandlesSpawnPoint(SpawnPoint _point, Event _event)
        {
            bool _dragHandle = CanDragSpawnPointHandle(_event.mousePosition);
            bool _selected = (_point.MouseOverRect(_event.mousePosition) && GetSelectedIdentity() == null) || _point.IsSelected;
            if (_dragHandle && _selected)
            {
                spawnpointID = _point.ID;
                if (!_point.MoveOnlySelf)
                {
                    _point.OnSpawnPointUpdatePosition(
                        GetAxisLockedPosition(
                            _point.Position,
                            GetRealPosition(_event.mousePosition - mouseDistanceToSelected, _point.Position)
                        )
                    );
                }

                else
                {
                    _point.UpdatePosition(
                        GetAxisLockedPosition(
                            _point.Position,
                            GetRealPosition(_event.mousePosition - mouseDistanceToSelected, _point.Position)
                        )
                    );
                }

                return true;
            }

            return false;
        }

        private bool ProcessHandlePath(SpawnPoint _spawnPoint, Path _path, Event _event)
        {
            // and drag will be canceled because its position isn't updated as fast as needed 
            bool _dragHandle = CanDragPathHandle(_event.mousePosition);
            bool _selected = (_path.MouseOverRect(_event.mousePosition) && GetSelectedIdentity() == null) || _path.IsSelected;
            if (_dragHandle && _selected)
            {
                pathID = _path.ID;
                // Move every point in the path when path is dragged.
                _path.OnPathUpdatePosition(
                    GetAxisLockedPosition(
                        _path.GetZeroPointPosition(),
                        GetRealPosition(_event.mousePosition - mouseDistanceToSelected, _path.GetZeroPointPosition())
                    )
                );


                return true;
            }

            return false;
        }

        private bool ProcessHandlesPathPoint(SpawnPoint _spawnPoint, Path _path, PathPoint _point, Event _event)
        {
            bool _dragHandle = CanDragPathPointHandle(_event.mousePosition);
            bool _selected = (_point.MouseOverRect(_event.mousePosition) && GetSelectedIdentity() == null) || _point.IsSelected;
            if (_dragHandle && _selected)
            {
                pathpointID = _point.ID;
                _point.UpdatePosition(
                    GetAxisLockedPosition(
                        _point.Position,
                        GetRealPosition(_event.mousePosition - mouseDistanceToSelected, _point.Position)
                    )
                );

                return true;
            }

            return false;
        }

        /// <summary>
        /// Only can drag a point if left mouse button is being held down
        /// </summary>
        /// <returns><c>true</c>, if drag handle was caned, <c>false</c> otherwise.</returns>
        private bool CanDragHandle(Vector2 _mousePosition)
        {
            return isLeftMouseHeldDown && !isMiddleMouseHeldDown && !IsOverInterface(_mousePosition);
        }

        private bool CanDragSpawnPointHandle(Vector2 _mousePosition)
        {
            return CanDragHandle(_mousePosition) && pathID == -1 && pathpointID == -1;
        }

        private bool CanDragPathHandle(Vector2 _mousePosition)
        {
            return CanDragHandle(_mousePosition) && spawnpointID == -1 && pathpointID == -1;
        }

        private bool CanDragPathPointHandle(Vector2 _mousePosition)
        {
            return CanDragHandle(_mousePosition) && spawnpointID == -1 && pathID == -1;
        }

        /// <summary>
        /// Null the dragging point if mouse is not being held down
        /// </summary>
        private void ResetDraggingPoint()
        {
            if (!isLeftMouseHeldDown)
            {
                spawnpointID = -1;
                pathID = -1;
                pathpointID = -1;
            }
        }

        private void DeselectAll()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _point = points[_i];
                _point.IsSelected = false;

                int _pathCount = _point.Paths.Count;
                for (int _j = 0; _j < _pathCount; _j++)
                {
                    Path _path = _point.Paths[_j];
                    _path.IsSelected = false;

                    int _pathPointCount = _path.Points.Count;
                    for (int _k = 0; _k < _pathPointCount; _k++)
                    {
                        _path.Points[_k].IsSelected = false;
                    }
                }
            }
        }

        /// <summary>
        /// Check if another identity is currently being dragged.
        /// </summary>
        private bool IsOtherIdentityDragged(Identity _identity)
        {
            switch (_identity.IdentityType)
            {
                case IdentityType.None:
                    return false;
                case IdentityType.SpawnPoint:
                    if (spawnpointID > -1 && spawnpointID != _identity.ID)
                    {
                        return true;
                    }

                    return false;
                case IdentityType.Path:
                    if (pathID != -1 && pathID != _identity.ID)
                    {
                        return true;
                    }

                    return false;
                case IdentityType.PathPoint:
                    if (pathpointID != -1 && pathpointID != _identity.ID)
                    {
                        return true;
                    }

                    return false;
            }

            return false;
        }


        #endregion Process

        ////////////////////////////////////////////////////////////////////////

        #region Identity Interface

        protected override void OnBeforeDrawToolbar()
        {
            base.OnBeforeDrawToolbar();

            // Unhide all hidden
            AddToolbarItem(
                contentUnHide,
                null,
                UnHideAll
            );

            // Measure distance
            AddToolbarItem(
                contentMeasureDistance,
                null,
                () => 
                {
                    measureStartIdentity = null;
                    isMeasuring = false;
                    distancePositionStart = Vector2.zero;
                    identityUIState = IdentityUIState.MeasureDistance; 
                }
            );

            // Selection box delete
            AddToolbarItem(
                contentSelectionDelete,
                null,
                SelectionBoxDelete
            );

            // Remove identity
            AddToolbarItem(
                contentRemoveIdentity,
                null,
                () => { identityUIState = IdentityUIState.Remove; }
            );

            // Add spawn point
            AddToolbarItem(
                contentCreateSpawnPoint,
                null,
                () => { identityUIState = IdentityUIState.AddSpawnPoint; }
            );

            // Add path, must have a selected spawn point to display the button
            AddToolbarItem(
                contentCreatePath,
                () => { return so.Collection.GetSelected() != null; },
                () => { identityUIState = IdentityUIState.AddPath; }
            );

            // Add path point, must have a selected path to display the button
            AddToolbarItem(
                contentCreatePathPoint,
                HasSelectedPath,
                () => { identityUIState = IdentityUIState.AddPathPoint; }
            );
        }

        private bool HasSelectedPath()
        {
            SpawnPoint _spawnPoint = so.Collection.GetSelected();
            // Must have a spawn point selected to add a new path
            if (_spawnPoint != null)
            {
                return _spawnPoint.GetSelected() != null;
            }

            return false;
        }

        #endregion Identity Interface

        ////////////////////////////////////////////////////////////////////////

        #region Add Identity

        /// <summary>
        /// Add spawn point after clicking somewhere in the graph.
        /// Set the created spawn point as the currently selected spawn point.
        /// </summary>
        /// <param name="_mousePosition">Mouse position.</param>
        private void AddSpawnPoint(Vector2 _mousePosition)
        {
            // Deselect previous
            SpawnPoint _spawnPoint = so.Collection.GetSelected();
            if (_spawnPoint != null)
            {
                _spawnPoint.IsSelected = false;
            }

            // Add new
            _spawnPoint = so.Collection.AddSpawnPoint(GetRealPosition(_mousePosition));
            _spawnPoint.IsSelected = true;
        }

        /// <summary>
        /// Add path to a spawn point after right mouse click at position.
        /// Path is automatically selected.
        /// </summary>
        /// <param name="_mousePosition">Mouse position.</param>
        private void AddPath(Vector2 _mousePosition)
        {
            SpawnPoint _spawnPoint = so.Collection.GetSelected();
            if(_spawnPoint != null)
            {
                // Deselect previous
                Path _path = _spawnPoint.GetSelected();
                if(_path != null)
                {
                    _path.IsSelected = false;
                }

                // Add new
                _path = _spawnPoint.Add(GetRealPosition(_mousePosition));
                _path.IsSelected = true;
            }

            else
            {
                Debug.LogError("Can't add a path to spawn point. Spawn point doesn't exist. Spawn point ID: " + spawnpointID + " Ignore this error if you forgot to select a spawn point.");
            }
        }

        /// <summary>
        /// Add a path point to path at right mouse clicked position.
        /// Point will be added to the list at index + 1 of currently selected
        /// path point. If there's no currently selected path point, new path
        /// point will be added to the end of the list.
        /// New path point is automatically selected.
        /// </summary>
        /// <param name="_mousePosition">Mouse position.</param>
        private void AddPathPoint(Vector2 _mousePosition)
        {
            SpawnPoint _spawnPoint = so.Collection.GetSelected();
            if (_spawnPoint != null)
            {
                Path _path = _spawnPoint.GetSelected();
                if(_path != null)
                {
                    // Deselect previous
                    PathPoint _point = _path.GetSelected();

                    // Insert after 
                    if(_point != null)
                    {
                        _point.IsSelected = false;
                        _point = _path.InsertAfter(GetRealPosition(_mousePosition), _point.ID);
                    }

                    else
                    {
                        // Add new
                        _point = _path.Add(GetRealPosition(_mousePosition));
                    }

                    _point.IsSelected = true;
                }

                else
                {
                    Debug.LogError("Can't add a path point to the path. Path doesn't exist. Path ID: " + pathID + " Ignore this error if you forgot to select a path.");
                }
            }

            else
            {
                Debug.LogError("Can't add a path point to spawn points path. Spawn point doesn't exist. Spawn point ID: " + spawnpointID);
            }
        }

        #endregion Add Identity

        ////////////////////////////////////////////////////////////////////////

        #region Remove Identity

        /// <summary>
        /// Remove identity if mouse is over it
        /// </summary>
        private void RemoveIdentity(Vector2 _mousePosition)
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].MouseOverRect(_mousePosition))
                        {
                            _path.RemoveWithID(_path.Points[_k].ID);

                            // No more points left in the path, remove the path
                            if (_path.Count == 0)
                            {
                                _point.RemoveWithID(_path.ID);
                            }
                            return;
                        }
                    }

                    if (_path.MouseOverRect(_mousePosition))
                    {
                        _point.RemoveWithID(_path.ID);
                        return;
                    }
                }

                if (_point.MouseOverRect(_mousePosition))
                {
                    so.Collection.RemoveWithID(_point.ID);
                    return;
                }
            }
        }

        #endregion Remove Identity

        ////////////////////////////////////////////////////////////////////////

        #region Distance

        /// <summary>
        /// Select identity as a current measure point
        /// </summary>
        private void OnMeasureDistance(Vector2 _mousePosition)
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].MouseOverRect(_mousePosition))
                        {
                            SetMeasure(_path.Points[_k].Position, _path.Points[_k].Name, _mousePosition);
                            return;
                        }
                    }

                    if (_path.MouseOverRect(_mousePosition))
                    {
                        SetMeasure(_path.GetZeroPointPosition(), _path.Name, _mousePosition);
                        return;
                    }
                }

                if (_point.MouseOverRect(_mousePosition))
                {
                    SetMeasure(_point.Position, _point.Name, _mousePosition);
                    return;
                }
            }
        }

        private void DrawMeasureLine(Vector2 _mousePosition)
        {
            if(identityUIState != IdentityUIState.MeasureDistance || !isMeasuring)
            {
                return;
            }

            Color _startColor = Handles.color;
            Handles.BeginGUI();
            Handles.color = colorDistance;

            Handles.DrawLine(distancePositionStart, _mousePosition);

            Handles.color = _startColor;
            Handles.EndGUI();
        }

        private void SetMeasure(Vector3 _position, string _name, Vector2 _mousePosition)
        {
            if (isMeasuring)
            {
                measureEndPosition = _position;
                measureEndIdentity = _name;

                isMeasuring = false;
            }

            // Select clicked as first and deselect the previous second selected
            else
            {
                distancePositionStart = _mousePosition;
                measureStartPosition = _position;
                measureStartIdentity = _name;

                measureEndIdentity = null;
                isMeasuring = true;
            }
        }

        private void DrawMeasureDistanceUI()
        {
            // Both must exist to display
            if (measureStartIdentity == null || measureEndIdentity == null || identityUIState != IdentityUIState.MeasureDistance)
            {
                return;
            }

            try
            {
                float _y = _RectInterfaceControlSize + rectDistanceBoxOffset.y;
                rectDistanceBox.Set(rectDistanceBoxOffset.x, _y, rectDistanceBoxSize.x, rectDistanceBoxSize.y);

                GUILayout.BeginArea(rectDistanceBox, "SCENE DISTANCE", GUI.skin.GetStyle("Window"));

                EditorStatics.CreateLabelField("1) " + measureStartIdentity + " " + measureStartPosition.ToString(), EditorStatics.Width_300);

                EditorStatics.CreateLabelField("2) " + measureEndIdentity + " " + measureEndPosition.ToString(), EditorStatics.Width_300);

                EditorGUILayout.Space();

                float _distance = Vector3.Distance(measureStartPosition, measureEndPosition);
                EditorStatics.CreateLabelField("Distance: " + _distance.ToString(), EditorStatics.Width_350);

                GUILayout.EndArea();
            }
            catch (Exception _e)
            {
                // Avoid Abort getting control n position error
                // Error explanation
                // https://forum.unity.com/threads/argumentexception-getting-control-0s-position-in-a-group-with-only-0-controls-when.135021/
            }
        }

        #endregion Distance

        ////////////////////////////////////////////////////////////////////////

        #region UnHide

        private void UnHideAll()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _point = points[_i];
                _point.IsDisplayingHandles = true;

                int _pathCount = _point.Paths.Count;
                for (int _j = 0; _j < _pathCount; _j++)
                {
                    Path _path = _point.Paths[_j];
                    _path.IsDisplayingHandles = true;

                    // Not doing this for path points
                }
            }
        }

        #endregion UnHide

        ////////////////////////////////////////////////////////////////////////

        #region Selected & Hovered Identity

        private Identity GetSelectedIdentity()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].IsSelected)
                        {
                            return _path.Points[_k];
                        }
                    }

                    if (_path.IsSelected)
                    {
                        return _path;
                    }
                }

                if (_point.IsSelected)
                {
                    return _point;
                }
            }

            return null;
        }

        private Identity GetHoveredIdentity(Vector2 _mousePosition)
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].MouseOverRect(_mousePosition))
                        {
                            return _path.Points[_k];
                        }
                    }

                    if (_path.MouseOverRect(_mousePosition))
                    {
                        return _path;
                    }
                }

                if (_point.MouseOverRect(_mousePosition))
                {
                    return _point;
                }
            }

            return null;
        }

        private void DrawSelectedIdentityUI()
        {
            float _y = _RectInterfaceControlSize + rectSelectedBoxOffset.y;
            rectSelectedBox.Set(rectSelectedBoxOffset.x, _y, rectSelectedBoxSize.x, rectSelectedBoxSize.y);

            GUILayout.BeginArea(rectSelectedBox, "SELECTED", GUI.skin.GetStyle("Window"));

            Identity _identity = GetSelectedIdentity();

            if (_identity != null)
            {
                try
                {
                    EditorGUILayout.LabelField("ID: " + _identity.ID, EditorStatics.Width_140);

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Name:", EditorStatics.Width_50);

                    _identity.Name = EditorStatics.CreateTextField(
                        string.Empty,
                        string.Empty,
                        ref _identity.Name,
                        EditorStatics.Width_210
                    );

                    EditorGUILayout.EndHorizontal();

                    if (_identity is SpawnPoint)
                    {
                        DrawSpawnPointIdentity((SpawnPoint)_identity);
                    }

                    else if (_identity is Path)
                    {
                        DrawPathIdentity((Path)_identity);
                    }

                    else if (_identity is PathPoint)
                    {
                        DrawPathPointIdentity((PathPoint)_identity);
                    }
                }

                catch (Exception _e)
                {
                    // Avoid Abort getting control n position error
                    // Error explanation
                    // https://forum.unity.com/threads/argumentexception-getting-control-0s-position-in-a-group-with-only-0-controls-when.135021/
                }

            }

            GUILayout.EndArea();
        }

        private void DrawSpawnPointIdentity(SpawnPoint _identity)
        {
            DrawPointIdentity(_identity);

            EditorGUILayout.BeginHorizontal();

            // Move scene camera to this spawn point
            if (GUILayout.Button(DBSpawnPointsEditorWindow.ContentFocusButton, EditorStatics.Width_30))
            {
                DBSpawnPointsEditorWindow.TeleportSceneCamera(_identity.Position);
            }

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentSelf, ref _identity.MoveOnlySelf);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentHandles, ref _identity.IsDisplayingHandles);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentGizmos, ref _identity.IsDisplayingGizmo);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentLabels, ref _identity.IsDisplayingLabel);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentMesh, ref _identity.IsDisplayingMesh);

            _identity.ColorGizmo = EditorGUILayout.ColorField(string.Empty, _identity.ColorGizmo, EditorStatics.Width_50);
 
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPathIdentity(Path _identity)
        {
            EditorGUILayout.BeginHorizontal();

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentHandles, ref _identity.IsDisplayingHandles);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentGizmos, ref _identity.IsDisplayingGizmo);

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentLabels, ref _identity.IsDisplayingLabel);

            _identity.PathColor = EditorGUILayout.ColorField(string.Empty, _identity.PathColor, EditorStatics.Width_50);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPathPointIdentity(PathPoint _identity)
        {
            DrawPointIdentity(_identity);

            EditorGUILayout.BeginHorizontal();

            // Move scene camera to this spawn point
            if (GUILayout.Button(DBSpawnPointsEditorWindow.ContentFocusButton, EditorStatics.Width_30))
            {
                DBSpawnPointsEditorWindow.TeleportSceneCamera(_identity.Position);
            }

            DBSpawnPointsEditorWindow.EditDisplay(DBSpawnPointsEditorWindow.ContentMesh, ref _identity.IsDisplayingMesh);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawPointIdentity(Point _identity)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Position:", EditorStatics.Width_50);

            _identity.Position = EditorGUILayout.Vector3Field(
                string.Empty,
                _identity.Position,
                EditorStatics.Width_210
            );

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Rotation:", EditorStatics.Width_50);

            _identity.Rotation = EditorGUILayout.Vector3Field(
                string.Empty,
                _identity.Rotation,
                EditorStatics.Width_210
            );

            EditorGUILayout.EndHorizontal();
        }

        private void DrawHoveredIdentityUI(Vector2 _mousePosition)
        {
            try
            {
                float _y = _RectInterfaceControlSize + rectHoveredBoxOffset.y;
                rectHoveredBox.Set(rectHoveredBoxOffset.x, _y, rectHoveredBoxSize.x, rectHoveredBoxSize.y);

                GUILayout.BeginArea(rectHoveredBox, "MOUSE OVER", GUI.skin.GetStyle("Window"));

                Identity _identity = GetHoveredIdentity(_mousePosition);
                if (_identity != null)
                {
                    try
                    {
                        EditorGUILayout.LabelField(
                            "Name: " + _identity.Name + " " + GetIdentityPosition(_identity),
                            EditorStatics.Width_300
                        );
                    }
                    catch (Exception _e)
                    {
                        // Avoid Abort getting control n position error
                        // Error explanation
                        // https://forum.unity.com/threads/argumentexception-getting-control-0s-position-in-a-group-with-only-0-controls-when.135021/
                    }
                }

                GUILayout.EndArea();
            }
            catch (Exception _e)
            {
                // Avoid Abort getting control n position error
                // Error explanation
                // https://forum.unity.com/threads/argumentexception-getting-control-0s-position-in-a-group-with-only-0-controls-when.135021/
            }
        }

        private Vector3 GetIdentityPosition(Identity _identity)
        {
            if(_identity is Path)
            {
                return ((Path)_identity).GetZeroPointPosition();
            }

            if (_identity is Point)
            {
                return ((Point)_identity).Position;
            }

            return Vector3.zero;
        }

        #endregion Selected & Hovered Identity

        ////////////////////////////////////////////////////////////////////////

        #region Selection Box

        private void ProcessSelectionBox()
        {
            if(!selectionBox.IsSelecting)
            {
                return;
            }

            selectionBox.ClearSelected();

            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if(selectionBox.SelectionBoxContains(_path.Points[_k].RectPosition))
                        {
                            selectionBox.AddSelected(_path.Points[_k]);
                        }
                    }

                    if (selectionBox.SelectionBoxContains(_path.RectPosition))
                    {
                        selectionBox.AddSelected(_path);
                    }
                }

                if (selectionBox.SelectionBoxContains(_point.RectPosition))
                {
                    selectionBox.AddSelected(_point);
                }
            }
        }

        private void ProcessHandlesActiveSelectionBoxDrag(Vector2 _mousePosition)
        {
            if (!isLeftMouseHeldDown)
            {
                return;
            }

            Vector3 _direction = ProcessSelectionBoxDragItemsFirstPass(_mousePosition);

            int _count = so.Collection.GetCount();
            List<SpawnPoint> _points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = _points[_i];

                // All paths in a spawn point
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].InSelectionBox)
                        {
                            _path.Points[_k].UpdateDirection(_direction);
                        }
                    }
                }

                if (_point.InSelectionBox)
                {
                    _point.UpdateDirection(_direction);
                }
            }
        }

        /// <summary>
        /// Returns a drag offset of one single point
        /// </summary>
        private Vector3 ProcessSelectionBoxDragItemsFirstPass(Vector2 _mousePosition)
        {
            bool _isDraggable = false;

            int _count = so.Collection.GetCount();
            List<SpawnPoint> _points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = _points[_i];

                // Draw handle in scene view for toggled spawn point
                if (_point.IsDisplayingHandles)
                {
                    // All paths in a spawn point
                    int _pathCount = _point.Paths.Count;
                    for (int _j = _pathCount - 1; _j >= 0; _j--)
                    {
                        Path _path = _point.Paths[_j];

                        // All path so.Collection.Points in a path
                        int _pathPointCount = _path.Points.Count;
                        for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                        {
                            _isDraggable = _path.Points[_k].MouseOverRect(_mousePosition) || pathpointID == _path.Points[_k].ID;
                            if (_isDraggable && _path.Points[_k].InSelectionBox)
                            {
                                pathpointID = _path.Points[_k].ID;
                                Vector3 _endPosition = GetRealPosition(_mousePosition - mouseDistanceToSelected, _path.Points[_k].Position);
                                // Where the point should be moved to - its start position
                                return _endPosition - _path.Points[_k].Position;
                            }
                        }

                        _isDraggable = _path.MouseOverRect(_mousePosition) || pathID == _path.ID;
                        if (_isDraggable && _path.InSelectionBox)
                        {
                            pathID = _path.ID;
                            Vector3 _endPosition = GetRealPosition(_mousePosition - mouseDistanceToSelected, _path.GetZeroPointPosition());
                            // Where the point should be moved to - its start position
                            return _endPosition - _path.GetZeroPointPosition();
                        }
                    }

                    _isDraggable = _point.MouseOverRect(_mousePosition) || spawnpointID == _point.ID;
                    if (_isDraggable && _point.InSelectionBox)
                    {
                        spawnpointID = _point.ID;
                        Vector3 _endPosition = GetRealPosition(_mousePosition - mouseDistanceToSelected, _point.Position);
                        // Where the point should be moved to - its start position
                        return _endPosition - _point.Position;
                    }
                }
            }

            return Vector3.zero;
        }

        private void SelectionBoxDelete()
        {
            while (HasInSelectionBox())
            {
                DeleteItems();
            }

            selectionBox.ClearSelected();
        }

        private void DeleteItems()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _point = points[_i];
                if (_point.InSelectionBox)
                {
                    so.Collection.RemoveWithID(_point.ID);
                    return;
                }

                int _pathCount = _point.Paths.Count;
                for (int _j = 0; _j < _pathCount; _j++)
                {
                    Path _path = _point.Paths[_j];
                    if (_path.InSelectionBox)
                    {
                        _point.RemoveWithID(_path.ID);
                        return;
                    }

                    int _pathPointCount = _path.Points.Count;
                    for (int _k = 0; _k < _pathPointCount; _k++)
                    {
                        _path.Points[_k].IsSelected = false;
                        if (_path.Points[_k].InSelectionBox)
                        {
                            _path.RemoveWithID(_path.Points[_k].ID);
                            return;
                        }
                    }
                }
            }
        }

        private bool HasInSelectionBox()
        {
            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _point = points[_i];
                if(_point.InSelectionBox)
                {
                    return true;
                }

                int _pathCount = _point.Paths.Count;
                for (int _j = 0; _j < _pathCount; _j++)
                {
                    Path _path = _point.Paths[_j];
                    if (_path.InSelectionBox)
                    {
                        return true;
                    }

                    int _pathPointCount = _path.Points.Count;
                    for (int _k = 0; _k < _pathPointCount; _k++)
                    {
                        if (_path.Points[_k].InSelectionBox)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion Selection Box

        ////////////////////////////////////////////////////////////////////////

        #region Mouse

        private bool IsOverInterface(Vector2 _mousePosition)
        {
            return rectDistanceBox.Contains(_mousePosition) || 
                rectSelectedBox.Contains(_mousePosition) ||
                rectHoveredBox.Contains(_mousePosition) ||
                IsOverControlInterface(_mousePosition);
        }

        protected override bool IsMouseOverPriorityUI(Vector2 _mousePosition)
        {
            // If point is not null, mouse is being held down over it and it's being dragged
            return spawnpointID != -1 || pathID != -1 || pathpointID != -1 || IsOverInterface(_mousePosition);
        }

        /// <summary>
        /// Deselect any selected spawn point, path or path point when left
        /// mouse button clicked outside of identities rect
        /// </summary>
        /// <param name="_mousePosition">Mouse position.</param>
        protected override void OnLeftMouseButtonReleased(Vector2 _mousePosition)
        {
            if(!IsMouseOverPriorityUI(_mousePosition))
            {
                DeselectAll();
            }
        }

        protected override void OnLeftMouseButtonDown(Vector2 _mousePosition)
        {
            if (IsMouseOverPriorityUI(_mousePosition))
            {
                return;
            }

            if(!isShiftHeldDown)
            {
                selectionBox.ClearSelected();
            }

            DeselectAll();

            int _count = so.Collection.GetCount();
            List<SpawnPoint> points = so.Collection.Points;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                SpawnPoint _point = points[_i];
                int _pathCount = _point.Paths.Count;
                for (int _j = _pathCount - 1; _j >= 0; _j--)
                {
                    Path _path = _point.Paths[_j];

                    // All path so.Collection.Points in a path
                    int _pathPointCount = _path.Points.Count;
                    for (int _k = _pathPointCount - 1; _k >= 0; _k--)
                    {
                        if (_path.Points[_k].MouseOverRect(_mousePosition))
                        {
                            _path.Points[_k].IsSelected = true;
                            _path.IsSelected = true;
                            _point.IsSelected = true;
                            OnSelectIdentity(_path.Points[_k], _mousePosition);
                            return;
                        }
                    }

                    if (_path.MouseOverRect(_mousePosition))
                    {
                        _path.IsSelected = true;
                        _point.IsSelected = true;
                        OnSelectIdentity(_path, _mousePosition);
                        return;
                    }
                }

                if (_point.MouseOverRect(_mousePosition))
                {
                    // Can't add if there's no paths selected
                    if(identityUIState == IdentityUIState.AddPathPoint)
                    {
                        identityUIState = IdentityUIState.None;
                    }

                    _point.IsSelected = true;
                    OnSelectIdentity(_point, _mousePosition);
                    return;
                }
            }

            // Reset to zero if none selected
            SetMouseDistanceToSelected(Vector2.zero, Vector2.zero, Vector2.zero);
        }

        protected override void OnRightMouseButtonReleased(Vector2 _mousePosition)
        {
            switch (identityUIState)
            {
                case IdentityUIState.None:
                    break;
                case IdentityUIState.AddSpawnPoint:
                    AddSpawnPoint(_mousePosition);
                    break;
                case IdentityUIState.AddPath:
                    AddPath(_mousePosition);
                    break;
                case IdentityUIState.AddPathPoint:
                    AddPathPoint(_mousePosition);
                    break;
                case IdentityUIState.Remove:
                    RemoveIdentity(_mousePosition);
                    break;
                case IdentityUIState.MeasureDistance:
                    OnMeasureDistance(_mousePosition);
                    break;
            }
        }

        /// <summary>
        /// Identity was selected.
        /// </summary>
        /// <param name="_clickedIdentity">Clicked identity.</param>
        /// <param name="_mousePosition">Mouse position.</param>
        private void OnSelectIdentity(Identity _clickedIdentity, Vector2 _mousePosition)
        {
            SetMouseDistanceToSelected(_clickedIdentity.RectPosition, _mousePosition, _clickedIdentity.GetHandleRectSize());
        }

        #endregion Mouse

        ////////////////////////////////////////////////////////////////////////

        #region Keyboard

        protected override void OnKeyboardKeyReleased(Vector2 _mousePosition, KeyCode _keyCode)
        {
            base.OnKeyboardKeyReleased(_mousePosition, _keyCode);

            if(_keyCode == so.KeyCodeAddSpawnPoint)
            {
                AddSpawnPoint(_mousePosition);
            }

            if (_keyCode == so.KeyCodeAddPath)
            {
                AddPath(_mousePosition);
            }

            if (_keyCode == so.KeyCodeAddPathPoint)
            {
                AddPathPoint(_mousePosition);
            }

            if (_keyCode == so.KeyCodeDelete)
            {
                RemoveIdentity(_mousePosition);

                SelectionBoxDelete();
            }
        }

        #endregion Keyboard

        ////////////////////////////////////////////////////////////////////////

        #region Editor Reload

        public void OnBeforeSerialize()
        {
            //
        }

        /// <summary>
        /// Reset identityUIState to none if something is missing
        /// </summary>
        public void OnAfterDeserialize()
        {
            if(so == null || !so)
            {
                return;
            }

            if(so.Collection == null)
            {
                return;
            }

            SpawnPoint _spawnPoint = so.Collection.GetSelected();
            if(_spawnPoint != null)
            {
                Path _path = _spawnPoint.GetSelected();
                if(_path != null)
                {

                }

                else if (identityUIState == IdentityUIState.AddPathPoint)
                {
                    identityUIState = IdentityUIState.None;
                }
            }

            else if(identityUIState == IdentityUIState.AddPath)
            {
                identityUIState = IdentityUIState.None;
            }
        }

        #endregion Editor Reload

        ////////////////////////////////////////////////////////////////////////
    }
}
