using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace PortForward
{
    public class ForwardItem
    {
        public ForwardItem()
        {
            _id = Guid.NewGuid().ToString();
            State = ForwardState.Stopped;
        }

        private ListViewItem _item;
        public ListViewItem Item
        {
            get => _item;
            set
            {
                _item = value;
                _item.SubItems.Clear();
                _item.Text = Title;
                _item.SubItems.AddRange(_buildSubItems(_item));
            }
        }

        private ListViewSubItem[] _buildSubItems(ListViewItem owner)
        {
            return new ListViewSubItem[] {
                //new ListViewSubItem(owner, Title),
                new ListViewSubItem(owner, LocalListenAddress),
                new ListViewSubItem(owner, LocalListenPort.ToString()),
                new ListViewSubItem(owner, RemoteAddress),
                new ListViewSubItem(owner, RemotePort.ToString()),
                new ListViewSubItem(owner, UploadSpeed.ToString()),
                new ListViewSubItem(owner, DownloadSpeed.ToString()),
                new ListViewSubItem(owner, TotalUpload.ToString()),
                new ListViewSubItem(owner, TotalDownload.ToString()),
            };
        }

        private string _id;
        public string Id { get => _id; }

        public string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                if(Item != null) Item.Text = _title;
            }
        }

        public string _localListenAddress;
        public string LocalListenAddress
        {
            get => _localListenAddress;
            set
            {
                _localListenAddress = value;
                if (Item != null) Item.SubItems[1].Text = _localListenAddress;
            }
        }

        public int _localListenPort;
        public int LocalListenPort
        {
            get => _localListenPort;
            set
            {
                _localListenPort = value;
                if (Item != null) Item.SubItems[2].Text = _localListenPort.ToString();
            }
        }

        public string _remoteAddress;
        public string RemoteAddress
        {
            get => _remoteAddress;
            set
            {
                _remoteAddress = value;
                if (Item != null) Item.SubItems[3].Text = _remoteAddress;
            }
        }

        public int _remotePort;
        public int RemotePort
        {
            get => _remotePort;
            set
            {
                _remotePort = value;
                if (Item != null) Item.SubItems[4].Text = _remotePort.ToString();
            }
        }

        public long _uploadSpeed;
        public long UploadSpeed
        {
            get => _uploadSpeed;
            set
            {
                _uploadSpeed = value;
                if (Item != null) Item.SubItems[5].Text = _uploadSpeed.ToString();
            }
        }

        public long _downloadSpeed;
        public long DownloadSpeed
        {
            get => _downloadSpeed;
            set
            {
                _downloadSpeed = value;
                if (Item != null) Item.SubItems[6].Text = _downloadSpeed.ToString();
            }
        }

        public long _totalUpload;
        public long TotalUpload
        {
            get => _totalUpload;
            set
            {
                _totalUpload = value;
                if (Item != null) Item.SubItems[7].Text = _totalUpload.ToString();
            }
        }

        public long _totalDownload;
        public long TotalDownload
        {
            get => _totalDownload;
            set
            {
                _totalDownload = value;
                if (Item != null) Item.SubItems[8].Text = _totalDownload.ToString();
            }
        }

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
            client.BeginReceive(message.Buffer, message.GetIndex, message.RemainSize, SocketFlags.None, _BeginReceiveClientData, message);

            LocalServerSocket.BeginAccept(_BeginAcceptFromLocalServer, null);
        }

        private void _BeginReceiveClientData(IAsyncResult ar)
        {
            MessageHandler message = (MessageHandler)ar.AsyncState;

            int count = message.clientSocket.EndReceive(ar);

            Item.ListView.Invoke(new Action(() => {
                TotalDownload += count; // 总下载字节数
            }));

            byte[] data = message.GetData(count);

            message.clientSocket.BeginReceive(message.Buffer, message.GetIndex, message.RemainSize, SocketFlags.None, _BeginReceiveClientData, message);
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

        public byte[] Buffer
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

        public byte[] GetData(int recv)
        {
            byte[] data = new byte[recv];
            Array.Copy(Buffer, data, recv);
            return data;
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
