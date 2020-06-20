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
    /// <summary>
    /// Feeds vJoy with mouse data based on interval from self-managed timer.
    /// </summary>
    public class Feeder : IDisposable
    {
        public Feeder(vJoy joy, ISimpleLogger logger, ISettingsBindable settings, Interop interop)
        {
            this._joy = joy;
            this._logger = logger;
            this._settings = settings;
            this._interop = interop;

            this._timer.Interval = settings.TimerInterval;

            this._timer.Elapsed += this.OnTimerTick;
            this._settings.PropertyChanged += this.OnSettingsPropertyChanged;

            this._joy.ResetVJD(this._settings.DeviceID);
        }

        public bool Start()
        {
            this.CheckVJoyState();
            this.ShowAdditionalVJoyInfo();

            var acquireResult = this._joy.AcquireVJD(this._settings.DeviceID);

            if (acquireResult)
            {
                this._timer.Start();
                Feed();
            }

            return acquireResult;
        }

        public void Stop()
        {
            this._timer.Stop();

            ResetAxes();
            ResetButtons();

            Update();

            this._joy.RelinquishVJD(this._settings.DeviceID);

            this.CheckVJoyState();
            this.ShowAdditionalVJoyInfo();
        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Settings.TimerInterval))
            {
                this._timer.Interval = this._settings.TimerInterval;
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
            var mousePosition = this._interop.GetCursorPosition();

            var xAxisValue = CalculateAxisValue(mousePosition, this._settings.AxisX);
            var yAxisValue = CalculateAxisValue(mousePosition, this._settings.AxisY);

            SetAxes(xAxisValue, yAxisValue);
        }

        // extract button logic
        private void UpdateButtons()
        {
            ResetButtons();

            if (this._settings.Buttons[0] && this._interop.IsMouseButtonDown(MouseButton.Left))
            {
                this._joystickState.Buttons |= 1;
            }
            if (this._settings.Buttons[1] && this._interop.IsMouseButtonDown(MouseButton.Right))
            {
                this._joystickState.Buttons |= 1 << 1;
            }
            if (this._settings.Buttons[2] && this._interop.IsMouseButtonDown(MouseButton.Middle))
            {
                this._joystickState.Buttons |= 1 << 2;
            }
            if (this._settings.Buttons[3] && this._interop.IsMouseButtonDown(MouseButton.Extra))
            {
                this._joystickState.Buttons |= 1 << 3;
            }
            if (this._settings.Buttons[4] && this._interop.IsMouseButtonDown(MouseButton.Extra2))
            {
                this._joystickState.Buttons |= 1 << 4;
            }
        }

        private void ResetButtons()
            => this._joystickState.Buttons = 0;

        private int CalculateAxisValue(IntPoint mousePosition, AxisSettings axisSettings)
        {
            switch (axisSettings.MouseAxis)
            {
                case MouseAxis.X:
                    return CalculateAxisValue(mousePosition.X, this._settings.ResolutionX, axisSettings);
                case MouseAxis.Y:
                    return CalculateAxisValue(mousePosition.Y, this._settings.ResolutionY, axisSettings);
                case MouseAxis.None:
                    return this.AxisDisabledValue;
                default:
                    this._logger.Log("Invalid MouseAxis");
                    return this.AxisDisabledValue;
            }
        }

        // TODO: refactor
        private int CalculateAxisValue(int pixel, int pixelsCount, AxisSettings axisSettings)
        {
            var zoneDistribution = axisSettings.ZoneDistribution;

            var value = (double)pixel / (pixelsCount - 1);
            value *= zoneDistribution.Total;
            var zone = GetZone(value, zoneDistribution);
            switch (zone)
            {
                case Zone.NegativeDead:
                    value = 0;
                    break;
                case Zone.Negative:
                    // value is hard to define
                    value -= zoneDistribution.NegativeDeadZoneEnd;
                    value /= zoneDistribution.NegativeZone;

                    // value is between 0 and 1 (easiest to manipulate)
                    switch (axisSettings.NegativeFunctionType)
                    {
                        case FunctionType.Square:
                            value = Math.Pow(value, 2);
                            break;
                        case FunctionType.InvertedSquare:
                            value = Math.Sqrt(value);
                            break;
                        case FunctionType.Linear:
                        default:
                            break;
                    }
                    //
                    value /= 2;
                    // value is between 0 and 0.5
                    break;
                case Zone.NeutralDead:
                    value = 0.5f;
                    break;
                case Zone.Positive:
                    // value is hard to define
                    value -= zoneDistribution.NeutralDeadZoneEnd;
                    value /= zoneDistribution.PositiveZone;

                    // value is between 0 and 1 (easiest to manipulate)
                    switch (axisSettings.PositiveFunctionType)
                    {
                        case FunctionType.Square:
                            value = Math.Pow(value, 2);
                            break;
                        case FunctionType.InvertedSquare:
                            value = Math.Sqrt(value);
                            break;
                        case FunctionType.Linear:
                        default:
                            break;
                    }
                    //
                    value /= 2;
                    value += 0.5f;
                    // value is between 0.5 and 1
                    break;
                case Zone.PositiveDead:
                    value = 1;
                    break;
                default:
                    this._logger.Log("Feeder: Invalid Zone");
                    break;
            }
            return Convert.ToInt32(value * this.AxisMaxValue);
        }

        private void ResetAxes()
            => this.SetAxes(this.AxisDisabledValue);

        private void SetAxes(int value)
            => this.SetAxes(value, value);

        private void SetAxes(int x, int y)
        {
            this._joystickState.AxisX = x;
            this._joystickState.AxisY = y;
        }

        private void Update()
        {
            this._joy.UpdateVJD(this._settings.DeviceID, ref this._joystickState);
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
            if (this._timer != null)
            {
                this._timer.Elapsed -= this.OnTimerTick;
                this._timer = null;
            }

            if (this._settings != null)
            {
                this._settings.PropertyChanged -= this.OnSettingsPropertyChanged;
            }

            Stop();
        }

        // TODO: extract
        private bool CheckVJoyState()
        {
            StringBuilder stringBuilder = new StringBuilder().AppendLine("vJoy Device ID: " + this._settings.DeviceID);
            bool canContinue = true;
            string message;

            if (!this._joy.vJoyEnabled())
            {
                stringBuilder.AppendLine("vJoy is not enabled.");
                canContinue = false;
            }

            VjdStat status = this._joy.GetVJDStatus(this._settings.DeviceID);

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

            this._logger.Log(stringBuilder.ToString());

            return canContinue;
        }

        // TODO: extract
        private void ShowAdditionalVJoyInfo()
        {
            int virtualButtonsCount = this._joy.GetVJDButtonNumber(this._settings.DeviceID);

            if (virtualButtonsCount < 5)
            {
                this._logger.Log($"Only {virtualButtonsCount} virtual buttons are enabled, while 5 might be needed for handling all mouse buttons");
            }
            else
            {
                this._logger.Log($"{virtualButtonsCount} virtual buttons are enabled.");
            }

            bool xAxisAvailable = this._joy.GetVJDAxisExist(this._settings.DeviceID, HID_USAGES.HID_USAGE_X);
            bool yAxisAvailable = this._joy.GetVJDAxisExist(this._settings.DeviceID, HID_USAGES.HID_USAGE_Y);
            bool zAxisAvailable = this._joy.GetVJDAxisExist(this._settings.DeviceID, HID_USAGES.HID_USAGE_Z);

            if (!xAxisAvailable) this._logger.Log($"Virtual axis X is disabled and will be unavailable.");
            if (!yAxisAvailable) this._logger.Log($"Virtual axis Y is disabled and will be unavailable.");
            if (!zAxisAvailable) this._logger.Log($"Virtual axis Z is disabled and will be unavailable.");
        }
    }
}
