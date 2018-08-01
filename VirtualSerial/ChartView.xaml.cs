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
using System.IO.Ports;
using System.Threading;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Threading;

namespace VirtualSerial
{
    /// <summary>
    /// ChartView.xaml 的交互逻辑
    /// </summary>
    public partial class ChartView : Window
    {
        private List<byte> bytesData = new List<byte>();
        private List<byte> bytesTemp = new List<byte>();
        private object obj = new object();
        private SerialPort Port = new SerialPort();

        private List<byte[]> data = new List<byte[]>();

        /// <summary>
        /// 上位机取当前重量
        /// 02 33 17 47 44 17 30 30 30 30 17 43 33 03
        /// <![CDATA[<STX>+<Add>+<ETB>+’GD’+<ETB>+<Len>+<ETB>+<ChkSum>+<ETX>]]>
        /// </summary>
        private static readonly byte[] GetRealTimeData = new byte[] { 0x02, 0x33, 0x17, 0x47, 0x44, 0x17, 0x30, 0x30, 0x30, 0x30, 0x17, 0x43, 0x33, 0x03 };

        private static readonly byte[] GetRealTimeData_Reply = new byte[] { 0x02, 0x33, 0x17, 0x47, 0x44, 0x17, 0x06, 0x17, 0x30, 0x30, 0x31, 0x36, 0x17, 0x30, 0x30, 0x30, 0x32, 0x17, 0x30, 0x30, 0x31, 0x30, 0x17, 0x1E, 0x00, 0x17, 0x30, 0x31, 0x17, 0x36, 0x39, 0x03 };
        private Dictionary<string, string> dics;

        public ChartView()
        {
            InitializeComponent();
            Port.DataReceived += Port_DataReceived;
            this.Closed += ChartView_Closed;
            this.chart.DataContext = DataSource;
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;
            //timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime argument = DateTime.Now;
            DateTime minDate = argument.AddSeconds(-5);

            var count = dataSource.Where(x => x.DateAndTime < minDate).Count();
            if (count > 0)
                dataSource.RemoveFromBegin(count);
            axisX.WholeRange.SetMinMaxValues(minDate, argument);
        }

        private void ChartView_Closed(object sender, EventArgs e)
        {
            Port.DataReceived -= Port_DataReceived;
            Port.Close();
            Core.DeleteSerialPort(Port);
        }

        public ChartView(string port, Dictionary<string, string> dics) : this()
        {
            Port.PortName = port;
            this.dics = dics;

            if (!Port.IsOpen)
                Port.Open();
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine("change");
            Thread.Sleep(60);
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
                    var length = GetRealTimeData_Reply.Length;
                    if (bytesData.Count >= length)
                    {
                        for (int i = 0; i < bytesData.Count; i++)
                        {
                            var copyData = new byte[length];
                            bytesData.CopyTo(i, copyData, 0, length);
                            var equalResult = copyData[6] == 0x06;
                            if (equalResult)
                            {
                                //data.Add(copyData);
                                var tempData = System.Text.Encoding.Default.GetString(copyData, 13, 4);

                                this.Dispatcher.Invoke(new Action(() =>
                                {
                                    //保留200点
                                    if (dataSource.Count > 200)
                                    {
                                        dataSource.RemoveFromBegin(dataSource.Count - 200);
                                    }
                                    dataSource.Add(new ProcessItem() { DateAndTime = DateTime.Now, Process1 = int.Parse(tempData) });
                                    DateTime argument = DateTime.Now;
                                    DateTime minDate = argument;
                                    if (dataSource.Count > 0)
                                        minDate = dataSource.First().DateAndTime;

                                    axisX.WholeRange.SetMinMaxValues(minDate, argument);
                                }));

                                Console.WriteLine($"get date:{tempData}");
                                i += length;
                                last = i;
                                break;
                            }
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            NewMethod();
        }

        private DispatcherTimer timer = new DispatcherTimer();
        private readonly DataCollection dataSource = new DataCollection();
        public DataCollection DataSource { get { return dataSource; } }
        private CancellationTokenSource waitTokenSource;

        private void NewMethod()
        {
            waitTokenSource = new CancellationTokenSource();
            Task task = new TaskFactory().StartNew(() =>
            {
                while (!waitTokenSource.IsCancellationRequested)
                {
                    Port.Write(GetRealTimeData, 0, GetRealTimeData.Length);
                    int ss = 100;
                    this.slider.Dispatcher.Invoke(new Action(() => { ss = (int)this.slider.Value; }));
                    System.Threading.Thread.Sleep(ss);
                }
            }, waitTokenSource.Token);
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            waitTokenSource.Cancel();
        }
    }

    public struct ProcessItem
    {
        public DateTime DateAndTime { get; set; }
        public double Process1 { get; set; }
    }

    public class DataCollection : ObservableCollection<ProcessItem>
    {
        public void AddRange(IList<ProcessItem> items)
        {
            foreach (ProcessItem item in items)
                Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, (IList)items, Items.Count - items.Count));
        }

        public void RemoveFromBegin(int count)
        {
            IList<ProcessItem> removedItems = new List<ProcessItem>(count);
            for (int i = 0; i < count; i++)
            {
                removedItems.Add(Items[0]);
                Items.RemoveAt(0);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, (IList)removedItems, 0));
        }
    }
}