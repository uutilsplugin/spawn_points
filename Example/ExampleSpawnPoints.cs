using System.Collections.Generic;
using UnityEngine;

namespace UUtils.SpawnPoints
{
    public class ExampleSpawnPoints : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        [SerializeField]
        private SpawnPointCollectionSO collectionSO;

        [Tooltip("Game object prefab which is spawned to a path point location")]
        [SerializeField]
        private GameObject prefab;

        [Tooltip("Holds instantiated prefab clones")]
        [SerializeField]
        private Transform container;

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Methods

        /// <summary>
        /// Instantiate the prefab to all path points
        /// </summary>
        public void InstantiateToAllPositions()
        {
            if(!CanInstantiate())
            {
                return;
            }

            ClearContainer();
        
            List<SpawnPoint> _points = collectionSO.Collection.Points;
            int _count = _points.Count;
            for (int _i = 0; _i < _count; _i++)
            {
                SpawnPoint _spawnPoint = _points[_i];
                int _pathCount = _spawnPoint.Paths.Count;
                for (int _j = 0; _j < _pathCount; _j++)
                {
                    Path _path = _spawnPoint.Paths[_j];
                    int _pointsCount = _path.Count;
                    for (int _k = 0; _k < _pointsCount; _k++)
                    {
                        PathPoint _point = _path.Points[_k];
                        GameObject _go = Instantiate(prefab, container);
                        _go.transform.position = _point.Position;
                        _go.transform.rotation = _point.Quaternion;
                        _go.name = "Instantiated_" + _point.Name;
                    }
                }
            }
        }

        private bool CanInstantiate()
        {
            if(prefab == null || !prefab)
            {
                Debug.LogError("Prefab missing. Can't instantiate!");
                return false;
            }

            if (container == null || !container)
            {
                Debug.LogError("Container missing. Can't instantiate!");
                return false;
            }

            return true;
        }

        public void ClearContainer()
        {
            int _count = container.childCount;
            for (int _i = _count - 1; _i >= 0; _i--)
            {
                DestroyImmediate(container.GetChild(_i).gameObject);
            }
        }

        #endregion Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
