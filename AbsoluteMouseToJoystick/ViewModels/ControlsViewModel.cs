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
        public ControlsViewModel(ISimpleLogger logger, ISettingsManager settings, JsonFileManager jsonFileManager, Feeder feeder)
        {
            _logger = logger;
            _jsonFileManager = jsonFileManager;
            _feeder = feeder;

            Settings = settings;

            StartStopCommand = new RelayCommand(this.StartStop);
            LoadCommand = new RelayCommand(this.LoadSettings);
            SaveCommand = new RelayCommand(this.SaveSettings);

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
        private readonly Feeder _feeder;
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

                if (IsRunning)
                {
                    Stop();
                    Start();
                }
            }
        }
        private void Start()
        {
            try
            {
                if (_feeder.Start())
                {
                    IsRunning = true;
                    _logger.Log("Device acquired. Feeder started.");
                }
                else _logger.Log("Device acquire FAILED.");
            }

            catch (Exception e)
            {
                _logger.Log(e.Message);
            }
        }

        private void Stop()
        {
            _feeder.Stop();
            IsRunning = false;

            _logger.Log("Device relinquished.");
        }
    }
}
