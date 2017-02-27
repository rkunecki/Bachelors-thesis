using System;
using System.IO.Ports;
using System.Windows;

namespace PCBFrez
{
    class SerialConnection
    {
        public SerialPort Port { get; set; }
        private delegate void SetTextCallback(string text);
        public string TextToSend { get; set; }
        public string InputData { get; set; }

        public bool Connect(string logPatch, string portName, int baudrate, int parityBits, int dataBits, int stopBits)
        {
            StopBits stop = new StopBits();
            Parity parity = new Parity();

            switch (stopBits)
            {
                case 0:
                    stop = StopBits.None;
                    break;
                case 1:
                    stop = StopBits.One;
                    break;
                case 2:
                    stop = StopBits.Two;
                    break;
            }

            switch (parityBits)
            {
                case 0:
                    parity = Parity.None;
                    break;
                case 1:
                    parity = Parity.Even;
                    break;
                case 2:
                    parity = Parity.Odd;
                    break;
            }
            try
            {
                this.Port = new SerialPort(portName, baudrate, parity, dataBits, stop);
                this.Port.DtrEnable = true;
                this.Port.RtsEnable = true;
                this.Port.Handshake = Handshake.None;
                this.Port.DataReceived += new SerialDataReceivedEventHandler(Serial_DataReceived);

                this.Port.ReadTimeout = 1000;
                this.Port.WriteTimeout = 1000;

                this.Port.Open();

                return true;
            }
            catch (Exception ex)
            {
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
                return false;
            }
        }

        public bool SendStringData(string logPatch, string text)
        {
            try
            {
                Port.WriteLine(text);
                return true;
            }
            catch (Exception ex)
            {
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
                return false;
            }
        }

        public void Serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp = (SerialPort)sender;
                InputData = sp.ReadLine();
                if (InputData != String.Empty)
                {
                    Application.Current.Dispatcher.Invoke(new SetTextCallback(SetText), new object[] { InputData });
                }
            }
            catch { }
        }

        private void SetText(string text)
        {
            string sHelp = string.Empty;
            this.TextToSend = text;

            for (int i = 0; i < 2; i++)
            {
                sHelp += text[i];
            }
            MainWindow.SetFlag(sHelp);
        }

        public bool Disconnect(string logPatch)
        {
            try
            {
                if (this.Port.IsOpen)
                    this.Port.Close();
                return false;
            }
            catch (Exception ex)
            {
                ConfigRead.SaveErrorToLog(logPatch, ex.Message);
                return true;
            }
        }
    }
}
