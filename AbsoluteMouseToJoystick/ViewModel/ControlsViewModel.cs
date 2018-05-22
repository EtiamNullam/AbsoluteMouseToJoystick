using AbsoluteMouseToJoystick.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick.ViewModel
{
    public class ControlsViewModel : ViewModelBase
    {
        public ControlsViewModel(ISimpleLogger logger)
        {
            _logger = logger;

            StartStopCommand = new RelayCommand(this.StartStop);
        }

        public ICommand StartStopCommand { get; private set; }

        public int ResolutionX
        {
            get => _resolutionX;
            set => Set(ref _resolutionX, value);
        }
        public int ResolutionY
        {
            get => _resolutionY;
            set => Set(ref _resolutionY, value);
        }

        public double TimerInterval
        {
            get => _timerInterval;
            set => Set(ref _timerInterval, value);
        }
        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                Set(ref _isRunning, value);
            }
        }

        public uint DeviceID
        {
            get => _deviceID;
            set => Set(ref _deviceID, value);
        }
        public ZoneDistribution ZoneDistributionX
        {
            get => _zoneDistributionX;
            set
            {
                Set(ref _zoneDistributionX, value);
            }
        }
        public ZoneDistribution ZoneDistributionY
        {
            get => _zoneDistributionY;
            set => Set(ref _zoneDistributionY, value);
        }

        private int _resolutionX = 1920;
        private int _resolutionY = 1080;
        private bool _isRunning = false;
        private double _timerInterval = 5;
        private uint _deviceID = 1;
        private ZoneDistribution _zoneDistributionX = new ZoneDistribution
        {
            NegativeDeadZone = 1,
            NegativeZone = 100,
            NeutralDeadZone = 0,
            PositiveZone = 100,
            PositiveDeadZone = 1,
        };
        private ZoneDistribution _zoneDistributionY = new ZoneDistribution
        {
            NegativeDeadZone = 1,
            NegativeZone = 12,
            NeutralDeadZone = 2,
            PositiveZone = 8,
            PositiveDeadZone = 3,
        };

        private ISimpleLogger _logger;
        private vJoy _joy = new vJoy();
        private Feeder _feeder;
        private Timer _timer = new Timer { AutoReset = true };

        private void StartStop()
        {
            if (!IsRunning) Start();
            else Stop();
        }

        // TODO:
        // - Acquire somewhere else
        // - Relinquish on exit (or some button press)
        private bool ShowJoystickInfo(vJoy joy, uint deviceID)
        {
            UInt32 DllVer = 0, DrvVer = 0;
            if (joy.DriverMatch(ref DllVer, ref DrvVer) && joy.vJoyEnabled())
            {
                // Get the state of the requested device
                VjdStat status = joy.GetVJDStatus(deviceID);

                switch (status)
                {
                    case VjdStat.VJD_STAT_OWN:
                        _logger.Log($"vJoy Device {deviceID} is already owned by this feeder");
                        break;
                    case VjdStat.VJD_STAT_FREE:
                        _logger.Log($"vJoy Device {deviceID} is free");
                        break;
                    case VjdStat.VJD_STAT_BUSY:
                        _logger.Log($"vJoy Device {deviceID} is already owned by another feeder\nCannot continue");
                        return false;
                    case VjdStat.VJD_STAT_MISS:
                        _logger.Log($"vJoy Device {deviceID} is not installed or disabled\nCannot continue");
                        return false;
                    default:
                        _logger.Log($"vJoy Device {deviceID} general error\nCannot continue");
                        return false;
                }

                ///// vJoy Device properties
                int nBtn = joy.GetVJDButtonNumber(deviceID);
                int nDPov = joy.GetVJDDiscPovNumber(deviceID);
                int nCPov = joy.GetVJDContPovNumber(deviceID);
                bool X_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_X);
                bool Y_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_Y);
                bool Z_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_Z);
                bool RX_Exist = joy.GetVJDAxisExist(deviceID, HID_USAGES.HID_USAGE_RX);
                _logger.Log($"Device[{deviceID}]: Buttons={nBtn}; DiscPOVs:{nDPov}; ContPOVs:{nCPov}");
                return true;
            }
            else return false;
        }

        private void Start()
        {
            try
            {
                _timer.Interval = TimerInterval;

                if (_joy.AcquireVJD(this.DeviceID))
                {
                    _logger.Log("Device acquired.");
                    _timer.Start();

                    _feeder = new Feeder(_joy, DeviceID, _logger, _timer, this.ZoneDistributionX, this.ZoneDistributionY, ResolutionX, ResolutionY);
                }
                else
                {
                    _logger.Log("Device acquire FAILED.");
                }

                IsRunning = true;
            }
            catch (Exception e)
            {
                _logger?.Log(e.Message);
            }
        }

        private void Stop()
        {
            IsRunning = false;

            _timer.Stop();

            if (_feeder != null)
            {
                _feeder.Dispose();
                _feeder = null;
            }

            _joy.RelinquishVJD(DeviceID);

            _logger.Log("Device relinquished.");

            ShowJoystickInfo(_joy, DeviceID);
        }
        /*
                public bool Running
                {
                    get
                    {
                        return _running;
                    }
                    set
                    {
                        _running = value;
                        //this.runningCheckBox.IsChecked = _running;
                        //this.TimerIntervalTextBox.IsEnabled = this.resXtb.IsEnabled = this.resYtb.IsEnabled = !_running;
                    }
                }

                private bool _running;

                private void OnZoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
                {
                    _logger?.Log("Zone text changed");

                    UpdateZoneDistributions();
                }


                private void UpdateZoneDistributions()
                {
                    try
                    {
                        negativeDeadColumnX.Width = new GridLength(_zoneDistributionX.NegativeDeadZone = Convert.ToDouble(this.negativeDeadXtb.Text), GridUnitType.Star);
                        negativeColumnX.Width = new GridLength(_zoneDistributionX.NegativeZone = Convert.ToDouble(this.negativeXtb.Text), GridUnitType.Star);
                        neutralColumnX.Width = new GridLength(_zoneDistributionX.NeutralDeadZone = Convert.ToDouble(this.neutralXtb.Text), GridUnitType.Star);
                        positiveColumnX.Width = new GridLength(_zoneDistributionX.PositiveZone = Convert.ToDouble(this.positiveXtb.Text), GridUnitType.Star);
                        positiveDeadColumnX.Width = new GridLength(_zoneDistributionX.PositiveDeadZone = Convert.ToDouble(this.positiveDeadXtb.Text), GridUnitType.Star);

                        negativeDeadRowY.Height = new GridLength(_zoneDistributionY.NegativeDeadZone = Convert.ToDouble(this.negativeDeadYtb.Text), GridUnitType.Star);
                        negativeRowY.Height = new GridLength(_zoneDistributionY.NegativeZone = Convert.ToDouble(this.negativeYtb.Text), GridUnitType.Star);
                        neutralRowY.Height = new GridLength(_zoneDistributionY.NeutralDeadZone = Convert.ToDouble(this.neutralYtb.Text), GridUnitType.Star);
                        positiveRowY.Height = new GridLength(_zoneDistributionY.PositiveZone = Convert.ToDouble(this.positiveYtb.Text), GridUnitType.Star);
                        positiveDeadRowY.Height = new GridLength(_zoneDistributionY.PositiveDeadZone = Convert.ToDouble(this.positiveDeadYtb.Text), GridUnitType.Star);
                    }
                    catch (Exception ex)
                    {
                        _logger?.Log(ex.Message);
                        _logger?.Log("Invalid zone format");
                    }
                }
            */
    }
}
