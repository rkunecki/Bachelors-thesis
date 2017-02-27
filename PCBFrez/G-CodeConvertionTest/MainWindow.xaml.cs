using System;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using PCBFrez;

namespace G_CodeConvertionTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string recivedTextToConvert = string.Empty;
        string recivedConvertedText = string.Empty;
        int lineCounter = 0;
        Thread oThread;
        GCodeConversion codeConversion = new GCodeConversion();

        private void BTN_Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "G-Code NCD files(*.ncd)|*.ncd|Text files(*.txt)|*.txt|All files(*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                    TB_GCodeView.Text = File.ReadAllText(openFileDialog.FileName);

                TBlock_NumberOfLines.Text = TB_GCodeView.LineCount.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku!\nTekst błędu:\n"+ex.Message, "Open File Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                codeConversion.lastPosition.xPosition = 0;
                codeConversion.lastPosition.yPosition = 0;
                codeConversion.lastPosition.zPosition = 0;

                oThread = new Thread(new ThreadStart(ConvertGCode));

                PB_StatusBar.Value = 0;
                lineCounter = 0;
                TB_OperationView.Clear();
                recivedTextToConvert = string.Empty;
                recivedConvertedText = string.Empty;
                TBlock_StatusText.Text = "Przetwarzanie";

                oThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas tworzenia wątku!\nTekst błędu:\n" + ex.Message, "Creation Thread Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BTN_Stop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (oThread.IsAlive)
                {
                    oThread.Abort();
                    Application.Current.Dispatcher.Invoke(() => TB_OperationView.Text += "Przerwano\n");
                    Application.Current.Dispatcher.Invoke(() => TBlock_StatusText.Text = "Przerwano");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas zatrzymania wątku!\nTekst błędu:\n" + ex.Message, "Stop Thread Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if (oThread.IsAlive)
                {
                    oThread.Abort();
                }
            }
            catch (ThreadAbortException ex)
            {
                MessageBox.Show("Błąd przerwania wątku!", "Thread Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (System.Security.SecurityException ex)
            {
                MessageBox.Show("Wystąpił nieznany bład!", "Thread Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieznany bład!", "Thread Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ConvertGCode()
        {
            while(lineCounter < TB_GCodeView.LineCount)
            {                
                recivedTextToConvert = Application.Current.Dispatcher.Invoke(() => TB_GCodeView.GetLineText(lineCounter));
                recivedConvertedText = codeConversion.GetConvertedSteps(recivedTextToConvert, 25, "E\\");
                
                Application.Current.Dispatcher.Invoke(() => TB_OperationView.Text += recivedConvertedText+"\n");
                Application.Current.Dispatcher.Invoke(() => PB_StatusBar.Value = ((lineCounter+1)*100)/TB_GCodeView.LineCount);

                lineCounter++;
                Application.Current.Dispatcher.Invoke(() => TBlock_ConvertedLines.Text = lineCounter.ToString());

                if (lineCounter == TB_GCodeView.LineCount)
                {
                    Application.Current.Dispatcher.Invoke(() => TB_OperationView.Text += "Zakończono\n");
                    Application.Current.Dispatcher.Invoke(() => TBlock_StatusText.Text = "Zakończono");
                }
            } 
        }                
    }
}
