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

namespace nubeees_refFrame
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    class Server : MySessionComponentBase
    {
        public List<ReferenceFrame> referenceFrames = new List<ReferenceFrame>(); // Everything lumped into one list right now. Suffling to be done in the future. Priority system will be needed.

        private uint counter = 0;
        private const uint updateSlowTime = 100;

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

                    // Dumb loop for now. TO-DO: Prioritization.
                    foreach (ReferenceFrame frame in referenceFrames)
                    {
                        frame.Update();
                    }


                    counter++;
                    if (counter > updateSlowTime)
                    {
                        UpdateSlow();
                        counter = 0;
                    }
                }
            }
            catch (Exception e)
            {
                // ...
            }
        }

        private void UpdateSlow()
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
            CreateFrameCommand command = MyAPIGateway.Utilities.SerializeFromXML<CreateFrameCommand>(Encoding.Unicode.GetString(package));

            var playerList = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(playerList, (IMyPlayer player)=>{ return player.SteamUserId == steamID; });

            Util.DebugMessage("!TEST!: " + (fromServer ? "from server" : "not from server"));

            // Vector3D playerPos = playerList[0].Character.GetPosition();

            // Util.DrawDebugSphere(command.position, 100.0f);
            referenceFrames.Add(new ReferenceFrame(command.position, Vector3D.Forward, 100.0f));
            Util.DebugMessage("Added new reference frame to the list!");

            
            Util.DebugMessage("____________________DONE_______________");
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
