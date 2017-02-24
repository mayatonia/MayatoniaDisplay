using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Timers;
using System.Threading;

namespace MayatoniaDisplay
{
    /// <summary>
    /// Interaction logic for NetworkStatus.xaml
    /// </summary>
    public partial class NetworkStatus : UserControl
    {

        private System.Timers.Timer _TimerConnectivity = new System.Timers.Timer(10000); // 10 seconds

        private String _Timer = String.Empty;
        private Connection _Connection = null;
        private DateTime _LastConnectionDateTime = DateTime.Now;

        private Brush _COLOR_NOT_CONNECTED = Brushes.Red;
        private Brush _COLOR_CONNECTING = Brushes.Transparent;
        private Brush _COLOR_CONNECTED = Brushes.LightGreen;


        public NetworkStatus()
        {
            InitializeComponent();
        }

        public NetworkStatus(Connection connection) : this()
        {
            _Connection = connection;

            lblDisplayName.Content = _Connection.Title;

            ChangeBackgroundColor(_COLOR_NOT_CONNECTED);

            _TimerConnectivity.Elapsed += _TimerConnectivity_Elapsed;
            _TimerConnectivity.Start();
        }

        private void _TimerConnectivity_Elapsed(object sender, ElapsedEventArgs e)
        {
            ChangeBackgroundColor(_COLOR_CONNECTING);
            bool Connected = Common.TestTCPConnection(_Connection.Host, _Connection.Port, _Connection.Timeout);
            ChangeBackgroundColor(Connected ? _COLOR_CONNECTED : _COLOR_NOT_CONNECTED);
        }

        public void ChangeBackgroundColor(Brush newColor)
        {
            try
            {
                if (this.Dispatcher.Thread.ManagedThreadId != Thread.CurrentThread.ManagedThreadId)
                    this.Dispatcher.Invoke(() =>
                    {
                        ChangeBackgroundColor(newColor);
                    });
                else
                {
                    borderStatus.Background = newColor;
                    if (newColor == _COLOR_CONNECTED)
                    {
                        _LastConnectionDateTime = DateTime.Now;
                        this.ToolTip = String.Format("Last connected to {0} at {1} {2}", 
                            _Connection.Host,
                            _LastConnectionDateTime.ToShortDateString(), 
                            _LastConnectionDateTime.ToShortTimeString());
                    } 
                    else if(newColor == _COLOR_CONNECTING)
                    {
                        this.ToolTip = String.Format("Attempting connect to {0}.... Last successful connection was at {1} {2}",
                            _Connection.Host,
                            _LastConnectionDateTime.ToShortDateString(), 
                            _LastConnectionDateTime.ToShortTimeString());
                    }
                    else if(newColor == _COLOR_NOT_CONNECTED)
                    {
                        this.ToolTip = String.Format("Not connected to {0}... Last successful connect was at {1} {2}",
                            _Connection.Host,
                            _LastConnectionDateTime.ToShortDateString(), 
                            _LastConnectionDateTime.ToShortTimeString());
                    }

                    //lblDisplayName.Content = String.Format("{0}\n\r{1} {2}",
                    //    _Connection.Title,
                    //    _LastConnectionDateTime.ToShortDateString(),
                    //    _LastConnectionDateTime.ToShortTimeString());
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public void Cleanup()
        {
            if (_TimerConnectivity != null)
            {
                _TimerConnectivity.Elapsed -= _TimerConnectivity_Elapsed;
                _TimerConnectivity.Stop();
                _TimerConnectivity.Dispose();
                _TimerConnectivity = null;
            }

        }        
    }
}
