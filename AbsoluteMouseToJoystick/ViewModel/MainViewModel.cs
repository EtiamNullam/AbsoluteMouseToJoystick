using GalaSoft.MvvmLight;

namespace AbsoluteMouseToJoystick.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    
    /* TODO:
     * - Show current values in app (button)
     *  - Also show dot on preview
     * - Add some kind of MVVM
     *  - Split into regions (region for axisX region for axisX preview etc)
     *  - Make them reusable (region for axisX and Y should only differ in a parameter passed
     * - Save options to files (json)
     * - Add textbox for device ID
     * - Allow for use of different axes
     * - Add tooltips over preview, buttons and fields
     * - Allow creation of profiles to save preset for different conditions or games
     */

    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}