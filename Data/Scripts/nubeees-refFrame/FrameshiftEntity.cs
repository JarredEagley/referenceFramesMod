using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using VRage.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using SpaceEngineers.ObjectBuilders;
using VRage.Game.SessionComponents;
using Sandbox.ObjectBuilders;
using VRage.ObjectBuilders;
using Sandbox;
using SpaceEngineers;
using VRage.Game;

//TTODO: SERVER CHECK

// MIgHT DEPRICATE

namespace nubeees_refFrame
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_EntityBase), false, "")]
    class FrameshiftEntity : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            if (Entity is IMyCubeGrid)
            {

            }
            if (Entity is IMyCharacter)
            {

            }
        }


    }
}
