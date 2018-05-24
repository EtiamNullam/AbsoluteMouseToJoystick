namespace AbsoluteMouseToJoystick.Data
{
    public interface ISettingsManager : ISettings
    {
        void Load(ISettings settings);
    }
}