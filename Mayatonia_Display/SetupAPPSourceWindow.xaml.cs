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
    /// Interaction logic for SetupAPPSourceWindow.xaml
    /// </summary>
    public partial class SetupAPPSourceWindow : SourceWindow
    {
       
        public SetupAPPSourceWindow()
        {
            InitializeComponent();
        }

        public SetupAPPSourceWindow(DisplaySource source): this()
        {            
            base.OriginalSource = source;

            LoadConfiguration();
        }


        //Load Source settings to UI
        private void LoadConfiguration()
        {
            AppSource app = base.OriginalSource as AppSource;

            txtName.Text = base.OriginalSource.Name;

            txtName.Name = app.Name;
            txtPath.Text = app.Path;
            txtParameters.Text = app.Parameters;
        }

        /// <summary>
        /// Persist configuration changes to the source object
        /// </summary>
        private void SaveConfiguration()
        {
            base.OriginalSource.Name = txtName.Text.Trim();


            AppSource app = base.OriginalSource as AppSource;
            if (app != null)
            {
                app.Name = txtName.Name.Trim();
                app.Path = txtPath.Text.Trim();
                app.Parameters = txtParameters.Text.Trim();
            }



        }
            

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            base.Close();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            if (txtPath.Text.Trim().Length == 0)
            {
                MessageBox.Show("Please specify a source...", "Error");
                return;
            }

            SaveConfiguration();
            base.SavedSource = base.OriginalSource;

            base.Close();
        }


        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {


            OpenFileDialog ofd = new OpenFileDialog();            
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.Title = "Select an external Application or Program";

            ofd.ShowDialog();
            if (ofd.FileName.Trim().Length > 0)
                txtPath.Text = ofd.FileName.ToString().Trim();
            
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
