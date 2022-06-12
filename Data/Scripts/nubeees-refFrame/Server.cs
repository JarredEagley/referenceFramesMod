﻿using System;
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

namespace nubeees_refFrame
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class Server : MySessionComponentBase
    {
        public List<ReferenceFrame> referenceFrames = new List<ReferenceFrame>(); // Everything lumped into one list right now. Suffling to be done in the future. Priority system will be needed.

        private uint tick = 0;

        private bool isInitialized = false;


        private void Init()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(Util.MOD_ID, AdminCommandHandler);
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
                    if (referenceFrames.Count == 0)
                        return;

                    UpdateFast();

                    // Dumb loop for now. TO-DO: Prioritization.
                    /*foreach (ReferenceFrame frame in referenceFrames)
                    {
                        frame.Update();
                    }*/


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
            // ...
        }

        private void Update10()
        {
            // ...
        }

        private void Update100()
        {
            // TO-DO: Priority list reshuffling will happen here. Maybe use a queue/request kind of system.
            // Try to avoid using data structures that can cause the dreaded second-long-pause every few frames, ie what used to happen w/ weaponcore.
            return; // /!\ //

            // Merging behavior
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
                        referenceFrames.Add(new ReferenceFrame(senderPlayer.GetPosition() + framePos, frameVel, radius));
                    }
                    break;
                case "makeVelocityFake":
                    // Need to find player's frameentity for this.
                    break;
                case "makeVelocityReal":
                    break;
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
