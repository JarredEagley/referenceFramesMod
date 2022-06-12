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
        public List<FrameEntity> effectorEntities = new List<FrameEntity>();  // Entities which help define the reference frame. Can be empty. 

        // No acceleration terms yet.

        // Data which defines behavior of the reference frame.
        public float radius;
        // public float radiusSquared;
        public Vector3D position;
        public Vector3D velocity;

        private Vector3D deltaVelocity;
        private Vector3D deltaPositon;

        // Constructors
        public ReferenceFrame(Vector3D pos, Vector3D vel, float _radius)
        {
            Util.DebugMessage("Reference frame constructed.");
            this.radius = _radius;
            this.position = pos;
            this.velocity = vel;
        }

        public ReferenceFrame(IMyEntity parent, float _radius)
        {
            Util.DebugMessage("Reference frame constructed.");
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
                FrameEntity effectorEntity = new FrameEntity(entity, 100); // RADIUS?? THIS WAS NEVER THOUGHT THROUGH TOO WELL
                this.effectorEntities.Add(effectorEntity);
            }
        }
        public void AddToReferenceFrame(FrameEntity entity, bool isEffector = false)
        {
            // TODO
        }

        public bool RemoveFromReferenceFrame(FrameEntity effectorEntity)
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
