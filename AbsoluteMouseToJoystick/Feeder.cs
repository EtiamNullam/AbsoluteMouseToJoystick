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
    // TODO: extract non feeder related things to another class
    // TODO: make Feeder less mutable?
    public class Feeder : IDisposable
    {
        public Feeder(vJoy joy, ISimpleLogger logger, ISettingsBindable settings, Interop interop)
        {
            _joy = joy;
            _logger = logger;
            _settings = settings;
            _interop = interop;

            _timer.Interval = settings.TimerInterval;

            _timer.Elapsed += OnTimerTick;
            _settings.PropertyChanged += OnSettingsPropertyChanged;

            _joy.ResetVJD(_settings.DeviceID);
        }

        public bool Start()
        {
            this.CheckVJoyState();
            this.ShowAdditionalVJoyInfo();

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
            _timer.Stop();

            ResetAxes();
            ResetButtons();

            Update();

            _joy.RelinquishVJD(_settings.DeviceID);

            this.CheckVJoyState();
            this.ShowAdditionalVJoyInfo();
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.TimerInterval))
            {
                _timer.Interval = _settings.TimerInterval;
            }
        }

        private Timer _timer = new Timer { AutoReset = true };
        private vJoy.JoystickState _joystickState = new vJoy.JoystickState();

        private readonly ISimpleLogger _logger;
        private readonly vJoy _joy;
        private readonly ISettingsBindable _settings;
        private readonly Interop _interop;
        private readonly short AxisDisabledValue = short.MaxValue / 2;
        private readonly short AxisMaxValue = short.MaxValue;

        private void OnTimerTick(object sender, EventArgs e)
            => Feed();

        private void Feed()
        {
            UpdateAxes();
            UpdateButtons();

            Update();
        }

        private void UpdateAxes()
        {
            var mousePosition = _interop.GetCursorPosition();

            var xAxisValue = CalculateAxisValue(mousePosition, _settings.AxisX);
            var yAxisValue = CalculateAxisValue(mousePosition, _settings.AxisY);

            SetAxes(xAxisValue, yAxisValue);
        }

        // extract button logic
        private void UpdateButtons()
        {
            ResetButtons();

            if (_settings.Buttons[0] && _interop.IsMouseButtonDown(MouseButton.Left))
            {
                _joystickState.Buttons |= 1;
            }
            if (_settings.Buttons[1] && _interop.IsMouseButtonDown(MouseButton.Right))
            {
                _joystickState.Buttons |= 1 << 1;
            }
            if (_settings.Buttons[2] && _interop.IsMouseButtonDown(MouseButton.Middle))
            {
                _joystickState.Buttons |= 1 << 2;
            }
            if (_settings.Buttons[3] && _interop.IsMouseButtonDown(MouseButton.Extra))
            {
                _joystickState.Buttons |= 1 << 3;
            }
            if (_settings.Buttons[4] && _interop.IsMouseButtonDown(MouseButton.Extra2))
            {
                _joystickState.Buttons |= 1 << 4;
            }
        }

        private void ResetButtons()
            => _joystickState.Buttons = 0;

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

        private void ResetAxes()
            => this.SetAxes(AxisDisabledValue);

        private void SetAxes(int value)
            => this.SetAxes(value, value);

        private void SetAxes(int x, int y)
        {
            this._joystickState.AxisX = x;
            this._joystickState.AxisY = y;
        }

        private void Update()
        {
            this._joy.UpdateVJD(_settings.DeviceID, ref this._joystickState);
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

        // TODO: extract
        private bool CheckVJoyState()
        {
            StringBuilder stringBuilder = new StringBuilder().AppendLine("vJoy Device ID: " + _settings.DeviceID);
            bool canContinue = true;
            string message;

            // TODO: get actual DllVer and DrvVer
            UInt32 DllVer = 0, DrvVer = 0;

            if (!_joy.DriverMatch(ref DllVer, ref DrvVer))
            {
                stringBuilder.AppendLine("Driver and library versions doesn't match.");
                canContinue = false;
            }

            if (!_joy.vJoyEnabled())
            {
                stringBuilder.AppendLine("vJoy is not enabled.");
                canContinue = false;
            }

            VjdStat status = _joy.GetVJDStatus(_settings.DeviceID);

            switch (status)
            {
                case VjdStat.VJD_STAT_OWN:
                    message = "vJoy Device is already owned by this feeder.";
                    break;
                case VjdStat.VJD_STAT_FREE:
                    message = "vJoy Device is free.";
                    break;
                case VjdStat.VJD_STAT_BUSY:
                    message = "vJoy Device is already owned by another feeder.";
                    canContinue = false;
                    break;
                case VjdStat.VJD_STAT_MISS:
                    message = "vJoy Device is not installed or disabled.";
                    canContinue = false;
                    break;
                default:
                    message = "vJoy Device general error.";
                    canContinue = false;
                    break;
            }

            stringBuilder.AppendLine(message);

            if (canContinue)
            {
                stringBuilder.AppendLine("vJoy check OK.");
            }
            else
            {
                stringBuilder.AppendLine("Cannot continue.");
            }

            _logger.Log(stringBuilder.ToString());

            return canContinue;
        }

        // TODO: extract
        private void ShowAdditionalVJoyInfo()
        {
            int virtualButtonsCount = _joy.GetVJDButtonNumber(_settings.DeviceID);

            if (virtualButtonsCount < 5)
            {
                _logger.Log($"Only {virtualButtonsCount} virtual buttons enabled, while 5 might be needed for handling all mouse buttons");
            }
            else
            {
                _logger.Log($"{virtualButtonsCount} virtual buttons enabled.");
            }

            bool xAxisAvailable = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_X);
            bool yAxisAvailable = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_Y);
            bool zAxisAvailable = _joy.GetVJDAxisExist(_settings.DeviceID, HID_USAGES.HID_USAGE_Z);

            if (!xAxisAvailable) _logger.Log($"Virtual axis X is disabled and will be unavailable.");
            if (!yAxisAvailable) _logger.Log($"Virtual axis Y is disabled and will be unavailable.");
            if (!zAxisAvailable) _logger.Log($"Virtual axis Z is disabled and will be unavailable.");
        }
    }
}
