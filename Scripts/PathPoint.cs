using UnityEngine;
using UUtils.Utilities;

namespace UUtils.SpawnPoints
{
    [System.Serializable]
    public class PathPoint : Point
    {
        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public PathPoint(int _id)
        {
            base.id = _id;
            base.Name = Name + "_" + id;
            base.identityType = IdentityType.PathPoint;
        }

        public PathPoint(Vector3 _position, int _id)
        {
            base.id = _id;
            base.Name = Name + "_" + id;
            base.Position = _position;
            base.identityType = IdentityType.PathPoint;
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Editor Vars

        #if UNITY_EDITOR

        /// <summary>
        /// Graph handle size for a path point
        /// </summary>
        private static Vector2 rectSizeOverride = new Vector2(8, 8);

        /// <summary>
        /// Graph handle color for a path point
        /// </summary>
        private static Color colorOverride = Color.magenta;

        #endif

        #endregion Editor Vars

        ////////////////////////////////////////////////////////////////////////

        #region Editor Methods

        #if UNITY_EDITOR

        /// <summary>
        /// Moves a point along a direction.
        /// Used when moving a spawn point.
        /// </summary>
        public void OnPointUpdatePosition(Vector3 _direction)
        {
            base.Position += _direction;
        }

        public override Vector2 GetHandleRectSize()
        {
            return rectSizeOverride;
        }

        public override Color GetDefaultColor()
        {
            return colorOverride;
        }

        #endif

        #endregion Editor Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
