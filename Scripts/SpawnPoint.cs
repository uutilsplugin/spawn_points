using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    [System.Serializable]
    public class SpawnPoint : Point
    {
        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// Should the point be folded out in the inspector
        /// </summary>
        public bool Foldout { get; set; }

        /// <summary>
        /// Should paths be showing in the inspector
        /// </summary>
        public bool FoldoutPaths { get; set; }

        /// <summary>
        /// Scene view handles line color.
        /// Connects spawn point with its paths.
        /// </summary>
        public Color ColorGizmo = Color.green;

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
        /// Should move only the spawn point when dragging it in scene view or graph.
        /// True will not affect path points positions.
        /// </summary>
        public bool MoveOnlySelf = true;

        /// <summary>
        /// Graph handle size for a spawn point
        /// </summary>
        private static Vector2 rectSizeOverride = new Vector2(25, 25);

        /// <summary>
        /// Graph handle color for a spawn point
        /// </summary>
        private static Color colorOverride = Color.red;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Spawn Point Variables

        /// <summary>
        /// List of all paths
        /// </summary>
        public List<Path> Paths = new List<Path>();

        #endregion Spawn Point Variables

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public SpawnPoint(int _id)
        {
            base.id = _id;
            base.Name = "New_Spawn_Point" + "_" + _id;
            base.identityType = IdentityType.SpawnPoint;
        }

        public SpawnPoint(Vector3 position, int _id)
        {
            base.id = _id;
            base.Name = Name + "_" + id;
            base.Position = position;
            base.identityType = IdentityType.SpawnPoint;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Editor

        #if UNITY_EDITOR

        /// <summary>
        /// Moves every path point with spawn point when a spawn point is moved.
        /// </summary>
        public void OnSpawnPointUpdatePosition(Vector3 newPosition)
        {
            Vector3 direction = newPosition - Position;
            base.Position = newPosition;

            for (int i = 0; i < Paths.Count; i++)
            {
                Paths[i].OnUpdateDirection(direction);
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
        /// Get the selected path in graph editor.
        /// </summary>
        /// <returns>The selected.</returns>
        public Path GetSelected()
        {
            return Paths.FirstOrDefault(x => x.IsSelected);
        }

        /// <summary>
        /// Check if there's a selected path
        /// </summary>
        public bool HasSelected()
        {
            return Paths.Any(x => x.IsSelected);
        }

        #endif

        #endregion Editor

        ////////////////////////////////////////////////////////////////////////

        #region Get Remove Add

        /// <summary>
        /// Get a unique path id
        /// </summary>
        public int GetUniquePathID()
        {
            if (Paths.Count == 0)
            {
                return 0;
            }

            return Paths.Max(x => x.ID) + 1;
        }

        /// <summary>
        /// Get a path with ID.
        /// </summary>
        public Path GetPathWithID(int id)
        {
            return Paths.FirstOrDefault(x => x.ID == id);
        }

        /// <summary>
        /// Remove a path with ID.
        /// </summary>
        /// <param name="_id">Identifier.</param>
        public void RemoveWithID(int _id)
        {
            int _count = Paths.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                if (Paths[_i].ID == _id)
                {
                    Paths.RemoveAt(_i);
                    return;
                }
            }
        }

        /// <summary>
        /// Add a path at position and return it.
        /// </summary>
        public Path Add(Vector3 _position)
        {
            Path _path = new Path(GetUniquePathID(), _position);
            Paths.Add(_path);
            return _path;
        }

        #endregion Get Remove Add

        ////////////////////////////////////////////////////////////////////////
    }
}
