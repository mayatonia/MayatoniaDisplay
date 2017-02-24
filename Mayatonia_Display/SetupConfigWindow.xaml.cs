using Microsoft.Win32;
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
    /// Interaction logic for SetupConfiogWindow.xaml
    /// </summary>
    public partial class SetupConfigWindow : SourceWindow
    {
       
        public SetupConfigWindow()
        {
            InitializeComponent();
        }

        public SetupConfigWindow(Settings source): this()
        {            
            base.OriginalSettings = source;

            LoadConfiguration();
        }


        //Load Source settings to UI
        private void LoadConfiguration()
        {
            Settings app = base.OriginalSettings as Settings;

            txtBanner.Text = app.Banner_Text;
            txtAbbreviation.Text = app.Office_Abbreviation;
            txtStartSeconds.Text = app.AutoStartSeconds.ToString();
            txtConnections.Text = app.ConnectionsPointsString;
        }

        /// <summary>
        /// Persist configuration changes to the source object
        /// </summary>
        private void SaveConfiguration()
        {

            Settings app = base.OriginalSettings as Settings;
            if (app != null)
            {
                app.Banner_Text = txtBanner.Text.Trim();
                app.Office_Abbreviation = txtAbbreviation.Text.Trim();
                int nAutoStart = 0;
                try
                {
                    nAutoStart = Convert.ToInt32(txtStartSeconds.Text.Trim());
                }
                catch { } //Any failure to parse the string will result in 0 // I.e No Auto Start

                app.AutoStartSeconds = nAutoStart;
                app.ConnectionsPointsString = txtConnections.Text.Trim();
            }



        }
            

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            base.Close();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            SaveConfiguration();
            base.SavedSettings = base.OriginalSettings;

            base.Close();
        }


        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {


            //OpenFileDialog ofd = new OpenFileDialog();            
            //ofd.CheckFileExists = true;
            //ofd.CheckPathExists = true;
            //ofd.Multiselect = false;
            //ofd.Title = "Select an external Application or Program";

            //ofd.ShowDialog();
            //if (ofd.FileName.Trim().Length > 0)
            //    txtPath.Text = ofd.FileName.ToString().Trim();
            
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
