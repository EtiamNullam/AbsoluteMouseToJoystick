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
            get => this._negativeDeadZone;
            set
            {
                Set(ref this._negativeDeadZone, value);
                RaisePropertyChanged(nameof(this.NegativeDeadZoneEnd));
                RaisePropertyChanged(nameof(this.NegativeZoneEnd));
                RaisePropertyChanged(nameof(this.NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(this.Total));
                LogZoneChanged();
            }
        }
        public double NegativeZone
        {
            get => this._negativeZone;
            set
            {
                Set(ref this._negativeZone, value);
                RaisePropertyChanged(nameof(this.NegativeZoneEnd));
                RaisePropertyChanged(nameof(this.NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(this.Total));
                LogZoneChanged();
            }
        }
        public double NeutralDeadZone
        {
            get => this._neutralDeadZone;
            set
            {
                Set(ref this._neutralDeadZone, value);
                RaisePropertyChanged(nameof(this.NeutralDeadZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(this.Total));
                LogZoneChanged();
            }
        }
        public double PositiveZone
        {
            get => this._positiveZone;
            set
            {
                Set(ref this._positiveZone, value);
                RaisePropertyChanged(nameof(this.PositiveZoneEnd));
                RaisePropertyChanged(nameof(this.PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(this.Total));
                LogZoneChanged();
            }
        }
        public double PositiveDeadZone
        {
            get => this._positiveDeadZone;
            set
            {
                Set(ref this._positiveDeadZone, value);
                RaisePropertyChanged(nameof(this.PositiveDeadZoneEnd));
                RaisePropertyChanged(nameof(this.Total));
                LogZoneChanged();
            }
        }

        [JsonIgnore]
        public double NegativeDeadZoneEnd => this.NegativeDeadZone;
        [JsonIgnore]
        public double NegativeZoneEnd => this.NegativeDeadZoneEnd + this.NegativeZone;
        [JsonIgnore]
        public double NeutralDeadZoneEnd => this.NegativeZoneEnd + this.NeutralDeadZone;
        [JsonIgnore]
        public double PositiveZoneEnd => this.NeutralDeadZoneEnd + this.PositiveZone;
        [JsonIgnore]
        public double PositiveDeadZoneEnd => this.PositiveZoneEnd + this.PositiveDeadZone;

        [JsonIgnore]
        public double Total => this.NegativeDeadZone + this.NegativeZone + this.NeutralDeadZone + this.PositiveZone + this.PositiveDeadZone;

        private double _negativeDeadZone = 0;
        private double _negativeZone = 1;
        private double _neutralDeadZone = 0;
        private double _positiveZone = 1;
        private double _positiveDeadZone = 0;

        private void LogZoneChanged()
        {
            this.Logger?.Log("Property of zone distribution changed");
        }
    }
}
