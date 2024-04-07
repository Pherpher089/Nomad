﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations.Utilities
{
    [AddComponentMenu("Malbers/Utilities/Mesh/Material Lerp")]

    public class MaterialLerp : MonoBehaviour
    {
        public bool LerpOnEnable = true;
        public List<InternalMaterialLerp> materials;



        private void OnEnable()
        {
            if (LerpOnEnable) Lerp();
        }

        public virtual void Lerp() => StartCoroutine(Lerper());



        IEnumerator Lerper()
        {
            //float elapsedTime = 0;

            //var rendererMaterials = new List<Material>();

            //foreach (var item in materials)
            //{
            //    rendererMaterials.Add(item.sharedMaterials[materialIndex]);   //get the Material from the renderer)
            //}



            //while (elapsedTime <= time.Value)
            //{
            //    float value = curve.Evaluate(elapsedTime / time);

            //    mesh.material.Lerp(rendererMaterial, ToMaterial, value);
            //    elapsedTime += Time.deltaTime;

            //    // Debug.Log("value = " + value.ToString("F2"));
            //    yield return null;
            //}

            //mesh.material.Lerp(rendererMaterial, ToMaterial, curve.Evaluate(1f));
            yield return null;
        }
    }

    [System.Serializable]
    public class InternalMaterialLerp
    {
        public Renderer renderer;
        [ExposeScriptableAsset] public MaterialLerpSO materials;
    }
}

