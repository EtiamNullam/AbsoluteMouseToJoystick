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
    // TODO: save and load from file (json)
    public class ZoneDistribution : ObservableObject
    {
        public ISimpleLogger Logger { get; set; }

        public double NegativeDeadZone
        {
            get => _negativeDeadZone;
            set
            {
                Set(ref _negativeDeadZone, value);
                LogZoneChanged();
            }
        }
        public double NegativeZone
        {
            get => _negativeZone;
            set
            {
                Set(ref _negativeZone, value);
                LogZoneChanged();
            }
        }
        public double NeutralDeadZone
        {
            get => _neutralDeadZone;
            set
            {
                Set(ref _neutralDeadZone, value);
                LogZoneChanged();
            }
        }
        public double PositiveZone
        {
            get => _positiveZone;
            set
            {
                Set(ref _positiveZone, value);
                LogZoneChanged();
            }
        }
        public double PositiveDeadZone
        {
            get => _positiveDeadZone;
            set
            {
                Set(ref _positiveDeadZone, value);
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

        private double _negativeDeadZone;
        private double _negativeZone;
        private double _neutralDeadZone;
        private double _positiveZone;
        private double _positiveDeadZone;

        private void LogZoneChanged()
        {
            Logger?.Log("Distribution: Zone distribution changed");
        }
    }
}
