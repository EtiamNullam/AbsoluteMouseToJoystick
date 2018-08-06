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
        private FunctionType _functionType = FunctionType.Linear;

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

        public FunctionType FunctionType
        {
            get => this._functionType;
            set => Set(ref this._functionType, value);
        }
    }
}
