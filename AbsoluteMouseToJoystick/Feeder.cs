using AbsoluteMouseToJoystick.Data;
using AbsoluteMouseToJoystick.Logging;
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
        public Feeder(vJoy joy, ISimpleLogger logger, Timer timer, ISettings settings)
        {
            _joy = joy;
            _logger = logger;
            _settings = settings;

            _joy.ResetVJD(_settings.DeviceID);

            _timer = timer;
            _timer.Elapsed += Execute;
        }

        private Timer _timer;

        private readonly ISimpleLogger _logger;
        private readonly vJoy _joy;
        private readonly ISettings _settings;

        private readonly short AxisNeutralValue = short.MaxValue / 2;
        private readonly short AxisMaxValue = short.MaxValue;
        private readonly short AxisMinValue = 0;

        // TODO: use efficient way instead? (from readme.pdf)
        private void Execute(object sender, EventArgs e)
        {
            var mousePosition = Interop.GetCursorPosition();

            var xAxisValue = CalculateAxisValue(mousePosition, _settings.AxisX);
            var yAxisValue = CalculateAxisValue(mousePosition, _settings.AxisY);
            var zAxisValue = CalculateAxisValue(mousePosition, _settings.AxisZ);

            SetAxes(xAxisValue, yAxisValue, zAxisValue);
        }

        private int CalculateAxisValue(IntPoint mousePosition, AxisSettings axisSettings)
        {
            switch (axisSettings.MouseAxis)
            {
                case MouseAxis.X:
                    return CalculateAxisValue(mousePosition.X, _settings.ResolutionX, axisSettings.ZoneDistribution);
                case MouseAxis.Y:
                    return CalculateAxisValue(mousePosition.Y, _settings.ResolutionY, axisSettings.ZoneDistribution);
                case MouseAxis.None:
                    return AxisNeutralValue;
                default:
                    _logger.Log("Invalid MouseAxis");
                    return AxisNeutralValue;
            }
        }

        private int CalculateAxisValue(int pixel, int pixelsCount, ZoneDistribution zoneDistribution)
        {
            var value = (double)pixel / (pixelsCount - 1);
            value *= zoneDistribution.Total;
            var zone = GetZone(value, zoneDistribution);
            switch (zone)
            {
                case Zone.NegativeDead:
                    value = AxisMinValue;
                    break;
                case Zone.Negative:
                    value = (value - zoneDistribution.NegativeDeadZoneEnd) / zoneDistribution.NegativeZone / 2;
                    break;
                case Zone.NeutralDead:
                    value = AxisNeutralValue;
                    break;
                case Zone.Positive:
                    value = (value - zoneDistribution.NeutralDeadZoneEnd) / zoneDistribution.PositiveZone / 2 + 0.5f;
                    break;
                case Zone.PositiveDead:
                    value = AxisMaxValue;
                    break;
                default:
                    _logger.Log("Feeder: Invalid Zone");
                    break;
            }
            return Convert.ToInt32(value * AxisMaxValue);
        }

        private void SetAxes(int value)
            => this.SetAxes(value, value, value);

        private void SetAxes(int x, int y, int z)
        {
            this._joy.SetAxis(x, _settings.DeviceID, HID_USAGES.HID_USAGE_X);
            this._joy.SetAxis(y, _settings.DeviceID, HID_USAGES.HID_USAGE_Y);
            this._joy.SetAxis(z, _settings.DeviceID, HID_USAGES.HID_USAGE_Z);
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

            SetAxes(AxisNeutralValue);
        }
    }
}
