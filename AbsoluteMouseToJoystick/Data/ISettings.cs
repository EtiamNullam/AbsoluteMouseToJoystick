namespace AbsoluteMouseToJoystick.Data
{
    // derive from INotifyPropertyChanged?
    public interface ISettings
    {
        uint DeviceID { get; set; }
        int ResolutionY { get; set; }
        int ResolutionX { get; set; }
        double TimerInterval { get; set; }
        AxisSettings AxisX { get; set; }
        AxisSettings AxisY { get; set; }
        AxisSettings AxisZ { get; set; }
    }
}