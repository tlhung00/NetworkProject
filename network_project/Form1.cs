using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
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

        private void btnHost_Click(object sender, EventArgs e)
        {
            socket.isServer = true;
            btnJoin.Enabled = false;

            socket.StartServer();
            richTextBox1.AppendText("Player 2 joined the game!\n");
        }

        private void btnJoin_Click(object sender, EventArgs e)
        {
            socket.isServer = false;
            pnlBoard.Enabled = false;
            btnHost.Enabled = false;

            socket.StartClient();
            richTextBox1.AppendText("You joined the server!\n");
            Listen();
        }

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
            listenThread.IsBackground = true;
            listenThread.Start();
        }

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

        
    }
}
