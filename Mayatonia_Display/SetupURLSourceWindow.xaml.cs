using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MayatoniaDisplay
{

   
    /// <summary>
    /// Interaction logic for SetupURLSourceWindow.xaml
    /// </summary>
    public partial class SetupURLSourceWindow : SourceWindow
    {

        public SetupURLSourceWindow()
        {
            InitializeComponent();
        }

        public SetupURLSourceWindow(DisplaySource source): this()
        {            
            base.OriginalSource = source;

            LoadConfiguration();
        }


        //Load Source settings to UI
        private void LoadConfiguration()
        {
            URLSource url = base.OriginalSource as URLSource;

            txtName.Text = base.OriginalSource.Name;

            sliderPlayDuration.Value = (double)base.OriginalSource.OnPlayDuration;
            sliderRefreshInterval.Value = (double)base.OriginalSource.RefreshInterval;

            if (url != null)
                txtSource.Text = url.URL;

            if (url.DimensionsAutomatic)
                rbDimensionsAuto.IsChecked = true;
            else
                rbDimensionsCustom.IsChecked = true;

            txtHeightCustom.Text = url.Height.ToString();
            txtWidthCustom.Text = url.Width.ToString();

            sliderZoom.Value = url.Zoom;


        }

        /// <summary>
        /// Persist configuration changes to the source object
        /// </summary>
        private void SaveConfiguration()
        {
            base.OriginalSource.Name = txtName.Text.Trim();
            base.OriginalSource.OnPlayDuration = (int)sliderPlayDuration.Value;
            base.OriginalSource.RefreshInterval = (int)sliderRefreshInterval.Value;
            base.OriginalSource.DimensionsAutomatic = rbDimensionsAuto.IsChecked.Value;

            if (rbDimensionsCustom.IsChecked.Value)
            {
                base.OriginalSource.Height = int.Parse(txtHeightCustom.Text.Trim());
                base.OriginalSource.Width = int.Parse(txtWidthCustom.Text.Trim());
            }

            base.OriginalSource.Zoom = sliderZoom.Value;

            URLSource url = base.OriginalSource as URLSource;
            if (url != null)
                url.URL = txtSource.Text.Trim();


        }
            

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            base.Close();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (txtSource.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please specify a source...", "Error");
                return;
            }

            SaveConfiguration();
            base.SavedSource = base.OriginalSource;

            base.Close();
        }


        private void sliderRefreshInterval_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = (int)sliderRefreshInterval.Value;
            int minutes = seconds / 60;

            if (minutes >= 1)
                seconds = seconds - (minutes * 60);

            if (lblRefreshInterval != null)
                lblRefreshInterval.Content = String.Format("{0} Min(s) {1} Sec(s)", minutes, seconds);

        }

        private void sliderPlayDuration_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblPlayDuration != null)
                lblPlayDuration.Content = String.Format("{0} Minutes", (int)sliderPlayDuration.Value);
        }

        private void rbDimensionsAuto_Checked(object sender, RoutedEventArgs e)
        {
            if (txtWidthCustom != null)
            {
                txtWidthCustom.IsEnabled = rbDimensionsCustom.IsChecked.Value;
                txtHeightCustom.IsEnabled = rbDimensionsCustom.IsChecked.Value;
            }
        }
        private void rbDimensionsCustom_Checked(object sender, RoutedEventArgs e)
        {
            if (txtWidthCustom != null)
            {
                txtWidthCustom.IsEnabled = rbDimensionsCustom.IsChecked.Value;
                txtHeightCustom.IsEnabled = rbDimensionsCustom.IsChecked.Value;
            }
        }


        private void sliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (lblZoom != null)
                lblZoom.Content = String.Format("{0:0#}% Zoom", (int)(sliderZoom.Value*100));
        }

        private void txtWidthCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtWidthCustom.Text = RegexRemoveText(@"\D", txtWidthCustom.Text.Trim());
        }

        private void txtHeightCustom_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtHeightCustom.Text = RegexRemoveText(@"\D", txtHeightCustom.Text.Trim());
        }


        private String RegexRemoveText(String Expression, string InputString)
        {
            String strCleaned = InputString;
            Regex reg = new Regex(Expression);

            foreach (Match match in reg.Matches(InputString))
            {
                strCleaned.Replace(match.Value, "");
            }

            return strCleaned;
        }

    }
}
