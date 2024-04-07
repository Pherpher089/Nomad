using MalbersAnimations.Reactions;
using UnityEngine;

namespace MalbersAnimations.Controller
{
    [AddComponentMenu("Malbers/Animal Controller/Ground Speed Changer")]
    public class GroundSpeedChanger : MonoBehaviour
    {
        [Tooltip("Adittional Position added to the Movement on the Floor")]
        public float Position;

        [Tooltip("This will make the ground slippery if the value is very low")]
        public float Lerp = 2f;

        [Tooltip("Slide Override on the Animal Controller")]
        public float SlideAmount = 0.25f;

        [Tooltip("Slide activation using the Max Slope Limit")]
        public float SlideThreshold = 30f;

        [Tooltip("Lerp value to smoothly slide down the ramp")]
        public float SlideDamp = 20f;

        [Tooltip("Values used on the [Slide] State")]
        public SlideData SlideData;

        [Tooltip("Slide activation angle to activate the state. The character needs to be looking at the Slo")]
        public float ActivationAngle = 90;

        [SubclassSelector, SerializeReference]
        public Reaction OnEnter;
        [SubclassSelector, SerializeReference]
        public Reaction OnExit;
    }

    [System.Serializable]
    public struct SlideData
    {
        [Tooltip("If is set to true then this Ground Changer can activate the Slide State on the Animal")]
        public bool Slide;
        
        [Tooltip("If true, then the rotation will be ignored in the Slide State")]
        public bool IgnoreRotation;

        [Tooltip("Minimun Slope Direction Angle to activate the Slide State")]
        [Min(0)]public float MinAngle;
    }
}
