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
            get => this._resolutionX;
            set => Set(ref this._resolutionX, value);
        }
        public int ResolutionY
        {
            get => this._resolutionY;
            set => Set(ref this._resolutionY, value);
        }

        public double TimerInterval
        {
            get => this._timerInterval;
            set => Set(ref this._timerInterval, value);
        }

        public uint DeviceID
        {
            get => this._deviceID;
            set => Set(ref this._deviceID, value);
        }

        public AxisSettings AxisX
        {
            get => this._axisX;
            set => Set(ref this._axisX, value);
        }

        public AxisSettings AxisY
        {
            get => this._axisY;
            set => Set(ref this._axisY, value);
        }

        public bool[] Buttons
        {
            get => this._buttons;
            set => Set(ref this._buttons, value);
        }

        private int _resolutionX = 1920;
        private int _resolutionY = 1080;
        private double _timerInterval = 5;
        private uint _deviceID = 1;

        private AxisSettings _axisX = new AxisSettings { MouseAxis = MouseAxis.X, NegativeFunctionType = FunctionType.InvertedSquare, PositiveFunctionType = FunctionType.Square};
        private AxisSettings _axisY = new AxisSettings { MouseAxis = MouseAxis.Y, NegativeFunctionType = FunctionType.Linear, PositiveFunctionType = FunctionType.Linear };

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
