﻿using UnityEngine;

namespace MalbersAnimations.Weapons
{
    public interface IWeaponManager
    {
        void Equip_Weapon();
        void Unequip_Weapon();
        void CheckAim();
        void FreeHandUse();
        void FreeHandRelease();
        void ExitByAnimation(bool value);
        Transform transform { get; }
        
        MWeapon Weapon { get; }
    }

    public interface IMWeapon : IMDamager, IObjectCore
    {
        /// <summary>Gets the Weapon ID Value</summary>
        WeaponID WeaponType { get; }
        /// <summary>Unique Weapon ID, Random numbers  diferenciate 2 weapons of the same Type</summary>
        int WeaponID { get; }
        /// <summary>This will give information to use if on the animation for Store and Draw weapon.</summary>
        int HolsterID { get; }
        /// <summary>Description to use on the UI for every weapon</summary>
        string Description { get; }
        
        /// <summary>Is the Weapon Right-Handed or Left-Handed</summary>
        bool IsRightHanded { get; }

        /// <summary>The Weapon will continue attacking if the Input is Down</summary>
        bool Automatic { get; set; }

        /// <summary>Minimum Damage the Weapon can casueto a Stat </summary>
        float MinDamage { get; }
        /// <summary>Maximum Damage the Weapon can casue to a Stat </summary>
        float MaxDamage { get; }
        /// <summary>Minimum Force the Weapon can casue to an object </summary>
        float MinForce { get; }
        /// <summary>Maximum Force the Weapon can casue to an object </summary>
        float MaxForce { get; }
        /// <summary> Is the Weapon Equiped </summary>
        bool IsEquiped { get; set; }
        /// <summary>Enables the Main Attack</summary>
        bool Input { get; set; }
        
        /// <summary>Reset all the Weapons Properties</summary>
        void ResetWeapon();
        /// <summary>Which Side the Weapon can Aim</summary>
        AimSide AimSide { set; get; }

        /// <summary>Transform to Set the Aim Origin</summary>
        Transform AimOrigin { get; }
        /// <summary>Owner of the Weapon</summary>
        IMWeaponOwner CurrentOwner { get; set; }
        /// <summary>Play the sounds clips</summary>
        /// <param name="ID">ID is the index on the list of clips</param>
        void PlaySound(int ID);

        System.Action<int> WeaponAction { get; set; }

        /// <summary> Make all the Calcultations to Restore ammo in the Chamber</summary>
        /// <returns>True = if it can reload</returns>
        bool TryReload();
    }

    public interface IShootableWeapon : IMWeapon
    {
        /// <summary>Projectile to Shoot</summary>
        GameObject Projectile { get; set; }

        /// <summary>Is the weapon Reloading</summary>
        bool IsReloading { get; set; }

        /// <summary> Total Ammo of the gun</summary>
        int TotalAmmo { get; set; }

        /// <summary> Ammo in Chamber</summary>
        int AmmoInChamber { get; set; }

        /// <summary>Size of the Chamber</summary>
        int ChamberSize { get; set; }
    }

    /// <summary>  Character who is currenlty using the weapon  </summary>
    public interface IMWeaponOwner  
    { 
        /// <summary>Character Animator</summary>
        Animator Anim { get; }

        /// <summary>is the Character aiming</summary>
        bool Aim { get; }

        /// <summary>is the Character Riding?</summary>
        bool IsRiding { get;}

        /// <summary>is the Character Reloading a weapon</summary>
        bool IsReloading { get; }

        /// <summary>is the Character Attacking or firing a projectile?</summary>
        bool IsAttacking { get; }

        /// <summary>is the Character Playing the Draw Weapon Animation</summary>
        bool DrawWeapon { get; }
        /// <summary>is the Character Playing the Store Weapon Animation</summary>
        bool StoreWeapon { get; }

        /// <summary>is the Character an Animal Controller?</summary>
        bool HasAnimal { get; }

        /// <summary>Aim Horizontal angle regarding the camera</summary>
        float HorizontalAngle { get; }

        /// <summary> Get the Aimer Component of the Character</summary>
        IAim Aimer { get; }

        /// <summary>Direction the weapon is Aiming</summary>
        Vector3 AimDirection { get; }

        /// <summary> Returns where is the Camera regarding the player, False: Right; True: Left</summary>
        bool AimingSide { get; }

        /// <summary>IK Weight for IK Modifications</summary>
        float IKAimWeight { get; set; }

        GameObject Owner { get; }

        /// <summary>Equiped/Active Weapon on the Character</summary>
        MWeapon Weapon { get; }

        //Transform RightShoulder {get;}
        //Transform LeftShoulder {get;}
        //Transform Chest {get;}
        Transform RightHand { get; }
        Transform LeftHand { get; }

        //Transform Head { get;}

        /// <summary>External Transform to be use so the Rider does not hurt the Mount</summary>
        Transform IgnoreTransform { get; set; }


        /// <summary>Set Aiming on the Weapon Owner</summary>
        void Aim_Set(bool value);
        void UnEquip();
    }

}
