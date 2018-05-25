using GalaSoft.MvvmLight;
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

        public bool IsEnabled => MouseAxis != MouseAxis.None;

        public MouseAxis MouseAxis
        {
            get => _mouseAxis;
            set
            {
                Set(ref _mouseAxis, value);
                RaisePropertyChanged(nameof(IsEnabled));
            }
        }

        public ZoneDistribution ZoneDistribution
        {
            get => _zoneDistribution;
            set => Set(ref _zoneDistribution, value);
        }
    }
}
