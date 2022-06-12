﻿using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using VRage.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using Sandbox.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox;
using SpaceEngineers;
using VRage.Game;

namespace nubeees_refFrame
{
    /// <summary>
    /// Effectively a wrapper around entities. Adds in an apparent velocity which
    /// can be detached from the object's world velocity.
    /// </summary>
    public class FrameEntity
    {
        // This is the entity this FrameEntity attaches to.
        public IMyEntity entity;

        // Total apparent velocity.
        public Vector3D apparentVelocity;

        // The fake velocity of this entity
        public Vector3D fakeVelocity;

        // For merging behavior. Big WIP.
        public float radius; 


        /// <summary>
        /// Constructor for a FrameEntity wrapper around IMyEntity.
        /// </summary>
        /// <param name="_entity">Please god just don't ever pass in a null.</param>
        /// <param name="_radius">Radius of this effector. May be adjustable in the future.</param>
        public FrameEntity(IMyEntity _entity, float _radius = 10.0f)
        {
            entity = _entity;
            radius = _radius;

            // TO-DO: Physics sanity checking needs to get pretty angry here if it fails. I'll leave it absent for now so it screams bloody mary if anything at all goes wrong.
            fakeVelocity = Vector3D.Zero;
            apparentVelocity = entity.Physics.LinearVelocity;
        }

        /// <summary>
        /// Use this to update fake velocity and update apparent velocity accordingly.
        /// </summary>
        public void SetFakeVelocity(Vector3D _fakeVelocity)
        {
            fakeVelocity = _fakeVelocity;
            UpdateApparentVelocity();
        }
        
        /// <summary>
        /// Use this to alter real velocity and automatically update apparent velocity accordingly. 
        /// </summary>
        public void SetRealVelocity(Vector3D _realVelocity)
        {
            entity.Physics.LinearVelocity = _realVelocity;
            UpdateApparentVelocity();
        }

        /// <summary>
        /// Helper for modifying real velocity. Adds _additionalVelocity to the current real velocity and updates apparent velocity.
        /// </summary>
        public void AddToRealVelocity(Vector3D _additionalVelocity)
        {
            entity.Physics.LinearVelocity += _additionalVelocity;
            UpdateApparentVelocity();
        }

        /// <summary>
        /// Update the apparent velocity based on this effector's real and fake velocity.
        /// This is done because many things ingame can alter real velocity.
        /// </summary>
        public void UpdateApparentVelocity()
        {
            apparentVelocity = fakeVelocity + entity.Physics.LinearVelocity;
        }

        public void MakeAllVelocityFake()
        {
            SetFakeVelocity(apparentVelocity);
            SetRealVelocity(Vector3D.Zero);
        }

        public void MakeAllVelocityReal()
        {
            SetRealVelocity(apparentVelocity);
            SetFakeVelocity(Vector3D.Zero);
        }
    }
}
