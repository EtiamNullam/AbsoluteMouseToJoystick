using AbsoluteMouseToJoystick.Logging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class Settings : ObservableObject, ISettingsBindable
    {
        public int ResolutionX
        {
            get => _resolutionX;
            set => Set(ref _resolutionX, value);
        }
        public int ResolutionY
        {
            get => _resolutionY;
            set => Set(ref _resolutionY, value);
        }

        public double TimerInterval
        {
            get => _timerInterval;
            set => Set(ref _timerInterval, value);
        }

        public uint DeviceID
        {
            get => _deviceID;
            set => Set(ref _deviceID, value);
        }

        public AxisSettings AxisX
        {
            get => _axisX;
            set => Set(ref _axisX, value);
        }

        public AxisSettings AxisY
        {
            get => _axisY;
            set => Set(ref _axisY, value);
        }

        public AxisSettings AxisZ
        {
            get => _axisZ;
            set => Set(ref _axisZ, value);
        }

        public bool[] Buttons
        {
            get => _buttons;
            set => Set(ref _buttons, value);
        }

        private int _resolutionX = 1920;
        private int _resolutionY = 1080;
        private double _timerInterval = 5;
        private uint _deviceID = 1;

        private AxisSettings _axisX = new AxisSettings { MouseAxis = MouseAxis.X };
        private AxisSettings _axisY = new AxisSettings { MouseAxis = MouseAxis.Y };
        private AxisSettings _axisZ = new AxisSettings();

        private bool[] _buttons = new bool[5]
        {
            false,
            false,
            true,
            true,
            true
        };
    }
}
