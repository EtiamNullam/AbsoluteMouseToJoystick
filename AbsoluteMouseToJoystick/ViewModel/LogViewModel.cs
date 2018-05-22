using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick.ViewModel
{
    public class LogViewModel : ViewModelBase
    {
        public LogViewModel()
        {
            Messenger.Default.Register<string>(this, LogMessage);
        }

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
    }
}
