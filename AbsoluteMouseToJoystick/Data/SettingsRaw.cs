using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class SettingsRaw : ISettings
    {
        public uint DeviceID { get; set; } = 1;
        public int ResolutionX { get; set; } = 1920;
        public int ResolutionY { get; set; } = 1080;
        public double TimerInterval { get; set; } = 5;
        public AxisSettings AxisX { get; set; } = new AxisSettings();
        public AxisSettings AxisY { get; set; } = new AxisSettings();
        public AxisSettings AxisZ { get; set; } = new AxisSettings();
        public bool[] Buttons { get; set; } = new bool[5]
        {
            true,
            true,
            true,
            true,
            true
        };

        // Not used
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
