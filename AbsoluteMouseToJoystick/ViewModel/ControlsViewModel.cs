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
        public ControlsViewModel(ISimpleLogger logger, Settings settings)
        {
            _logger = logger;
            Settings = settings;

            StartStopCommand = new RelayCommand(this.StartStop);
        }

        public ICommand StartStopCommand { get; private set; }

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

        private ISimpleLogger _logger;
        private vJoy _joy = new vJoy();
        public Settings Settings { get; set; }
        private Feeder _feeder;
        private Timer _timer = new Timer { AutoReset = true };
        private bool _isRunning = false;

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
                _timer.Interval = Settings.TimerInterval;

                if (_joy.AcquireVJD(Settings.DeviceID))
                {
                    _logger.Log("Device acquired.");
                    _timer.Start();

                    _feeder = new Feeder(_joy, _logger, _timer, Settings);
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

            _joy.RelinquishVJD(Settings.DeviceID);

            _logger.Log("Device relinquished.");

            ShowJoystickInfo(_joy, Settings.DeviceID);
        }
    }
}
