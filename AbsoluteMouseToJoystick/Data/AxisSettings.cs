using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class AxisSettings : ObservableObject
    {
        private MouseAxis _mouseAxis = MouseAxis.None;
        private ZoneDistribution _zoneDistribution = new ZoneDistribution();
        private FunctionType _positiveFunctionType = FunctionType.Linear;
        private FunctionType _negativeFunctionType = FunctionType.Linear;

        [JsonIgnore]
        public bool IsEnabled => this.MouseAxis != MouseAxis.None;

        public MouseAxis MouseAxis
        {
            get => this._mouseAxis;
            set
            {
                Set(ref this._mouseAxis, value);
                RaisePropertyChanged(nameof(this.IsEnabled));
            }
        }

        public ZoneDistribution ZoneDistribution
        {
            get => this._zoneDistribution;
            set => Set(ref this._zoneDistribution, value);
        }

        public FunctionType NegativeFunctionType
        {
            get => this._negativeFunctionType;
            set => Set(ref this._negativeFunctionType, value);
        }

        public FunctionType PositiveFunctionType
        {
            get => this._positiveFunctionType;
            set => Set(ref this._positiveFunctionType, value);
        }
    }
}
