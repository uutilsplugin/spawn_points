using System;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UUtils.Utilities
{
    /// <summary>
    /// TODO
    /// </summary>
    public class EditorSettings
    {
        ////////////////////////////////////////////////////////////////////////

        #region Vars

        private Texture2D background;

        #endregion Vars

        ////////////////////////////////////////////////////////////////////////

        #region Methods

        /// <summary>
        /// Create a scriptable object asset in Assets/ folder
        /// </summary>
        /// <param name="_type">SO class</param>
        /// <param name="_path">Use forward slashes and INCLUDE .asset extension</param>
        public void CreateAsset(Type _type, string _path)
        {
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance(_type), "Assets/" + _path);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Full OS path to Editor Default Resources folder
        /// </summary>
        public string GetEditorResourcesPath()
        {
            return Path.Combine(Application.dataPath, "Editor Default Resources");
        }

        #endregion Methods

        ////////////////////////////////////////////////////////////////////////
    }
}
