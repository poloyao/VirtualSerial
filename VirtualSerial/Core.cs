using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualSerial
{
    public class Core
    {
        private static List<SerialPort> SerialPorts = new List<SerialPort>();

        public static void AddSerialPort(SerialPort port)
        {
            if (SerialPorts.Where(x => x.PortName == port.PortName).Count() == 0)
            {
                SerialPorts.Add(port);
            }
        }

        public static void DeleteSerialPort(SerialPort port)
        {
            SerialPorts.Remove(port);
        }
    }
}