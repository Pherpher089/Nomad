 
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations
{
    public class MalbersDefineSymbol : EditorWindow
    {
        /// <summary> Symbol that will be added to the editor  </summary>
        private static string _EditorSymbol = "MALBERS_DEBUG";


        [MenuItem("Tools/Malbers Animations/Debug Gizmos [On]", false, 8000)]
        public static void DebugGizmosOn()
        {
            string lSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!lSymbols.Split(';').Contains(_EditorSymbol))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, lSymbols + ";" + _EditorSymbol);
            }
            Debug.Log($"Scripting Define Symbol Added: [{_EditorSymbol}]");
        }

        [MenuItem("Tools/Malbers Animations/Debug Gizmos [Off]", false, 8000)]
        public static void DebugGizmosOff()
        {
            string lSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (lSymbols.Split(';').Contains(_EditorSymbol))
            {
                lSymbols = lSymbols.Replace($";{_EditorSymbol}", "");

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, lSymbols);

            }
            Debug.Log($"Scripting Define Symbol Removed: [{_EditorSymbol}]");
        }
    }
}
#endif