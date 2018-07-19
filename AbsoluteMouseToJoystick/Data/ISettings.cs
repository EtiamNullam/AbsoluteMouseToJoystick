using System.Collections.Generic;
using System.ComponentModel;

namespace AbsoluteMouseToJoystick.Data
{
    public interface ISettings
    {
        uint DeviceID { get; set; }
        int ResolutionY { get; set; }
        int ResolutionX { get; set; }
        double TimerInterval { get; set; }
        AxisSettings AxisX { get; set; }
        AxisSettings AxisY { get; set; }
        bool[] Buttons { get; set; }
    }
}