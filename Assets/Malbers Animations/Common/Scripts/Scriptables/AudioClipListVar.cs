﻿using UnityEngine; 

namespace MalbersAnimations.Scriptables
{
    ///<summary> Store a list of Materials</summary>
    [CreateAssetMenu(menuName = "Malbers Animations/Collections/Audio Clip Set", order = 1000)]
    public class AudioClipListVar : ScriptableList<AudioClip>
    {

        [MinMaxRange(-3, 3)]
        public RangedFloat pitch = new(1, 1);
        [MinMaxRange(0, 1)]
        public RangedFloat volume = new(1, 1);

        [Range(0, 1)]
        public float SpatialBlend = 1;

        public void Play(AudioSource source)
        {
            var clip = Item_GetRandom();
            source.clip = clip;
            source.pitch = pitch.RandomValue;
            source.volume = volume.RandomValue;
            source.spatialBlend = SpatialBlend;
            source.Play();
        }


        public void Play()
        {
            var NewGO = new GameObject() { name = "Audio [" + this.name +"]"};
            var source = NewGO.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            var clip = Item_GetRandom();
            source.clip = clip;
            source.pitch = pitch.RandomValue;
            source.volume = volume.RandomValue;
            source.spatialBlend = SpatialBlend;
            source.Play();
        }

        void Reset() => Description = "Store a Collection of AudioClip";
    }

    [System.Serializable]
    public class AudioClipReference : ReferenceVar
    {
        public AudioClip ConstantValue;
        [RequiredField] public AudioClipListVar Variable;

        public AudioClip Value => UseConstant ? ConstantValue : (Variable != null ? Variable.Item_GetRandom() : null);

        /// <summary>Check if the Audio Clip list var is not empty or Null </summary>
        public bool NullOrEmpty() => UseConstant ? (ConstantValue == null) : (Variable == null);

        public void Play(AudioSource source)
        {
            if (source == null || !source.isActiveAndEnabled) return;

            if (UseConstant)
            {
                source.clip = ConstantValue;
                source.Play();
            }
            else
            {
                Variable.Play(source);
            }
        }
    }
}