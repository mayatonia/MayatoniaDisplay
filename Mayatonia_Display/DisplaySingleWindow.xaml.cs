using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Interop;
using System.IO;

namespace MayatoniaDisplay
{
    /// <summary>
    /// Interaction logic for DisplaySingleWindow.xaml
    /// </summary>
    public partial class DisplaySingleWindow : Window
    {

        [DllImport("user32.dll")]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);[DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);[DllImport("user32.dll")]
        static extern bool MoveWindow(IntPtr Handle, int x, int y, int w, int h, bool repaint);


        /// <summary>
        /// This object is the main display configuration that will be driving the UI
        /// </summary>
        DisplayConfiguration DisplayConfiguration;

        /// <summary>
        /// This timer tracks the date and prints it on the screen.
        /// </summary>
        private System.Timers.Timer _TimerForDateTime = new System.Timers.Timer(15000); //run every 10 seconds


        /// <summary>
        /// The Startup Timer that will automatically launch the Situation Awareness Show
        /// </summary>
        private System.Timers.Timer _TimerStartup = new System.Timers.Timer(15000);


        /// <summary>
        /// The Main event timer that will navigate to the next item for display
        /// </summary>
        private System.Timers.Timer _TimerChangeToNextItem = new System.Timers.Timer(100); // .1 seconds


        /// <summary>
        /// The Refresh timer that will refresh the web browser
        /// </summary>
        private System.Timers.Timer _TimerRefresh = new System.Timers.Timer(1000); // 1 second

        /// <summary>
        /// Track when the next item should appear
        /// </summary>
        private DateTime dtNextItem;

        /// <summary>
        /// Track when the web browser should refresh
        /// </summary>
        private DateTime dtNextRefresh;


        /// <summary>
        /// Tracks the currently displayed item
        /// </summary>
        ListBoxItem _CurrentItem = null;

        /// <summary>
        /// Tracks the currently displayed source
        /// </summary>
        DisplaySource _CurrentSource = null;

        /// <summary>
        /// Track whether the web browser has been silenced
        /// </summary>
        private bool isWBSilenced = false;

        /// <summary>
        /// The Index of the NWS BANNER
        /// </summary>
        private int _ROW_NWSBANNER = 0;

        /// <summary>
        /// The Index of the OFFICE BANNER
        /// </summary>
        private int _ROW_OFFICE = 1;

        /// <summary>
        /// The A managed process object for the external application 
        /// </summary>
        public Process ExternalProcess
        {
            get
            {
                Process process = null;
                AppSource src = DisplayConfiguration.Sources[0] as AppSource;
                FileInfo fi = new FileInfo(src.Path);
                String fileName = fi.Name.Split('.')[0];
                
                //Lookup process by the Source's File name
                Process[] procs = Process.GetProcessesByName(fileName);
                foreach(Process proc in procs)
                {
                    process = proc;
                    break;
                }

                //Lookup process by the Source name
                if (process == null)
                {
                    procs = Process.GetProcessesByName(src.Name);
                    foreach (Process proc in procs)
                    {
                        process = proc;
                        break;
                    }
                }

                return process;

            }
        }

        public DisplaySingleWindow()
        {
            InitializeComponent();            
        }

        public DisplaySingleWindow(DisplayConfiguration displayConfig) : this()
        {
            this.Title = String.Format("Display #{0:0#}", displayConfig.DisplayIndex + 1);
            if (Settings.Instance != null)
            {
                this.lblBanner.Content = Settings.Instance.Banner_Text;
                this.lblOffice.Content = Settings.Instance.Office_Abbreviation;
            }
            

            DisplayConfiguration = displayConfig;


            ProcessDateTimerEvent(DateTime.Now); //set date time

            this.Closing += DisplaySingleWindow_Closing;
            _TimerStartup.Elapsed += _TimerStartup_Elapsed;
            _TimerChangeToNextItem.Elapsed += _TimerChangeToNextItem_Elapsed;
            _TimerRefresh.Elapsed += _TimerRefresh_Elapsed;
            _TimerForDateTime.Elapsed += _TimerForDateTime_Elapsed;

            //Only begin the startup timer
            _TimerStartup.Start();
            _TimerForDateTime.Start();


            switch (displayConfig.DisplayTypeID)
            {
                case 1: //Single Display
                case 2: //Quad Display
                case 3: //Hybrid Display
                    PopulateList();
                    break;
                case 4:
                     if (displayConfig.Sources.Count > 0)
                    {

                        gridBrowser.ColumnDefinitions[1].Width = new GridLength(0.0);

                        AppSource app = displayConfig.Sources[0] as AppSource;
                        using (Process proc = new Process())
                        {
                            ProcessStartInfo psi = new ProcessStartInfo(app.Path, app.Parameters);
                            FileInfo fi = new FileInfo(app.Path);
                            psi.WorkingDirectory = fi.Directory.FullName.Trim();
                            psi.UseShellExecute = true;
                            proc.StartInfo = psi;

                            proc.Start();

                        }

                    }
                    break;

            }


            if (displayConfig.DisplayNetworkStatus == true) // Only on first monitor add network status control
            {
                lblStatus.Content = "  Network Status: ";
                lblStatus.FontSize = 32.0;                
                foreach(Connection con in Settings.Instance.ConnectionPoints)
                {
                    NetworkStatus netStat = new NetworkStatus(con);
                    stackStatus.Children.Add(netStat);
                }
            }

            //Hide Headers if applicable
            if (displayConfig.DisplayHeader == false)
            {
                gridMaster.RowDefinitions[_ROW_NWSBANNER].Height = new GridLength(0.0);
                gridMaster.RowDefinitions[_ROW_OFFICE].Height = new GridLength(0.0);
            }
            

        }

        /// <summary>
        /// Responsible for sizing a new Process Window
        /// </summary>
        /// <param name="proc"></param>
        private void ResizeProcess(Process proc)
        {
            Window window = Window.GetWindow(this);
            var wih = new WindowInteropHelper(window);
            IntPtr hWnd = wih.Handle;
            SetParent(proc.MainWindowHandle, hWnd);

            MoveWindow(proc.MainWindowHandle, (int)this.Left, (int)this.Top, (int)this.ActualWidth, (int)this.ActualHeight, true);
        }

        private void DisplaySingleWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
           
            try
            {
                this.Closing -= DisplaySingleWindow_Closing;
                Stop();
                foreach (NetworkStatus netStat in this.stackStatus.Children)
                {
                    try
                    {
                        netStat.Cleanup();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                    }
                }
            }
            catch { }


        }

        private void _TimerForDateTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessDateTimerEvent(e.SignalTime);
                    });
                else
                    ProcessDateTimerEvent(e.SignalTime);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void _TimerStartup_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessStartupTimerEvent(e.SignalTime);
                    });
                else
                    ProcessStartupTimerEvent(e.SignalTime);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void _TimerRefresh_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessRefreshEvent(e.SignalTime);
                    });
                else
                    ProcessRefreshEvent(e.SignalTime);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void _TimerChangeToNextItem_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ProcessChangeToNextItemEvent(e.SignalTime);
                    });
                else
                    ProcessChangeToNextItemEvent(e.SignalTime);
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }



        private void PopulateList()
        {            
            foreach (DisplaySource src in DisplayConfiguration.Sources)
            {
                ListBoxItem li = new ListBoxItem();
                li.Content = " " + src.Name;
                li.FontSize = 28.0;
                li.FontWeight = FontWeights.Bold;
                li.Tag = src;

                lstItems.Items.Add(li);
            }


        }


        private void ProcessDateTimerEvent(DateTime timerEventTime)
        {
            DateTime dtNOW = DateTime.Now;
            DateTime dtUTC = dtNOW.ToUniversalTime();
            lblDateTime.Content = String.Format("{0} {1} | {2:0#}{3:0#} | {4:0#}{5:0#} Z",
                dtNOW.ToLongDateString(),
                dtNOW.ToShortTimeString(),
                dtNOW.Hour,
                dtNOW.Minute,
                dtUTC.Hour,
                dtUTC.Minute
                );
        }



        private void ProcessStartupTimerEvent(DateTime timerEventTime)
        {
            _TimerStartup.Stop();
            
            if (DisplayConfiguration.DisplayTypeID < 4 && DisplayConfiguration.Sources.Count > 0) //Anything but external application
            {
                Initialize(null);                
            }
            else
            {
                if (ExternalProcess != null && ExternalProcess.HasExited == false)
                    ResizeProcess(ExternalProcess);
            }

        }


        private void ProcessChangeToNextItemEvent(DateTime timerEventTime)
        {
            SetTimers();
        }


        private void ProcessRefreshEvent(DateTime timerEventTime)
        {
            //wb.Refresh();
        }


        private void Initialize(ListBoxItem item)
        {

            if (item == null)
                _CurrentItem = lstItems.Items[0] as ListBoxItem;
            else
                _CurrentItem = item;

            _CurrentSource = _CurrentItem.Tag as DisplaySource;

            _TimerChangeToNextItem.Interval = _CurrentSource.OnPlayDuration*60*1000;
            _TimerRefresh.Interval = _CurrentSource.RefreshInterval * 1000;

            lstItems.SelectedItem = _CurrentItem;
            lstItems.ScrollIntoView(_CurrentItem);

            stackBrowser.SetValue(StackPanel.VisibilityProperty, Visibility.Visible);

            if (_CurrentSource is URLSource)
            {
                LoadPage(((URLSource)_CurrentSource).URL);

                _TimerChangeToNextItem.Start();
                _TimerRefresh.Start();
            }

        }

        private void lstItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Stop();
            if (lstItems.SelectedItem != null)
                Initialize(lstItems.SelectedItem as ListBoxItem);
            else
                Initialize(null);
            
        }


        /// <summary>
        /// Stage and setup the appropriate display sequence timers
        /// </summary>
        private void SetTimers()
        {

            if (_TimerChangeToNextItem.Enabled == true)
                _TimerChangeToNextItem.Stop();

            if (_TimerRefresh.Enabled == true)
                _TimerRefresh.Stop();


            if (lstItems.Items.Count <= (lstItems.SelectedIndex+1))
                _CurrentItem = lstItems.Items[0] as ListBoxItem;
            else
                _CurrentItem = lstItems.Items[lstItems.SelectedIndex + 1] as ListBoxItem;


            _CurrentSource = _CurrentItem.Tag as DisplaySource;

            lstItems.SelectedItem = _CurrentItem;
            lstItems.ScrollIntoView(_CurrentItem);

            if (_CurrentSource is URLSource)
                LoadPage(((URLSource)_CurrentSource).URL);

            _TimerChangeToNextItem.Interval = _CurrentSource.OnPlayDuration * 60 * 1000;
            _TimerRefresh.Interval = _CurrentSource.RefreshInterval * 1000;

            if (_TimerChangeToNextItem.Enabled == false)
                _TimerChangeToNextItem.Start();

            if (_TimerRefresh.Enabled == false)
                _TimerRefresh.Start();

        }

        private void LoadPage(String URL)
        {

            wb.Height = 0.0;
            wb.Width = 0.0;
            wb.Navigate(URL);
            
        }

        /// <summary>
        /// Stops the situational display session
        /// </summary>
        public void Stop()
        {
            try
            {

                

                if (_TimerChangeToNextItem.Enabled)
                {
                    _TimerChangeToNextItem.Stop();
                }

                if (_TimerRefresh.Enabled)
                {
                    _TimerRefresh.Stop();
                }

                if (_TimerStartup.Enabled)
                {
                    _TimerStartup.Stop();
                }

                if (_TimerForDateTime.Enabled)
                {
                    _TimerForDateTime.Stop();
                }

                if (ExternalProcess != null && ExternalProcess.HasExited == false)
                    ExternalProcess.Kill();
            }
            catch (Exception ex)
            {
                ex.ToString();
            }            

        }


        private void wb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            //Create instance of HTMLDocument
            mshtml.IHTMLDocument2 doc = wb.Document as mshtml.IHTMLDocument2;

            double zoomRatio = 1.0;

            if  (_CurrentSource != null)
            {
                zoomRatio = _CurrentSource.Zoom;
                wb.Width = _CurrentSource.Width * _CurrentSource.Zoom;
                wb.Height = _CurrentSource.Height * _CurrentSource.Zoom;

                try
                {
                    double contentWidth = (wb.Document as dynamic).body.scrollWidth;              
                if (wb.Width + 25 < contentWidth)
                        zoomRatio = wb.Width / contentWidth;
                }
                catch(Exception ex)
                {
                    
                }
            }

            if (wb.Height > gridBrowser.ActualHeight)
                wb.Height = gridBrowser.ActualHeight - 20;

            if (wb.Width > gridBrowser.ActualWidth)
                wb.Width = gridBrowser.ActualWidth - 320;

            doc.parentWindow.execScript("document.body.style.zoom=" + zoomRatio.ToString().Replace(",", ".") + ";");
            doc.parentWindow.execScript("document.body.style.border=\"0\";");
            doc.parentWindow.execScript("document.body.style.margin=\"0\";");
            doc.parentWindow.execScript("document.body.style.padding=\"0\";");
            doc.parentWindow.execScript("document.body.style.overflow=\"hidden\";");

            
            //if (false)
            //{
                //Size small content to fit big screen otherwise display content within limits of browser window
            //    double contentHeight = (wb.Document as dynamic).body.scrollHeight;
            //    double contentWidth = (wb.Document as dynamic).body.scrollWidth;
            //    double ratioHeight = contentHeight / wb.Height;
            //    double ratioWidth = contentWidth / wb.Width;
            //    double ratio = 1.0;

            //    //if (contentHeight < wb.RenderSize.Height && contentWidth < wb.RenderSize.Width)
            //    //{
            //    //    //Resize small to large for both dimensions
            //    //    double avgContent = (contentHeight + contentWidth) / 2.0;
            //    //    double avgControl = (wb.RenderSize.Height + wb.RenderSize.Width) / 2.0;
            //    //    ratio = (1.0 -(avgContent / avgControl)) + 1.0;

            //    //}
            //    if (contentHeight < wb.Height)
            //    {
            //        //resize small to large only for height
            //        //ratio =  (1.0 - (contentHeight / wb.RenderSize.Height)) + 1.0;
            //        ratio = wb.Height / contentHeight;
            //    }
            //    else if (contentWidth < (wb.Width - 50))
            //    {
            //        //resize small to large only for width
            //        //ratio = (1.0 - (contentWidth / wb.RenderSize.Width)) + 1.0;
            //        ratio = wb.Width / contentWidth;
            //    }
            //    else if (ratioHeight > 1.0 && ratioHeight < 1.30)
            //    {
            //        //Zoom out to show more of the content only if the content is within 30% tolerance
            //        ratio = 2 - ratioHeight;
            //    }
            //    else if (ratioWidth > 1.0 && ratioWidth < 1.30)
            //    {
            //        //Zoom out to show more of the content only if the content is within 30% tolerance
            //        ratio = 2 - ratioWidth;
            //    }





               

            //    //Resize the control 

            //    //wb.Width = (wb.Document as dynamic).body.clientWidth;         

            //    if ((contentHeight * ratioHeight) < gridBrowser.ActualHeight)
            //        stackBrowser.Height = (contentHeight * ratioHeight);
            //    else
            //        stackBrowser.Height = gridBrowser.ActualHeight - 20;

            //    if ((contentWidth * ratioWidth) < gridBrowser.ActualWidth)
            //        stackBrowser.Width = (contentWidth * ratioWidth);
            //    else
            //        stackBrowser.Width = gridBrowser.ActualWidth - 20;
            //}
                        
        }

        private void wb_Navigated(object sender, NavigationEventArgs e)
        {

            if (!isWBSilenced)
            {
                FieldInfo fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
                if (fiComWebBrowser == null) return;
                object objComWebBrowser = fiComWebBrowser.GetValue(wb);
                if (objComWebBrowser == null) return;
                objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { true });
                Marshal.ReleaseComObject(objComWebBrowser);
                fiComWebBrowser = null;

                

                isWBSilenced = true;
            }
        
        }
    }
}
