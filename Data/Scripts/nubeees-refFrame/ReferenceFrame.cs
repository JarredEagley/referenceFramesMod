using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using VRage.ModAPI;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using SpaceEngineers.ObjectBuilders;
using Sandbox.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox;
using SpaceEngineers;
using VRage.Game.Components;

namespace nubeees_refFrame
{
    public class ReferenceFrame
    {
        public uint updatePriority = 0;

        public List<IMyEntity> effectedEntities = new List<IMyEntity>();   
        public List<EffectorEntity> effectorEntities = new List<EffectorEntity>();  // Entities which help define the reference frame. Can be empty. 
        // ^ this might need to become a dictionary. It's in a place of uncertainty currently.

        // TODO
        //private const float decayTime = 1000; // Maybe data drive eventually?
        //private float decayTimer = 0;

        // Data which defines behavior of the reference frame.
        public float radius;
        public float radiusSquared; // TO-DO
        public Vector3D position;
        public Vector3D velocity;
        // public Vector3D acceleration; // Not sure I'll use this??

        private Vector3D deltaVelocity;
        private Vector3D deltaPositon;
        // private Vector3 deltaAcceleration;

        // Constructors
        public ReferenceFrame(Vector3D pos, Vector3D vel, float _radius)
        {
            this.radius = _radius;
            this.position = pos;
            this.velocity = vel;
        }

        public ReferenceFrame(IMyEntity parent, float _radius)
        {
            //this.parentEntity = parent;
            this.radius = _radius;
        }


        /// <summary>
        /// This is the main update function. Whenever a reference frame
        /// is to be update, this is what's used.
        /// </summary>
        public void Update()
        {
            Util.DrawDebugSphere(this.position, this.radius);

            // Determine what is in range.

            // Do any necessary merging. ?? Might belong in server.cs...

            // Perform pseudophysics integrations.
            Integrate();
        }

        private void FindEntitiesInRange()
        {
            /*
                note to self: do some intersection tests
            in server.cs using a radius squared (which i need to add).

            if two bubbles' radius squared overlaps, then test for their
            actual distance overlap.
                -- consider: expanding further w/ apparent velocity so that
                    fast moving objects are more likely to find eachother.
                    Could be an optional thing. Could be worth experimenting with.
                    Maybe not. I don't know yet.

            If two bubbles are touching at all, merge them into one superbubble
            that has a radius equal to the two radii added together.

            This should be robust enough to handle both merging and
            unmerging-- the latter happening when an effector reaches the edge
            of the containing bubble.
                -- Maybe base it on the radii? Seems more sound.
                -- with r1, r2, rAdd:
                    -- for bubble 1:
                    -- if effector 1's position is greater than r1 distance from
                       combined reference frame bubble, then split?
                        -- need to think this through more when less sleepy.
             
            tl;dr of this is that server.cs will be a lot busier.

            thing to think about: Where is drawing handled? client? Probably client.
            Drawing the reference frame bounds is not only useful, but necessary.

            Thing to think about: entities like grids which can 'carry' other unparented
            entities with them need to switch reference frame together with all the stuff
            overlapping.
             */


            BoundingSphereD bounds = new BoundingSphereD(position, radius); 
            effectedEntities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounds); // Don't need to get individual blocks.
        }

        /// <summary>
        /// Perform physics integrations on this reference frame. Uses own delta-time in order to account for slow 'LOD' updating.
        /// Logic will be slightly different with/without effector entities.
        /// </summary>
        private void Integrate()
        {
            // If no effectors, follow a newtonian trajectory. No accounting for gravity yet.
            if (effectorEntities.Count == 0)
            {
                deltaPositon = velocity * VRage.Game.MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS;
                position += deltaPositon;
            }
            // If effectors exist, average their position and velocity. TO-DO: THIS IS TOTALLY BROKEN!!! Averaging position will work but velocity not.
            else 
            {
                Util.DebugMessage("This should not execute.");
                // /!\ THIS IS ALL BROKEN CODE /!\ //


                var oldPosition = position;
                var oldVelocity = velocity;

                // Average out effector data.
                position = Vector3D.Zero;
                velocity = Vector3D.Zero;

                float temp = 0.0f; // Could be an int, but don't want to even think about integer division problems.
                foreach (var effector in this.effectorEntities)
                {
                    if (effector.entity.Physics == null)
                        continue;

                    this.position += effector.entity.GetPosition();
                    this.velocity += effector.apparentVelocity;
                    temp += 1.0f;
                }
                // Sanity check against division by zero.
                if (temp > 0.0f)
                {
                    this.position /= temp;
                    this.velocity /= temp;
                }

                // A bit backwards-- find the deltas.
                deltaPositon = position - oldPosition;
                deltaVelocity = velocity - oldVelocity;
            }

            FindEntitiesInRange(); // Might want to slow update this.
            IntegrateHelper_UpdateEntities();
        }

        /// <summary>
        /// Helper function for integration which applies position and velocity updates to all effected entities.
        /// </summary>
        private void IntegrateHelper_UpdateEntities()
        {

            foreach (var entity in effectedEntities)
            {
                entity.SetPosition(entity.GetPosition() + (velocity * VRage.Game.MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS));
            }

            return;
            foreach (var entity in effectedEntities)
            {
                var entPosition = entity.GetPosition();
                entity.SetPosition(entPosition - deltaPositon);

                // Physics sanity check.
                if (entity.Physics == null)
                    continue;

                entity.Physics.LinearVelocity -= deltaVelocity;
            }
        }


        public void AddToReferenceFrame(IMyEntity entity)
        {
            this.effectedEntities.Add(entity);
        }
        public void AddToReferenceFrame(IMyEntity entity, bool isEffector = false)
        {
            AddToReferenceFrame(entity);
            if (isEffector)
            {
                // Add as an effector too.
                EffectorEntity effectorEntity = new EffectorEntity(entity, 100); // RADIUS?? THIS WAS NEVER THOUGHT THROUGH TOO WELL
                this.effectorEntities.Add(effectorEntity);
            }
        }
        public void AddToReferenceFrame(EffectorEntity entity, bool isEffector = false)
        {
            // TODO
        }

        public bool RemoveFromReferenceFrame(EffectorEntity effectorEntity)
        {
            // Always make sure its removed from effectors.
            effectorEntities.Remove(effectorEntity); // Just in case. Less efficient, though.
            
            // Remove from being effected
            bool exists = effectedEntities.Remove(effectorEntity.entity);
            // Wasn't found in this reference frame.
            if (!exists)
            {
                return false;
            }

            // Apply transition into world physics
            if (effectorEntity.entity.Physics != null)
            {
                effectorEntity.entity.Physics.LinearVelocity += this.velocity; // TO-DO: Parented entities? Other entities inside that entity's bounding box? Lots of edge cases to consider here.
            }

            return true;
        }

    }
}
