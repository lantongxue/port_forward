using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace PortForward
{
    class NetworkTools
    {
        /// <summary>
        /// 检查端口是否占用
        /// </summary>
        /// <param name="port">端口号</param>
        /// <param name="protocol">协议类型，仅支持Tcp和Udp协议</param>
        /// <returns></returns>
        public static bool CheckPortUsed(int port, ProtocolType protocol = ProtocolType.Tcp)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = null;
            switch(protocol)
            {
                case ProtocolType.Udp:
                    ipEndPoints = ipProperties.GetActiveUdpListeners();
                    break;
                case ProtocolType.Tcp:
                default:
                    ipEndPoints = ipProperties.GetActiveTcpListeners();
                    break;
            }

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }

        public static string BytesFormat(long bytes)
        {
            string[] units = new string[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            int i = 0;
            for (; bytes >= 1024; i++)
            {
                bytes /= 1024;
            }
            string unit = units[i];
            return (Math.Floor(bytes * 100.00) / 100) + unit;
        }
    }
}
