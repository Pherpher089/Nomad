﻿using UnityEngine;
using System.Collections;
 

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MalbersAnimations.Weapons
{
    [AddComponentMenu("Malbers/Weapons/MBow")]
    public class MBow : MShootable
    {
        #region Bow Stuff
        public Transform knot;                                  //Point of the bow to put the arrow (End)

        public bool KnotToHand { get; set; }
        /// <summary>Max Bending the Bow can do</summary>
        public float MaxTension;
        [Range(0, 1)] public float BowTensionPrev;


        public Transform[] UpperBn;                             //Upper Chain of the bow
        public Transform[] LowerBn;                             //Upper Chain of the bow

        /// <summary> Default Rotation of the Upper Bow Bones </summary>
        [SerializeField] private Quaternion[] UpperBnInitRot;
        /// <summary> Default Rotation of the Lower Bow Bones </summary>
        [SerializeField] private Quaternion[] LowerBnInitRot;

        /// <summary> Maximun Rotation of the Upper Bow Bones </summary>
        [SerializeField] private Quaternion[] UpperBnMaxRot;
        /// <summary> Maximun Rotation of the Lower Bow Bones </summary>
        [SerializeField] private Quaternion[] LowerBnMaxRot;

        [Tooltip("Default position of the Knot to return to when the String is on its default position")]
        public Vector3 DefaultPosKnot;
        public Vector3 KnotHandOffset;

        public Vector3 RotUpperDir = -Vector3.forward;
        public Vector3 RotLowerDir = Vector3.forward;

        /// <summary> Is the Bow Bones Setted Correctly?... Is this bow functional?? </summary>
        public bool BowIsSet = false;

        public override bool IsAiming
        {
            get => base.IsAiming;
            set
            {
                base.IsAiming = value;

                if (!value) 
                {
                    DestroyProjectileInstance(); //IF you finish aiming then destroy the Instance of the projectile
                }
            }
        }


        public override bool IsEquiped
        {
            get => base.IsEquiped;
            set
            {
                base.IsEquiped = value;

                if (value) BowKnotToHand(false);//Restore the Knot when the weapon is equipped for the first time
                else
                {
                    DestroyProjectileInstance(); //IF you unequip the bow then destroy the Instance of the projectile
                }
            }
        }



        #endregion

        public virtual void SerializeBow()
        {
            if (UpperBn == null || LowerBn == null)
            {
                Debug.LogWarning("Please fill the Upper and Low Joints on the Bow");
                BowIsSet = false;
                return;
            }

            if (UpperBn.Length == 0 || LowerBn.Length == 0)
            {
                Debug.LogWarning("Please fill the Upper and Low Joints on the Bow");
                BowIsSet = false;
                return;
            }

            ChargeCurrentTime = 0;

            UpperBnInitRot = new Quaternion[UpperBn.Length];   //Get the Initial Upper ChainRotation
            LowerBnInitRot = new Quaternion[LowerBn.Length];    //Get the Initial Lower ChainRotation

            UpperBnMaxRot = new Quaternion[UpperBn.Length];   //Get the Initial Upper ChainRotation
            LowerBnMaxRot = new Quaternion[LowerBn.Length];    //Get the Initial Lower ChainRotation

            for (int i = 0; i < UpperBn.Length; i++)
            {
                if (UpperBn[i] == null)
                {
                    BowIsSet = false;
                    return;
                }
                UpperBnInitRot[i] = UpperBn[i].localRotation;
                UpperBnMaxRot[i] = Quaternion.Euler(RotUpperDir * MaxTension) * UpperBnInitRot[i];
            }
            for (int i = 0; i < LowerBn.Length; i++)
            {
                if (LowerBn[i] == null)
                {
                    BowIsSet = false;
                    return;
                }
                LowerBnInitRot[i] = LowerBn[i].localRotation;
                LowerBnMaxRot[i] = Quaternion.Euler(RotLowerDir * MaxTension) * LowerBnInitRot[i];

            }

            BowIsSet = true;
            Debug.Log("The Initial Position and Rotation of the bow has been stored corretly");
        }

        public override void FreeHandUse() => BowKnotToHand(true);
        public override void FreeHandRelease () => BowKnotToHand(false);


        /// <summary> Called by the Animator</summary>
        public virtual void BowKnotToHand(bool enabled)
        {
           // Debug.Log("BowKnotToHand = " + enabled);
            FreeHand = !enabled;
            KnotToHand = enabled;
            if (!KnotToHand)
            {
                RestoreKnot();
            }
        }

        /// <summary>Updates the BowKnot position in the center of the hand if is active</summary>
        protected void BowKnotInHand(IMWeaponOwner RC)
        {
            if (RC == null) return;
            if (!RC.StoreWeapon && !RC.DrawWeapon && KnotToHand)

            {
                knot.position = IsRightHanded ?
                    RC.LeftHand.TransformPoint(KnotHandOffset) :
                    RC.RightHand.TransformPoint(KnotHandOffset);

                //knot.SetPositionAndRotation(IsRightHanded ?
                //    RC.LeftHand.TransformPoint(KnotHandOffset) :
                //    RC.RightHand.TransformPoint(KnotHandOffset),
                //    Quaternion.LookRotation((AimOrigin.position - knot.position).normalized, -Gravity));
            }
        }

        /// <summary>Rotate and modify the bow Bones to bend it from Min = 0 to Max = 1</summary>
        public virtual void BendBow(float normalizedTime)
        {
            if (!BowIsSet) return;

            for (int i = 0; i < UpperBn.Length; i++)
            {
                UpperBn[i].localRotation = Quaternion.Lerp(UpperBnInitRot[i], UpperBnMaxRot[i], normalizedTime);  //Bend the Upper Chain on the Bow
            }

            for (int i = 0; i < LowerBn.Length; i++)
            {
                LowerBn[i].localRotation = Quaternion.Lerp(LowerBnInitRot[i], LowerBnMaxRot[i], normalizedTime);  //Bend the Lower Chain of the Bow
            }

            if (knot && AimOrigin) Debug.DrawRay(knot.position, knot.forward, Color.red);
        }

        /// <summary>  Is called  when the Rider is not holding the string of the bow  </summary>
        public virtual void RestoreKnot()
        {
            knot.localPosition = DefaultPosKnot;
        }

        /// <summary> Charge the Weapon!! </summary>
        internal override void Attack_Charge(IMWeaponOwner RC, float time)
        {
            base.Attack_Charge(RC, time);
            if (IsCharging) BendBow(ChargedNormalized);    //Bend the Bow
        }

        public override void ResetCharge()
        {
            base.ResetCharge();
            BendBow(0);
            if (Sounds.Length > 5 && m_audio.isPlaying && m_audio.clip == Sounds[5]) m_audio.Stop();
        }
         

        internal override void Weapon_LateUpdate(IMWeaponOwner RC)
        {
            base.Weapon_LateUpdate(RC);
           
         
            BowKnotInHand(RC);
            knot.rotation = Quaternion.LookRotation((AimOrigin.position - knot.position).normalized, -Gravity); //Align
        }

        internal override void StoringWeapon()
        {
            RestoreKnot();
        }

        /// CallBack from the RiderCombat Layer in the Animator to reproduce a sound on the weapon
        public override void PlaySound(int ID)
        {
            if (ID < Sounds.Length && Sounds[ID] != null)
            {
                var newSound = Sounds[ID];

                if (m_audio && !playingSound && gameObject.activeInHierarchy)
                {
                    if (ID == 5 && CanCharge)                                    //THIS IS THE SOUND FOR BEND THE BOW
                    {
                        m_audio.pitch = 1.03f / ChargeTime;
                        StartCoroutine(BowChargeTimePlay(newSound));
                    }
                    else
                    {
                        m_audio.Stop();
                        m_audio.pitch = 1;
                        //HACK FOR THE SOUND
                        this.Delay_Action(2, () =>
                        {
                            m_audio.PlayOneShot(newSound);
                            playingSound = false;
                        }
                        );
                    }
                }
            }
        }

        IEnumerator BowChargeTimePlay(AudioClip sound)
        {
            while (ChargedNormalized == 0) yield return null;

            m_audio.PlayOneShot(sound);
        }

        public override void ResetWeapon()
        {
            base.ResetWeapon();
            RestoreKnot();
        }

       

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            AimLimit = new RangedFloat(-90, 180);
        }
#endif

        //Editor variables
        [HideInInspector] public bool BonesFoldout, proceduralfoldout;
        [HideInInspector] public int LowerIndex, UpperIndex;
    }

    #region Inspector

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(MBow))]
    public class MBowEditor : MShootableEditor
    {
        readonly string[] axis = { "+X", "-X", "+Y", "-Y", "+Z", "-Z" };
        SerializedProperty
              UpperBn, LowerBn, UpperIndex, BowTension, LowerIndex, MaxTension, DefaultPosKnot, knot, MaxArmTension, KnotHandOffset,
              BonesFoldout, RotUpperDir, RotLowerDir, BowIsSet;

        MBow mBow;

        private void OnEnable()
        {
            mBow = (MBow)target;
            mShoot = (MBow)target;

            SetOnEnable();
            Tabs2 = new string[] { "Bow", "Shootable", "Sounds", "Events" };

            MaxArmTension = serializedObject.FindProperty("MaxArmTension");
            //   AimWeight = serializedObject.FindProperty("AimWeight");

            UpperBn = serializedObject.FindProperty("UpperBn");
            BowIsSet = serializedObject.FindProperty("BowIsSet");
            knot = serializedObject.FindProperty("knot");
            KnotHandOffset = serializedObject.FindProperty("KnotHandOffset");
            DefaultPosKnot = serializedObject.FindProperty("DefaultPosKnot");
            LowerBn = serializedObject.FindProperty("LowerBn");
            UpperIndex = serializedObject.FindProperty("UpperIndex");
            LowerIndex = serializedObject.FindProperty("LowerIndex");
            BowTension = serializedObject.FindProperty("BowTensionPrev");
            MaxTension = serializedObject.FindProperty("MaxTension");
            BonesFoldout = serializedObject.FindProperty("BonesFoldout");
            RotUpperDir = serializedObject.FindProperty("RotUpperDir");
            RotLowerDir = serializedObject.FindProperty("RotLowerDir");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            MalbersEditor.DrawDescription("Bow Weapons Properties");
            WeaponInspector(false);
            serializedObject.ApplyModifiedProperties();
        }


        protected override void WeaponInspector(bool showAim = true)
        {
            Editor_Tabs1.intValue = GUILayout.Toolbar(Editor_Tabs1.intValue, Tabs1);
            if (Editor_Tabs1.intValue != Tabs1.Length) Editor_Tabs2.intValue = Tabs2.Length;

            Editor_Tabs2.intValue = GUILayout.Toolbar(Editor_Tabs2.intValue, Tabs2);
            if (Editor_Tabs2.intValue != Tabs2.Length) Editor_Tabs1.intValue = Tabs1.Length;


            //First Tabs
            int Selection = Editor_Tabs1.intValue;
            if (Selection == 0) DrawWeapon(showAim);
            else if (Selection == 1) { DrawDamage(); DrawStatModifiers(); }
            else if (Selection == 2) DrawIK();
            else if (Selection == 3) DrawExtras();


            //2nd Tabs
            Selection = Editor_Tabs2.intValue;
            if (Selection == 0) DrawBow();
            else if (Selection == 1) DrawAdvancedWeapon();
            else if (Selection == 2) DrawSound();
            else if (Selection == 3) DrawEvents();
        }



        protected void DrawBow()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.indentLevel++;

                BonesFoldout.boolValue = EditorGUILayout.Foldout(BonesFoldout.boolValue, new GUIContent("Bow Joints", "All References for the Bow Bones"));
                if (BonesFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(knot, new GUIContent("Knot", "Transform reference for the Bow middle String point"));
                    EditorGUILayout.PropertyField(KnotHandOffset, new GUIContent("Knot Hand Offset", "Offset to Position the Knot to the Hand"));

                    if (knot.objectReferenceValue != null)
                    {
                        using (new GUILayout.HorizontalScope())

                        {
                                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                                    EditorGUILayout.PropertyField(DefaultPosKnot, new GUIContent("Def Knot Pos"));

                            if (GUILayout.Button(new GUIContent("D", "Sets the Default position of the Knot Point"), EditorStyles.miniButton, GUILayout.Width(25)))
                            {
                                DefaultPosKnot.vector3Value = (knot.objectReferenceValue as Transform).localPosition;
                            }
                        }
                    }
                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.PropertyField(UpperBn, new GUIContent("Upper Chain", "Upper bone chain of the bow"), true);
                        EditorGUILayout.PropertyField(LowerBn, new GUIContent("Lower Chain", "Lower bone chain of the bow"), true);
                    }
                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Store Initial Bow Position|Rotation"))
                {
                    mBow.SerializeBow();
                    EditorUtility.SetDirty(mBow);
                    serializedObject.ApplyModifiedProperties();
                }
            }


            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                UpperIndex.intValue = EditorGUILayout.Popup("Upper Rot Axis", UpperIndex.intValue, axis);
                LowerIndex.intValue = EditorGUILayout.Popup("Lower Rot Axis", LowerIndex.intValue, axis);
            }

            RotUpperDir.vector3Value = Axis(UpperIndex.intValue);
            RotLowerDir.vector3Value = Axis(LowerIndex.intValue);

            EditorGUI.BeginChangeCheck();
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(MaxTension, new GUIContent("Max Tension", "Max Angle that the Bow can Bent"));

                    if (BowIsSet.boolValue)
                    {
                        EditorGUILayout.PropertyField(BowTension, new GUIContent("Bow Tension (P)", "Previews the Bow tension"));
                        if (BowTension.floatValue > 0)
                            EditorGUILayout.HelpBox("This is for visual purpose only, please return the Bow Tension to 0", MessageType.Warning);
                    }
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                if (MaxTension.floatValue < 0) MaxTension.floatValue = 0;

                if (BowIsSet.boolValue)
                    mBow.BendBow(BowTension.floatValue);

                EditorUtility.SetDirty(mBow);
            }
        }

        protected override string CustomEventsHelp()
        {
            return "\n\nOn Load Arrow: Invoked when the arrow is instantiated.\n (GameObject) the instance of the Arrow. \n\nOnHold: Invoked when the bow is being bent (0 to 1)\n\nOn Release Arrow: Invoked when the Arrow is released.\n (GameObject) the instance of the Arrow.";
        }
        Vector3 Axis(int Index)
        {
            return Index switch
            {
                0 => Vector3.right,
                1 => -Vector3.right,
                2 => Vector3.up,
                3 => -Vector3.up,
                4 => Vector3.forward,
                5 => -Vector3.forward,
                _ => Vector3.zero,
            };
        }
    }

#endif
    #endregion
}