using System;
using System.Windows;
using System.IO.Ports;

namespace PCBFrez
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
       MainWindow mainWindow;

       public Window1(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            try
            {
                string[] availablePorts = SerialPort.GetPortNames();

                cbBaudrate.Text = mainWindow.baudrate.ToString();
                cbDataBits.Text = mainWindow.dataBits.ToString();
                cbStopBits.Text = mainWindow.stopBits.ToString();

                if (mainWindow.parityBits == 0)
                    cbParityBits.Text = "Brak";
                if (mainWindow.parityBits == 1)
                    cbParityBits.Text = "Suma parzysta";
                if (mainWindow.parityBits == 2)
                    cbParityBits.Text = "Suma nieparzysta";

                foreach (string portNumber in availablePorts)
                {
                   cbPort.Items.Add(portNumber);
                }
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
                if (cbBaudrate.SelectedIndex == -1 || cbDataBits.SelectedIndex == -1 || cbStopBits.SelectedIndex == -1 || cbParityBits.SelectedIndex == -1 || cbPort.SelectedIndex == -1)
                    MessageBox.Show("Wybierz wszystkie pola!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    string parity = cbParityBits.SelectionBoxItem.ToString();

                    mainWindow.portName = cbPort.Text;
                    mainWindow.baudrate = Convert.ToInt32(cbBaudrate.Text);
                    mainWindow.dataBits = Convert.ToInt16(cbDataBits.Text);
                    mainWindow.stopBits = Convert.ToInt16(cbStopBits.Text);

                    if (parity == "Brak")
                        mainWindow.parityBits = 0;
                    if (parity == "Suma parzysta")
                        mainWindow.parityBits = 1;
                    if (parity == "Suma nieparzysta")
                        mainWindow.parityBits = 2;

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