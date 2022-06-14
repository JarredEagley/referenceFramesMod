using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using VRage.ModAPI;
using VRage.Game.Components;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using SpaceEngineers.ObjectBuilders;
using VRage.Game.SessionComponents;
using Sandbox.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox;
using SpaceEngineers;
using VRage.Game;
using VRage.Game.ModAPI;
using Sandbox.Engine.Physics;

namespace nubeees_refFrame
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class Server : MySessionComponentBase
    {
        public List<ReferenceFrame> referenceFrames = new List<ReferenceFrame>(); // Everything lumped into one list right now. Suffling to be done in the future. Priority system will be needed.

        public List<spintest> spinnyvoxels = new List<spintest>();

        private uint tick = 0;

        private bool isInitialized = false;

        private bool IGNOREPLAYER = false;

        public override void LoadData()
        {
            base.LoadData();
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(Util.MOD_ID, AdminCommandHandler);
            MyAPIGateway.Entities.OnEntityAdd -= EntityAddHandler;
        }

        private void Init()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Util.MOD_ID, AdminCommandHandler);
            MyAPIGateway.Entities.OnEntityAdd += EntityAddHandler;
            Util.DebugMessage("Server init done!");
        }


        public override void UpdateBeforeSimulation()
        {

            if (MyAPIGateway.Session == null)
                return;

            var isHost = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE
                || MyAPIGateway.Multiplayer.IsServer;
            //var isDedicatedHost = isHost && MyAPIGateway.Utilities.IsDedicated;

            try
            {
                // Server check.
                if (MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer)
                {
                    if (!isInitialized)
                    {
                        isInitialized = true;
                        Init();
                    }

                    // Note to self: Consider parallelism.
                    //if (referenceFrames.Count == 0)
                    //    return;

                    UpdateFast();

                    if (tick % 10 == 0) Update10();

                    if (tick > 100)
                    {
                        Update100();
                        tick = 0;
                    }

                    tick++;
                }
            }
            catch (Exception e)
            {
                // ...
            }
        }

        private void UpdateFast()
        {
            MyAPIGateway.Utilities.ShowNotification("there are this many spinnies: " + spinnyvoxels.Count, 10);
            foreach (var foo in spinnyvoxels) foo.Update();

            // Dumb loop for now. TO-DO: Prioritization.
            foreach (ReferenceFrame frame in referenceFrames)
            {
                // literally just the draw loop now. Will probably be tossed out later.
                frame.Update();
            }
        }

        private void Update10()
        {
            // ...
            foreach (ReferenceFrame frame in referenceFrames)
            {
                var bound = new BoundingSphereD(frame.position, frame.radius);
                List<IMyEntity> entitiesinrange = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bound);
                List<IMyCharacter> charactersInRange = new List<IMyCharacter>();
                List<Vector3D> characterVectors = new List<Vector3D>();

                List<IMyEntity> nonCharacterEntities = new List<IMyEntity>();
                foreach (var ent in entitiesinrange) 
                {
                    if (ent is IMyCharacter) 
                    { 
                        charactersInRange.Add(ent as IMyCharacter);
                        characterVectors.Add(new Vector3D(ent.GetPosition()));
                    }
                    else 
                    { nonCharacterEntities.Add(ent); 
                    }
                }


                foreach(var ent in nonCharacterEntities)
                {
                    MatrixD movementMatrix = ent.PositionComp.WorldMatrixRef * MatrixD.CreateTranslation(frame.velocity);
                    ent.Teleport(movementMatrix, null, false);
                }


                //foreach (var character in charactersInRange)
                for (int i = 0; i < charactersInRange.Count; ++i)
                {
                    var character = charactersInRange[i];
                    var previousPosition = characterVectors[i];

                    bool isStandingOnDynamic = false;
                    var tempList = new List<IMyEntity>();
                    var bs = new BoundingSphereD(character.GetPosition(), 1.0);
                    MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bs);
                    foreach (var temp in tempList)
                    {
                        if (temp is IMyCubeGrid)
                        {
                            isStandingOnDynamic = !(temp as IMyCubeGrid).IsStatic;
                            break;
                        }
                        else if (temp is IMySlimBlock)
                        {
                            var block = temp as IMySlimBlock;
                            isStandingOnDynamic = !block.CubeGrid.IsStatic;
                            break;
                        }
                    }

                    character.Physics.UpdateFromSystem();

                    if (IGNOREPLAYER) break;
                    if (previousPosition != character.PositionComp.GetPosition()) break;

                    MatrixD movementMatrix = character.PositionComp.WorldMatrixRef * MatrixD.CreateTranslation(frame.velocity );
                    character.Teleport(movementMatrix);
                }



                frame.position += frame.velocity;
            }


        }

        private void Update100()
        {
            foreach (var foo in spinnyvoxels) foo.UpdateSlow();

            // TO-DO: Priority list reshuffling will happen here. Maybe use a queue/request kind of system.
            // Try to avoid using data structures that can cause the dreaded second-long-pause every few frames, ie what used to happen w/ weaponcore.
            return; // /!\ //

/*            // Merging behavior
            for (int i = 0; i < referenceFrames.Count-1; i++)
            {
                for (int j = i+1; j < referenceFrames.Count; j++)
                {
                    ReferenceFrame frame1 = referenceFrames[i];
                    ReferenceFrame frame2 = referenceFrames[j];
                    if (IntersectSphere(frame1, frame2))
                    {
                        // Intersecting. These should be merged.

                    }
                    else
                    {
                        // Not intersecting. Do nothing.
                    }
                }
            }*/
        }

        private void EntityAddHandler(IMyEntity ent)
        {
            if (ent is IMyVoxelMap)
            {
                var foo = new spintest(ent as IMyVoxelMap);
                spinnyvoxels.Add(foo);
            }
        }

        private void AdminCommandHandler(ushort handlerID, byte[] package, ulong steamID, bool fromServer)
        {
            Command command = MyAPIGateway.Utilities.SerializeFromXML<Command>(Encoding.Unicode.GetString(package));

            // Get the sender player.
            var playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList, (IMyPlayer player) => { return player.SteamUserId == steamID; });
            var senderPlayer = playerList[0];

            switch (command.content)
            {
                case "createframe":
                    {
                        // Util.DebugMessage("CreateFrame()");
                        if (command.contentArr.Count < 7)
                            break;
                        // I'll use relative coordinates for convenience.
                        Vector3D framePos = new Vector3D(Double.Parse(command.contentArr[1]), Double.Parse(command.contentArr[2]), Double.Parse(command.contentArr[3]));
                        Vector3D frameVel = new Vector3D(Double.Parse(command.contentArr[4]), Double.Parse(command.contentArr[5]), Double.Parse(command.contentArr[6]));
                        float radius = float.Parse(command.contentArr[7]);

                        referenceFrames.Add(new ReferenceFrame(senderPlayer.Character.GetPosition() + framePos, frameVel, radius));
                        Util.DebugMessage("CreateFrame operation done: pos: " +framePos + ", vel: " + frameVel + ", radius: " + radius);
                    }
                    break;
                case "makevelocityfake":
                    // Need to find player's frameentity for this.
                    break;
                case "makevelocityreal":
                    break;
                case "bumpme":
                    {
                        if (command.contentArr.Count < 3) break;
                        Vector3D bump = new Vector3D(Double.Parse(command.contentArr[1]), Double.Parse(command.contentArr[2]), Double.Parse(command.contentArr[3]));
                        Vector3D characterpos = senderPlayer.Character.GetPosition();
                        Util.DebugMessage("[VERSION 2] Bumped character at " + senderPlayer.Character.GetPosition() + " by " + bump);
                        senderPlayer.Character.SetPosition(characterpos + bump);
                        Util.DebugMessage("New position is " + senderPlayer.Character.GetPosition());
                    }
                    break;
                case "teleport":
                    {
                        if (command.contentArr.Count < 3) break;
                        Vector3D bump = new Vector3D(Double.Parse(command.contentArr[1]), Double.Parse(command.contentArr[2]), Double.Parse(command.contentArr[3]));
                        senderPlayer.Character.SetPosition(bump);
                    }
                    break;
                case "where":
                    {
                        Util.DebugMessage(""+senderPlayer.Character.GetPosition());
                    }
                    break;
                case "ignoreme":
                    {
                        IGNOREPLAYER = !IGNOREPLAYER;
                    }
                    break;
                case "teleportgrid":
                    {
                        if (command.contentArr.Count < 3) break;
                        Vector3D bump = new Vector3D(Double.Parse(command.contentArr[1]), Double.Parse(command.contentArr[2]), Double.Parse(command.contentArr[3]));
                        MatrixD bumpMatrix = MatrixD.CreateTranslation(bump);
                        Util.DebugMessage("Teleporting closest grid to " + bump);
                        var sphere = new BoundingSphereD(senderPlayer.GetPosition(), 1.0);
                        var entlist = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);
                        foreach (var ent in entlist)
                        {
                            if (ent is IMyCubeGrid)
                            {
                                ent.Teleport(bumpMatrix);
                                return;
                            }
                            else if (ent is IMySlimBlock)
                            {
                                var block = ent as IMySlimBlock;
                                block.CubeGrid.Teleport(bumpMatrix);
                                return;
                            }
                        }
                        break;
                    }
                default:break;
            }
        }


        private bool IntersectSphere(ReferenceFrame frame1, ReferenceFrame frame2)
        {
            // Get dist sqr
            Vector3D d = frame1.position - frame2.position;
            float distSqr = Vector3.Dot(d, d);  

            // Intersection test.
            float radiusSum = frame1.radius + frame2.radius;
            float radiusSumSqr = radiusSum * radiusSum;

            // TO-DO: Do I actually need ref frames to hold their own radsqr?

            return distSqr <= radiusSumSqr;
        }

        private void MergeReferenceFrames(ReferenceFrame frame1, ReferenceFrame frame2)
        {
            float newFrameRadius = frame1.radius + frame2.radius;
            Vector3D newFramePosition = frame1.position+frame2.position; // An average of the previous two reference frame positions.
            newFramePosition /= 2.0;

            // TO-DO: How to clean up the previous reference frames?
            // TO-DO: How to correctly create the new reference frames.
            // TO-DO: Full relationship between reference frames and antennas...
        }
       
    }
}
