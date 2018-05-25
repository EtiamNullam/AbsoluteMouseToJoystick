using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class SettingsRaw : ISettings
    {
        public uint DeviceID { get; set; }
        public int ResolutionY { get; set; }
        public int ResolutionX { get; set; }
        public double TimerInterval { get; set; }
        public AxisSettings AxisX { get; set; }
        public AxisSettings AxisY { get; set; }
        public AxisSettings AxisZ { get; set; }
    }
}
