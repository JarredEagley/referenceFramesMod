using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using Sandbox.ModAPI;
using VRageMath;

namespace nubeees_refFrame
{
    // Structure mimicked from dailyneeds. I had to learn somewhere afterall!

    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    class Client : MySessionComponentBase
    {
        private bool isInitialized = false;
        private int tick = 0;

        private void Init()
        {
            MyAPIGateway.Utilities.MessageEnteredSender += onMessageEntered;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null) return;

            try
            {
                bool isHost = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                bool isDedicatedHost = isHost && MyAPIGateway.Utilities.IsDedicated;

                if (isDedicatedHost) return;

                if (!isInitialized)
                {
                    MyAPIGateway.Utilities.SendMessage("Initializing client");
                    isInitialized = true; 
                    Init();
                }

                tick = ++tick % 600;
            }
            catch (Exception e)
            {
                // error
            }
        }

        private void onMessageEntered(ulong sender, string messageText, ref bool sendToOthers)
        {
            sendToOthers = true;
            if (!messageText.StartsWith("/")) return;
            
            var words = messageText.Trim().ToLower().Replace("/", "").Split(' ');
            if (words.Length <= 0) return;

            // Main switch block for sending commands.
            switch (words[0])
            {
                case "createframe":
                    Vector3D framePosition;
                    Vector3D frameVelocity;

                    framePosition.X = Double.Parse(words[1]);
                    framePosition.Y = Double.Parse(words[1]);
                    framePosition.Z = Double.Parse(words[1]);
                    //bool valid = Vector3D.TryParse(words[1] + words[2] + words[3], out framePosition);
                    /*if (!valid)
                    {
                        Util.DebugMessage("Failed to parse vector.");
                        break;
                    }*/

                    CreateFrameCommand cmd = new CreateFrameCommand(sender, words[0], framePosition, new Vector3D(1.0f, 1.0f, 1.0f), 100.0f);
                    
                    string message = MyAPIGateway.Utilities.SerializeToXML<CreateFrameCommand>(cmd);
                    MyAPIGateway.Multiplayer.SendMessageToServer(Util.MOD_ID, Encoding.Unicode.GetBytes(message));
                    break;
            }
        }

    }
}
