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
        public ControlsViewModel(ISimpleLogger logger, ISettingsManager settings, Feeder feeder)
        {
            _logger = logger;
            _feeder = feeder;

            Settings = settings;

            StartStopCommand = new RelayCommand(this.StartStop);
            LoadCommand = new RelayCommand(this.Settings.LoadFromFile);
            SaveCommand = new RelayCommand(this.Settings.SaveToFile);
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
        private readonly Feeder _feeder;
        private bool _isRunning = false;

        private void StartStop()
        {
            if (!IsRunning) Start();
            else Stop();
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
