/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:AbsoluteMouseToJoystick"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using AbsoluteMouseToJoystick.Data;
using AbsoluteMouseToJoystick.IO;
using AbsoluteMouseToJoystick.Logging;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using System;
using vJoyInterfaceWrap;

namespace AbsoluteMouseToJoystick.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var container = SimpleIoc.Default;

            if (ViewModelBase.IsInDesignModeStatic) { }
            else
            {
                container.Register<JsonSerializer>(() => new JsonSerializer { Formatting = Formatting.Indented });
            }

            container.Register<MainViewModel>();
            container.Register<LogViewModel>();
            container.Register<ControlsViewModel>();

            container.Register<vJoy>();
            container.Register<ISimpleLogger, MessageBasedLogger>();
            container.Register<ISettingsBindable, Settings>();
            container.Register<ISettingsManager, SettingsManager>();
            container.Register<JsonFileManager>();
            container.Register<Feeder>();
            container.Register<Interop>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public LogViewModel Log => ServiceLocator.Current.GetInstance<LogViewModel>();

        public ControlsViewModel Controls => ServiceLocator.Current.GetInstance<ControlsViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}