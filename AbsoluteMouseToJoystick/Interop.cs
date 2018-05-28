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

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(UInt16 virtualKeyCode);

        public IntPoint GetCursorPosition()
        {
            GetCursorPos(out IntPoint lpPoint);

            return lpPoint;
        }

        // bitwise testing needed
        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return GetAsyncKeyState((ushort)mouseButton) < 0;
        }
    }
}
