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
        public double NegativeDeadZone;
        public double NegativeZone;
        public double NeutralDeadZone;
        public double PositiveZone;
        public double PositiveDeadZone;

        public double NegativeDeadZoneEnd => NegativeDeadZone;
        public double NegativeZoneEnd => NegativeDeadZoneEnd + NegativeZone;
        public double NeutralDeadZoneEnd => NegativeZoneEnd + NeutralDeadZone;
        public double PositiveZoneEnd => NeutralDeadZoneEnd + PositiveZone;
        public double PositiveDeadZoneEnd => PositiveZoneEnd + PositiveDeadZone;

        public double Total => NegativeDeadZone + NegativeZone + NeutralDeadZone + PositiveZone + PositiveDeadZone;
    }
}
