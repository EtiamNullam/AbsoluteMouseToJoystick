using System.Runtime.InteropServices;

namespace AbsoluteMouseToJoystick.Data
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IntPoint
    {
        public int X;
        public int Y;
    }
}
