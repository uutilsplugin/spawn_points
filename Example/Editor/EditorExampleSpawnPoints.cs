using UnityEngine;
using UnityEditor;

namespace UUtils.SpawnPoints
{
    [CustomEditor(typeof(ExampleSpawnPoints))]
    public class EditorExampleSpawnPoints : Editor
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        private ExampleSpawnPoints exampleSpawnPoints;

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Methods

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if(exampleSpawnPoints == null || !exampleSpawnPoints)
            {
                exampleSpawnPoints = (ExampleSpawnPoints)target;
                if (exampleSpawnPoints == null || !exampleSpawnPoints)
                {
                    return;
                }
            }

            if(GUILayout.Button("Instantiate"))
            {
                exampleSpawnPoints.InstantiateToAllPositions();
            }

            if (GUILayout.Button("Clear Container"))
            {
                exampleSpawnPoints.ClearContainer();
            }
        }

        #endregion Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
