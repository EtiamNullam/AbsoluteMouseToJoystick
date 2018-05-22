using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AbsoluteMouseToJoystick
{
    public class MessageBasedLogger : ISimpleLogger
    {
        public void Log(string message)
        {
            Messenger.Default.Send(message);
        }
    }
}
