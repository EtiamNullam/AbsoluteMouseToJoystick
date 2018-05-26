using AbsoluteMouseToJoystick.Logging;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.Data
{
    public class ZoneDistribution : ObservableObject
    {
        [JsonIgnore]
        public ISimpleLogger Logger { get; set; }

        public double NegativeDeadZone
        {
            get => _negativeDeadZone;
            set
            {
                Set(ref _negativeDeadZone, value);
                RaisePropertyChanged(nameof(NegativeDeadZoneEnd));
                RaisePropertyChanged(nameof(NegativeZoneEnd));
                RaisePropertyChanged(nameof(NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(PositiveZoneEnd));
                RaisePropertyChanged(nameof(PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(Total));
                LogZoneChanged();
            }
        }
        public double NegativeZone
        {
            get => _negativeZone;
            set
            {
                Set(ref _negativeZone, value);
                RaisePropertyChanged(nameof(NegativeZoneEnd));
                RaisePropertyChanged(nameof(NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(PositiveZoneEnd));
                RaisePropertyChanged(nameof(PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(Total));
                LogZoneChanged();
            }
        }
        public double NeutralDeadZone
        {
            get => _neutralDeadZone;
            set
            {
                Set(ref _neutralDeadZone, value);
                RaisePropertyChanged(nameof(NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(PositiveZoneEnd));
                RaisePropertyChanged(nameof(PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(Total));
                LogZoneChanged();
            }
        }
        public double PositiveZone
        {
            get => _positiveZone;
            set
            {
                Set(ref _positiveZone, value);
                RaisePropertyChanged(nameof(PositiveZoneEnd));
                RaisePropertyChanged(nameof(PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(Total));
                LogZoneChanged();
            }
        }
        public double PositiveDeadZone
        {
            get => _positiveDeadZone;
            set
            {
                Set(ref _positiveDeadZone, value);
                RaisePropertyChanged(nameof(PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(Total));
                LogZoneChanged();
            }
        }

        [JsonIgnore]
        public double NegativeDeadZoneEnd => NegativeDeadZone;
        [JsonIgnore]
        public double NegativeZoneEnd => NegativeDeadZoneEnd + NegativeZone;
        [JsonIgnore]
        public double NeutralDeadZoneEnd => NegativeZoneEnd + NeutralDeadZone;
        [JsonIgnore]
        public double PositiveZoneEnd => NeutralDeadZoneEnd + PositiveZone;
        [JsonIgnore]
        public double PositiveDeadZoneEnd => PositiveZoneEnd + PositiveDeadZone;

        [JsonIgnore]
        public double Total => NegativeDeadZone + NegativeZone + NeutralDeadZone + PositiveZone + PositiveDeadZone;

        private double _negativeDeadZone = 0;
        private double _negativeZone = 1;
        private double _neutralDeadZone = 0;
        private double _positiveZone = 1;
        private double _positiveDeadZone = 0;

        private void LogZoneChanged()
        {
            Logger?.Log("Property of zone distribution changed");
        }
    }
}
