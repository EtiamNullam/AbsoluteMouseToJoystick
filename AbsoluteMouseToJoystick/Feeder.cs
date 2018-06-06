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
            ShowInfo();
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
            _timer.Stop();

            ResetAxes();
            ResetButtons();

            Update();

            _joy.RelinquishVJD(_settings.DeviceID);

            ShowInfo();
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
            var zAxisValue = CalculateAxisValue(mousePosition, _settings.AxisZ);

            SetAxes(xAxisValue, yAxisValue, zAxisValue);
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
            => this.SetAxes(value, value, value);

        private void SetAxes(int x, int y, int z)
        {
            this._joystickState.AxisX = x;
            this._joystickState.AxisY = y;
            this._joystickState.AxisZ = z;
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

        // TODO: refactor (move to other class?)
        private bool ShowInfo()
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
