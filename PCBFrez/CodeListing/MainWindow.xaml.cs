using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;

namespace CodeListing
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

        Thread oThread;

        private void btnProcessButton_Click(object sender, RoutedEventArgs e)
        {
                oThread = new Thread(new ThreadStart(ProcessingCodeToListing));
                oThread.Start();
        }

        private void ProcessingCodeToListing()
        {
            Application.Current.Dispatcher.Invoke(() => pbStatus.Value = 0);

            string lineNumber = string.Empty;

            try
            {
                Application.Current.Dispatcher.Invoke(() => tboxListingView.Clear() );

                for (int i = 0; i < tboxCodeView.LineCount; i++)
                {
                    lineNumber = "Line " + (i + 1).ToString();
                    
                    Application.Current.Dispatcher.Invoke(() => tboxListingView.Text += lineNumber + AddSpaceBeforeText(i) + tboxCodeView.GetLineText(i));

                    Application.Current.Dispatcher.Invoke(() => pbStatus.Value = ((i + 1)*100)/tboxCodeView.LineCount);
                }
            }
            catch
            {
                MessageBox.Show("Błąd przetwarzania!", "Process Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string AddSpaceBeforeText(int actualLineNumber)
        {
            if ((actualLineNumber + 1) < 10)
            {
                return "         ";
            }
            else if ((actualLineNumber + 1) < 100)
            {
                return "       ";
            }
            else if ((actualLineNumber + 1) < 1000)
            {
                return "     ";
            }
            else
            {
                return "    ";
            }
        }

        private void btnSaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            try
            {
                saveFileDialog.Filter = "Text files(*.txt)|*.txt";
                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, tboxListingView.Text);
            }
            catch (Exception ex) 
            {
                MessageBox.Show("Błąd zapisu!", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AbortAliveThread(this.oThread);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            AbortAliveThread(this.oThread);
        }
        
        private void AbortAliveThread (Thread thread)
        {
            try
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
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
    }
}
