using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AbsoluteMouseToJoystick.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        public LogViewModel()
        {
            this.ClearLogCommand = new RelayCommand(this.ClearLog);

            Messenger.Default.Register<string>(this, this.LogMessage);
        }

        public ICommand ClearLogCommand { get; private set; }

        public string Log
        {
            get => this._log;
            set => Set(nameof(this.Log), ref this._log, value);
        }

        private string _log;

        private void LogMessage(string message)
        {
            this.Log = $"{message}\n{this.Log}";
        }

        private void ClearLog()
        {
            this.Log = "Log cleared";
        }
    }
}
