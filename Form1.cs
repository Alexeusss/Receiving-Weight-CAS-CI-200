using System;
using System.Drawing;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ComPortReader
{
    public partial class Form1 : Form
    {
        private SerialPort serialPort;
        private const int maxBufferLength = 100; // Максимальный размер буфера данных

        public Form1()
        {
            InitializeComponent();
            LoadAvailablePorts();
        }

        private void LoadAvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            comPortComboBox.Items.AddRange(ports);
            if (ports.Length > 0)
            {
                comPortComboBox.SelectedIndex = 0;
            }
        }

        private void ComPortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
                serialPort.Dispose();
            }

            string selectedPort = comPortComboBox.SelectedItem.ToString();
            serialPort = new SerialPort(selectedPort, 9600)
            {
                ReadTimeout = 1000
            };
            serialPort.DataReceived += SerialPort_DataReceived;

            try
            {
                serialPort.Open();
            }
            catch (Exception ex)
            {
                outputLabel.ForeColor = Color.White;
                outputLabel.Text = $"Error: {ex.Message}";
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string data = serialPort.ReadLine();
                this.Invoke(new Action(() => ProcessData(data)));
            }
            catch (TimeoutException) { }
        }

        private void ProcessData(string data)
        {
            if (data.Length > maxBufferLength)
            {
                data = data.Substring(0, maxBufferLength);
            }

            Regex weightPattern = new Regex(@"(\d+\.\d+)\s*kg");
            Match match = weightPattern.Match(data);

            if (match.Success)
            {
                string weight = match.Groups[1].Value;
                if (data.Contains("ST"))
                {
                    outputLabel.ForeColor = Color.Green;
                    outputLabel.Text = "СТАБ: " + weight + "0 kg";
                }
                else if (data.Contains("US"))
                {
                    outputLabel.ForeColor = Color.Red;
                    outputLabel.Text = "НСТАБ: " + weight + "0 kg";
                }
                else
                {
                    outputLabel.ForeColor = Color.White;
                    outputLabel.Text = "Received: " + data;
                }
            }
            else
            {
                outputLabel.ForeColor = Color.White;
                outputLabel.Text = "Received: " + data;
            }

            // Принудительная сборка мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }


        private void alwaysOnTopCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = alwaysOnTopCheckBox.Checked;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
