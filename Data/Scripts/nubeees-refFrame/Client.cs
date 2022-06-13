using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using Sandbox.ModAPI;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using Sandbox.Engine.Physics;

namespace nubeees_refFrame
{
    // Structure mimicked from dailyneeds. I had to learn somewhere afterall!

    [ MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    class Client : MySessionComponentBase
    {
        private bool isInitialized = false;
        private int tick = 0;

        private void Init()
        {
            MyAPIGateway.Utilities.MessageEnteredSender += onMessageEntered;
        }

        protected override void UnloadData()
        {
            base.UnloadData();
            MyAPIGateway.Utilities.MessageEnteredSender -= onMessageEntered;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null) return;

            bool isHost = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
            bool isDedicatedHost = isHost && MyAPIGateway.Utilities.IsDedicated;
            if (isDedicatedHost) return;

            // TESTING //
            /*
            try
            {
                var playerList = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(playerList, (IMyPlayer player) => { return player.SteamUserId == MyAPIGateway.Multiplayer.MyId; });
                var senderPlayer = playerList[0];
                var myCharacter = senderPlayer.Character;
                bool hasparent = myCharacter.Entity;
                MyAPIGateway.

                MyAPIGateway.Utilities.ShowNotification(""+hasparent, 10);
            }
            catch (Exception e) { }
            */

            try
            {
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

            Command cmd = new Command(sender, words[0]);
            cmd.contentArr = new List<string>(words);

            string msg = MyAPIGateway.Utilities.SerializeToXML<Command>(cmd);
            MyAPIGateway.Multiplayer.SendMessageToServer(Util.MOD_ID, Encoding.Unicode.GetBytes(msg));
        }

    }
}
