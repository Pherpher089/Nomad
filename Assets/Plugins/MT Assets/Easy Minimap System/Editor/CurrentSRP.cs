using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace MTAssets.EasyMinimapSystem.Editor
{
    /*
     * This class is responsible for detecting if Unity Project have another SRPs.
    */

    [InitializeOnLoad]
    public class CurrentSRP
    {
        //Variable of built RP in detector 
        public static ListRequest requestListAllPackages;

        static CurrentSRP()
        {
            //Run the checker (unregister automatically after get list of packages)
            if (requestListAllPackages == null)
                requestListAllPackages = Client.List();
            EditorApplication.update += VerifyIfHaveAnotherRenderPipelinePackage;
        }

        public static void VerifyIfHaveAnotherRenderPipelinePackage()
        {
            //If request is done
            if (requestListAllPackages.IsCompleted == true)
            {
                bool have = false;
                string packageName = "";

                //Scan all packages, and if is using BuiltIn Render Pipeline, return true
                foreach (UnityEditor.PackageManager.PackageInfo package in requestListAllPackages.Result)
                {
                    if (package.name.Contains("render-pipelines.universal"))
                    {
                        have = true;
                        packageName += (packageName.Length > 0) ? " | URP" : "URP";
                    }
                    if (package.name.Contains("render-pipelines.high-definition"))
                    {
                        have = true;
                        packageName += (packageName.Length > 0) ? " | HDRP" : "HDRP";
                    }
                    if (package.name.Contains("render-pipelines.lightweight"))
                    {
                        have = true;
                        packageName += (packageName.Length > 0) ? " | Lightweight" : "Lightweight";
                    }
                }

                //Unregister this method from Editor update
                EditorApplication.update -= VerifyIfHaveAnotherRenderPipelinePackage;

                //Apply all changes in Defined Symbols
                ApplyChanges(have, packageName);
            }
        }

        public static void ApplyChanges(bool haveAnotherRenderPipelinePackage, string renderPipelinePackageName)
        {
            //Get active build target
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            //If is not using any RP, add BIRP define symbol
            if (haveAnotherRenderPipelinePackage == false)
            {
                AddDefineIfNecessary("EMS_BIRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                RemoveDefineIfNecessary("EMS_URP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                RemoveDefineIfNecessary("EMS_HDRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
            }

            //If is using other RP...
            if (haveAnotherRenderPipelinePackage == true)
            {
                //If is URP
                if (renderPipelinePackageName.Contains("URP") == true)
                {
                    RemoveDefineIfNecessary("EMS_BIRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                    AddDefineIfNecessary("EMS_URP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                    RemoveDefineIfNecessary("EMS_HDRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                }

                //If is HDRP
                if (renderPipelinePackageName.Contains("HDRP") == true)
                {
                    RemoveDefineIfNecessary("EMS_BIRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                    RemoveDefineIfNecessary("EMS_URP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                    AddDefineIfNecessary("EMS_HDRP_On", BuildPipeline.GetBuildTargetGroup(buildTarget));
                }
            }
        }

        public static void AddDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
#if UNITY_2023_1_OR_NEWER
            var defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup));
#endif
#if !UNITY_2023_1_OR_NEWER
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);
#endif

            if (defines == null) { defines = _define; }
            else if (defines.Length == 0) { defines = _define; }
            else { if (defines.IndexOf(_define, 0) < 0) { defines += ";" + _define; } }

#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup), defines);
#endif
#if !UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
#endif
        }

        public static void RemoveDefineIfNecessary(string _define, BuildTargetGroup _buildTargetGroup)
        {
#if UNITY_2023_1_OR_NEWER
            var defines = PlayerSettings.GetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup));
#endif
#if !UNITY_2023_1_OR_NEWER
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(_buildTargetGroup);
#endif

            if (defines.StartsWith(_define + ";"))
            {
                // First of multiple defines.
                defines = defines.Remove(0, _define.Length + 1);
            }
            else if (defines.StartsWith(_define))
            {
                // The only define.
                defines = defines.Remove(0, _define.Length);
            }
            else if (defines.EndsWith(";" + _define))
            {
                // Last of multiple defines.
                defines = defines.Remove(defines.Length - _define.Length - 1, _define.Length + 1);
            }
            else
            {
                // Somewhere in the middle or not defined.
                var index = defines.IndexOf(_define, 0, System.StringComparison.Ordinal);
                if (index >= 0) { defines = defines.Remove(index, _define.Length + 1); }
            }

#if UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(_buildTargetGroup), defines);
#endif
#if !UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbolsForGroup(_buildTargetGroup, defines);
#endif
        }
    }
}