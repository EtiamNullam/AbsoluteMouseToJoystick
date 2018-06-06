namespace AbsoluteMouseToJoystick.Data
{
    public interface ISettingsManager
    {
        void Load(ISettings settings);
        void SaveToFile();
        void LoadFromFile();
    }
}