using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Game;
using Sandbox.ModAPI;
using VRage.Game;
using VRageMath;

namespace nubeees_refFrame
{
    public static class Util
    {
        public static ushort MOD_ID = 1414;

        public static bool DEBUG_MODE = true;

        public static void DebugMessage(string msg)
        {
            if (!DEBUG_MODE) return;
            //MyAPIGateway.Utilities.ShowMessage("ReferenceFrames", msg);
            MyVisualScriptLogicProvider.SendChatMessage(msg, "ReferenceFrames");
        }


        public static void DrawDebugSphere(Vector3D position, float radius)
        {
            MatrixD matrix = MatrixD.CreateWorld(position, Vector3.Forward, Vector3.Up);
            Color color = (Color.Lime * 0.2f).ToVector4();
            float wireframethickness = 0.1f;
            MySimpleObjectDraw.DrawTransparentSphere(ref matrix, radius, ref color, MySimpleObjectRasterizer.Wireframe, 360/15, lineThickness:wireframethickness);
        }
    }
}
