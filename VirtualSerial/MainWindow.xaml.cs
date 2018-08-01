using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VirtualSerial
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            comboBox.ItemsSource = SerialPort.GetPortNames();
            comboBox.SelectedIndex = 0;

            comboBox_Copy.ItemsSource = new int[] { 9600, 19200 };
            comboBox_Copy.SelectedIndex = 0;
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void comboBox_Copy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private SerialPort CreateSerialPort()
        {
            var port = new SerialPort();
            try
            {
                port.PortName = comboBox.SelectedItem as string;
                port.BaudRate = int.Parse(comboBox_Copy.SelectedItem.ToString());

                port.Open();
                port.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误:{ex.Message}");
                port = null;
            }

            return port;
        }

        /// <summary>
        /// 速度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Click(object sender, RoutedEventArgs e)
        {
            var port = CreateSerialPort();
            if (port != null)
            {
                Core.AddSerialPort(port);

                Dictionary<string, string> dics = new Dictionary<string, string>();
                dics.Add("02 31 17 4C 44 17 30 30 30 30 17 43 36 03", "02 31 17 4C 44 17 06 17 30 30 30 30 17 31 30 03 ");
                dics.Add("02 31 17 47 44 17 30 30 30 30 17 43 31 03", "02 31 17 47 44 17 06 17 30 30 31 30 17 30 30 30 2E 30 30 17 00 00 17 34 33 03");
                dics.Add("02 31 17 4C 55 17 30 30 30 30 17 47 37 03", "02 31 17 4C 55 17 06 17 30 30 30 30 17 32 37 03 ");

                var view = new SerialPortView(port, dics);
                view.Title = $"模拟速度({port.PortName})";

                view.Show();
            }
        }

        /// <summary>
        /// 称重
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            var port = CreateSerialPort();
            if (port != null)
            {
                Core.AddSerialPort(port);

                Dictionary<string, string> dics = new Dictionary<string, string>();
                dics.Add("02 33 17 47 44 17 30 30 30 30 17 43 33 03", "02 33 17 47 44 17 06 17 30 30 31 36 17 30 30 30 32 17 30 30 31 30 17 1E 00 17 30 31 17 36 39 03");

                var view = new SerialPortView(port, dics);
                view.Title = $"模拟称重({port.PortName})";

                view.Show();
            }
        }

        /// <summary>
        /// 侧滑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Copy1_Click(object sender, RoutedEventArgs e)
        {
            var port = CreateSerialPort();
            if (port != null)
            {
                Core.AddSerialPort(port);

                Dictionary<string, string> dics = new Dictionary<string, string>();
                dics.Add("02 32 17 47 44 17 30 30 30 30 17 43 32 03", "02 32 17 47 44 17 06 17 30 30 31 30 17 30 30 30 2E 30 30 17 00 00 17 34 32 03");

                var view = new SerialPortView(port, dics);
                view.Title = $"模拟侧滑({port.PortName})";

                view.Show();
            }
        }

        private void button_Copy3_Click(object sender, RoutedEventArgs e)
        {
            var port = CreateSerialPort();
            if (port != null)
            {
                Core.AddSerialPort(port);

                Dictionary<string, string> dics = new Dictionary<string, string>();
                var view = new SerialPortView(port, dics);
                view.Title = $"模拟光电({port.PortName})";

                view.Show();
            }
        }

        private void button_Copy4_Click(object sender, RoutedEventArgs e)
        {
            var port = CreateSerialPort();
            if (port != null)
            {
                Core.AddSerialPort(port);

                Dictionary<string, string> dics = new Dictionary<string, string>();
                var view = new ChartView(port.PortName, dics);
                view.Title = $"Chart({port.PortName})";

                view.Show();
            }
        }
    }
}