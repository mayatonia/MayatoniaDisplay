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
using System.Windows.Shapes;

namespace MayatoniaDisplay
{
    /// <summary>
    /// Interaction logic for SetupWindow.xaml
    /// </summary>
    public partial class SetupWindow : Window
    {
        DisplayConfiguration displayConfiguration;

        
        public SetupWindow()
        {
            InitializeComponent();
        }

        public SetupWindow(DisplayConfiguration configuration): this()
        {

            displayConfiguration = configuration;

            //Set Title
            lblTitle.Content += (displayConfiguration.DisplayIndex + 1).ToString();

            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
           
            //set selection
            switch(displayConfiguration.DisplayTypeID)
            {
                case 1:
                    rbDisplaySingle.IsChecked = true;
                    break;

                case 2:
                    rbDisplayFour.IsChecked = true;
                    break;

                case 3:
                    rbDisplayHybrid.IsChecked = true;
                    break;

                case 4:
                    rbDisplayExternal.IsChecked = true;
                    break;
            }

            //Setup Sources
            lstContentSources.Items.Clear();

            rbEnabled.IsChecked = displayConfiguration.DisplayEnabled;
            rbDisabled.IsChecked = !rbEnabled.IsChecked.Value;

            rbEnabledHeader.IsChecked = displayConfiguration.DisplayHeader;
            rbDisabledHeader.IsChecked = !rbEnabledHeader.IsChecked.Value;

            foreach (DisplaySource ds in displayConfiguration.Sources)
                lstContentSources.Items.Add(ds);
        }

        private void SaveConfiguration()
        {
            if (rbDisplaySingle.IsChecked.Value)
                displayConfiguration.DisplayTypeID = 1;
            else if (rbDisplayFour.IsChecked.Value)
                displayConfiguration.DisplayTypeID = 2;
            else if (rbDisplayHybrid.IsChecked.Value)
                displayConfiguration.DisplayTypeID = 3;
            else if (rbDisplayExternal.IsChecked.Value)
                displayConfiguration.DisplayTypeID = 4;

            displayConfiguration.Sources.Clear();

            displayConfiguration.DisplayEnabled = rbEnabled.IsChecked.Value;
            displayConfiguration.DisplayHeader = rbEnabledHeader.IsChecked.Value;

            foreach (DisplaySource src in lstContentSources.Items)
                displayConfiguration.Sources.Add(src);
        }
            

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (lstContentSources.Items.Count == 0)
            {
                MessageBox.Show("Sorry, you must specify 1 or more display sources or click the Cancel button to exit the window.", "Error");
                return;
            }


            if (rbDisplayExternal.IsChecked.Value)                
            {
                if (lstContentSources.Items.Count > 1)
                {
                    MessageBox.Show("Sorry, an external application may only contain 1 App display source", "Error");
                    return;
                }
                else if (lstContentSources.Items[0].GetType() != typeof(AppSource))
                {

                    MessageBox.Show("Sorry, you cannot mix a URL Source with an External Display type. Choose Single, Quad or Hybrid Display type instead.", "Error");
                    return;
                }
            }

            if (rbDisplayExternal.IsChecked.Value == false)
            {
                bool foundAppSource = false;
                foreach (DisplaySource src in lstContentSources.Items)
                {
                    if (src.GetType() == typeof(AppSource))
                    {
                        foundAppSource = true;
                        break;
                    }
                }

                if (foundAppSource)
                {
                    MessageBox.Show("Sorry, you cannot include an App Source with an URL Based Display Type. Choose the External Display type instead.", "Error");
                    return;
                }


            }


            

            

            SaveConfiguration();
            
            this.Close();
        }

        private void lstContentSources_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEdit.IsEnabled = lstContentSources.SelectedItems.Count == 1;
            btnMoveUp.IsEnabled = btnMoveDown.IsEnabled = btnEdit.IsEnabled;

        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

            if (lstContentSources.SelectedIndex >= 0)
            {
                DisplaySource src = lstContentSources.Items[lstContentSources.SelectedIndex] as DisplaySource;
                lstContentSources.Items.RemoveAt(lstContentSources.SelectedIndex);
            }
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (rbDisplayExternal.IsChecked.Value && lstContentSources.Items.Count > 0)
            {
                MessageBox.Show("Sorry, an external application may only contain 1 display source", "Error");
                return;
            }

            DisplaySource src = null;
            SourceWindow window = null;

            if (rbDisplayExternal.IsChecked.Value == false) //URL Based Displays
            {
                src = new URLSource();
                window = new SetupURLSourceWindow(src);
            }
            else if (rbDisplayExternal.IsChecked.Value == true) //External App                    
            {
                src = new AppSource();
                window = new SetupAPPSourceWindow(src);
            }

            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.ShowDialog();

            if (window.SavedSource != null)
            { 
                lstContentSources.Items.Add(window.SavedSource);                
                lstContentSources.SelectedItem = window.SavedSource;
            }


        }



        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lstContentSources.SelectedItem != null)
            {

                SourceWindow window = null;
                DisplaySource src = lstContentSources.SelectedItem as DisplaySource;
                if (rbDisplayExternal.IsChecked.Value == false) //URL Based Displays
                    window = new SetupURLSourceWindow(src);
                else if (rbDisplayExternal.IsChecked.Value == true) //External App                    
                    window = new SetupAPPSourceWindow(src);

                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.ShowDialog();

                if (window.SavedSource != null)
                {
                    lstContentSources.SelectedItem = src;
                    lstContentSources.Items.Refresh();
                }
            }
        }

        private void rbDisplaySingle_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                rbDisplayFour.IsChecked = false;
                rbDisplayHybrid.IsChecked = false;
                rbDisplayExternal.IsChecked = false;
            }
            catch(Exception ex)
            {
                ex.ToString();
            }
        }

        private void rbDisplayFour_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                rbDisplaySingle.IsChecked = false;
                rbDisplayHybrid.IsChecked = false;
                rbDisplayExternal.IsChecked = false;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void rbDisplayHybrid_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                rbDisplaySingle.IsChecked = false;
                rbDisplayFour.IsChecked = false;
                rbDisplayExternal.IsChecked = false;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void rbDisplayExternal_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                                 
                rbDisplaySingle.IsChecked = false;
                rbDisplayFour.IsChecked = false;
                rbDisplayHybrid.IsChecked = false;
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

        }

        private void imgDisplaySingle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rbDisplaySingle.IsChecked = true;
        }

        private void imgDisplayFour_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rbDisplayFour.IsChecked = true;
        }

        private void imgDisplayHybrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rbDisplayHybrid.IsChecked = true;
        }

        private void imgDisplayExternal_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rbDisplayExternal.IsChecked = true;
        }

        private void lstContentSources_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.Source is ListBox)
                btnEdit_Click(null, null);

        }

        private void rbEnabled_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void rbDisabled_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (lstContentSources.SelectedItem != null)
            {
                try
                {
                    DisplaySource src = lstContentSources.SelectedItem as DisplaySource;

                    int currIndex = lstContentSources.Items.IndexOf(src);
                    int newIndex = currIndex - 1;
                    if (newIndex >= 0)
                    {
                        lstContentSources.Items.RemoveAt(currIndex);
                        lstContentSources.Items.Insert(newIndex, src);
                        lstContentSources.SelectedItem = src;
                    }
                }
                catch { }
            }
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (lstContentSources.SelectedItem != null)
            {
                try
                {
                    DisplaySource src = lstContentSources.SelectedItem as DisplaySource;

                    int currIndex = lstContentSources.Items.IndexOf(src);
                    int newIndex = currIndex+1;

                    if (newIndex < lstContentSources.Items.Count)
                    {
                        lstContentSources.Items.RemoveAt(currIndex);
                        lstContentSources.Items.Insert(newIndex, src);
                        lstContentSources.SelectedItem = src;
                    }
                }
                catch { }
            }
        }
    }
}
