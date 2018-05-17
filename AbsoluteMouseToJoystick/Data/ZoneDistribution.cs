using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    // TODO: save and load from file (json)
    public class ZoneDistribution
    {
        public float NegativeDeadZone;
        public float NegativeZone;
        public float NeutralDeadZone;
        public float PositiveZone;
        public float PositiveDeadZone;

        public float NegativeDeadZoneEnd => NegativeDeadZone;
        public float NegativeZoneEnd => NegativeDeadZoneEnd + NegativeZone;
        public float NeutralDeadZoneEnd => NegativeZoneEnd + NeutralDeadZone;
        public float PositiveZoneEnd => NeutralDeadZoneEnd + PositiveZone;
        public float PositiveDeadZoneEnd => PositiveZoneEnd + PositiveDeadZone;
    }
}
