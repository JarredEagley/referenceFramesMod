﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common.ObjectBuilders;
using VRage.Common;
using SpaceEngineers;
using VRage.Game.Components;
using VRage.Game;
using VRage.ModAPI;
using VRageMath;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;

namespace nubeees_refFrame
{
    class spintest
    {
        IMyVoxelMap voxel;

        Vector3D centerOfRotation;
        Vector3D rotationVector;

        MatrixD rotationMatrix;
        List<IMyEntity> entitiesInRange = new List<IMyEntity>();


        public spintest(IMyVoxelMap _voxel)
        {
            Util.DebugMessage("spinny constructor called");
            voxel = _voxel;
            Init();
        }

        public void Init()
        {
            rotationVector = new Vector3D(1, 1, 1); rotationVector.Normalize();

            centerOfRotation = voxel.PositionComp.GetPosition();
            MatrixD translate = MatrixD.CreateTranslation(centerOfRotation);
            MatrixD translateinv = MatrixD.Invert(translate);

            rotationMatrix = translateinv * MatrixD.CreateFromAxisAngle(rotationVector, 0.004) * translate;
        }

        public void Update()
        {
            //Util.DebugMessage("updating spintest");
            voxel.Teleport(voxel.WorldMatrix * rotationMatrix);
            foreach (var entity in entitiesInRange)
            {
                entity.Teleport(entity.WorldMatrix * rotationMatrix);
                if (entity is IMyCharacter)
                {
                    var character = entity as IMyCharacter;
                    // just pointing toward center for now.
                    var axisDisplacement = centerOfRotation - entity.GetPosition();
                    
                    if (character.CurrentMovementState == MyCharacterMovementEnum.Falling)
                    {
                        Vector3D rotateCharacter = Vector3D.Cross(axisDisplacement, character.WorldMatrix.Up);
                        var tr = MatrixD.CreateTranslation(character.GetPosition());
                        character.Teleport(MatrixD.Orthogonalize( character.WorldMatrix * MatrixD.Invert(tr) * MatrixD.CreateFromAxisAngle(rotateCharacter, 0.0001) * tr ));

                    }
                }
            }
        }

        public void UpdateSlow()
        {
            BoundingSphereD boundingSphere = voxel.WorldVolume;
            
            // Handle removals.
            for (int i = entitiesInRange.Count-1; i >= 0; --i)
            {
                var trackedEntity = entitiesInRange[i];
                var r2 = boundingSphere.Radius * boundingSphere.Radius;
                var relativeposition = boundingSphere.Center - trackedEntity.PositionComp.GetPosition();
                if (relativeposition.LengthSquared() > r2+10)
                {
                    entitiesInRange.RemoveAt(i);
                }
            }

            // Additions.
            var tempList = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref boundingSphere);
            foreach (var entity in tempList)
            {
                if (entity.EntityId == (voxel.EntityId)) continue;
                if (entitiesInRange.Contains(entity)) continue;
                entitiesInRange.Add(entity);
            }
        }
    }
}