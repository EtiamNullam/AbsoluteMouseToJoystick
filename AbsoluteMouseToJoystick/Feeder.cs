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
        public Feeder(vJoy joy, uint deviceID, ISimpleLogger logger, Timer timer, ZoneDistribution zoneDistributionX, ZoneDistribution zoneDistributionY)
        {
            _joy = joy;
            _deviceID = deviceID;
            _logger = logger;
            _zoneDistributionX = zoneDistributionX;
            _zoneDistributionY = zoneDistributionY;

            _joy.ResetVJD(deviceID);

            _timer = timer;
            _timer.Elapsed += Execute;
        }

        private ISimpleLogger _logger;
        private vJoy _joy;
        private uint _deviceID;
        private ZoneDistribution _zoneDistributionX;
        private ZoneDistribution _zoneDistributionY;
        private Timer _timer;

        // TODO: use efficient way instead? (from readme.pdf)
        private void Execute(object sender, EventArgs e)
        {
            Interop.GetCursorPos(out Interop.POINT point);

            var valueX = (float)point.X / (1920 - 1);
            var valueY = (float)point.Y / (1080 - 1);

            var zoneX = GetZone(valueX, _zoneDistributionX);
            var zoneY = GetZone(valueY, _zoneDistributionY);

            switch (zoneY)
            {
                case Zone.NegativeDead:
                    valueY = 0;
                    break;
                case Zone.Negative:
                    valueY = (valueY - this._zoneDistributionY.NegativeDeadZoneEnd) / this._zoneDistributionY.NegativeZone / 2;
                    break;
                case Zone.NeutralDead:
                    valueY = 0.5f;
                    break;
                case Zone.Positive:
                    valueY = (valueY - this._zoneDistributionY.NeutralDeadZoneEnd) / this._zoneDistributionY.PositiveZone / 2 + 0.5f;
                    break;
                case Zone.PositiveDead:
                    valueY = 1;
                    break;
                default:
                    _logger.Log("Feeder: Invalid Zone");
                    return;
            }

            var finalValueX = Convert.ToInt32(valueX * short.MaxValue);
            var finalValueY = Convert.ToInt32(valueY * short.MaxValue);

            this._joy.SetAxis(finalValueX, _deviceID, HID_USAGES.HID_USAGE_X);
            this._joy.SetAxis(finalValueY, _deviceID, HID_USAGES.HID_USAGE_Y);

            //_logger.Log($"Feeder: Zones: {zoneX}/{zoneY}, Pixel: {point.X}/{point.Y}, Final: {finalValueX}/{finalValueY}");
        }

        private Zone GetZone(float value, ZoneDistribution zoneDistribution)
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
                _timer.Dispose();
                _timer = null;
            }
        }
    }
}
