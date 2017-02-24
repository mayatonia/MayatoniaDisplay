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

namespace MayatoniaDisplay
{
    /// <summary>
    /// Interaction logic for Display.xaml
    /// </summary>
    public partial class Display : UserControl
    {

        private Brush _DISABLED = Brushes.Red;
        private Brush _ENABLED = Brushes.LightGreen;

        private DisplayConfiguration _DisplayConfiguration;

        public DisplayConfiguration DisplayConfiguration
        {
            get
            {
                return _DisplayConfiguration;
            }

            set
            {
                _DisplayConfiguration = value;
            }
        }

        public Display()
        {
            InitializeComponent();
        }

        public Display(String Title, DisplayConfiguration displayConfig) : this()
        {
            InitializeDisplay(Title, displayConfig);

        }

        private void InitializeDisplay(string Title, DisplayConfiguration displayConfig)
        {
            this.lblDisplayName.Content = Title;
            _DisplayConfiguration = displayConfig;

            //cbConfiguration.SelectedIndex = _DisplayConfiguration.DisplayTypeID - 1;
            SetDisplayTypeLabel(DisplayConfiguration.DisplayTypeID);
            SetEnabledDisabledStatus(DisplayConfiguration.DisplayEnabled);
        }

        /// <summary>
        /// Sets the Status Indicator for Enabled / Disabled Status
        /// </summary>
        /// <param name="isEnabled"></param>
        private void SetEnabledDisabledStatus(bool isEnabled)
        {
            if (isEnabled == true)
                borderStatus.Background = _ENABLED;
            else
                borderStatus.Background = _DISABLED;
        }

        //private void cbConfiguration_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (DisplayConfiguration != null)
        //    {
        //        _DisplayConfiguration.DisplayTypeID = cbConfiguration.SelectedIndex + 1;
        //    }
        //}

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            if (this.DisplayConfiguration != null)
            {
                SetupWindow window = new SetupWindow(this.DisplayConfiguration);
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();

                InitializeDisplay(this.lblDisplayName.Content.ToString(), this.DisplayConfiguration);


                //Save settings to disk
                Settings.Save(Settings.Instance);

            }
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Tag = this.Background;
            this.Background = new SolidColorBrush(Colors.Wheat);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = this.Tag as Brush;
        }


        private void SetDisplayTypeLabel(int DisplayTypeId)
        {
            switch(DisplayTypeId)
            {
                case 1:
                    lblDisplayType.Content = "Single Display";
                    break;
                case 2:
                    lblDisplayType.Content = "Quad Display";
                    break;
                case 3:
                    lblDisplayType.Content = "Hybrid Display";
                    break;
                case 4:
                    lblDisplayType.Content = "External Application";
                    break;
            }

        }

    }
}
