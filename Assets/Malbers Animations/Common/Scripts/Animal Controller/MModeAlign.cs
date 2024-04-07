using MalbersAnimations.Controller;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections; 


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Animal Controller/Mode Align")]
    public class MModeAlign : MonoBehaviour
    {
        [RequiredField] public MAnimal animal;

        [ContextMenuItem("Attack Mode", "AddAttackMode")]
        public List<ModeID> modes = new();

        [Tooltip("Exclude these abilities when playing the mode")]
        public List<int> ExcludeAbilities = new();


        [Tooltip("If the Target animal is on any of these states then ignore alignment.")]
        public List<StateID> ignoreStates = new();

        [Tooltip("The animal will keep attacking the current target until the target enters in any of the ignore states")]
        public BoolReference KeepCurrentTarget = new(true);

        //[Tooltip("It will search any gameobject that is a Animals on the Radius. ")]
        //public bool Animals = true;
        public LayerReference Layer = new(0);
        [Tooltip("Radius used for the Search")]
        [Min(0)] public float SearchRadius = 2f;
        [Tooltip("Radius used push closer/farther the Target when playing the Mode"), UnityEngine.Serialization.FormerlySerializedAs("DistanceRadius")]
        [Min(0)] public float Distance = 0;
        //[Tooltip("Multiplier To apply to AITarget found. Set this to Zero to ignore IAI Targets")]
        //[Min(0)] public float TargetMultiplier = 1;
        [Tooltip("Time needed to complete the Position aligment")]
        [Min(0)] public float AlignTime = 0.3f;
        


        [Tooltip("Front Offset of the Animal")]
        [Min(0)] public float FrontOffet = 0.15f;

        [Tooltip("Align Curve")]
        public AnimationCurve AlignCurve = new(new Keyframe[] { new Keyframe(0, 0), new Keyframe(1, 1) });

        public Color debugColor = new(1, 0.5f, 0, 0.2f);



        public bool debug;

        void Awake()
        {
            if (animal == null)
                animal = this.FindComponent<MAnimal>();

            if (modes == null || modes.Count == 0)
            {
                Debug.LogWarning("Please Add Modes to the Mode Align. ", this);
                enabled = false;
            }

            ignoreStates ??= new();
        }

        void OnEnable()
        { 
            animal.OnModeStart.AddListener(StartingMode); 
            
        }

        void OnDisable()
        { 
            animal.OnModeStart.RemoveListener(StartingMode);
        }

        void StartingMode(int ModeID, int ability)
        {
            if (!isActiveAndEnabled) return;

            //Debug.Log($"ability: {ability}");

            if (modes == null || modes.Count == 0 || modes.FirstOrDefault(x => x.ID == ModeID))
            {
                if (ExcludeAbilities.Contains(ability))
                {
                   // Debug.Log("DO NOT ALIGN");
                    return; //Skip the abilities included in this list
                }

                Align();
            }
        }

        public void Align()
        {
            //Search first Animals ... if did not find anyone then search for colliders
            if (!FindAnimals())
            {
                AlignCollider();
            }
        }

        //Store the closest Animal
        private MAnimal ClosestAnimal = null;

        private bool FindAnimals()
        {
            ClosestAnimal = null;
            float ClosestDistance = float.MaxValue;
            var Origin = transform.position;
            var radius = SearchRadius * animal.ScaleFactor;
            MDebug.DrawWireSphere(Origin, Color.red, radius, 1f);

            foreach (var a in MAnimal.Animals)
            {
                if (a == animal                                                 //We are the same animal
                    || a.Sleep                                                  //The Animal is sleep (Not working)
                    || !a.enabled                                               //The Animal Component is disabled    
                    || !MTools.Layer_in_LayerMask(a.gameObject.layer, Layer)    //Is not using the correct layer
                    || !a.gameObject.activeInHierarchy
                    || ignoreStates.Contains(a.ActiveStateID)       //Playing a skip State
                    )
                    continue; //Don't Find yourself or don't find death animals

                if (animal.transform.SameHierarchy(a.transform)) continue;      //Meaning that the animal is mounting the other animal.

                var TargetPos = a.Center;

                var dist = Vector3.Distance(Origin, TargetPos);

                Debug.DrawRay(Origin, TargetPos - Origin, Color.red, 2f);

                if (radius >= dist && ClosestDistance >= dist)
                {
                    ClosestDistance = dist;
                    ClosestAnimal = a;
                }
            }

            if (ClosestAnimal == animal) ClosestAnimal = null; //Clear the Closest animal is it the Animal owner

            //  Debug.Log($"closest {ClosestAnimal} ");

            if (ClosestAnimal)
            {
                if (!GetClosestAITarget(ClosestAnimal.transform))
                    Debuging($"Alinging to [{ClosestAnimal.name}]", this);

                return true;
            }
            return false;
        }

        private void AlignCollider()
        {
            var pos = animal.Center;

            Collider[] AllColliders = Physics.OverlapSphere(pos, SearchRadius * animal.ScaleFactor, Layer.Value, QueryTriggerInteraction.Ignore);

            Collider ClosestCollider = null;

            float ClosestDistance = float.MaxValue;

            foreach (var col in AllColliders)
            {
                if ((col.transform.SameHierarchy(animal.transform)  //Don't Find yourself
                    || !col.gameObject.activeInHierarchy            //Don't Find invisible colliders
                    || col.gameObject.isStatic                      //Don't Find Static colliders
                    || col.GetComponentInParent<MAnimal>()          //we already searched for Animals
                    || !col.enabled))       //Don't look  disabled colliders
                    continue;

                var DistCol = Vector3.Distance(transform.position, col.transform.position);

                if (ClosestDistance > DistCol)
                {
                    ClosestDistance = DistCol;
                    ClosestCollider = col;
                }
            }

            if (ClosestCollider)
            {
                if (!GetClosestAITarget(ClosestCollider.transform))
                    Debuging($"[{name}], Alinging to [{ClosestCollider.name}]", this);
            }
        }

        private bool GetClosestAITarget(Transform target)
        {
            var Center = target.position;
            var Origin = animal.Position;
            var radius = SearchRadius * animal.ScaleFactor;

            var Core = target.GetComponentInParent<IObjectCore>();
            if (Core != null) { target = Core.transform; } //Find the Core Transform

            var AllAITargets = target.GetComponentsInChildren<IAITarget>();

            IAITarget ClosestAI = null;
            var ClosestDistance = float.MaxValue;

            if (AllAITargets != null)
            {
                if (AllAITargets.Length == 1)
                {
                    ClosestAI = AllAITargets[0];
                    Center = ClosestAI.GetCenterPosition();
                }
                else
                {
                    foreach (var a in AllAITargets)
                    {
                        if (!a.transform.gameObject.activeInHierarchy) continue; //Do not search Disabled AI Targets

                        var dist = Vector3.Distance(Origin, a.GetCenterPosition());

                        if (radius >= dist && ClosestDistance >= dist)
                        {
                            ClosestDistance = dist;
                            Center = a.GetCenterPosition();
                            ClosestAI = a;
                        }
                    }
                }
            }

            StartAligning(Center, ClosestAI);

            return ClosestAI != null;
        }

        private void StartAligning(Vector3 TargetCenter, IAITarget isAI)
        {
            StopAllCoroutines();

            if (!animal.FreeMovement) TargetCenter.y = animal.transform.position.y;

            var distance = Distance * animal.ScaleFactor;

            if (isAI != null)
            {
                distance = isAI.StopDistance();// * TargetMultiplier;
                if (Distance == 0) distance = 0; //Remove if Distance is 0

                TargetCenter = isAI.GetCenterPosition();
                Debuging($" Alinging <B>AI Target</B> [{isAI.transform.name}]", isAI.transform);
            }

            if (debug)
            {
                var t = 1f;
                Debug.DrawLine(transform.position, TargetCenter, Color.white, t);
                MDebug.DrawWireSphere(TargetCenter, Quaternion.identity, 0.1f, Color.white, t);

                if (animal.FreeMovement)
                {
                    MDebug.DrawWireSphere(TargetCenter, Quaternion.identity, distance, Color.white, t);
                }
                else
                {
                    MDebug.DrawCircle(TargetCenter, Quaternion.identity, distance, Color.white, t);
                }
                Debug.DrawRay(TargetCenter, Vector3.up, Color.white, t);
            }


            //Align Look At the Zone Using Distance
            if (distance > 0)
            {
                distance += FrontOffet * animal.ScaleFactor; //Align from the Position of the Aligner
                StartCoroutine(MTools.AlignLookAtTransform(animal.transform, TargetCenter, AlignTime, AlignCurve));


                StartCoroutine(MTools.AlignTransformRadius(animal.transform, TargetCenter, AlignTime, distance, AlignCurve));
            }
            else
            {
                StartCoroutine(
                    AlignLookAtTransform(animal.transform, TargetCenter,
                    FrontOffet, AlignTime, animal.ScaleFactor, AlignCurve));
            }
        }

         
        private void Debuging(string deb, Object ob)
        {
            if (debug) Debug.Log($"<B>[{animal.name}]</B> {deb}", ob);
        }


        public IEnumerator AlignLookAtTransform(Transform t1, Vector3 target, float AlignOffset, float time, float scale, AnimationCurve AlignCurve)
        {
            float elapsedTime = 0;
            var wait = new WaitForFixedUpdate();

            Quaternion CurrentRot = t1.rotation;
            Vector3 direction = (target - t1.position);

            direction = Vector3.ProjectOnPlane(direction, t1.up);
            Quaternion FinalRot = Quaternion.LookRotation(direction);
            Vector3 Offset = t1.position + AlignOffset * scale * t1.forward; //Use Offset

            if (AlignOffset != 0)
            {
                //Calculate Real Direction at the End! 
                Quaternion TargetInverse_Rot = Quaternion.Inverse(t1.rotation);
                Quaternion TargetDelta = TargetInverse_Rot * FinalRot;

                var TargetPosition = t1.position + t1.DeltaPositionFromRotate(Offset, TargetDelta);
                direction = ((target) - TargetPosition);

                var debTime = 3f;

                if (debug)
                {
                    MDebug.Draw_Arrow(TargetPosition, direction, Color.yellow, debTime);
                    MDebug.DrawWireSphere(TargetPosition, 0.1f, Color.green, debTime);
                    MDebug.DrawWireSphere(target, 0.1f, Color.yellow, debTime);
                }
                direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values
            }

            if (direction.CloseToZero())
            {
                Debug.LogWarning("Direction is Zero. Please set a correct rotation", t1);
                yield return null;
            }
            else
            {
                direction = Vector3.ProjectOnPlane(direction, t1.up); //Remove Y values
                FinalRot = Quaternion.LookRotation(direction);

                Quaternion Last_Platform_Rot = t1.rotation;

                while ((time > 0) && (elapsedTime <= time))
                {
                    //Evaluation of the Pos curve
                    float result = AlignCurve != null ? AlignCurve.Evaluate(elapsedTime / time) : elapsedTime / time;         

                    t1.rotation = Quaternion.SlerpUnclamped(CurrentRot, FinalRot, result);

                    if (AlignOffset != 0)
                    {
                        Quaternion Inverse_Rot = Quaternion.Inverse(Last_Platform_Rot);
                        Quaternion Delta = Inverse_Rot * t1.rotation;
                        t1.position += t1.DeltaPositionFromRotate(Offset, Delta);
                    }

                    elapsedTime += Time.fixedDeltaTime;
                    Last_Platform_Rot = t1.rotation;

                    if (debug)
                    {
                        Debug.DrawRay(Offset, Vector3.up, Color.white);
                        MDebug.DrawWireSphere(t1.position, t1.rotation, 0.05f * scale, Color.white, 0.2f);
                        MDebug.DrawWireSphere(t1.position, t1.rotation, 0.05f * scale, Color.white, 0.2f);
                        MDebug.DrawWireSphere(Offset, 0.05f * scale, Color.white, 0.2f);
                        MDebug.Draw_Arrow(t1.position, t1.forward, Color.white, 0.2f);
                    }

                    yield return wait;
                }
            }
        }



#if UNITY_EDITOR

        [ContextMenu("Add Attack Mode")]
        private void AddAttackMode()
        {
            Reset();
        }


        void Reset()
        {

            animal = gameObject.FindComponent<MAnimal>();

            modes = new List<ModeID>
            {
                MTools.GetResource<ModeID>("Attack1")
            };

            ignoreStates = new List<StateID>
            {
                MTools.GetResource<StateID>("Death")
            };


            MTools.SetDirty(this);
        }

#if MALBERS_DEBUG
        void OnDrawGizmosSelected()
        {
            if (animal && debug)
            {
                var scale = animal.ScaleFactor;
                var Pos = animal.Position + (FrontOffet * scale * animal.Forward);

                Handles.color = debugColor;
                var c = debugColor; c.a = 1;
                Handles.color = c;
                Handles.DrawWireDisc(Pos, Vector3.up, SearchRadius * scale);

                Handles.color = (c + Color.white) / 2;
                Handles.DrawWireDisc(Pos, Vector3.up, Distance * scale);


                Gizmos.color = debugColor;
                Gizmos.DrawSphere(Pos, 0.1f * scale);
                Gizmos.color = c;
                Gizmos.DrawWireSphere(Pos, 0.1f * scale);
            }
        }
#endif
#endif
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(MModeAlign)), CanEditMultipleObjects]
    public class MModeAlignEditor : Editor
    {
        SerializedProperty animal,
            modes, ignoreStates, ExcludeAbilities, 
            // AnimalsOnly,
            Layer, debug, LookRadius, DistanceRadius, AlignTime, FrontOffet, //TargetMultiplier, 
            AlignCurve, debugColor/*, pushTarget*/;
        private void OnEnable()
        {
            animal = serializedObject.FindProperty("animal");
            modes = serializedObject.FindProperty("modes");
            ExcludeAbilities = serializedObject.FindProperty("ExcludeAbilities");
            ignoreStates = serializedObject.FindProperty("ignoreStates");
            // AnimalsOnly = serializedObject.FindProperty("Animals");
            Layer = serializedObject.FindProperty("Layer");
            LookRadius = serializedObject.FindProperty("SearchRadius");
            DistanceRadius = serializedObject.FindProperty("Distance");

            AlignTime = serializedObject.FindProperty("AlignTime");
            debugColor = serializedObject.FindProperty("debugColor");
            //TargetMultiplier = serializedObject.FindProperty("TargetMultiplier");
            FrontOffet = serializedObject.FindProperty("FrontOffet");
            debug = serializedObject.FindProperty("debug");
            AlignCurve = serializedObject.FindProperty("AlignCurve");

        }

        private GUIContent _ReactIcon;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MalbersEditor.DrawDescription($"Execute a LookAt towards the closest Animal or GameObject  when is playing a Mode on the list");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) 
            {
                using (new GUILayout.HorizontalScope())
                { 
                    if (Application.isPlaying)
                    {

                        if (_ReactIcon == null)
                        {
                            _ReactIcon = EditorGUIUtility.IconContent("d_PlayButton@2x");
                            _ReactIcon.tooltip = "Play at Runtime";
                        }

                        if (GUILayout.Button(_ReactIcon, EditorStyles.miniButton, GUILayout.Width(28f), GUILayout.Height(20)))
                        {
                            (target as MModeAlign).Align();
                        }
                    }


                    EditorGUILayout.PropertyField(animal);
                    if (debug.boolValue)
                        EditorGUILayout.PropertyField(debugColor, GUIContent.none, GUILayout.Width(36));

                    MalbersEditor.DrawDebugIcon(debug);
                }

                EditorGUILayout.PropertyField(Layer);

                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(modes, true);
                EditorGUILayout.PropertyField(ExcludeAbilities, true);
                EditorGUILayout.PropertyField(ignoreStates, true);
                EditorGUI.indentLevel--;
                //  EditorGUILayout.PropertyField(AnimalsOnly);
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(LookRadius);
                if (LookRadius.floatValue > 0)
                {
                    EditorGUILayout.PropertyField(AlignTime);
                    EditorGUILayout.PropertyField(AlignCurve);
                    EditorGUILayout.PropertyField(DistanceRadius);
                    EditorGUILayout.PropertyField(FrontOffet);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}