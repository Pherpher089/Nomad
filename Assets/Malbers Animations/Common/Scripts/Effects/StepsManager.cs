using MalbersAnimations.Scriptables;
using UnityEngine;

namespace MalbersAnimations
{
    /// <summary> This will manage the steps sounds and tracks for each animal, on each feet there's a Script StepTriger (Basic)  </summary>
    [AddComponentMenu("Malbers/Utilities/Effects - Audio/Step Manager")]
    public class StepsManager : MonoBehaviour, IAnimatorListener
    {
        [Tooltip("Enable Disable the Steps Manager")]
        public bool Active = true;
        [Tooltip("Layer Mask used to find the ground")]
        public LayerReference GroundLayer = new LayerReference(1);
        [Tooltip("Global Particle System for the Tracks, to have more individual tracks ")]
        public ParticleSystem Tracks;
        private ParticleSystem Instance;
        [Tooltip("Particle System for the Dust")]
        public ParticleSystem Dust;
        [Tooltip("This will instantiate a gameObject instead of using the Particle system")]
        public bool instantiateTracks = false;
        [Tooltip("Create Foot Tracks Particles only on Static GameObjects")]
        public bool GroundIsStatic = false;
        public float StepsVolume = 0.2f;
        public int DustParticles = 30;

        [Tooltip("Scale of the dust and track particles")]
        public Vector3 Scale = Vector3.one;

        [Tooltip("Sounds to play when the animal creates a track")]
        public AudioClipReference sounds;
        [Tooltip("Distance to Instantiate the tracks on a terrain")]
        public float trackOffset = 0.0085f;



        void Awake()
        {
            if (Tracks != null)
            {
                if (Tracks.gameObject.IsPrefab())
                {
                    Instance = Instantiate(Tracks, transform, false); //Instantiate in case the Track is a refab
                }
                else
                {
                    Instance = Tracks; 
                }

                Instance.transform.localScale = Scale;
            }
        }

        //Is Called by any of the "StepTrigger" Script on a feet when they collide with the ground.
        internal void EnterStep(StepTrigger foot, Collider surface)
        {
            if (!Active) return;

            if (Dust && Dust.gameObject.IsPrefab())
            {
                Dust = Instantiate(Dust, transform, false);             //If is a prefab clone it!
                Dust.transform.localScale = Scale;
            }

            if (foot.StepAudio && foot.StepAudio.enabled && !sounds.NullOrEmpty())    //If the track has an AudioSource Component and whe have some audio to play
            {
                sounds.Play(foot.StepAudio);
            }


            var Ray = new Ray(foot.transform.position, -transform.up);

            if (surface.Raycast(Ray, out RaycastHit hit, 1))
            {
                var TrackPosition = foot.transform.position;     TrackPosition.y += trackOffset;
                var TrackRotation = (Quaternion.FromToRotation(-foot.transform.forward, hit.normal) * foot.transform.rotation);

                if (Dust)
                {
                    Dust.transform.position = TrackPosition; //Set The Position
                    Dust.transform.rotation = TrackRotation;
                    Dust.transform.Rotate(-90, 0, 0);
                    Dust.Emit(DustParticles);
                }

                if (Instance)
                {
                    ParticleSystem.EmitParams ptrack = new ParticleSystem.EmitParams
                    {
                        rotation3D = TrackRotation.eulerAngles, //Set The Rotation
                        position = TrackPosition, //Set The Position
                    };

                    if (instantiateTracks)
                    {
                        if ((GroundIsStatic && surface.gameObject.isStatic))
                        {
                            Instance.Emit(ptrack, 1);
                        }
                        else
                        {
                            var newtrack = Instantiate(Instance); //Instantiate in case the Track is a refab

                            var ParentFixer = newtrack.transform.SetParentScaleFixer(hit.transform, TrackPosition);


                            ParticleSystem.EmitParams tr = new ParticleSystem.EmitParams
                            {
                                rotation3D = TrackRotation.eulerAngles, //Set The Rotation
                                position = Vector3.zero, //Set The Position
                            };

                            var main = newtrack.main;
                            main.simulationSpace = ParticleSystemSimulationSpace.Local;
                            newtrack.Emit(tr, 1);
                            this.Delay_Action(() => newtrack.isPlaying,  () =>
                            {
                              if (ParentFixer != null)  Destroy(ParentFixer.gameObject);
                            });
                        }
                    }
                    else
                    {
                        Instance.Emit(ptrack, 1);
                    }
                }
            }
        }

        /// <summary>Disable this sfcript, e.g.. deactivate when is sleeping or death </summary>
        public virtual void EnableSteps(bool value) => Active = value;

        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);
    }
}
