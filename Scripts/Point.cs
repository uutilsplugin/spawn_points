using UnityEngine;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    [System.Serializable]
    public class Point : Identity
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        public Vector3 Position = new Vector3(0, 0, 0);

        public Vector3 Rotation = new Vector3(0, 0, 0);

        public Quaternion Quaternion
        {
            get
            {
                Quaternion _quaterion = new Quaternion();
                _quaterion.eulerAngles = Rotation;
                return _quaterion;
            }
        }

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// Is this point displaying the preview mesh
        /// </summary>
        public bool IsDisplayingMesh;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor Methods

        #if UNITY_EDITOR

        public void UpdatePosition(Vector3 _position)
        {
            Position = _position;
        }

        public void UpdateDirection(Vector3 _direction)
        {
            Position += _direction;
        }

        #endif

        #endregion Editor Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
