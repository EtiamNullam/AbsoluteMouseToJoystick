using System;
using AbsoluteMouseToJoystick.Data;
using GalaSoft.MvvmLight;

namespace AbsoluteMouseToJoystick.ViewModels
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
     * - Add textbox for device ID
     * - Allow for use of different axes
     * - Add tooltips over preview, buttons and fields
     */

    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(ISettings settings)
        {
            Settings = settings;
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }

        public ISettings Settings { get; set; }
    }
}