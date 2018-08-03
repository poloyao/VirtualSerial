using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VirtualSerial
{
    /// <summary>
    /// SerialPortView.xaml 的交互逻辑
    /// </summary>
    public partial class SerialPortView : Window
    {
        private SerialPort Port;
        private List<byte> bytesData = new List<byte>();
        private List<byte> bytesTemp = new List<byte>();
        private object obj = new object();

        private List<byte[]> Commands = new List<byte[]>();
        private List<byte[]> ReplyCommands = new List<byte[]>();

        private CancellationTokenSource cancellationTokenSource;
        private byte temp;
        private byte[] writeBytes;
        private Task task;

        public SerialPortView()
        {
            InitializeComponent();
            this.Closed += SerialPortView_Closed;
            button_Copy.IsEnabled = false;
        }

        private void SerialPortView_Closed(object sender, EventArgs e)
        {
            Port.Close();
            Core.DeleteSerialPort(Port);
        }

        public SerialPortView(SerialPort port, Dictionary<string, string> dics) : this()
        {
            Port = port;
            port.Open();

            int i = 0;
            foreach (var item in dics)
            {
                switch (i)
                {
                    case 0:
                        tb_1.Text = item.Key;
                        tb_11.Text = item.Value;
                        break;

                    case 1:
                        tb_2.Text = item.Key;
                        tb_21.Text = item.Value;
                        break;

                    case 2:
                        tb_3.Text = item.Key;
                        tb_31.Text = item.Value;
                        break;

                    case 3:
                        tb_4.Text = item.Key;
                        tb_41.Text = item.Value;
                        break;

                    case 4:
                        tb_5.Text = item.Key;
                        tb_51.Text = item.Value;
                        break;

                    default:
                        break;
                }
                i++;
            }
        }

        private void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            button_Copy.IsEnabled = false;
            button.IsEnabled = true;

            Port.DataReceived -= Port_DataReceived;
            Port.Close();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            button_Copy.IsEnabled = true;
            button.IsEnabled = false;

            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();

            if (!Port.IsOpen)
                Port.Open();

            Commands.Clear();
            ReplyCommands.Clear();
            //被动模式
            if (radioButton.IsChecked == true)
            {
                //获取允许发送发字段
                if (checkBox1.IsChecked == true)
                {
                    Commands.Add(strToToHexByte(tb_1.Text));
                    ReplyCommands.Add(strToToHexByte(tb_11.Text));
                }
                if (checkBox2.IsChecked == true)
                {
                    Commands.Add(strToToHexByte(tb_2.Text));
                    ReplyCommands.Add(strToToHexByte(tb_21.Text));
                }

                if (checkBox3.IsChecked == true)
                {
                    Commands.Add(strToToHexByte(tb_3.Text));
                    ReplyCommands.Add(strToToHexByte(tb_31.Text));
                }

                if (checkBox4.IsChecked == true)
                {
                    Commands.Add(strToToHexByte(tb_4.Text));
                    ReplyCommands.Add(strToToHexByte(tb_41.Text));
                }

                if (checkBox5.IsChecked == true)
                {
                    Commands.Add(strToToHexByte(tb_5.Text));
                    ReplyCommands.Add(strToToHexByte(tb_51.Text));
                }

                Port.DataReceived += Port_DataReceived;
            }
            else //主动模式
            {
                var modeO = 0x00;
                var modeA = 0x01;
                var modeB = 0x02;
                var modeC = 0x04;
                var modeD = 0x08;
                var temp = modeO;
                byte[] writeBytes = new byte[] { 0xAA, (byte)modeO, 0x0D };
                List<byte> sad = new List<byte>();
                if (cB1.IsChecked == true)
                {
                    temp += modeA;
                    sad.Add((byte)modeA);
                }
                if (cB2.IsChecked == true)
                {
                    temp += modeB;
                    sad.Add((byte)modeB);
                }
                if (cB3.IsChecked == true)
                {
                    temp += modeC;
                    sad.Add((byte)modeC);
                }
                if (cB4.IsChecked == true)
                {
                    temp += modeD;
                    sad.Add((byte)modeD);
                }
                //writeBytes[1] = (byte)temp;
                new TaskFactory().StartNew(() =>
                {
                    try
                    {
                        int i = 0;
                        int j = 0;
                        while (true)
                        {
                            if (Port.IsOpen)
                            {
                                System.Threading.Thread.Sleep(50);
                                if (i < 100)
                                { }
                                else
                                {
                                    i = 0;
                                    if (j == sad.Count())
                                        j = 0;
                                    writeBytes[1] = sad[j];
                                    j++;
                                }

                                Port.Write(writeBytes, 0, 3);
                                i++;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            }
        }

        /// <summary>
        /// 主动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Port_DataReceived1(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesRead;
            try
            {
                lock (obj)
                {
                    //获取接收缓冲区中字节数
                    bytesRead = Port.BytesToRead;

                    //保存本次接收的数据
                    for (int i = 0; i < bytesRead; i++)
                    {
                        bytesData.Add(Convert.ToByte(Port.ReadByte()));
                    }

                    int last = 0;
                    var length = Commands.Max(x => x.Length);
                    if (bytesData.Count >= length)
                    {
                        for (int i = 0; i < bytesData.Count; i++)
                        {
                            for (int j = 0; j < Commands.Count; j++)
                            {
                                var command = Commands[j];
                                var tempLength = command.Length;
                                if (bytesData.Count >= tempLength + i)
                                {
                                    var copyData = new byte[tempLength];
                                    bytesData.CopyTo(i, copyData, 0, tempLength);
                                    var equalResult = copyData.SequenceEqual(command);
                                    if (equalResult)
                                    {
                                        Port.Write(ReplyCommands[j], 0, ReplyCommands[j].Length);
                                        i += tempLength;
                                        break;
                                    }
                                }
                            }

                            last = i;
                        }

                        //将未处理的内容赋给bytesData
                        if (bytesData.Count > last)
                        {
                            bytesData.RemoveRange(0, bytesData.Count - last);
                        }
                        else
                        {
                            bytesData.Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Helper.NLogHelper.log.Error(ex, ex.Message);
            }
        }

        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesRead;
            try
            {
                lock (obj)
                {
                    //获取接收缓冲区中字节数
                    bytesRead = Port.BytesToRead;

                    //保存本次接收的数据
                    for (int i = 0; i < bytesRead; i++)
                    {
                        bytesData.Add(Convert.ToByte(Port.ReadByte()));
                    }

                    int last = 0;
                    //应该是最小长度 Commands.Min(x => x.Length);
                    var length = Commands.Max(x => x.Length);
                    if (bytesData.Count >= length)
                    {
                        for (int i = 0; i < bytesData.Count; i++)
                        {
                            for (int j = 0; j < Commands.Count; j++)
                            {
                                var command = Commands[j];
                                var tempLength = command.Length;
                                if (bytesData.Count >= tempLength + i)
                                {
                                    var copyData = new byte[tempLength];
                                    bytesData.CopyTo(i, copyData, 0, tempLength);
                                    var equalResult = copyData.SequenceEqual(command);
                                    if (equalResult)
                                    {
                                        Port.Write(ReplyCommands[j], 0, ReplyCommands[j].Length);
                                        this.Dispatcher.BeginInvoke(new Action(() =>
                                        {
                                            var brushe = Brushes.Pink;
                                            switch (j)
                                            {
                                                case 0:
                                                    tb_1.Background = tb_1.Background == brushe ? Brushes.White : brushe;
                                                    tb_11.Background = tb_11.Background == brushe ? Brushes.White : brushe;
                                                    break;

                                                case 1:
                                                    tb_2.Background = tb_2.Background == brushe ? Brushes.White : brushe;
                                                    tb_21.Background = tb_21.Background == brushe ? Brushes.White : brushe;
                                                    break;

                                                case 2:
                                                    tb_3.Background = tb_3.Background == brushe ? Brushes.White : brushe;
                                                    tb_31.Background = tb_31.Background == brushe ? Brushes.White : brushe; ;
                                                    break;

                                                case 3:
                                                    tb_4.Background = tb_4.Background == brushe ? Brushes.White : brushe;
                                                    tb_41.Background = tb_41.Background == brushe ? Brushes.White : brushe;
                                                    break;

                                                case 4:
                                                    tb_5.Background = tb_5.Background == brushe ? Brushes.White : brushe;
                                                    tb_51.Background = tb_51.Background == brushe ? Brushes.White : brushe;
                                                    break;

                                                default:
                                                    break;
                                            }
                                        }));
                                        i += tempLength;
                                        break;
                                    }
                                }
                            }

                            last = i;
                        }

                        //将未处理的内容赋给bytesData
                        //if (bytesData.Count > last)
                        //{
                        //    bytesData.RemoveRange(0, last + 1);
                        //}
                        //else
                        //{
                        bytesData.Clear();
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                //Helper.NLogHelper.log.Error(ex, ex.Message);
            }
        }

        private void SingleRoute(byte temp, Button button)
        {
            if (button.IsEnabled == false)
                return;

            var brushe = Brushes.Blue;
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
            else
                cancellationTokenSource = new CancellationTokenSource();

            if (button.Background == brushe)
            {
                button.Background = Brushes.White;
                this.temp -= temp;
            }
            else
            {
                button.Background = brushe;
                this.temp += temp;
            }

            if (this.temp == 0x00)
                cancellationTokenSource.Cancel();
            else
            {
                writeBytes = new byte[] { 0xAA, (byte)this.temp, 0x0D };
                cancellationTokenSource = new CancellationTokenSource();
                if (task == null || task.Status == TaskStatus.RanToCompletion)
                    asda();
            }
        }

        private void asda()
        {
            task = new TaskFactory().StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            Console.WriteLine("~~~~~~~~");
                            return;
                        }
                        if (Port.IsOpen)
                        {
                            Port.Write(writeBytes, 0, 3);
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                }
                catch (Exception)
                {
                }
            }, cancellationTokenSource.Token);
        }

        private void button_cB1_Click(object sender, RoutedEventArgs e)
        {
            SingleRoute(0x01, button_cB1);
        }

        private void button_cB2_Click(object sender, RoutedEventArgs e)
        {
            SingleRoute(0x02, button_cB2);
        }

        private void button_cB3_Click(object sender, RoutedEventArgs e)
        {
            SingleRoute(0x04, button_cB3);
        }

        private void button_cB4_Click(object sender, RoutedEventArgs e)
        {
            SingleRoute(0x08, button_cB4);
        }
    }
}