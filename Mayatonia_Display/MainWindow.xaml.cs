using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MayatoniaDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        /// <summary>
        /// Sets the Auto Start Delay
        /// </summary>
        private int _AUTO_START_DELAY = 60;

        /// <summary>
        /// This bool tracks the running state of the Situational Awareness Show
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// The Startup Timer that will automatically launch the Situation Awareness Show
        /// </summary>
        private System.Timers.Timer TimerStartup = new System.Timers.Timer(100.00); // Fire Every 0.1 seconds        

        /// <summary>
        /// Track When the timer started
        /// </summary>
        private DateTime dtStartUpTime;

        /// <summary>
        /// A list of the displays that are fired up for easy tracking/startup/shutdown
        /// </summary>
        private List<Window> _Displays = new List<Window>();


        /// <summary>
        /// Object contains all the display configurations for the app
        /// </summary>
        Settings displaySettings;


        public MainWindow()
        {
            InitializeComponent();

            //LoadDisplaySettings
            displaySettings = Settings.Load();
            Settings.Instance = displaySettings;

            this.lblBanner.Content = displaySettings.Banner_Text;

            //Add the Pre-Set 12 Displays
            AddDisplays(12, displaySettings);

            //Autostart only if indicated.
            if (displaySettings.AutoStartSeconds > 0)
            {

                _AUTO_START_DELAY = displaySettings.AutoStartSeconds;
                //Register the Automatic Timer Event Handler
                TimerStartup.Elapsed += TimerStartup_Elapsed;

                //Initialize Time Object
                dtStartUpTime = DateTime.Now;

                TimerStartup.Start();
            }
        }

        private void TimerStartup_Elapsed(object sender, ElapsedEventArgs e)
        {

            
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessTimerEvent(e.SignalTime);
                    });
                else
                    ProcessTimerEvent(e.SignalTime);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void ProcessTimerEvent(DateTime timerEventTime)
        {
            lblStatus.Content = String.Format("Auto start in {0} seconds...", _AUTO_START_DELAY - (int)timerEventTime.Subtract(dtStartUpTime).TotalSeconds);

            if (timerEventTime.Subtract(dtStartUpTime).TotalSeconds > _AUTO_START_DELAY)
            {
                if (TimerStartup != null)
                {
                    TimerStartup.Elapsed -= TimerStartup_Elapsed;
                    TimerStartup.Stop();
                    TimerStartup = null;
                }

                Start();
            }
        }

        /// <summary>
        /// Add Displays to the main window
        /// </summary>
        /// <param name="DisplayCount"></param>
        private void AddDisplays(int DisplayCount, Settings displaySettings)
        {
            int currentRow = 0;
            DisplayConfiguration displayConfig;

            for (int i = 0; i < DisplayCount; i++)
            {
                //Up to 4 Diplays per Row
                if (i % 4 == 0)
                    currentRow += 1;

                if (i < displaySettings.DisplayList.Count)
                    displayConfig = displaySettings.DisplayList[i];
                else
                {
                    displayConfig = new DisplayConfiguration();
                    displaySettings.DisplayList.Add(displayConfig);
                }

                displayConfig.DisplayIndex = i;
                Display displayControl = new Display(String.Format("Display #{0}", (i + 1).ToString()), displayConfig);
                         
                StackPanel currentStack = stackMainContent.Children[currentRow] as StackPanel;
                if (currentStack != null)
                    currentStack.Children.Add(displayControl);
            }
        }

        private void btnStopStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsRunning)
                Stop();
            else
                Start();  
        }

        /// <summary>
        /// Starts the situational display session
        /// </summary>
        private void Start()
        {
            if (!IsRunning)
            {
                IsRunning = true;

                lblStatus.Content = "Initializing Situational Awareness Displays...";
                //Make sure the timer has ended
                if (TimerStartup != null && TimerStartup.Enabled)
                    TimerStartup.Stop();

                txtStopStart.Text = "Stop ";
                imgStopStart.Source = new BitmapImage(new Uri("pack://application:,,,/NWSDisplay;component/Resources/Stop.png"));

                //Only load the count of displays we have
                int screenCount = Screen.AllScreens.Count();                
               
                //Dynamic Display Assignments based on available screens
                if (displaySettings.DynamicallyAssignDisplays == true) 
                {
                    //Create 2 lists, one for the situational display windows and another for the monitor/screens
                    List<DisplayConfiguration> dspList = new List<DisplayConfiguration>();
                    List<Screen> screenList = new List<Screen>();

                    foreach (DisplayConfiguration dsp in displaySettings.DisplayList)
                    {
                        //Only add enabled display screens
                        if (dsp.DisplayEnabled)
                            dspList.Add(dsp);
                    }

                    //add Screens beginning with primary
                    screenList.Add(Screen.PrimaryScreen);
                    foreach(Screen screen in Screen.AllScreens)
                    {
                        if (!screen.Primary)
                            screenList.Add(screen);
                    }

                    //Determine screen or display bounds
                    int arrayUpperBounds = dspList.Count;
                    if (arrayUpperBounds > screenList.Count)
                        arrayUpperBounds = screenList.Count;

                    for (int i = 0; i < arrayUpperBounds; i++)
                    {
                        Screen monitor = screenList[i];
                        DisplayConfiguration display = dspList[i];

                        ShowDisplayWindow(monitor, display);
                    }

                }
                else //Static Display Assignment based on Situational display index
                {
                    for (int i = 0; i < screenCount; i++)
                    {

                        DisplayConfiguration displayConfig = displaySettings.DisplayList[i];
                        Screen monitor = Screen.AllScreens[i] as Screen;
                        ShowDisplayWindow(monitor, displayConfig);

                    }
                }

                
            }
        }

        /// <summary>
        /// Initialize the Display Window and Assign it to a screen.
        /// </summary>
        /// <param name="monitor"></param>
        /// <param name="displayConfig"></param>
        private void ShowDisplayWindow(Screen monitor, DisplayConfiguration displayConfig)
        {
            if (displayConfig.DisplayEnabled)
            {
                Window wnd = new Window();

                wnd = new DisplaySingleWindow(displayConfig);
                wnd.WindowState = WindowState.Normal;
                //wnd.WindowStartupLocation = WindowStartupLocation.Manual;
                wnd.Top = monitor.WorkingArea.Location.Y;
                wnd.Left = monitor.WorkingArea.Location.X;
                wnd.Width = monitor.WorkingArea.Width;
                wnd.Height = monitor.WorkingArea.Height;

                //wnd.WindowState = WindowState.Maximized;
                wnd.Show();


                _Displays.Add(wnd);
            }
        }

        /// <summary>
        /// Stops the situational display session
        /// </summary>
        private void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;

                lblStatus.Content = "Stopping Situational Awareness Displays...";

                txtStopStart.Text = "Start ";
                imgStopStart.Source = new BitmapImage(new Uri("pack://application:,,,/NWSDisplay;component/Resources/Start.png"));

               
                foreach(DisplaySingleWindow wnd in _Displays)
                {
                    wnd.Stop();
                    wnd.Close();                    
                }

                _Displays.Clear();

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {


            displaySettings = null;
            Settings.Instance = null;  

            GC.Collect();
            
            Process.GetCurrentProcess().Kill();

        }

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            SetupConfigWindow wnd = new SetupConfigWindow(this.displaySettings);
            wnd.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            wnd.ShowDialog();

            //Save settings to disk
            if (wnd.SavedSettings != null)
                Settings.Save(wnd.SavedSettings);

        }
    }
}
