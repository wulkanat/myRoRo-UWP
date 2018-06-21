using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace myRoRo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Schedule> schedules;

        public MainPage()
        {
            #if DEBUG
                Debug.WriteLine("Starting App...");
            #endif
            InitializeComponent();

            LoadingProcessProgressRing.IsActive = true;
            LoadingProcessProgressRing.Visibility = Windows.UI.Xaml.Visibility.Visible;

            InitLoadSequence();
            #if DEBUG
                Debug.WriteLine("Initialization Complete.");
            #endif


            //Setting up byckground sync

            /*TimeTrigger backgroundSync = new TimeTrigger(15, false);
            BackgroundExecutionManager.RequestAccessAsync();

            string entryPoint = "myRoRo.Sync";
            string taskName = "NotificationSync";

            BackgroundTaskRegistration task = RegisterBackgroundTask(entryPoint, taskName, backgroundSync);*/
        }

        private async void InitLoadSequence()
        {
            #if DEBUG
                Debug.WriteLine("Network Task created!");
            #endif
            await ScheduleNetwork.Refresh();
            #if DEBUG
                Debug.WriteLine("Loading Schedule Complete.");
            #endif

            schedules = null;
            schedules = ScheduleManager.GetSchedules();

            //Updating Bindings (the data behind the Pivot)
            Binding binding = new Binding();
            binding.Source = schedules;
            binding.Mode = BindingMode.TwoWay;
            binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            BindingOperations.SetBinding(pivot1, Pivot.ItemsSourceProperty, binding);

            LoadingProcessProgressRing.IsActive = false;
            LoadingProcessProgressRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            Windows.Storage.ApplicationDataContainer localSettings =
                                    Windows.Storage.ApplicationData.Current.LocalSettings;
            pivot1.Title = (string)localSettings.Values["UpdateDate"];
        }
    }
}
