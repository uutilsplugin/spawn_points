using UnityEngine;

namespace UUtils.Utilities
{
    [CreateAssetMenu(fileName = "SO_Editor_Background", menuName = "UUtils/Editor Background")]
    public class EditorSettingSO : ScriptableObject
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        private Texture2D background;

        public Color BackgroundColor = new Color(70, 70, 70, 1);

        public Color GridSmallColor = new Color(202, 202, 202, 1);

        public Color GridLargeColor = new Color(43, 43, 43, 1);

        public Color SelectionBoxColor = new Color(10, 10, 10, 0.2f);

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////
    }
}
