using AbsoluteMouseToJoystick.Data;
using AbsoluteMouseToJoystick.Logging;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick
{
    public class Feeder : IDisposable
    {
        public Feeder(vJoy joy, ISimpleLogger logger, ISettings settings)
        {
            _joy = joy;
            _logger = logger;
            _settings = settings;

            _timer.Interval = settings.TimerInterval;

            _timer.Elapsed += OnTimerTick;
            _settings.PropertyChanged += OnSettingsPropertyChanged;

            _joy.ResetVJD(_settings.DeviceID);
            ShowJoystickInfo();
        }

        public bool Start()
        {
            var acquireResult = _joy.AcquireVJD(_settings.DeviceID);
        
            if (acquireResult)
            {
                _timer.Start();
                Feed();
            }

            return acquireResult;
        }

        public void Stop()
        {
            _joy.RelinquishVJD(_settings.DeviceID);
            _timer.Stop();
            SetAxes(AxisDisabledValue);
            ShowJoystickInfo();
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.TimerInterval))
            {
                _timer.Interval = _settings.TimerInterval;
            }
        }

        private Timer _timer = new Timer { AutoReset = true };

        private readonly ISimpleLogger _logger;
        private readonly vJoy _joy;
        private readonly ISettings _settings;

        private readonly short AxisDisabledValue = short.MaxValue / 2;
        private readonly short AxisMaxValue = short.MaxValue;

        private void OnTimerTick(object sender, EventArgs e)
            => Feed();

        // TODO: use efficient way instead? (from readme.pdf)
        private void Feed()
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
                    return AxisDisabledValue;
                default:
                    _logger.Log("Invalid MouseAxis");
                    return AxisDisabledValue;
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
                _timer.Elapsed -= OnTimerTick;
                _timer = null;
            }

            if (_settings != null)
            {
                _settings.PropertyChanged -= OnSettingsPropertyChanged;
            }

            Stop();
        }

        // TODO: refactor (move to other class?)
        private bool ShowJoystickInfo()
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (_joy.DriverMatch(ref DllVer, ref DrvVer) && _joy.vJoyEnabled())
            {
                // Get the state of the requested device
                VjdStat status = _joy.GetVJDStatus(_settings.DeviceID);

                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        _logger.Log($"vJoy Device {_settings.DeviceID} is already owned by this feeder");
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        _logger.Log($"vJoy Device {_settings.DeviceID} is free");
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        _logger.Log($"vJoy Device {_settings.DeviceID} is already owned by another feeder\nCannot continue");
                        return false;
                    case VjdStat.VJD_STAT_MISS:
                        _logger.Log($"vJoy Device {_settings.DeviceID} is not installed or disabled\nCannot continue");
                        return false;
                    default:
                        _logger.Log($"vJoy Device {_settings.DeviceID} general error\nCannot continue");
                        return false;
                }

                ///// vJoy Device properties
                int nBtn = _joy.GetVJDButtonNumber(_settings.DeviceID);
                int nDPov = _joy.GetVJDDiscPovNumber(_settings.DeviceID);
                int nCPov = _joy.GetVJDContPovNumber(_settings.DeviceID);
                bool X_Exist = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_X);
                bool Y_Exist = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_Y);
                bool Z_Exist = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_Z);
                bool RX_Exist = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_RX);
                _logger.Log($"Device[{_settings.DeviceID}]: Buttons={nBtn}; DiscPOVs:{nDPov}; ContPOVs:{nCPov}");
                return true;
            }
            else return false;
        }
    }
}
