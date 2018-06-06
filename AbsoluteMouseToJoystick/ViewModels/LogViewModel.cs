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
            ClearLogCommand = new RelayCommand(ClearLog);

            Messenger.Default.Register<string>(this, LogMessage);
        }

        public ICommand ClearLogCommand { get; private set; }

        public string Log
        {
            get => _log;
            set => Set(nameof(Log), ref _log, value);
        }

        private string _log;

        private void LogMessage(string message)
        {
            Log = $"{message}\n{Log}";
        }

        private void ClearLog()
        {
            Log = "Log cleared";
        }
    }
}
