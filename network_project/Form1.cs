using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static network_project.BoardManager;

namespace network_project
{
    public partial class Form1 : Form
    {
        SocketManager socket;
        BoardManager Board;
        
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            socket = new SocketManager();

            Board = new BoardManager(pnlBoard, txtPlayerName, pboxMark);
            Board.EndedGame += Board_EndedGame;
            Board.PlayerMarked += Board_PlayerMarked;

            NewGame();
        }
        //Host game
        private void btnHost_Click(object sender, EventArgs e)
        {
            socket.IP = txbIP.Text;
            btnJoin.Enabled = false;

            socket.StartServer();
            txtRoom.ReadOnly = true;
            txtRoom.Text = socket.PORT.ToString();
            richTextBox1.AppendText("You have created a room (ID:" + txtRoom.Text +")!\n");          
        }
        //Join game
        private void btnJoin_Click(object sender, EventArgs e)
        {
            socket.IP = txbIP.Text;
            socket.PORT = int.Parse(txtRoom.Text);
            pnlBoard.Enabled = false;
            btnHost.Enabled = false;

            socket.StartClient();
            richTextBox1.AppendText("You joined the room!\n");
            Listen();          
        }
        //New game
        private void newGameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewGame();
            socket.Send(new SocketData((int)SocketCommand.NEW_GAME, "", new Point()));
            pnlBoard.Enabled = true;
        }
        public void EndGame()
        {
            pnlBoard.Enabled = false;
            richTextBox1.AppendText("The game has ended\n");
        }
        public void NewGame()
        {
            richTextBox1.AppendText("New game has been started\n");
            Board.CreateBoard();
        }

        void Listen()
        {
            Thread listenThread = new Thread(() =>
            {
                try
                {
                    SocketData data = (SocketData)socket.Receive();
                    ProcessData(data);
                }
                catch 
                {
                }
            });
            //Cho listenThread thành luồng phụ
            listenThread.IsBackground = true;
            listenThread.Start();
        }
        //Xử lý data nhận về
        private void ProcessData(SocketData data)
        {
            switch (data.Command)
            {
                case (int)SocketCommand.NOTIFY:
                    MessageBox.Show(data.Message);
                    break;
                case (int)SocketCommand.NEW_GAME:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        NewGame();
                        pnlBoard.Enabled = false;
                    }));
                    break;
                case (int)SocketCommand.SEND_POINT:
                    this.Invoke((MethodInvoker)(() =>
                    {
                        pnlBoard.Enabled = true;
                        Board.OtherPlayerMove(data.Point);
                    }));
                    break;
                case (int)SocketCommand.END_GAME:
                    break;
                case (int)SocketCommand.CHAT:                  
                        richTextBox1.Text += data.Message+ "\n";
                    break;
                case (int)SocketCommand.SURRENDER:
                    EndGame();
                    richTextBox1.Text += "Đối thủ đã bỏ cuộc!!! Bạn đã chiến thắng";
                    break;
                default:
                    break;
            }
            Listen();
        }
        void Board_PlayerMarked(object sender, ButtonClickEvent e)
        {
            pnlBoard.Enabled = false;
            socket.Send(new SocketData((int)SocketCommand.SEND_POINT, "", e.ClickedPoint));
            Listen();
        }

        void Board_EndedGame(object sender, EventArgs e)
        {
            EndGame();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Wireless80211);

            if (string.IsNullOrEmpty(txbIP.Text))
            {
                txbIP.Text = socket.GetLocalIPv4(NetworkInterfaceType.Ethernet);
            }
        }
        //Thoát
        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {          
            this.Close();
        }

        private void txbChat_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txbChat.Text))
            {
                btnSend.Enabled = false;
            }
            else
            {
                btnSend.Enabled = true;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = txtPlayerName.Text + ": " + txbChat.Text;
            richTextBox1.Text += message+ "\n";
            socket.Send(new SocketData((int)SocketCommand.CHAT, message, new Point(1, 1)));
            Listen();
        }
        private void surrenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox1.Text += "Bạn đã bỏ cuộc!!! Đối thủ giành chiến thắng!!!";
            socket.Send(new SocketData((int)SocketCommand.SURRENDER, "", new Point(1, 1)));
            EndGame();
        }

        private void scanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int startPort = 10000; // Start of port range to scan
            int endPort = 10010; // End of port range to scan
            string IP = txbIP.Text;
            for (int port = startPort; port <= endPort; port++)
            {
                try
                {
                    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    client.Connect(IPAddress.Parse(IP), port);
                    client.Close();

                    // A server is listening on this port, so add it to the list of available rooms
                    string room = "Room " + port;
                    richTextBox1.AppendText(room + "\n");
                }
                catch { }
            }
        }
    }
}
