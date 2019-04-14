using UnityEngine;
using UnityEditor;
using System;

namespace UUtils.Utilities
{
    [Serializable]
    public class EditorZoom
    {
        ////////////////////////////////////////////////////////////////////////

        #region Toolbar Buttons

        public static GUIContent ContentZoomIn = new GUIContent("+", "Zoom in.");

        public static GUIContent ContentZoomOut = new GUIContent("-", "Zoom out.");

        #endregion Toolbar Buttons

        ////////////////////////////////////////////////////////////////////////

        #region Zoom

        private const int zoomMultiplier = 10;
        public int ZoomMultiplier { get { return zoomMultiplier; } }

        [SerializeField]
        protected float zoomValue;
        /// <summary>
        /// Real zoom amount. How many times was a graph zoomed OUT.
        /// </summary>
        public float ZoomValue { get { return zoomValue; }}

        [SerializeField]
        protected float zoomTotal = 2;
        /// <summary>
        /// Total times zoomed out
        /// </summary>
        public float ZoomTotal { get { return zoomTotal; } }

        [SerializeField]
        protected int zoomTotalMax = 8;
        /// <summary>
        /// Maximum zoom level
        /// </summary>
        public int ZoomTotalMax { get { return zoomTotalMax; } }

        [SerializeField]
        protected int zoomLabelDisplaySkip = 5; 
        /// <summary>
        /// Starts skiping every other unit label if zoomTotal reaches this level
        /// </summary>
        public int ZoomLabelDisplaySkip { get { return zoomLabelDisplaySkip; } }

        /// <summary>
        /// Mouse scroll zoom amount
        /// </summary>
        private const float scrollWheelZoom = 0.02f;
        public float ScrollWheelZoom { get { return scrollWheelZoom; } }

        #endregion Zoom

        ////////////////////////////////////////////////////////////////////////

        #region Constructor

        public EditorZoom()
        {
            UpdateZoom();
        }

        #endregion Constructor

        ////////////////////////////////////////////////////////////////////////

        #region Methods

        /// <summary>
        /// Zoom in
        /// </summary>
        public void ZoomIn()
        {
            if (zoomValue > 1)
            {
                zoomTotal--;
                UpdateZoom();
            }
        }

        /// <summary>
        /// Zoom in
        /// </summary>
        public void OnMouseScrollZoom(int _direction)
        {
            zoomTotal = Mathf.Clamp(zoomTotal + scrollWheelZoom * _direction, 1, zoomTotalMax);
            UpdateZoom();
        }

        /// <summary>
        /// Zoom out
        /// </summary>
        public void ZoomOut()
        {
            if (zoomTotal < zoomTotalMax)
            {
                zoomTotal++;
                UpdateZoom();
            }
        }

        private void UpdateZoom()
        {
            zoomValue = Mathf.Pow(zoomMultiplier, zoomTotal);
        }

        #endregion Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
