

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MalbersAnimations
{
    public class MalbersMenu : EditorWindow
    {
        const string URP16_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_16.unitypackage";
        const string URP14_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_URP_14.unitypackage";
        const string HRP_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_HDRP_15.unitypackage";
        const string D_Shader_Path = "Assets/Malbers Animations/Common/Shaders/Malbers_Standard.unitypackage";
        const string D_Cinemachine3_Path = "Assets/Malbers Animations/Common/Cinemachine/Cinemachine3.unitypackage";


        [MenuItem("Tools/Malbers Animations/Malbers URP v.16 Shaders", false, 2)]
        public static void UpgradeMaterialsURP16() => AssetDatabase.ImportPackage(URP16_Shader_Path, true);

        [MenuItem("Tools/Malbers Animations/Malbers URP v.14 Shaders", false, 1)]
        public static void UpgradeMaterialsURP14() => AssetDatabase.ImportPackage(URP14_Shader_Path, true);


        [MenuItem("Tools/Malbers Animations/Malbers HDRP v.15 Shaders", false, 3)]
        public static void UpgradeMaterialsHRPL2022() => AssetDatabase.ImportPackage(HRP_Shader_Path, true);


        [MenuItem("Tools/Malbers Animations/Malbers Standard Shaders", false, 1)]
        public static void UpgradeMaterialsStandard() => AssetDatabase.ImportPackage(D_Shader_Path, true);


        [MenuItem("Tools/Malbers Animations/Install Cinemachine 3", false, 600)]
        public static void InstallCM3()
        {
            AssetDatabase.ImportPackage(D_Cinemachine3_Path, true);
        }


        [MenuItem("Tools/Malbers Animations/Create Test Scene (Steve)", false, 100)]
        public static void CreateSampleSceneSteve()
        {
            CreateTestPlane("Assets/Malbers Animations/Animal Controller/Human/Steve.prefab");
        }


        [MenuItem("Tools/Malbers Animations/Create Test Scene (Wolf)", false, 100)]
        public static void CreateSampleSceneWolf()
        {
            CreateTestPlane("Assets/Malbers Animations/Animal Controller/Wolf Lite/Wolf Lite.prefab");
        }


        [MenuItem("Tools/Malbers Animations/Create Test Scene", false, 100)]
        public static void CreateSampleScene()
        {
            var all = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();

            var mainCam = all.Find(x => x.name == "Main Camera");
            if (mainCam)
            { DestroyImmediate(mainCam); }


            var TestPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            TestPlane.transform.localScale = new Vector3(20, 1, 20);
            TestPlane.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Shaders/Ground_20.mat", typeof(Material)) as Material;
            TestPlane.isStatic = true;

            var BrainCam = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Cinemachine/Cameras CM2.prefab", typeof(GameObject)) as GameObject;
            var CMFreeLook = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Cinemachine/Third Person Cinemachine/CM Third Person Main.prefab", typeof(GameObject)) as GameObject;
            // var CMFreeLook = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Cinemachine/CM FreeLook Main.prefab", typeof(GameObject)) as GameObject;
            var WolfLite = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Animal Controller/Wolf Lite/Wolf Lite.prefab", typeof(GameObject)) as GameObject;
            var Steve = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Animal Controller/Human/Steve.prefab", typeof(GameObject)) as GameObject;

            if (BrainCam) PrefabUtility.InstantiatePrefab(BrainCam);
            //if (CMFreeLook) PrefabUtility.InstantiatePrefab(CMFreeLook);
            if (WolfLite) PrefabUtility.InstantiatePrefab(WolfLite);
            if (Steve) PrefabUtility.InstantiatePrefab(Steve);
        }


        public static void CreateTestPlane(string character)
        {
            var all = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();

            var mainCam = all.Find(x => x.name == "Main Camera");
            if (mainCam)
            { DestroyImmediate(mainCam); }


            var TestPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            TestPlane.transform.localScale = new Vector3(20, 1, 20);
            TestPlane.GetComponent<MeshRenderer>().sharedMaterial =
                AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Shaders/Ground_20.mat", typeof(Material)) as Material;
            TestPlane.isStatic = true;

            var cameras = AssetDatabase.LoadAssetAtPath("Assets/Malbers Animations/Common/Cinemachine/Cameras CM2.prefab", typeof(GameObject)) as GameObject;
            var Char_ = AssetDatabase.LoadAssetAtPath(character, typeof(GameObject)) as GameObject;

            if (cameras) PrefabUtility.InstantiatePrefab(cameras);
            if (Char_) PrefabUtility.InstantiatePrefab(Char_);
        }





        [MenuItem("Tools/Malbers Animations/Integrations", false, 600)]
        public static void OpenIntegrations() => Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/annex/integrations");


        [MenuItem("Tools/Malbers Animations/What's New", false, 600)]
        public static void OpenWhatsNew() => Application.OpenURL("https://malbersanimations.gitbook.io/animal-controller/whats-new");


        [MenuItem("Tools/Malbers Animations/Tools/Remove All MonoBehaviours from Selected", false, 500)]
        public static void RemoveMono()
        {
            var allGo = Selection.gameObjects;

            if (allGo != null)
            {
                foreach (var selected in allGo)
                {
                    var AllComponents = selected.GetComponentsInChildren<MonoBehaviour>(true);

                    Debug.Log($"Removed {AllComponents.Length} from {selected}", selected);

                    foreach (var comp in AllComponents)
                    {
                        var t = comp.gameObject;
                        DestroyImmediate(comp);
                        EditorUtility.SetDirty(t);
                    }
                }
            }
        }
    }
}
#endif