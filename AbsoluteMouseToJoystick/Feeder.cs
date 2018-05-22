using AbsoluteMouseToJoystick.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick
{
    public class Feeder : IDisposable
    {
        public Feeder(vJoy joy, ISimpleLogger logger, Timer timer, Settings settings)
        {
            _joy = joy;
            _logger = logger;
            _settings = settings;

            _joy.ResetVJD(_settings.DeviceID);

            _timer = timer;
            _timer.Elapsed += Execute;
        }

        private ISimpleLogger _logger;
        private vJoy _joy;
        private Timer _timer;
        private Settings _settings;

        // TODO: use efficient way instead? (from readme.pdf)
        private void Execute(object sender, EventArgs e)
        {
            Interop.GetCursorPos(out Interop.POINT point);

            var xAxisValue = CalculateAxisValue(point.X, _settings.ResolutionX, _settings.ZoneDistributionX);
            var yAxisValue = CalculateAxisValue(point.Y, _settings.ResolutionY, _settings.ZoneDistributionY);

            SetAxes(xAxisValue, yAxisValue);
        }

        private int CalculateAxisValue(int pixel, int pixelsCount, ZoneDistribution zoneDistribution)
        {
            var value = (double)pixel / (pixelsCount - 1);
            value *= zoneDistribution.Total;
            var zone = GetZone(value, zoneDistribution);
            switch (zone)
            {
                case Zone.NegativeDead:
                    value = 0;
                    break;
                case Zone.Negative:
                    value = (value - zoneDistribution.NegativeDeadZoneEnd) / zoneDistribution.NegativeZone / 2;
                    break;
                case Zone.NeutralDead:
                    value = 0.5f;
                    break;
                case Zone.Positive:
                    value = (value - zoneDistribution.NeutralDeadZoneEnd) / zoneDistribution.PositiveZone / 2 + 0.5f;
                    break;
                case Zone.PositiveDead:
                    value = 1;
                    break;
                default:
                    _logger.Log("Feeder: Invalid Zone");
                    break;
            }
            return Convert.ToInt32(value * short.MaxValue);
        }

        private void SetAxes(int x, int y)
        {
            this._joy.SetAxis(x, _settings.DeviceID, HID_USAGES.HID_USAGE_X);
            this._joy.SetAxis(y, _settings.DeviceID, HID_USAGES.HID_USAGE_Y);
        }

        private Zone GetZone(double value, ZoneDistribution zoneDistribution)
        {
            if (value <= zoneDistribution.NegativeDeadZoneEnd)
                return Zone.NegativeDead;

            if (value <= zoneDistribution.NegativeZoneEnd)
                return Zone.Negative;

            if (value <= zoneDistribution.NeutralDeadZoneEnd)
                return Zone.NeutralDead;

            if (value <= zoneDistribution.PositiveZoneEnd)
                return Zone.Positive;

            if (value <= zoneDistribution.PositiveDeadZoneEnd)
                return Zone.PositiveDead;

            return Zone.Invalid;
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Elapsed -= Execute;
                _timer = null;
            }

            SetAxes(short.MaxValue / 2, short.MaxValue / 2);
        }
    }
}
