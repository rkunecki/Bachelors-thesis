using System;
using System.Windows;

namespace PCBFrez
{
    /// <summary>
    /// Interaction logic for SettingsMillingMachine.xaml
    /// </summary>
    public partial class SettingsMillingMachine : Window
    {
        MainWindow mainWindow;
        public SettingsMillingMachine(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            try
            {
                TB_steps.Text = mainWindow.steps.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bład odczytu ustawień! " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void buttonOK_SettingsPort_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TB_steps == null)
                    MessageBox.Show("Wybierz wszystkie pola!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    mainWindow.steps = Convert.ToInt32(TB_steps.Text);

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bład zapisu ustawień! " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void buttonCancel_SettingsPort_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}