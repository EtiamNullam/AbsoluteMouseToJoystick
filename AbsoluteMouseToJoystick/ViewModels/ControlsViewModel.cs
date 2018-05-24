using AbsoluteMouseToJoystick.Data;
using AbsoluteMouseToJoystick.IO;
using AbsoluteMouseToJoystick.Logging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick.ViewModels
{
    public class ControlsViewModel : ViewModelBase
    {
        public ControlsViewModel(ISimpleLogger logger, ISettingsManager settings, JsonFileManager jsonFileManager)
        {
            _logger = logger;
            _jsonFileManager = jsonFileManager;

            Settings = settings;

            StartStopCommand = new RelayCommand(this.StartStop);
            LoadCommand = new RelayCommand(this.LoadSettings);
            SaveCommand = new RelayCommand(this.SaveSettings);

            ShowJoystickInfo(_joy, Settings.DeviceID);

            LoadDefaultSettings();
        }

        // TODO: Remove magic string
        private void LoadDefaultSettings()
        {
            try
            {
                Settings.Load(_jsonFileManager.Open<SettingsRaw>(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "default.json"));
            }
            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }

        public ICommand StartStopCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public bool IsRunning
        {
            get => _isRunning;
            set => Set(ref _isRunning, value);
        }

        public ISettingsManager Settings { get; private set; }

        private readonly ISimpleLogger _logger;
        private readonly JsonFileManager _jsonFileManager;
        private readonly vJoy _joy = new vJoy();
        private Feeder _feeder;
        private Timer _timer = new Timer { AutoReset = true };
        private bool _isRunning = false;

        private void StartStop()
        {
            if (!IsRunning) Start();
            else Stop();
        }

        // Extract to other class
        private void SaveSettings()
        {
            _jsonFileManager.Save(Settings);
        }

        private void LoadSettings()
        {
            _jsonFileManager.OnFileLoaded += OnSettingsLoaded;
            _jsonFileManager.OpenWithDialog<SettingsRaw>();
        }

        private void OnSettingsLoaded(object sender, object settings)
        {
            _jsonFileManager.OnFileLoaded -= OnSettingsLoaded;

            if (settings is ISettings castedSettings)
            {
                Settings.Load(castedSettings);
            }
        }
        //

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
