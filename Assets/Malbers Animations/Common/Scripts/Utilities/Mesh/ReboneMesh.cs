using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Rebone Mesh")]
    public class ReboneMesh : MonoBehaviour
    {

        //[ContextMenuItem("Transfer Bones From Skin", "DuplicateBones")]
        //public GameObject _sourceSkinMesh;

        [ContextMenuItem("Transfer Bones From Root", "TransferRootBone")]
        public Transform RootBone;


        [ContextMenu("Transfer Bones From Root")]
        public void TransferRootBone()
        {
            if (RootBone != null)
            {
                CopyBonesSameBones();
            }
        }

        private void CopyBonesSameBones()
        {
            if (TryGetComponent<SkinnedMeshRenderer>(out var thisRenderer))
            {
                var OldRootBone = thisRenderer.rootBone;

                Transform[] rootBone = RootBone.GetComponentsInChildren<Transform>();

                Dictionary<string, Transform> boneMap = new();

                foreach (Transform bone in rootBone)
                {
                    boneMap[bone.name] = bone;
                }

                Transform[] boneArray = thisRenderer.bones;


                for (int idx = 0; idx < boneArray.Length; ++idx)
                {
                    string boneName = boneArray[idx].name;

                    if (false == boneMap.TryGetValue(boneName, out boneArray[idx]))
                    {
                        Debug.LogError("failed to get bone: " + boneName);
                    }
                }
                thisRenderer.bones = boneArray;

                if (boneMap.TryGetValue(OldRootBone.name, out Transform newRoot))
                {
                    thisRenderer.rootBone = newRoot; //Remap the rootbone
                }

                Debug.Log($"Bone Trasfer Completed: {name}");
            }
        }

        private void Reset()
        {
            if (RootBone == null)
            {
                var AllSkinMeshes = transform.root.GetComponentsInChildren<SkinnedMeshRenderer>(true);

                var thisRenderer = GetComponent<SkinnedMeshRenderer>();

                SkinnedMeshRenderer Old = AllSkinMeshes.ToList().Find(x => x.name == name && x != thisRenderer);
                 
                if ( Old != null)
                {
                    RootBone = Old.rootBone;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ReboneMesh)), CanEditMultipleObjects]
    public class ReboneMeshEd : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Retarget Bones"))
            {
                foreach (var targ in targets)
                {
                    (targ as ReboneMesh).TransferRootBone();
                    EditorUtility.SetDirty(targ);
                }
            }
        }
    }
#endif
}