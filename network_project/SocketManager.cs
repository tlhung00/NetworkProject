using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace network_project
{
    internal class SocketManager
    {
        public string IP = "127.0.0.1";
        public int PORT = 10000;
       //Hàm gửi data
        public bool Send(object data)
        {
            byte[] sendData = SerializeData(data);
            return SendData(client, sendData);
        }
        //Hàm nhận data
        public object Receive()
        {
            byte[] receiveData = new byte[1024];
            ReceiveData(client, receiveData);
            return DeserializeData(receiveData);
        }

        private bool SendData(Socket target, byte[] data)
        {
            return target.Send(data) == 1 ? true : false;
        }

        private bool ReceiveData(Socket target, byte[] data)
        {
            return target.Receive(data) == 1 ? true : false;
        }
        //Chuyển data thành mảng byte
        public byte[] SerializeData(Object o)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf1 = new BinaryFormatter();
            bf1.Serialize(ms, o);
            return ms.ToArray();
        }
        //Chuyển mảng byte thành data
        public object DeserializeData(byte[] theByteArray)
        {
            MemoryStream ms = new MemoryStream(theByteArray);
            BinaryFormatter bf1 = new BinaryFormatter();
            ms.Position = 0;
            return bf1.Deserialize(ms);
        }
        //Lấy địa chỉ IPV4 của card mạng đang dùng
        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }
        //------------------------Server------------------------
        Socket server;
        //Khởi tạo Server
        public void StartServer()
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            for (PORT = 10000; PORT <= 10100; PORT++)
            {
                try
                {
                    server.Bind(new IPEndPoint(IPAddress.Parse(IP), PORT));
                    server.Listen(10);
                    Thread accept = new Thread(() =>
                    {
                        try
                        {
                            client = server.Accept();
                            MessageBox.Show("Your opponent has joined!", "Notice", MessageBoxButtons.OK);
                        }
                        catch { }
                    });
                    accept.IsBackground = true;
                    accept.Start();
                    break;
                }
                catch { }
            }
        }

        //------------------------Client------------------------
        Socket client;
        //Khởi tạo Client
        public void StartClient()
        {
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                client.Connect(IPAddress.Parse(IP), PORT);
                MessageBox.Show("You have an opponent!", "Notice", MessageBoxButtons.OK);
            }
            catch { }
        }
    }
}
