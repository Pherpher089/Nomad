﻿using UnityEngine;
using System.Collections;
using MalbersAnimations.Events;

using UnityEngine.AI;
using MalbersAnimations.Scriptables;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Controller.AI
{
    [AddComponentMenu("Malbers/Animal Controller/AI/AI Control")]
    public class MAnimalAIControl : MonoBehaviour, IAIControl, IAITarget, IAnimatorListener
    {
        #region Components and References
        /// <summary> Reference for the Agent</summary>
        [SerializeField] private NavMeshAgent agent;

        /// <summary> Reference for the Animal</summary>
        [RequiredField] public MAnimal animal;

        [Tooltip("On Enable, If the animal has any Input component Disable it")]
        public bool disableInput = true;

        [Tooltip("On Disable, If the animal has any Input component Enable it")]
        public bool enableInput = true;

        /// <summary>Cache if the Animal has an Interactor</summary>
        public IInteractor Interactor { get; internal set; }

        public bool AIReady { get; internal set; }

        public bool ArriveLookAt => false; //do this later

        public virtual bool Active => enabled && gameObject.activeInHierarchy; 
        #endregion

        #region Internal Variables
        /// <summary>Target Last Position (Useful to know if the Target is moving)</summary>
        protected Vector3 TargetLastPosition;

        /// <summary>Remaining Distance to the Destination Point</summary>
        public virtual float RemainingDistance { get; set; } 

        public virtual bool IsMoving { get; set; } 

        /// <summary> Returns the Current Agent Remaining Distance </summary>
        public virtual float AgentRemainingDistance => Agent.remainingDistance;

        /// <summary>Store the Current Remaining Distance. This is used to slowdown the Animal when is circling around and it cannot arrive to the destination</summary>
        public virtual float MinRemainingDistance { get; set; }

        /// <summary>Used to Slow Down the Animal when its close the Destination</summary>
        public float SlowMultiplier
        {
            get
            {
                var result = 1f;
                if (CurrentSlowingDistance > CurrentStoppingDistance && RemainingDistance < CurrentSlowingDistance)
                    result = Mathf.Max(RemainingDistance / CurrentSlowingDistance, slowingLimit);

                return result;
            }
        }

        public Transform Transform { get; internal set; }


        /// <summary>Stores the Agent Direction used to move the Animal</summary>
        public Vector3 AIDirection { get; set; } 

        /// <summary>Is the Agent in a OffMesh Link</summary>       
        public bool InOffMeshLink  { get; set; }
     

        public virtual bool AgentInOffMeshLink => Agent.isOnOffMeshLink;

        /// <summary>Store if the Animal is on a Blocking Agent State</summary>       
        public bool StateIsBlockingAgent { get; set; }

        /// <summary>Is the Agent Enabled/Active ?</summary>       
        public virtual bool ActiveAgent
        {
            get => agent.enabled && agent.isOnNavMesh;
            set
            {
                agent.enabled = value;
                if (agent.isOnNavMesh) agent.isStopped = !value;
               // Debug.Log($"<B>{(agent.enabled? "[•]": "[  ]" )}</B> Agent Enable");
            }
        }

        /// <summary>Checks if the Animal Can Fly</summary>
        public virtual bool CanFly { get; private set; }

        /// <summary>Has the animal arrived to the destination</summary>
        public bool HasArrived { get; set; }
      
        /// <summary>Updates the Destination Position if the Target Moves</summary>
        public virtual bool UpdateDestinationPosition { get; set; }
      

        /// <summary>Destination Position to use on Agent.SetDestination()</summary>
        public virtual Vector3 DestinationPosition { get; set; }
    
         
        private IEnumerator I_WaitToNextTarget;
        private IEnumerator IFreeMoveOffMesh;
        private IEnumerator IClimbOffMesh;
        #endregion

        #region Public Variables 
        [Tooltip("When the animal is on any of these States, The AI agent will be disable to improve performance.")]
        [ContextMenuItem("Set Default", "SetDefaulStopAgent")]
        public List<StateID> StopAgentOn;

        [Tooltip("Multiplier used for Waypoints Wait time. Set it to zero if you want to ignore waiting on waypoints")]
        [Min(0), SerializeField] private float waitTimeMult = 1f;


        [Min(0)] public float UpdateAI = 0.2f;
        private float CurrentTime;

        [Min(0)][SerializeField] protected float stoppingDistance = 0.6f;
        [Min(0)][SerializeField] protected float PointStoppingDistance = 0.6f;

        /// <summary>The animal will change automatically to Walk if the distance to the target is this value</summary>
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("walkDistance")]
        [Min(0)] protected float slowingDistance = 1f;

        [Tooltip("If the AI Animal is scaled, use the scale factor to find the Target")]
        public bool UseScale = true;

        [Tooltip("How high a target can be from the terrain so the Animal can follow  it")]
        [SerializeField][Min(0)] private float targetHeight = 5f;

        [Tooltip("The Animal will stop if the target is too high to reach")]
        public bool StopOnTargetTooHigh = true;
      
        [Tooltip("Distance from the Animals Root to apply LookAt Target Logic when the Animal arrives to a target.")]
        [Min(0)] public float LookAtOffset = 1;

        [Tooltip("Limit for the Slowing Multiplier to be applied to the Speed Modifier")]
        [Range(0, 1)]
        [SerializeField] private float slowingLimit = 0.3f;

        [SerializeField] private Transform target;
        [SerializeField] private Transform nextTarget;

        /// <summary>When the AI Arrives to a Waypoint Target, it will set the Next Target from the AIWaypoint</summary>
        public bool AutoNextTarget { get; set; }

        /// <summary>The Animal will Rotate/Look at the Target when he arrives to it</summary>
        public bool LookAtTargetOnArrival { get; set; }
      

        public bool debug = false;
        public bool debugGizmos = true;
        public bool debugStatus = true;
        #endregion
  
        #region Events
        [Space]
        public Vector3Event OnTargetPositionArrived = new();
        public TransformEvent OnTargetArrived = new();
        public TransformEvent OnTargetSet = new();

        public TransformEvent TargetSet => OnTargetSet;
        public TransformEvent OnArrived => OnTargetArrived;
        public UnityEvent OnEnabled = new();
        public UnityEvent OnDisabled = new();

        #endregion 

        #region Properties 

        /// <summary>is the Animal, Flying, swimming, On Free Mode?</summary>
        public bool FreeMove { get; set; }

        /// <summary>height of the Agent</summary>
        public virtual float Height => targetHeight * animal.ScaleFactor;

        /// <summary> Is the Target too high?  </summary>
        public virtual bool TargetTooHigh { get; set; }

        /// <summary>Default Stopping Distance</summary>
        public virtual float StoppingDistance { get => stoppingDistance; set => stoppingDistance = value; }

        protected float currentStoppingDistance;

        /// <summary>Current Stoping distance of the Current Target/Destination</summary>
        public virtual float CurrentStoppingDistance
        {
            get => currentStoppingDistance * (UseScale ? animal.ScaleFactor :1f);
            set => Agent.stoppingDistance = currentStoppingDistance = value;
        }

        /// <summary>Default Slowing Distance</summary>
        public virtual float SlowingDistance => slowingDistance;
         

        /// <summary>Current Slowing Distance from the Current AI Target</summary>
        public virtual float CurrentSlowingDistance { get; set; }

        /// <summary>Is the Animal Playing a mode</summary>
        public bool IsOnMode => animal.IsPlayingMode;

        /// <summary>  Stop all Modes that does not allow Movement  </summary>
        private bool IsOnNonMovingMode => (IsOnMode && !animal.ActiveMode.AllowMovement);

        /// <summary>Is the Target a WayPoint?</summary>
        public IWayPoint IsWayPoint { get; set; }

        /// <summary>Is the Target an AITarget</summary>
        public IAITarget IsAITarget { get; set; }

        /// <summary>AITarget Position</summary>
        public Vector3 AITargetPos => IsAITarget.GetCenterPosition();      //Update the AI Target Pos if the Target moved

        /// <summary>Is the Target an AITarget</summary>
        public IInteractable IsTargetInteractable { get; protected set; }

        /// <summary>The Target is an Air Target</summary>
        internal bool IsAirDestination => IsAITarget != null && IsAITarget.TargetType == WayPointType.Air;
        internal bool IsGroundDestination => IsAITarget != null && IsAITarget.TargetType == WayPointType.Ground;

        /// <summary>Reference of the Nav Mesh Agent</summary>
        public virtual NavMeshAgent Agent => agent;

        public Transform AgentTransform;

        public virtual Vector3 GetCenterPosition() => AgentTransform.position;

        public Vector3 GetCenterY() => animal.Center;

        /// <summary> Self Target Type </summary>
        public virtual WayPointType TargetType => animal.FreeMovement ? WayPointType.Air : WayPointType.Ground;


        /// <summary>is the Target transform moving??</summary>
        public virtual bool TargetIsMoving { get; internal set; }


        /// <summary> Is the Animal waiting x time to go to the Next waypoint</summary>
        public virtual bool IsWaiting { get; internal set; }

        public virtual Vector3 LastOffMeshDestination { get; set; }

        /// <summary> Store where the Offmesh Link ends </summary>
        public virtual Vector3 EndOffMeshPos  { get; set; }

        public Vector3 NullVector { get; set; }

        public virtual Transform NextTarget { get => nextTarget; set => nextTarget = value; }

        public virtual Transform Target { get => target; set => target = value; }
        public float WaitTimeMult { get => waitTimeMult; set => waitTimeMult = value; }

        /// <summary>Stores the Local Agent Position relative to the Animal</summary>
        protected Vector3 AgentPosition;

        #endregion
         
        public virtual void SetActive(bool value)
        {
            // Debug.Log("value = " + value);
            if (gameObject.activeInHierarchy)
                enabled = value;
        }

        #region Unity Functions 
        public virtual bool OnAnimatorBehaviourMessage(string message, object value) => this.InvokeWithParams(message, value);


        protected virtual void Awake()
        {
            if (animal == null) animal = gameObject.FindComponent<MAnimal>();
            ValidateAgent();

            this.Transform = transform;

            Interactor = animal.FindInterface<IInteractor>();       //Check if there's any Interactor
            //InputSource = animal.FindInterface<IInputSource>();     //Check if there's any Input Source
            animal.UseSmoothVertical = true;                        //This needs to be disable so the slow distance works!!!!!!

            LookAtTargetOnArrival = true;                           //By Default Look Target on Arrival set it to True
            AutoNextTarget = true;                                  //By Default Auto Next Target is set to True
            UpdateDestinationPosition = true;

            NullVector = new Vector3(-998.9999f, -998.9999f, -998.9999f);
            DestinationPosition = NullVector;
            CanFly = animal.HasState(StateEnum.Fly);                //Check if the Animal can Fly
            SetAgent();
        }

        /// <summary>  Set the Default properties for the Nav mesh Agent </summary>
        protected virtual void SetAgent()
        {
            if (agent == null) AgentTransform.GetComponent<NavMeshAgent>(); //Re-Check if the Agent is not properly assigned

            if (agent)
            {
                AgentPosition = Agent.transform.localPosition;
                Agent.angularSpeed = 0;
                Agent.speed = 1;                                                    //The Agent needs a speed different from 0 to calculate the velocity
                Agent.acceleration = 0;
                Agent.autoBraking = false;
                Agent.updateRotation = false;                                       //The Animal will control the rotation . NOT THE AGENT
                Agent.updatePosition = false;                                       //The Animal will control the  postion . NOT THE AGENT
                Agent.autoTraverseOffMeshLink = false;                              //Offmesh links are handled by animation
                Agent.stoppingDistance = StoppingDistance;
            }
        }

        protected virtual void OnEnable()
        {
            animal.OnStateActivate.AddListener(OnState);
            animal.OnModeStart.AddListener(OnModeStart);
            animal.OnModeEnd.AddListener(OnModeEnd);

            IsWaiting = true; //The AI Has not Started yet
            FreeMove = false;
            AIReady  = false;

            if (animal.ActiveState) //If the animal has an active state.
                FreeMove = (animal.ActiveState.General.FreeMovement);

            if (FreeMove) ActiveAgent = false;
            if (Agent && !Agent.isOnNavMesh) ActiveAgent = false;
            HasArrived = false;
            TargetIsMoving = false;
            
            this.Delay_Action(StartAI);//Start AI a Frame later; 
 

            //Disable any Input Source in case it was active
            if (animal.InputSource != null && disableInput)
            {
                animal.InputSource.Enable(false);
                Debuging("Input Move Disabled");
            }

            OnEnabled.Invoke();
        }

        protected virtual void OnDisable()
        {
            animal.OnStateActivate.RemoveListener(OnState);           //Listen when the Animations changes..
            animal.OnModeStart.RemoveListener(OnModeStart);           //Listen when the Animations changes..
            animal.OnModeEnd.RemoveListener(OnModeEnd);           //Listen when the Animations changes..
            Stop();
            StopAllCoroutines();
            OnDisabled.Invoke();

            animal.Rotate_at_Direction = false;
            AIReady = false;


            //Disable any Input Source in case it was active
            if (animal.InputSource != null && enableInput)
            {
                animal.InputSource.Enable(true);
                animal.Reset_Movement();
                Debuging("Input Move Enabled");
            }
        }

        protected virtual void Update() { Updating(); }
        #endregion

        #region Animal Events Listen
        /// <summary>Called when the Animal Enter an Action, Attack, Damage or something similar</summary>
        public virtual void OnModeStart(int ModeID, int ability)
        {
            Debuging($"has started a Mode: <B>[{animal.ActiveMode.ID.name}]</B>. Ability: <B>[{animal.ActiveMode.ActiveAbility.Name}]</B>");
            if (animal.ActiveMode.AllowMovement) return; //Don't stop the Animal Movevemt if the Mode can make movements
            else
            {
                animal.InertiaPositionSpeed = Vector3.zero;
                animal.StopMoving();
                animal.MovementAxisSmoothed = Vector3.zero;
            }

            var Dest = DestinationPosition; //Store the Destination with modes
            Stop(); //If the Agent was moving Stop it
            DestinationPosition = Dest; //Restore the Destination with modes
        }

        /// <summary>  Listen if the Animal Has finished a mode  </summary>
        public virtual void OnModeEnd(int ModeID, int ability)
        {
            if (StateIsBlockingAgent) return; //Do nothing if the current State is blocking the agent.

            Debuging($"Mode End: <B>[{ModeID}]</B>. Ability: <B>[{ability}]</B>");


            if (!HasArrived) //Don't move if there's no destination
            {
                CalculatePath();
                Move();
            }

            CompleteOffMeshLink();
            CheckAirTarget(); //Everytime a State Changes Check again in case it failed by mistake
        }


        /// <summary>Listen to the Animal when it changes States</summary>
        public virtual void OnState(int stateID)
        {
            if (IsWaiting) return; //Do nothing if the Agent is waiting

            FreeMove = (animal.ActiveState.General.FreeMovement); //Recheck if the current State is a FreeState
            if (CheckAirTarget()) return; //Everytime a State Changes Check again in case it failed by mistake

            //Store the Active State Blocking
            StateIsBlockingAgent = animal.ActiveStateID != 0 && StopAgentOn != null && StopAgentOn.Contains(animal.ActiveStateID); 


            if (StateIsBlockingAgent) //Check if we are on a State that does not require the Agent
            {
                ActiveAgent = false; //Disable the Agent
            }
            else
            {
                if (!IsOnNonMovingMode)
                {
                    CalculatePath();
                    Move();
                }
                CompleteOffMeshLink();
            }
        }
        #endregion

        public virtual void StartAI()
        {
            var targ = target; target = null;
            SetTarget(targ);                                                  //Set the first Target (IMPORTANT)  it also set the next future targets

            if (AgentTransform == animal.transform)
                Debug.LogWarning("The Nav Mesh Agent needs to be attached to a child Gameobject, not in the same gameObject as the Animal Component");
            AIReady = true;
        }

   

        public virtual void Updating()
        {
            ResetAgentPosition();

            if (IsWaiting || InOffMeshLink) return;    //Do nothing while is in an offmeshLink or its Waiting

            CheckMovingTarget();

            if (FreeMove)
            {
                if (IsAirDestination && animal.ActiveStateID.ID != StateEnum.Fly)
                {
                    animal.State_Activate(StateEnum.Fly); //Forcing Fly if the animal was not flying
                    Debuging("Force! Flying!");
                }

                FreeMovement();
            }
            else
            {
                UpdateAgent();
            }
        }

        /// <summary>Reset the Agent Transform position to its Local Offset</summary>
        protected virtual void ResetAgentPosition()
        {
            AgentTransform.localPosition = AgentPosition;                  //Important! Reset the Agent Position to the default Position
            Agent.nextPosition = Agent.transform.position;                  //IMPORTANT!!!!Update the Agent Position to the Transform position 
        }

        /// <summary>Check if there's a path to go to</summary>
        public virtual bool PathPending() => ActiveAgent && Agent.isOnNavMesh && Agent.pathPending;

        /// <summary> Updates the Agents using he animation root motion </summary>
        public virtual void UpdateAgent()
        {
            if (HasArrived)
            {
                LookTargetOnArrival();
            }
            else if (ActiveAgent)
            {
                if (PathPending()) return;    //Means is still calculating the path to the Destination

                SetRemainingDistance(AgentRemainingDistance);

                if (!Arrive_Destination())   //if we havent't arrived to the destination ... Find the way 
                {
                    //If is not in OffMesh Link
                    if (!CheckOffMeshLinks())
                    {
                        CalculatePath();
                        Move();   //Calculate the AI DIRECTION
                    }
                }
            }
        }

        private void LookTargetOnArrival()
        {
            if (LookAtTargetOnArrival && LookAtOffset > 0)
            {

                if (DestinationPosition == NullVector)
                    DestinationPosition = (target != null ? target.position : transform.position + transform.forward);

                var Origin = (animal.Position - animal.ScaleFactor * LookAtOffset * animal.Forward);
                var LookAtDir = (target != null ? target.position : DestinationPosition) - Origin;

                if (debugGizmos)
                {
                    MDebug.Draw_Arrow(Origin, LookAtDir, Color.magenta);
                    MDebug.DrawWireSphere(Origin, Color.magenta, 0.1f);
                }

                animal.RotateAtDirection(LookAtDir);
            }
        }

        /// <summary>Check if we have Arrived to the Destination</summary>
        public virtual bool Arrive_Destination()
        {
            if (InOffMeshLink) return false;

            if (CurrentStoppingDistance >= RemainingDistance )
            {
                HasArrived = true;
                RemainingDistance = 0;                                 //Reset the Remaining Distance
                AIDirection = Vector3.zero;                          //Reset AI Direction

                if (IsPathIncomplete()) //Check when the Agent is trapped on an NavMesh that cannot exit
                {
                    Debuging($"[<color=orange>Agent Path Status: {Agent.pathStatus}]. Force Stop. <B>Checking Next Target </B></color>");

                    if (AutoNextTarget)  //Set and Move to the Next Target
                        MovetoNextTarget();
                    else
                        Stop();

                    return true;
                }

                Move();

                if (target)
                {
                    Debuging($"<color=green>has arrived to: <B>{target.name}</B> → {DestinationPosition} </color>");

                    CheckInteractions();

                    //If we have arrived to an AI Target and the Destination is the same one
                    if (IsAITarget != null/* && IsAITarget.GetPosition() == DestinationPosition*/)
                    {
                        //Call the method that the Target has arrived to the destination
                        IsAITarget.TargetArrived(animal.gameObject);

                        LookAtTargetOnArrival = IsAITarget.ArriveLookAt;

                        //if the next waypoing is on the Ground then set the free Movement to false
                        if (IsAITarget.TargetType == WayPointType.Ground) FreeMove = false;


                        if (AutoNextTarget)  //Set and Move to the Next Target
                            MovetoNextTarget();
                        else
                            Stop();
                    }

                    OnTargetArrived.Invoke(target);                         //Invoke the Event On Target Arrived
                    OnTargetPositionArrived.Invoke(DestinationPosition);    //Invoke the Event On Target Position Arrived
                }
                else
                {

                    OnTargetPositionArrived.Invoke(DestinationPosition);    //Invoke the Event On Target Position Arrived
                    Debuging($"<color=green>has arrived to: <B>{DestinationPosition}</B>.  Stop</color>");
                    Stop(); //The target was removed
                }
                return true;
            }
            return false;
        }

        protected virtual bool IsPathIncomplete()
        {
            return ActiveAgent && !FreeMove && Agent.pathStatus != NavMeshPathStatus.PathComplete;
        }

        /// <summary>Check if the Height of the Destination is near the Animal</summary>
        protected virtual bool CheckDestinationHeight()
        {
            TargetTooHigh = false;
            if (FreeMove) return true; //When Flying do not check the Height of the Point
            if (targetHeight == 0) return true; //Do nothing if Target heigh is zero

            if (NavMesh.SamplePosition(DestinationPosition, out var hit, Height, NavMesh.AllAreas))
            {
                if (debugGizmos)
                {
                    MDebug.DrawWireSphere(hit.position, Color.cyan, 0.2f, 3);
                    Debug.DrawRay(hit.position, animal.UpVector * Height, Color.cyan, 0.2f);
                }
                DestinationPosition = hit.position; //Use the Projected NavMesh Position
                return true;
            }
            else
            {
                TargetTooHigh = true;
                Debuging($"<color=orange>Target too High!: <B>{DestinationPosition}</B>.  Stopping</color>");
            }

            return !TargetTooHigh;
        }

        /// <summary> Check if the Target is moving </summary>
        public virtual void CheckMovingTarget()
        {
            if (MTools.ElapsedTime(CurrentTime, UpdateAI))
            {
                if (Target)
                {
                    TargetIsMoving = (Target.position - TargetLastPosition).sqrMagnitude > (0.01f / animal.ScaleFactor);
                    TargetLastPosition = Target.position;

                    if (TargetIsMoving) 
                        Update_DestinationPosition();
                }
                CurrentTime = Time.time;
            }
        }


        /// <summary>Calculates the Direction to move the Animal using the Agent Desired Velocity</summary>
        public virtual void CalculatePath()
        {
            if (FreeMove) return;               //Do nothing when its on Free Move
            //if (IsWaiting) return;            //Do nothing when its waiting

            if (!ActiveAgent) //Enable the Agent in case is disabled
            {
                ActiveAgent = true;
                ResetFreeMoveOffMesh();
            }

            if (Agent.isOnNavMesh)
            {
                if (Agent.destination != DestinationPosition) //Calculate the New Path **ONLY** when the Destination is Different
                {
                    Agent.SetDestination(DestinationPosition);  //Set the Current Destination;

                    if (IsWayPoint != null) DestinationPosition = Agent.destination; //Important use the Cast value on the terrain.
                }

                if (Agent.desiredVelocity != Vector3.zero) 
                    AIDirection = Agent.desiredVelocity.normalized;
            }
        }


        /// <summary> Move the Animal using the Agent Direction and the Slow Multiplier </summary>
        public virtual void Move()
        {
            IsMoving = AIDirection != Vector3.zero;
            animal.Move(AIDirection * SlowMultiplier);
           
        }

        /// <summary> Disable the AI Agent and it Stops the Animal</summary>
        public virtual void Stop()
        {
            ActiveAgent = false; //Disable the Agent
            AIDirection = Vector3.zero;
            DestinationPosition = NullVector;
            animal.StopMoving(); //Stop the Animal
            InOffMeshLink = false;
            Debuging($"[Stopped]. Agent Disabled");
            IsMoving = false;
        }


        /// <summary>Update The Target Position </summary>
        protected virtual void Update_DestinationPosition()
        {
            if (UpdateDestinationPosition)
            {
                DestinationPosition = GetTargetPosition();                          //Update the Target Position 

                CheckDestinationHeight(); //Check if the new destination position is too high

                if (TargetTooHigh && StopOnTargetTooHigh)
                {
                    Stop();
                    return;
                }

                //Double check if the Animal is far from the target
                var DistanceOnMovingTarget = Vector3.Distance(DestinationPosition, AgentTransform.position); 

                if (DistanceOnMovingTarget >= CurrentStoppingDistance)
                {
                    HasArrived = false;
                    CalculatePath();
                    Move();
                }
                else
                {
                    HasArrived = true; //Check if the animal hasn't arrived to a moving target
                    //IsMoving = false;
                }
            }
        }

        /// <summary> Store the remaining distance -- but if navMeshAgent is still looking for a path Keep Moving </summary>
        protected virtual void SetRemainingDistance(float current) => RemainingDistance = current;

        #region Set Assing Target and Next Targets

        /// <summary> Resets al the Internal Values of the AI Control  </summary>
        public virtual void ResetAIValues()
        {
            StopWait();                                                 //If the Animal was waiting Reset the waiting IMPORTANT!!
            RemainingDistance = float.PositiveInfinity;                 //Set the Remaining Distance as the Max Float Value
            MinRemainingDistance = float.PositiveInfinity;                 //Set the Remaining Distance as the Max Float Value
            HasArrived = false;
        }

        /// <summary> Find the Closest  </summary>
        private IAITarget ClosestTarget(IAITarget[] targets)
        {
            IAITarget result = null;

            if (targets != null)
            {
                float closeDist = float.PositiveInfinity;
                foreach (var t in targets)
                {
                    var Dist = (transform.position - t.GetCenterPosition()).sqrMagnitude;

                    if (closeDist > Dist)
                    {
                        result = t;
                        closeDist = Dist;
                    }
                }
            }

            return result;
        }

        /// <summary>Set the next Target</summary>   
        public virtual void SetTarget(Transform newTarget, bool move)
        {
            // if (target == Target && !HasArrived) return;         
            //Don't assign the same target if we are travelling to that target (Breaks Wander Areas)

            target = newTarget;
            OnTargetSet.Invoke(newTarget);                                 //Invoked that the Target has changed.

            if (target != null)
            {
                TargetLastPosition = newTarget.position;                   //Since is a new Target "Reset the Target last position"
                DestinationPosition = newTarget.position;                  //Update the Target Position 


                var AITargets = newTarget.FindInterfaces<IAITarget>(); //Find allthe AI Targets and find the closest one (Dragon Feet)
                IsAITarget = ClosestTarget(AITargets);

                // Debug.Log("isait = " + AITargets.Length);

                IsTargetInteractable = newTarget.FindInterface<IInteractable>();
                IsWayPoint = newTarget.FindInterface<IWayPoint>();

                NextTarget = null;

                if (IsWayPoint != null)
                {
                    NextTarget = IsWayPoint.NextTarget(); //Find the Next Target on the Waypoint
                }
                Debuging($"<color=yellow>New Target <B>[{newTarget.name}]</B> → [{DestinationPosition}]. Move = [{move}]</color>");

                CheckAirTarget();

                //Resume the Agent is MoveAgent is true
                if (move)
                {
                    ResetAIValues();
                    
                    CurrentStoppingDistance = GetTargetStoppingDistance();
                    CurrentSlowingDistance = GetTargetSlowingDistance();

                   // var OldDest = DestinationPosition;
                    DestinationPosition = GetTargetPosition();

                    CalculatePath();

                    Move();
                    Debuging($"<color=yellow>is travelling to <B>Target: [{newTarget.name}]</B> → [{DestinationPosition}] </color>");


                    ////Meaning we are already going to the same destination
                    //if ((DestinationPosition - OldDest).sqrMagnitude <= 0.001f) return;

                    ////In Case it was making any Mode Interrupt it because there's a new target to go to.
                    //if (animal.IsPlayingMode) animal.Mode_Interrupt();
                }
            }
            else
            {
                IsAITarget = null;                  //Reset the AI Target
                IsTargetInteractable = null;        //Reset the AI Target Interactable
                IsWayPoint = null;                  //Reset the Waypoint
                Debuging($"<color=yellow>Clear Target()</color>");
                if (move) Stop(); //Means the Target is null so Stop the Animal
            }
        }

        public virtual void SetTarget(GameObject target) => SetTarget(target, true);
        public virtual void SetTarget(GameObject target, bool move) => SetTarget(target != null ? target.transform : null, move);



        /// <summary>Remove the current Target and stop the Agent </summary>
        public virtual void ClearTarget() => SetTarget((Transform)null, false);

        /// <summary>Remove the current Target </summary>
        public virtual void NullTarget() => target = null;

        /// <summary>Assign a new Target but it does not move it to it</summary>
        public virtual void SetTargetOnly(Transform target) => SetTarget(target, false);
        public virtual void SetTargetOnly(GameObject target) => SetTarget(target, false);
        public virtual void SetTarget(Transform target) => SetTarget(target, true);

        /// <summary> Returns the Current Target Destination</summary>
        public virtual Vector3 GetTargetPosition()
        {
            var TargetPos = (IsAITarget != null) ? AITargetPos : target.position;
            if (TargetPos == Vector3.zero) TargetPos = target.position; //HACK FOR WHEN THE TARGET REMOVED THEIR AI TARGET COMPONENT
            return TargetPos;
        }

        public void TargetArrived(GameObject target) {/*Do nothing*/ }

        public virtual float GetTargetStoppingDistance() => IsAITarget != null ? IsAITarget.StopDistance() : StoppingDistance * animal.ScaleFactor;
        public virtual float GetTargetSlowingDistance() => IsAITarget != null ? IsAITarget.SlowDistance() : SlowingDistance * animal.ScaleFactor;

        /// <summary>Set the Next Target from  on the NextTargets Stored on the Waypoints or Zones</summary>

        public virtual void SetNextTarget(GameObject next)
        {
            NextTarget = next.transform;
            IsWayPoint = next.GetComponent<IWayPoint>(); //Check if the next gameobject is a Waypoint.
        }

        public virtual void MovetoNextTarget()
        {
            if (NextTarget == null)
            {
                Debuging("There's no Next Target");
                Stop();
                return;
            }

            if (IsWayPoint != null)
            {
                StopWait();

                if (WaitTimeMult > 0)
                {   //IMPORTANT YOU NEED TO WAIT 1 FRAME ALWAYS TO GO TO THE NEXT WAYPOINT
                    I_WaitToNextTarget = C_WaitToNextTarget(IsWayPoint.WaitTime * WaitTimeMult, NextTarget);

                    StartCoroutine(I_WaitToNextTarget);
                }
            }
            else
            {
                SetTarget(NextTarget);
            }
        }

        public void StopWait()
        {
            IsWaiting = false;
            if (I_WaitToNextTarget != null) StopCoroutine(I_WaitToNextTarget); //Stop the coroutine in case it was playing
        }

        /// <summary> Check if the Next Target is a Air Target, if true then go to it</summary>
        internal virtual bool CheckAirTarget()
        {
            if (!CanFly) return false;

            if (IsAirDestination && !FreeMove)    //If the animal can fly, there's a new wayPoint & is on the Air
            {
                if (Target) Debuging($"Target {Target} is in the Air.  Activating Fly State", Target.gameObject);
                animal.State_Activate(StateEnum.Fly);
                FreeMove = true;

                ActiveAgent = false; //Disable the Agent
            }

            return IsAirDestination;
        }

        #endregion

        public virtual void SetDestination(Vector3 PositionTarget) => SetDestination(PositionTarget, true);

        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3 newDestination, bool move)
        {
            LookAtTargetOnArrival = false; //Do not Look at the Target when its setting a destination

            if (newDestination == DestinationPosition) return;  //Means that you're already going to the same point so Skip the code

            //We are already near the destination point
            if (Vector3.Distance(newDestination, DestinationPosition) < stoppingDistance) return;

            CurrentStoppingDistance = PointStoppingDistance;    //Reset the stopping distance when Set Destination is used.

            ResetAIValues();

            if (IsOnNonMovingMode)
                animal.Mode_Interrupt();

            IsWayPoint = null;

            if (I_WaitToNextTarget != null)
                StopCoroutine(I_WaitToNextTarget);                          //if there's a coroutine active then stop it

            DestinationPosition = newDestination;                           //Update the Target Position

         

            if (move)
            {
                CalculatePath();
                Move();
                Debuging($"<color=yellow>is travelling to: {DestinationPosition} </color>");
            }
        }

        /// <summary>Set the next Destination Position without having a target</summary>   
        public virtual void SetDestination(Vector3Var newDestination) => SetDestination(newDestination.Value);


        public virtual void SetDestinationClearTarget(Vector3 PositionTarget)
        {
            target = null;
            SetDestination(PositionTarget, true);
        }



        /// <summary>Check Interactions when Arriving to the Destination</summary>
        protected virtual void CheckInteractions()
        {
            if (IsTargetInteractable != null && IsTargetInteractable.Auto) //If the interactable is set to Auto!!!!!!!
            {
                if (Interactor != null)
                {
                    Interactor.Interact(IsTargetInteractable); //Do an Interaction if the Animal has an Interactor
                    Debuging($"Interact with : <b><{IsTargetInteractable.Owner.name}></b>. Interactor [{Interactor.Owner.name}]");
                }
                else
                {
                    IsTargetInteractable.Interact(0, animal.gameObject); //Do an Empty Interaction does not have an interactor
                    Debuging($"Interact with : <b><{IsTargetInteractable.Owner.name}></b>.  Interactor:Null");
                }

            }
        }

        /// <summary> Move Freely towards the Destination.. No Obstacle is calculated</summary>
        protected virtual void FreeMovement()
        {
            if (!HasArrived)
            {
                AIDirection = (DestinationPosition - animal.transform.position); //Important to be normalized!!
                SetRemainingDistance(AIDirection.magnitude);

                AIDirection = AIDirection.normalized * SlowMultiplier; //Important to be normalized!!

                //Debug.Log("AIDirection = " + AIDirection);

                animal.Move(AIDirection);
                Arrive_Destination();
            }
        }


        protected virtual bool CheckOffMeshLinks()
        {
            if (AgentInOffMeshLink && !InOffMeshLink)                         //Check if the Agent is on a OFF MESH LINK (Do this once! per offmesh link)
            {
                InOffMeshLink = true;                                            //Just to avoid entering here again while we are on a OFF MESH LINK
                LastOffMeshDestination = DestinationPosition;

                Debug.DrawRay(DestinationPosition, Vector3.up * 3, Color.white, 2);

                OffMeshLinkData OMLData = Agent.currentOffMeshLinkData;

                if (OMLData.linkType == OffMeshLinkType.LinkTypeManual)        //Means that it has a OffMesh Link component
                {
                    var OffMesh_Link = OMLData.offMeshLink;                    //Check if the OffMeshLink is a Manually placed  Link

                    if (OffMesh_Link)
                    {
                        var AnimalLink = OffMesh_Link.GetComponent<MAIAnimalLink>();

                        //CUSTOM OFFMESHLINK
                        if (AnimalLink)
                        {
                            AnimalLink.Execute(this, animal);
                            EndOffMeshPos = AnimalLink.End.position;
                            return true;
                        }

                        IZone IsOffMeshZone =
                        OffMesh_Link.FindInterface<IZone>();                     //Search if the OFFMESH IS An ACTION ZONE (EXAMPLE CRAWL)

                        if (IsOffMeshZone != null)                                           //if the OffmeshLink is a zone and is not making an action
                        {
                            if (debug) Debuging($"<color=white>is on a <b>[OffmeshLink Zone]</b> -> [{IsOffMeshZone.transform.name}]</color>");

                            IsOffMeshZone.ActivateZone(animal);                      //Activate the Zone
                            return true;
                        }


                        var NearTransform = transform.NearestTransform(OffMesh_Link.endTransform, OffMesh_Link.startTransform);
                        var FarTransform = NearTransform == OffMesh_Link.endTransform ? OffMesh_Link.startTransform : OffMesh_Link.endTransform;

                        AIDirection = NearTransform.forward;
                        animal.Move(AIDirection);//Move where the AI DIRECTION FROM THE OFFMESH IS POINting

                        if (debugGizmos)
                        {
                            Debug.DrawRay(transform.position, AIDirection, Color.yellow, 2);
                            MDebug.DrawWireSphere(NearTransform.position, Color.green, 0.1f, 2f);
                            MDebug.Draw_Arrow(NearTransform.position, NearTransform.forward, Color.green, 2f);

                            if (FarTransform)
                            {
                                MDebug.DrawWireSphere(FarTransform.position, Color.red, 0.1f, 2f);
                                MDebug.Draw_Arrow(FarTransform.position, FarTransform.forward, Color.red, 2f);
                            }
                        }

                        if (OffMesh_Link.CompareTag("Fly"))
                        {
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Fly]</color>");
                            FlyOffMesh(FarTransform);
                        }
                        else if (OffMesh_Link.CompareTag("Climb"))
                        {
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Climb] -> {OffMesh_Link.transform.name}</color>");
                            ClimbOffMesh();
                        }
                        else if (OffMesh_Link.area == 2)  //2 is Off mesh Jump
                        {
                            animal.State_Activate(StateEnum.Jump);       //if the OffMesh Link is a Jump type activate the jump
                            Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [Jump]</color>");
                        }
                    }
                }
                else if (OMLData.linkType == OffMeshLinkType.LinkTypeJumpAcross)             //Means that it has a OffMesh Link component
                {
                    EndOffMeshPos = OMLData.endPos;
                    AIDirection = MTools.DirectionTarget(transform.position, EndOffMeshPos);
                    animal.Move(AIDirection);//Move where the AI DIRECTION FROM THE OFFMESH IS POINting

                    Debuging($"<color=white>is On a <b>[OffmeshLink]</b> -> [LinkTypeJumpAcross]</color>");

                    animal.State_Activate(StateEnum.Jump); //2 is Jump State
                }
                else if (OMLData.linkType == OffMeshLinkType.LinkTypeDropDown)
                {

                    EndOffMeshPos = OMLData.endPos;
                    Debug.DrawRay(OMLData.endPos, Vector3.up, Color.yellow, 2);

                    //This was causing issues on tiny slopes 
                    CompleteOffMeshLink();
                }

                return true;
            }
            return false;
        }





        /// <summary> Completes the OffmeshLink in case the animal was in one </summary>
        public virtual void CompleteOffMeshLink()
        {
            if (InOffMeshLink)
            {
                CompleteAgentOffMesh();

                InOffMeshLink = false;
                DestinationPosition = LastOffMeshDestination;   //restore the OffMesh Link
                CalculatePath();
                Move();

                Debuging($"<color=white>Complete <b>[OffmeshLink]</b></color>");
            }
        }

        protected virtual void CompleteAgentOffMesh()
        {
            if (Agent && Agent.isOnOffMeshLink)
                Agent.CompleteOffMeshLink();                    //Complete an offmesh link in case the Agent was in one
        }

        protected virtual void FlyOffMesh(Transform target)
        {
            ResetFreeMoveOffMesh();
            IFreeMoveOffMesh = C_FlyMoveOffMesh(target);
            StartCoroutine(IFreeMoveOffMesh);
        }

        protected virtual void ClimbOffMesh()
        {
            if (IClimbOffMesh != null) StopCoroutine(IClimbOffMesh);
            IClimbOffMesh = C_Climb_OffMesh();
            StartCoroutine(IClimbOffMesh);
        }


        /// <summary>Check if the The animal was moving on a Free OffMesh Link </summary>
        protected virtual void ResetFreeMoveOffMesh()
        {
            if (IFreeMoveOffMesh != null)
            {
                InOffMeshLink = false;
                StopCoroutine(IFreeMoveOffMesh);
                IFreeMoveOffMesh = null;
            }
        }

        protected virtual IEnumerator C_WaitToNextTarget(float time, Transform NextTarget)
        {
            IsWaiting = true;

            if (time > 0)
            {
                yield return null; //SUUUUUUUUUPER  IMPORTANT!!!!!!!!!
                Debuging($"<color=white> is waiting <B>{time:F2}</B> seconds to go to <B>[{NextTarget.name}]</B> → {DestinationPosition} </color>");

                animal.Move(AIDirection = Vector3.zero); //Stop the Animal
                yield return new WaitForSeconds(time);
            }
            SetTarget(NextTarget);
        }

        protected virtual IEnumerator C_FlyMoveOffMesh(Transform target)
        {
            animal.State_Activate(StateEnum.Fly); //Set the State to Fly
            InOffMeshLink = true;
            float distance = float.MaxValue;
            EndOffMeshPos = target.position;

            while (distance > StoppingDistance)
            {
                if (target == null) break;
                animal.Move((target.position - animal.transform.position).normalized * SlowMultiplier);
                distance = Vector3.Distance(animal.transform.position, target.position);
                yield return null;
            }
            animal.ActiveState.AllowExit();

            Debuging("Exit Fly State Off Mesh");

            InOffMeshLink = false;
        }

        protected virtual IEnumerator C_Climb_OffMesh()
        {
            animal.State_Activate(StateEnum.Climb); //Set the State to Climb
            InOffMeshLink = true;
            yield return null;
            ActiveAgent = false;

            EndOffMeshPos = target.position;

            while (animal.ActiveState.ID == StateEnum.Climb)
            {
                animal.SetInputAxis(Vector3.forward); //Move Upwards on the Climb
                yield return null;
            }

            Debuging("Exit Climb State Off Mesh");

            InOffMeshLink = false;

            IClimbOffMesh = null;
        }

        public void ResetStoppingDistance() => CurrentStoppingDistance = StoppingDistance;
        public void ResetSlowingDistance() => CurrentSlowingDistance = SlowingDistance;
        public float StopDistance() => StoppingDistance;
        public float SlowDistance() => SlowingDistance;

        public virtual void ValidateAgent()
        {
            if (agent == null) agent = gameObject.FindComponent<NavMeshAgent>();

            AgentTransform = (agent != null) ? agent.transform : transform;
        }


        protected virtual void Debuging(string Log) { if (debug) Debug.Log($"<B>[{animal.name} AI]</B> " + Log, this); }
        protected virtual void Debuging(string Log, GameObject obj) { if (debug) Debug.Log($"<B>[{animal.name} AI]</B> " + Log, obj); }

#if UNITY_EDITOR
        [HideInInspector] public int Editor_Tabs1;

        protected virtual void OnValidate()
        {
            if (animal == null) animal = gameObject.FindComponent<MAnimal>();
            ValidateAgent();
        }


        void Reset()
        {
            SetDefaulStopAgent();
        }

        void SetDefaulStopAgent()
        {
            StopAgentOn = new List<StateID>(3)
            {
                MTools.GetInstance<StateID>("Fall"),
                MTools.GetInstance<StateID>("Jump"),
                MTools.GetInstance<StateID>("Fly")
            };
        }

        private string CheckBool(bool val) => val ? "[X]" : "[  ]";

        protected virtual void OnDrawGizmos()
        {
            var isPlaying = Application.isPlaying;

            if (isPlaying && debugStatus)
            {
                string log = "\nTarget: [" + (Target != null ? Target.name : "-none-") + "]";
                log += "- NextTarget: [" + (NextTarget != null ? NextTarget.name : "-none-") + "]";
                log += "\nRemainingDistance: " + RemainingDistance.ToString("F2");
                log += "\nStopDistance: " + CurrentStoppingDistance.ToString("F2");
                log += "\n" + CheckBool(HasArrived) + " HasArrived";
                log += "\n" + CheckBool(ActiveAgent) + " Agent";
                log += "\n" + CheckBool(TargetIsMoving) + " Target is Moving";
                log += "\n" + CheckBool(IsAITarget != null) + "Target is AITarget";
                log += "\n" + CheckBool(IsWayPoint != null) + "Target is WayPoint";
                log += "\n" + CheckBool(IsWaiting) + " Waiting";
                log += "\n" + CheckBool(IsOnMode) + " On Mode";
                log += "\n" + CheckBool(FreeMove) + " Free Move";
                log += "\n" + CheckBool(InOffMeshLink) + " InOffMeshLink";

                var Styl = new GUIStyle(GUI.skin.box);
                Styl.normal.textColor = Color.white;
                Styl.fontStyle = FontStyle.Bold;
                Styl.alignment = TextAnchor.UpperLeft;


                UnityEditor.Handles.Label(transform.position, "AI Log:" + log, Styl);
            }
            if (!debugGizmos) return;


            //Paths
            if (Agent && ActiveAgent && Agent.path != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 1; i < Agent.path.corners.Length; i++)
                {
                    Gizmos.DrawLine(Agent.path.corners[i - 1], Agent.path.corners[i]);
                }
            }

            if (debugGizmos)
            {
                if (isPlaying)
                {
                    MDebug.Draw_Arrow(AgentTransform.position, AIDirection * 2, Color.white);

                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(DestinationPosition, stoppingDistance);
                }
                if (AgentTransform)
                {
                    var scale = animal ? animal.ScaleFactor : transform.lossyScale.y;
                    var Pos = (isPlaying) ? DestinationPosition : AgentTransform.position;
                    var Stop = (isPlaying) ? CurrentStoppingDistance : StoppingDistance * scale;
                    var Slow = (isPlaying) ? CurrentSlowingDistance : SlowingDistance * scale;



                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(AgentTransform.position, 0.1f);
                    if (Slow > Stop)
                    {
                        UnityEditor.Handles.color = Color.cyan;
                        UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, Slow);
                    }

                    UnityEditor.Handles.color = HasArrived ? Color.green : Color.red;
                    UnityEditor.Handles.DrawWireDisc(Pos, Vector3.up, Stop);
                }
            }
        }
#endif
    }

    #region Inspector


#if UNITY_EDITOR

    [CustomEditor(typeof(MAnimalAIControl), true), CanEditMultipleObjects]
    public class AnimalAIControlEd : Editor
    {
        private MAnimalAIControl M;

        protected SerializedProperty
            stoppingDistance, SlowingDistance, LookAtOffset, targett, UpdateAI, slowingLimit, targetHeight, StopOnTargetTooHigh, UseScale,
            agent, animal, PointStoppingDistance, OnEnabled, OnTargetPositionArrived, OnTargetArrived, disableInput, enableInput,
            OnTargetSet, debugGizmos, debugStatus, debug, Editor_Tabs1, nextTarget, OnDisabled, AgentTransform,// OffMeshAlignment,
            StopAgentOn, WaitTimeMult//, TurnAngle
            ;

        protected virtual void OnEnable()
        {
            M = (MAnimalAIControl)target;

            animal = serializedObject.FindProperty("animal");
            UseScale = serializedObject.FindProperty("UseScale");
            targetHeight = serializedObject.FindProperty("targetHeight");
            StopOnTargetTooHigh = serializedObject.FindProperty("StopOnTargetTooHigh");
            AgentTransform = serializedObject.FindProperty("AgentTransform");
            WaitTimeMult = serializedObject.FindProperty("waitTimeMult");
            disableInput = serializedObject.FindProperty("disableInput");
            enableInput = serializedObject.FindProperty("enableInput");
            GetAgentProperty();

            slowingLimit = serializedObject.FindProperty("slowingLimit");
            // TurnAngle = serializedObject.FindProperty("TurnAngle");

            OnEnabled = serializedObject.FindProperty("OnEnabled");
            OnDisabled = serializedObject.FindProperty("OnDisabled");

            OnTargetSet = serializedObject.FindProperty("OnTargetSet");
            OnTargetArrived = serializedObject.FindProperty("OnTargetArrived");
            OnTargetPositionArrived = serializedObject.FindProperty("OnTargetPositionArrived");
            stoppingDistance = serializedObject.FindProperty("stoppingDistance");
            PointStoppingDistance = serializedObject.FindProperty("PointStoppingDistance");
            SlowingDistance = serializedObject.FindProperty("slowingDistance");
            LookAtOffset = serializedObject.FindProperty("LookAtOffset");
            targett = serializedObject.FindProperty("target");
            nextTarget = serializedObject.FindProperty("nextTarget");
            //OffMeshAlignment = serializedObject.FindProperty("OffMeshAlignment");

            debugGizmos = serializedObject.FindProperty("debugGizmos");
            debugStatus = serializedObject.FindProperty("debugStatus");
            debug = serializedObject.FindProperty("debug");

            Editor_Tabs1 = serializedObject.FindProperty("Editor_Tabs1");
            StopAgentOn = serializedObject.FindProperty("StopAgentOn");

            UpdateAI = serializedObject.FindProperty("UpdateAI");


            if (M.StopAgentOn == null || M.StopAgentOn.Count == 0)
            {
                M.StopAgentOn = new(2) { MTools.GetInstance<StateID>("Fall"), MTools.GetInstance<StateID>("Fly") };
                StopAgentOn.isExpanded = true;
                MTools.SetDirty(M);
                serializedObject.ApplyModifiedProperties();
            }
        }

        public virtual void GetAgentProperty()
        {
            agent = serializedObject.FindProperty("agent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("AI Source. Moves the animal using an AI Agent");

            if (M.Agent != null && M.animal != null && M.Agent.transform == M.animal.transform)
            {
                EditorGUILayout.HelpBox("The NavMesh Agent needs to be attached to a child gameObject. " +
                    "It cannot be in the same gameObject as the Animal Controller", MessageType.Error);
            }


            EditorGUI.BeginChangeCheck();
            {
                // EditorGUILayout.BeginVertical(MalbersEditor.StyleGray);

                Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, new string[] { "General", "Events", "Debug" });

                int Selection = Editor_Tabs1.intValue;

                if (Selection == 0) ShowGeneral();
                else if (Selection == 1) ShowEvents();
                else if (Selection == 2) ShowDebug();


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Animal AI Control Changed");
                }
            }

           

            //   EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }
        private void ShowGeneral()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                targett.isExpanded = MalbersEditor.Foldout(targett.isExpanded, "Targets");

                if (targett.isExpanded)
                {
                    EditorGUILayout.PropertyField(targett, new GUIContent("Target", "Target to follow"));
                    EditorGUILayout.PropertyField(nextTarget, new GUIContent("Next Target", "Next Target the animal will go"));
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                {
                    UpdateAI.isExpanded = MalbersEditor.Foldout(UpdateAI.isExpanded, "AI Parameters");

                    if (UpdateAI.isExpanded)
                    {
                        // EditorGUILayout.LabelField("AI Parameters", EditorStyles.boldLabel);
                     
                        EditorGUILayout.PropertyField(UpdateAI, new GUIContent("Update Agent", " Recalculate the Path for the Agent every x seconds "));
                        EditorGUILayout.PropertyField(stoppingDistance, new GUIContent("Stopping Distance", "Agent Stopping Distance"));
                        EditorGUILayout.PropertyField(SlowingDistance, new GUIContent("Slowing Distance", "Distance to Start slowing the animal before arriving to the destination"));  
                        
                        EditorGUILayout.PropertyField(StopOnTargetTooHigh);
                       
                        if (StopOnTargetTooHigh.boolValue)
                            EditorGUILayout.PropertyField(targetHeight);

                        EditorGUILayout.PropertyField(UseScale);
                        
                        EditorGUILayout.PropertyField(LookAtOffset);
                        EditorGUILayout.PropertyField(PointStoppingDistance, new GUIContent("Point Stop Distance", "Stop Distance used on the SetDestination method. No Target Assigned"));

                        EditorGUILayout.PropertyField(slowingLimit);
                        EditorGUILayout.PropertyField(WaitTimeMult);
                        EditorGUILayout.PropertyField(disableInput);
                        EditorGUILayout.PropertyField(enableInput);
                        // EditorGUILayout.PropertyField(OffMeshAlignment);
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    if (M.Agent)
                    {
                        M.Agent.stoppingDistance = stoppingDistance.floatValue;
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                animal.isExpanded = MalbersEditor.Foldout(animal.isExpanded, "References");

                if (animal.isExpanded)
                {
                    EditorGUILayout.PropertyField(animal, new GUIContent("Animal", "Reference for the Animal Controller"));
                    EditorGUILayout.PropertyField(AgentTransform, new GUIContent("Agent", "Reference for the AI Agent Transform"));
                    //EditorGUILayout.PropertyField(agent, new GUIContent("Agent", "Reference for the Nav Mesh Agent")); 
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(StopAgentOn, new GUIContent($"{StopAgentOn.displayName} ({StopAgentOn.arraySize})"), true);

                    if (StopAgentOn.isExpanded && GUILayout.Button(new GUIContent("Set Default Off States", "By Default the AI should not be Active on Fly, Jump or Fall states"), GUILayout.MinWidth(150)))
                    {
                        M.StopAgentOn = new List<StateID>(3)
                    {
                        MTools.GetInstance<StateID>("Fall"),
                        MTools.GetInstance<StateID>("Jump"),
                        MTools.GetInstance<StateID>("Fly")
                    };
                        serializedObject.ApplyModifiedProperties();

                        Debug.Log("Stop Agent set to default: [Fall,Jump,Fly]");
                        MTools.SetDirty(target);
                    }
                    EditorGUI.indentLevel--;


                    M.ValidateAgent();

                    if (!M.AgentTransform)
                    {
                        EditorGUILayout.HelpBox("There's no Agent found on the hierarchy on this gameobject\nPlease add a NavMesh Agent Component", MessageType.Error);
                    }
                }
            }
        }

        private void ShowEvents()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(OnEnabled);
                EditorGUILayout.PropertyField(OnDisabled);
                EditorGUILayout.PropertyField(OnTargetPositionArrived, new GUIContent("On Position Arrived"));
                EditorGUILayout.PropertyField(OnTargetArrived, new GUIContent("On Target Arrived"));
                EditorGUILayout.PropertyField(OnTargetSet, new GUIContent("On New Target Set"));
            }
        }

        protected GUIStyle Bold(bool tru) => tru ? EditorStyles.boldLabel : EditorStyles.miniBoldLabel;

        private void ShowDebug()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUIUtility.labelWidth = 50f;
                    EditorGUILayout.PropertyField(debug, new GUIContent("Console"));
                    EditorGUILayout.PropertyField(debugGizmos, new GUIContent("Gizmos"));
                    EditorGUIUtility.labelWidth = 80f;
                    EditorGUILayout.PropertyField(debugStatus, new GUIContent("In-Game Log"));
                    EditorGUIUtility.labelWidth = 0f;
                }

                if (Application.isPlaying)
                {


                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.PropertyField(targett);
                        EditorGUILayout.ObjectField("Next Target", M.NextTarget, typeof(Transform), false);
                        EditorGUILayout.Vector3Field("Destination", M.DestinationPosition);
                        EditorGUILayout.Vector3Field("AI Direction", M.AIDirection);
                        EditorGUILayout.Space();
                        EditorGUILayout.FloatField("Current Stop Distance", M.StoppingDistance);
                        EditorGUILayout.FloatField("Remaining Distance", M.RemainingDistance);
                        EditorGUILayout.FloatField("Slow Multiplier", M.SlowMultiplier);

                        EditorGUILayout.Space();

                        EditorGUIUtility.labelWidth = 70;

                        using (new GUILayout.HorizontalScope())
                        {
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.ToggleLeft("Target is Moving", M.TargetIsMoving, Bold(M.TargetIsMoving));
                                EditorGUILayout.ToggleLeft("AI Is Moving", M.IsMoving, Bold(M.IsMoving));
                                EditorGUILayout.ToggleLeft("Target is AITarget", M.IsAITarget != null, Bold(M.IsAITarget != null));
                                EditorGUILayout.ToggleLeft("Target is WayPoint", M.IsWayPoint != null, Bold(M.IsWayPoint != null));
                                EditorGUILayout.Space();
                                EditorGUILayout.ToggleLeft("LookAt Target", M.LookAtTargetOnArrival, Bold(M.LookAtTargetOnArrival));
                                EditorGUILayout.ToggleLeft("Auto Next Target", M.AutoNextTarget, Bold(M.AutoNextTarget));
                                EditorGUILayout.ToggleLeft("UpdateDestinationPos", M.UpdateDestinationPosition, Bold(M.UpdateDestinationPosition));
                                EditorGUILayout.ToggleLeft("Is Target Too High", M.TargetTooHigh, Bold(M.UpdateDestinationPosition));

                                if (M.Agent && M.ActiveAgent)
                                {
                                    EditorGUILayout.ToggleLeft("Agent in NavMesh", M.Agent.isOnNavMesh, Bold(M.Agent.isOnNavMesh));
                                }
                            }

                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUILayout.ToggleLeft("Is On Mode", M.IsOnMode, Bold(M.IsOnMode));
                                EditorGUILayout.ToggleLeft("Free Move", M.FreeMove, Bold(M.FreeMove));
                                EditorGUILayout.ToggleLeft("In OffMesh Link", M.InOffMeshLink, Bold(M.InOffMeshLink));

                                EditorGUILayout.Space();
                                EditorGUILayout.ToggleLeft("Waiting", M.IsWaiting, Bold(M.IsWaiting));
                                EditorGUILayout.ToggleLeft("Has Arrived to Destination", M.HasArrived, Bold(M.HasArrived)); 
                                EditorGUILayout.ToggleLeft("Active Agent", M.ActiveAgent, Bold(M.ActiveAgent));
                               
                                if (M.Agent && M.ActiveAgent)
                                {
                                    EditorGUILayout.ToggleLeft("Agent in OffMesh", M.AgentInOffMeshLink, Bold(M.Agent.isOnNavMesh));
                                }
                            }
                        }

                        EditorGUIUtility.labelWidth = 0;

                        DrawChildDebug();

                        Repaint();
                    }
                }
            }
        }

        protected virtual void DrawChildDebug()
        { }

    }
#endif
    #endregion
}