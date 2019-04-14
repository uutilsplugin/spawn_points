using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UUtils.SpawnPoints
{
    /// <summary>
    /// Container for spawn points.
    /// </summary>
    [System.Serializable]
    public class SpawnPointCollection
    {
        ////////////////////////////////////////////////////////////////////////

        #region Variables

        public string Name = "New_Collection";

        [SerializeField]
        private List<SpawnPoint> _points = new List<SpawnPoint>();
        public List<SpawnPoint> Points { get { return _points; }}

        #endregion Variables

        ////////////////////////////////////////////////////////////////////////

        #region Add

        /// <summary>
        /// Create a new spawn point and add it to the list
        /// </summary>
        public SpawnPoint AddSpawnPoint()
        {
            SpawnPoint _spawnPoint = new SpawnPoint(GetUniqueID());
            Points.Add(_spawnPoint);
            return _spawnPoint;
        }

        /// <summary>
        /// Create a new spawn point at position and add it to the list
        /// </summary>
        /// <param name="_position">Position.</param>
        public SpawnPoint AddSpawnPoint(Vector3 _position)
        {
            SpawnPoint _spawnPoint = new SpawnPoint(_position, GetUniqueID());
            Points.Add(_spawnPoint);
            return _spawnPoint;
        }

        private int GetUniqueID()
        {
            if(GetCount() > 0)
            {
                // Find the point with largest id and add 1 to it
                return Points.Select(x => x.ID).Max() + 1;
            }

            return 0;
        }

        #endregion Add

        ////////////////////////////////////////////////////////////////////////

        #region Remove

        public void RemoveWithID(int _id)
        {
            int _count = Points.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                if (Points[_i].ID == _id)
                {
                    Points.RemoveAt(_i);
                    return;
                }
            }
        }

        public void Remove(int index)
        {
            if(index >= 0 && index < GetCount())
            {
                _points.RemoveAt(index);
            }
        }

        public void Remove()
        {
            int count = GetCount();
            if(count > 0)
            {
                _points.RemoveAt(count - 1);
            }
        }

        #endregion Remove

        ////////////////////////////////////////////////////////////////////////

        #region Get

        /// <summary>
        /// Get a spawn point with its id.
        /// Returns null if point isn't found.
        /// </summary>
        /// <param name="id">Spawn point ID.</param>
        public SpawnPoint GetSpawnPointWithID(int id)
        {
            return Points.FirstOrDefault(x => x.ID == id);
        }

        /// <summary>
        /// Get a spawn point with its name.
        /// Returns null if point isn't found.
        /// </summary>
        /// <param name="name">Spawn point name.</param>
        public SpawnPoint GetSpawnPointWithName(string name)
        {
            return Points.FirstOrDefault(x => x.Name == name);
        }

        public int GetCount()
        {
            return Points.Count;
        }

        #endregion Get

        ////////////////////////////////////////////////////////////////////////

        #region Editor Get

        #if UNITY_EDITOR

        /// <summary>
        /// Get the selected spawn point in graph editor.
        /// </summary>
        /// <returns>The selected.</returns>
        public SpawnPoint GetSelected()
        {
            return _points.FirstOrDefault(x => x.IsSelected);
        }

        /// <summary>
        /// Check if there's a selected spawn point
        /// </summary>
        public bool HasSelected()
        {
            return _points.Any(x => x.IsSelected);
        }

        #endif

        #endregion Editor Get

        ////////////////////////////////////////////////////////////////////////
    }
}
