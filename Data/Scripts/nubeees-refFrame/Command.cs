using VRageMath;

namespace nubeees_refFrame
{
    /// <summary>
    /// Credit to Stollie's daily needs mod, which I referenced
    /// for this code.
    /// </summary>
    public class Command
    {
        public ulong sender;
        public string content;

        public Command()
        {
        }
        public Command(ulong sender, string content)
        {
            this.sender = sender;
            this.content = content;
        }
    }

    public class CreateFrameCommand : Command
    {
        public Vector3D position;
        public Vector3D velocity;
        public float radius;
        
        public CreateFrameCommand() 
        { 
        }

        public CreateFrameCommand(ulong _sender, string _content, Vector3D _position, Vector3D _velocity, float _radius)
        {
            sender = _sender;
            content = _content;
            position = _position;
            velocity = _velocity;
            radius = _radius;
        }
    }

}
