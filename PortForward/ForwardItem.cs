using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PortForward
{
    public class ForwardItem
    {
        public ForwardItem()
        {
            _id = Guid.NewGuid().ToString();
            State = ForwardState.Stopped;
        }

        private string _id;
        public string Id { get => _id; }

        public string Name { get; set; }

        public string LocalListenAddress { get; set; }

        public int LocalListenPort { get; set; }

        public string RemoteAddress { get; set; }

        public int RemotePort { get; set; }

        public long UploadSpeed { get; set; }

        public long DownloadSpeed { get; set; }

        public long TotalUpload { get; set; }

        public long TotalDownload { get; set; }


        public ForwardProtocol Protocol { get; set; }

        public ForwardState State { get; set; }

        public Socket LocalServerSocket { get; set; }

        private List<Socket> _LocalClientSockets = new List<Socket>();

        public Socket RemoteClientSocket { get; set; }

        public string ErrorMessage { get; set; }

        public bool Start()
        {
            if (string.IsNullOrEmpty(LocalListenAddress))
            {
                ErrorMessage = "本地监听地址不能为空";
                return false;
            }

            if (LocalListenPort <= 0)
            {
                ErrorMessage = "本地监听端口错误";
                return false;
            }

            if (string.IsNullOrEmpty(RemoteAddress))
            {
                ErrorMessage = "目标地址不能为空";
                return false;
            }

            if (RemotePort <= 0)
            {
                ErrorMessage = "目标端口错误";
                return false;
            }

            if (NetworkTools.CheckPortUsed(LocalListenPort))
            {
                ErrorMessage = $"本地监听端口[{LocalListenPort}]已被使用";
                return false;
            }

            if (!IPAddress.TryParse(LocalListenAddress, out IPAddress address))
            {
                ErrorMessage = "本地监听地址错误";
                return false;
            }

            _CreateSocketServer(address);

            if (LocalServerSocket == null)
            {
                ErrorMessage = "Socket创建失败";
                return false;
            }

            State = ForwardState.Runing;

            _StartAccept();

            return true;
        }

        public bool Stop()
        {
            if(LocalServerSocket == null)
            {
                ErrorMessage = "本地监听服务尚未初始化";
                return false;
            }

            if (RemoteClientSocket == null)
            {
                ErrorMessage = "尚未连接目标远程服务";
                return false;
            }

            LocalServerSocket.Close();
            _LocalClientSockets.Clear();
            RemoteClientSocket.Close();

            State = ForwardState.Stopped;

            return true;
        }

        protected void _CreateSocketServer(IPAddress address)
        {
            switch (Protocol)
            {
                case ForwardProtocol.Tcp:
                    _CreateTcpServer(address);
                    break;
                case ForwardProtocol.Udp:
                    _CreateUdpServer(address);
                    break;
            }
        }

        protected void _CreateTcpServer(IPAddress address)
        {
            LocalServerSocket = new Socket(
                address.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp
            );
            LocalServerSocket.Bind(new IPEndPoint(address, LocalListenPort));
            LocalServerSocket.Listen(1024);
        }

        protected void _CreateUdpServer(IPAddress address)
        {

        }

        protected void _StartAccept()
        {
            LocalServerSocket.BeginAccept(_BeginAcceptFromLocalServer, null);
        }

        private void _BeginAcceptFromLocalServer(IAsyncResult ar)
        {
            Socket client = LocalServerSocket.EndAccept(ar);

            _LocalClientSockets.Add(client);

            MessageHandler message = new MessageHandler(client);
            client.BeginReceive(message.GetData, message.GetIndex, message.RemainSize, SocketFlags.None, _BeginReceiveClientData, message);

            LocalServerSocket.BeginAccept(_BeginAcceptFromLocalServer, null);
        }

        private void _BeginReceiveClientData(IAsyncResult ar)
        {
            MessageHandler message = (MessageHandler)ar.AsyncState;

            int count = message.clientSocket.EndReceive(ar);

            TotalDownload += count; // 总下载字节数
            
            message.clientSocket.BeginReceive(message.GetData, message.GetIndex, message.RemainSize, SocketFlags.None, _BeginReceiveClientData, message);
        }
    }

    class MessageHandler
    {
        public MessageHandler(Socket clientSocket)
        {
            this.clientSocket = clientSocket;
        }

        public Socket clientSocket;

        const int MaxLength = 1024;

        private byte[] data = new byte[MaxLength];
        private int Index = 0;

        public byte[] GetData
        {
            get { return data; }
        }

        public int RemainSize
        {
            get { return data.Length - Index; }
        }

        public int GetIndex
        {
            get { return Index; }
        }
    }

    public enum ForwardProtocol
    {
        Tcp = 1,
        Udp = 2
    }

    public enum ForwardState
    {
        Stopped = 1,
        Runing = 2
    }
}
