﻿using System;
using UnityEngine;


namespace MalbersAnimations
{
    /// <summary> Interface to identify Projectiles   </summary>
    public interface IProjectile : IMLayer
    {
        /// <summary>Initial Velocity (Direction * Power) </summary>
        Vector3 Velocity { get; set; }

        /// <summary>Gravity Direction Vector</summary>
        Vector3 Gravity { get; set; }

        /// <summary>Apply Gravity after certain distance is reached</summary>
        float AfterDistance { get; set; }

        /// <summary>Offset to position the projectile</summary>
        Vector3 PosOffset { get; set; }

        /// <summary>Offset to rotate the projectile </summary>
        Vector3 RotOffset { get; set; } 

        /// <summary>Has the projectile impacted with something</summary>
        bool HasImpacted { get; set; }

        /// <summary>Prefab to instantiate when the projectile has collider with something</summary>
        GameObject HitEffect { get; set; }

        /// <summary>Prepares the Projectile to be fired</summary>
        void Prepare(GameObject Owner, Vector3 Gravity, Vector3 ProjectileVelocity, LayerMask HitLayer, QueryTriggerInteraction triggerInteraction);

        /// <summary>Multiplies the Damage for a value, Same as the MDamager</summary>
        void SetDamageMultiplier(float multiplier);

        /// <summary>Prepares the Bullet Damage</summary>
        void PrepareDamage(StatModifier modifier, float CriticalChance, float CriticalMultiplier, StatElement element);

        /// <summary>Fires the Projectile after being prepared</summary>
        void Fire();

        /// <summary>Fires the Projectile after being prepared</summary>
        void Fire(Vector3 Velocity);
    }

    /// <summary>Interface to Identify Thrower components</summary>
    public interface IThrower
    {
        /// <summary>Gravity Direction Vector</summary>
        Vector3 Gravity { get; }

        /// <summary>Apply Gravity after certain distance is reached</summary>
        float AfterDistance { get; set; }

        /// <summary>Starting Position for the  Projectile Launch</summary>
        Vector3 AimOriginPos { get; } 

        /// <summary>Start Position of the Thrower</summary>
        Transform AimOrigin { get; }

        /// <summary> Projectile Launch Velocity(v3) Direction*Power </summary>
        Vector3 Velocity { get; }

        /// <summary>Sends if the Trajectory can be predicted or not</summary>
        Action<bool> Predict { get; set; }

        /// <summary>Layers to Interact</summary>
        LayerMask Layer { get; set; }
        QueryTriggerInteraction TriggerInteraction { get; set; }

        GameObject Owner { get; }

        /// <summary>Shoot the projectile </summary>
        void Fire();

        /// <summary>Set a projectile to shoot</summary>
        void SetProjectile(GameObject newProjectile);
    }
}