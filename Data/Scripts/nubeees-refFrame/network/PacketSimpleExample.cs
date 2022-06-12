using ProtoBuf;
using Sandbox.ModAPI;
using VRage.Utils;

/// <summary>
/// Credit to Digi
/// https://github.com/THDigi/SE-ModScript-Examples/blob/master/Data/Scripts/Examples/Example_NetworkProtobuf/PacketSimpleExample.cs
/// </summary>
namespace nubeees_refFrame.network
{
    // An example packet extending another packet.
    // Note that it must be ProtoIncluded in PacketBase for it to work.
    [ProtoContract]
    public class PacketSimpleExample : PacketBase
    {
        // tag numbers in this class won't collide with tag numbers from the base class
        [ProtoMember(1)]
        public string Text;

        [ProtoMember(2)]
        public int Number;

        public PacketSimpleExample() { } // Empty constructor required for deserialization

        public PacketSimpleExample(string text, int number)
        {
            Text = text;
            Number = number;
        }

        public override bool Received()
        {
            var msg = $"PacketSimpleExample received: Text='{Text}'; Number={Number}";
            MyLog.Default.WriteLineAndConsole(msg);
            MyAPIGateway.Utilities.ShowNotification(msg, Number);

            return true; // relay packet to other clients (only works if server receives it)
        }
    }
}
