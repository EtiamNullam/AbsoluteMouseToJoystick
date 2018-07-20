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
        public ControlsViewModel(ISimpleLogger logger, ISettingsManager settingsManager, ISettingsBindable settings, Feeder feeder)
        {
            this._logger = logger;
            this._feeder = feeder;

            this._settingsManager = settingsManager;
            this.Settings = settings;

            this.StartStopCommand = new RelayCommand(this.StartStop);
            this.LoadCommand = new RelayCommand(this._settingsManager.LoadFromFile);
            this.SaveCommand = new RelayCommand(this._settingsManager.SaveToFile);
        }

        public ICommand StartStopCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public bool IsRunning
        {
            get => this._isRunning;
            set => Set(ref this._isRunning, value);
        }

        public ISettingsBindable Settings { get; private set; }

        private readonly ISettingsManager _settingsManager;

        private readonly ISimpleLogger _logger;
        private readonly Feeder _feeder;
        private bool _isRunning = false;

        private void StartStop()
        {
            if (!this.IsRunning) Start();
            else Stop();
        }

        private void Start()
        {
            try
            {
                if (this._feeder.Start())
                {
                    this.IsRunning = true;
                    this._logger.Log("Device acquired. Feeder started.");
                }
                else this._logger.Log("Device acquire FAILED.");
            }

            catch (Exception e)
            {
                this._logger.Log(e.Message);
            }
        }

        private void Stop()
        {
            this._feeder.Stop();
            this.IsRunning = false;

            this._logger.Log("Device relinquished.");
        }
    }
}
