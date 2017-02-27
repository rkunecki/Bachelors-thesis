using System;
using System.Windows;
using System.Windows.Media;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.ComponentModel;
using System.IO.Ports;

namespace PCBFrez
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            logPatch = @"..\..\Log";
            int[] defaultValues = ConfigRead.ReadAndWriteInt(System.Environment.CurrentDirectory + @"\config", logPatch);

            baudrate = defaultValues[0];
            stopBits = defaultValues[1];
            parityBits = defaultValues[2];
            dataBits = defaultValues[3];
            steps  = defaultValues[4];
        }

        #region VARIABLES
        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);
        delegate void SetTextCallback(string text);

        SerialConnection serialConnect = new SerialConnection();
        GCodeConversion codeConversion = new GCodeConversion();
        public int baudrate;
        public int stopBits;
        public int parityBits;
        public int dataBits;
        public string portName = null;
        public string[] ports = null;

        public int steps;
        private string sendString = null;

        private bool connected = false;
        public string logPatch;

        public string textToSend = null;
        private int numberOfLines = 0;
        #endregion

        #region BUTTONS
        private void menuOpen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "G-Code File NCD(*.ncd)|*.ncd|Text files(*.txt)|*.txt|All files(*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    GCode_window.Text = File.ReadAllText(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania pliku!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuStart_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                if (connected)
                {
                    codeConversion.lastPosition.xPosition = 0;
                    codeConversion.lastPosition.yPosition = 0;
                    codeConversion.lastPosition.zPosition = 0;

                    numberOfLines = GCode_window.LineCount;
                if (m_oBackgroundWorker == null)
                {
                    m_oBackgroundWorker = new BackgroundWorker();
                    m_oBackgroundWorker.DoWork += new DoWorkEventHandler(m_oBackgroundWorker_DoWork);
                    m_oBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_oBackgroundWorker_RunWorkerCompleted);
                    m_oBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(m_oBackgroundWorker_ProgressChanged);

                    m_oBackgroundWorker.WorkerReportsProgress = true;
                    m_oBackgroundWorker.WorkerSupportsCancellation = true;
                } 
                    pbStatus.Value = 0;
                    m_oBackgroundWorker.RunWorkerAsync();
                }

                bEditGCodeWindow.IsEnabled = false;
                GCode_window.IsEnabled = false;
                bCloseEditGCodeWindow.IsEnabled = false;
                GCode_window.IsReadOnly = true;
                BlockButtons("N", "Y", "Y", "none", "none", "none", "none");
                statusWork.Background = Brushes.LightGreen;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas startu frezarki!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuPauza_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                if (connected)
                {

                }

                BlockButtons("Y", "none", "none", "none", "none", "none", "none");
                statusWork.Background = Brushes.Yellow;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd wstrzymania pracy frezarki!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuStop_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                if ((m_oBackgroundWorker != null) && m_oBackgroundWorker.IsBusy)
                {
                    m_oBackgroundWorker.CancelAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zatrzymania frezarki!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((m_oBackgroundWorker != null) && m_oBackgroundWorker.IsBusy)
                {
                    m_oBackgroundWorker.CancelAsync();
                }
                serialConnect.Disconnect(logPatch);
                Application.Current.Shutdown();
            }
            catch { }
        }
        #region MENU
        private void menuConnect_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                connected = serialConnect.Connect(logPatch, portName, baudrate, parityBits, dataBits, stopBits);
                if (connected)
                {
                    BlockButtons("Y", "none", "none", "N", "Y", "none", "none");
                    statusConnection.Background = Brushes.LightGreen;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd połączenia!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuDisconnect_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                connected = serialConnect.Disconnect(logPatch);
                if (!connected)
                {
                    BlockButtons("N", "none", "none", "Y", "N", "none", "none");
                    statusConnection.Background = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd rozłączenia!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuBased_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                BlockButtons("none", "none", "none", "none", "none", "N", "none");
                if (connected)
                {

                }
                BlockButtons("none", "none", "none", "none", "none", "Y", "none");
            }
            catch (Exception ex)
            {
                BlockButtons("none", "none", "none", "none", "none", "Y", "none");
                MessageBox.Show("Błąd bazowania!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuTest_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                BlockButtons("none", "none", "none", "none", "none", "none", "N");
                if (connected)
                {
                    serialConnect.SendStringData(logPatch, "Test połączenia, polskie znaki:śćźżąęłó, liczby:1234567890 LRUD lrud");
                    Thread.Sleep(500);
                }              
                BlockButtons("none", "none", "none", "none", "none", "none", "Y");
            }
            catch (Exception ex)
            {
                BlockButtons("none", "none", "none", "none", "none", "none", "Y");
                MessageBox.Show("Błąd Testu!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuSettingsPort_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                Window1 s = new Window1(this);
                s.Owner = this;
                s.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania ustawień!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuSettingsMillingMachine_Click(object sender, RoutedEventArgs a)
        {
            try
            {
                SettingsMillingMachine s = new SettingsMillingMachine(this);
                s.Owner = this;
                s.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania ustawień!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void menuAboutProgram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Program PCBFrez jest częścią pracy inżynierskiej wykonywanej przez:" + 
                    "\nRafał Kunecki\nArkadiusz Hallmann\n\nProgram służy do obsługi wykonanej frezarki" + 
                    "do płyt PCB. Działa on w oparciu G-Code produkowany przez zewnętrzny program." +
                    "Kod ten jest przetwarzany na " + "rozkazy dla sterowników silników krokowych, a" + 
                    "następnie wysyłany kolejno do sterowników.\n\nCopyright (C) Rafał Kunecki",
                    "O Programie", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas otwierania informacji o programie!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        #endregion
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                if ((m_oBackgroundWorker != null) && m_oBackgroundWorker.IsBusy)
                {
                    m_oBackgroundWorker.CancelAsync();
                }
                serialConnect.Disconnect(logPatch);
            }
            catch { }
        }
        #endregion

        private void bEditGCodeWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GCode_window.IsReadOnly = false;
                bCloseEditGCodeWindow.IsEnabled = true;
                bEditGCodeWindow.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd edycji!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }

        }
        private void bCloseEditGCodeWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GCode_window.IsReadOnly = true;
                bEditGCodeWindow.IsEnabled = true;
                bCloseEditGCodeWindow.IsEnabled = false;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "G-Code File NCD(*.ncd)|*.ncd|Text files(*.txt)|*.txt";

                if (saveFileDialog.ShowDialog() == true)
                    File.WriteAllText(saveFileDialog.FileName, GCode_window.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd zapisu!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
            }
        }
        private void BlockButtons(string start, string stop, string pauza, string connect, string disconnect, string based, string test)
        {
            if (String.Compare(start, "none") != 0)
            {
                if(String.Compare(start, "Y") == 0)
                {
                    menuStart.IsEnabled = true;
                    buttonStart.IsEnabled = true;
                }
                else
                {
                    menuStart.IsEnabled = false;
                    buttonStart.IsEnabled = false;
                }
                    
            }
            
            if (String.Compare(stop, "none") != 0)
            {
                if(String.Compare(stop, "Y") == 0)
                {
                    
                    menuStop.IsEnabled = true;
                    buttonStop.IsEnabled = true;
                }
                else
                {
                    
                    menuStop.IsEnabled = false;
                    buttonStop.IsEnabled = false;
                }
            }
            if (String.Compare(pauza, "none") != 0)
            {
                if(String.Compare(pauza, "Y") == 0)
                {
                    menuPauza.IsEnabled = true;
                    buttonPauza.IsEnabled = true;
                }
                else
                {
                    menuPauza.IsEnabled = false;
                    buttonPauza.IsEnabled = false;
                }
            }
            
            if (String.Compare(connect, "none") != 0)
            {
                if(String.Compare(connect, "Y") == 0)
                {
                    menuConnect.IsEnabled = true;
                    buttonConnect.IsEnabled = true;
                }
                else
                {
                    
                    menuConnect.IsEnabled = false;
                    buttonConnect.IsEnabled = false;
                }
            }
            if (String.Compare(disconnect, "none") != 0)
            {
                if(String.Compare(disconnect, "Y") == 0)
                {
                    menuDisconnect.IsEnabled = true;
                    buttonDisconnect.IsEnabled = true;
                }
                else
                {
                    menuDisconnect.IsEnabled = false;
                    buttonDisconnect.IsEnabled = false;
                }
            }
            
            if (String.Compare(based, "none") != 0)
            {
                if(String.Compare(based, "Y") == 0)
                {
                    menuBased.IsEnabled = true;
                    buttonBased.IsEnabled = true;
                }
                else
                {
                    menuBased.IsEnabled = false;
                    buttonBased.IsEnabled = false;
                }                    
            }
            
            if (String.Compare(test, "none") != 0)
            {
                if(String.Compare(test, "Y") == 0)
                {
                    menuTest.IsEnabled = true;
                    buttonTest.IsEnabled = true;
                }
                else
                {
                    menuTest.IsEnabled = false;
                    buttonTest.IsEnabled = false;
                }
            }
            
        }

        #region BACKGROUND WORK
        private BackgroundWorker m_oBackgroundWorker = null;
        public delegate void InvokeDelegate();
        int lineCounter = 0;
        string gCodeTextLine = string.Empty;
        private static object m_oPadlock = new object();
        private static string flag = "ok";

        void m_oBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                if (m_oBackgroundWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                Application.Current.Dispatcher.Invoke(() => { if (flag == "ok") { statusFlag.Background = Brushes.LightGreen; } else { statusFlag.Background = Brushes.Red; } });

                if (flag == "ok")
                {
                    SetFlag(null);
                    Application.Current.Dispatcher.Invoke(new InvokeDelegate(GetTextLineFromGCode_window));
                    sendString = codeConversion.GetConvertedSteps(gCodeTextLine, steps, logPatch);

                    if (sendString == "S;nothing;END;")
                    {
                        m_oBackgroundWorker.ReportProgress(lineCounter + 1);
                        lineCounter++;
                        SetFlag("ok");
                    }
                    else
                    {
                        Thread.Sleep(10);
                        serialConnect.SendStringData(logPatch, sendString);
                        m_oBackgroundWorker.ReportProgress(lineCounter + 1);
                        lineCounter++;
                    }
                }
            } while (lineCounter <= numberOfLines);
            gCodeTextLine = string.Empty;
        }
        void m_oBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = ((e.ProgressPercentage*100)/numberOfLines);
        }
        void m_oBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lineCounter = 0;
                gCodeTextLine = string.Empty;
                flag = "ok";

                bEditGCodeWindow.IsEnabled = true;
                GCode_window.IsEnabled = true;
                BlockButtons("Y", "N", "N", "none", "none", "none", "none");

                statusWork.Background = Brushes.Red;
            }
            else
            {
                lineCounter = 0;
                gCodeTextLine = string.Empty;
                flag = "ok";

                bEditGCodeWindow.IsEnabled = true;
                GCode_window.IsEnabled = true;
                BlockButtons("Y", "N", "N", "none", "none", "none", "none");

                statusWork.Background = Brushes.Red;
            }
        }
        private void GetTextLineFromGCode_window()
        {
            gCodeTextLine = GCode_window.GetLineText(lineCounter).ToString();
        }
        public static void SetFlag(string text)
        {
            lock (m_oPadlock)
            {
                flag = text;
            }
        } 
        #endregion
    }
}