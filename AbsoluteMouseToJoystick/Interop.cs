using AbsoluteMouseToJoystick.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AbsoluteMouseToJoystick
{
    public class Interop
    {
        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out IntPoint lpPoint);

        public IntPoint GetCursorPosition()
        {
            GetCursorPos(out IntPoint lpPoint);

            return lpPoint;
        }
    }
}
