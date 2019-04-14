using UnityEngine;

namespace UUtils.SpawnPoints
{
    [CreateAssetMenu(fileName = "Spawn_Point_Collection_", menuName = "UUtils/Spawn Point Collection")]
    public class SpawnPointCollectionSO : ScriptableObject
    {
        ////////////////////////////////////////////////////////////////////////

        #region List

        [HideInInspector]
        public SpawnPointCollection Collection;

        /// <summary>
        /// When pressed, will add a point to the toggled path.
        /// </summary>
        [HideInInspector]
        public KeyCode KeyCodeAddSpawnPoint = KeyCode.B;

        /// <summary>
        /// When pressed, toggles keyboard controls for the next path in the current spawnpoint
        /// </summary>
        [HideInInspector]
        public KeyCode KeyCodeAddPath = KeyCode.N;

        /// <summary>
        /// When pressed, will add a point to the toggled path
        /// </summary>
        [HideInInspector]
        public KeyCode KeyCodeAddPathPoint = KeyCode.M;

        /// <summary>
        /// When pressed, will delete a spawn point, path or path point.
        /// ONLY works in graph window.
        /// </summary>
        [HideInInspector]
        public KeyCode KeyCodeDelete = KeyCode.Delete;

        #endregion List

        ////////////////////////////////////////////////////////////////////////
    }
}
