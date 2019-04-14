using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    /// <summary>
    /// Contains a list of points in a path
    /// </summary>
    [System.Serializable]
    public class Path : Identity
    {
        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// EDITOR USE ONLY. Points can be added using keyboard controls.
        /// </summary>
        public bool IsKeyboardEnabled { get; set; }

        public Color PathColor = Color.yellow;

        /// <summary>
        /// Display connection lines with other points and preview object.
        /// </summary>
        public bool IsDisplayingGizmo = true;

        /// <summary>
        /// Display label in scene view. 
        /// Will not be displayed if IsDisplayingGizmo = false.
        /// </summary>
        public bool IsDisplayingLabel;

        /// <summary>
        /// Should points be folded or unfolded when showing in inspector
        /// </summary>
        public bool Foldout;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Vars

        public List<PathPoint> Points = new List<PathPoint>();

        public int Count
        {
            get
            {
                return Points == null ? 0 : Points.Count;
            }
        }

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        /// <summary>
        /// Adds spawnpoints position as the first PathPoint
        /// </summary>
        public Path(int _id, Vector3 _position)
        {
            base.id = _id;
            base.Name = "Path_" + id;
            base.identityType = IdentityType.Path;

            // points count is zero at creation, hence passing zero to the new PathPoint
            Points.Add(new PathPoint(_position, Count));
        }

        public Path(string _name, Vector3 _position)
        {
            base.Name = _name;
            Points.Add(new PathPoint(_position, Count));
            base.identityType = IdentityType.Path;
        }

        public Path(string name, List<PathPoint> _points)
        {
            base.Name = name;
            Points = _points;
            base.identityType = IdentityType.Path;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// Graph handle size for a path
        /// </summary>
        private static Vector2 rectSizeOverride = new Vector2(18, 18);

        /// <summary>
        /// Graph handle color for a path
        /// </summary>
        private static Color colorOverride = Color.green;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor

        #if UNITY_EDITOR

        public Vector3 GetZeroPointPosition()
        {
            if(Count > 0)
            {
                return Points[0].Position;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Moves all points in a path along a direction
        /// </summary>
        /// <param name="_newPosition">Position of the first point in the path.</param>
        public void OnPathUpdatePosition(Vector3 _newPosition)
        {
            if(Count == 0)
            {
                return;
            }

            // Paths current position is ALWAYS first point in the path
            Vector3 _currentPosition = Points[0].Position;

            int count = Count;
            for (int i = 0; i < count; i++)
            {
                Points[i].OnPointUpdatePosition(_newPosition - _currentPosition);
            }
        }

        public void OnUpdateDirection(Vector3 _direction)
        {
            int count = Count;
            for (int i = 0; i < count; i++)
            {
                Points[i].OnPointUpdatePosition(_direction);
            }
        }

        public override Vector2 GetHandleRectSize()
        {
            return rectSizeOverride;
        }

        public override Color GetDefaultColor()
        {
            return colorOverride;
        }

        /// <summary>
        /// Get the selected path point in graph editor.
        /// </summary>
        /// <returns>The selected.</returns>
        public PathPoint GetSelected()
        {
            return Points.FirstOrDefault(x => x.IsSelected);
        }

        /// <summary>
        /// Check if there's a selected path point
        /// </summary>
        public bool HasSelected()
        {
            return Points.Any(x => x.IsSelected);
        }

        #endif

        #endregion Editor

        //////////////////////////////////////////////////////////////////////////////

        #region Remove Add

        /// <summary>
        /// Remove path point with id.
        /// </summary>
        public void RemoveWithID(int _id)
        {
            int _count = Count;
            for (int _i = 0; _i < _count; _i++)
            {
                if(Points[_i].ID == _id)
                {
                    Points.RemoveAt(_i);
                    return;
                }
            }
        }
        public int GetUniqueID()
        {
            if (Count == 0)
            {
                return 0;
            }

            return Points.Max(x => x.ID) + 1;
        }

        /// <summary>
        /// Add path point to the path at the end of the path
        /// </summary>
        /// <param name="_position">Point position.</param>
        public PathPoint Add(Vector3 _position)
        {
            PathPoint _point = new PathPoint(_position, GetUniqueID());
            Points.Add(_point);
            return _point;
        }

        /// <summary>
        /// Insert a new point after the selected one
        /// </summary>
        /// <returns>The after.</returns>
        /// <param name="_position">Position.</param>
        /// <param name="_pointID">ID of the point after which a new one is inserted into the point list.</param>
        public PathPoint InsertAfter(Vector3 _position, int _pointID)
        {
            int _count = Count;
            for (int _i = 0; _i < _count; _i++)
            {
                if(Points[_i].ID == _pointID)
                {
                    PathPoint _point = new PathPoint(_position, GetUniqueID());
                    Points.Insert(_i + 1, _point);
                    return _point;
                }
            }

            return null;
        }

        #endregion Remove Add

        //////////////////////////////////////////////////////////////////////////////
    }
}
