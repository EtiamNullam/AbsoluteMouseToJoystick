namespace AbsoluteMouseToJoystick.Data
{
    // derive from INotifyPropertyChanged?
    public interface ISettings
    {
        uint DeviceID { get; set; }
        int ResolutionY { get; set; }
        int ResolutionX { get; set; }
        double TimerInterval { get; set; }
        ZoneDistribution ZoneDistributionX { get; set; }
        ZoneDistribution ZoneDistributionY { get; set; }
    }
}