using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace network_project
{
    #region SocketData
    //------------------------SocketData------------------------
    [Serializable]
    public class SocketData
    {
        private int command;

        public int Command
        {
            get { return command; }
            set { command = value; }
        }

        private Point point;

        public Point Point
        {
            get { return point; }
            set { point = value; }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public SocketData(int command, string message, Point point)
        {
            this.Command = command;
            this.Point = point;
            this.Message = message;
        }
    }
    //Các loại command mà client và server sẽ gửi
    public enum SocketCommand
    {
        SEND_POINT,
        NOTIFY,
        NEW_GAME,
        END_GAME,
        CHAT,
        SURRENDER,
    }
    #endregion
    #region Player
    //------------------------Player------------------------
    public class Player
    {
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Image mark;

        public Image Mark
        {
            get { return mark; }
            set { mark = value; }
        }

        public Player(string name, Image mark)
        {
            this.Name = name;
            this.Mark = mark;
        }
    }
    #endregion
    #region Board
    //------------------------Board------------------------
    public class BoardProp
    {
        public static int pieceWidth = 20;

        public static int pieceHeight = 20;

        public static int boardWidth = 20;

        public static int boardHeight = 20;
    }
    public class BoardManager
    {

        private Panel board;
        public Panel Board
        {
            get { return board; }
            set { board = value; }
        }

        private List<Player> player;

        public List<Player> Player
        {
            get { return player; }
            set { player = value; }
        }

        private int currentPlayer;

        public int CurrentPlayer
        {
            get { return currentPlayer; }
            set { currentPlayer = value; }
        }

        private TextBox playerName;

        public TextBox PlayerName
        {
            get { return playerName; }
            set { playerName = value; }
        }

        private PictureBox playerMark;

        public PictureBox PlayerMark
        {
            get { return playerMark; }
            set { playerMark = value; }
        }

        private List<List<Button>> matrix;

        public List<List<Button>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        private event EventHandler<ButtonClickEvent> playerMarked;
        public event EventHandler<ButtonClickEvent> PlayerMarked
        {
            add
            {
                playerMarked += value;
            }
            remove
            {
                playerMarked -= value;
            }
        }

        private event EventHandler endedGame;
        public event EventHandler EndedGame
        {
            add
            {
                endedGame += value;
            }
            remove
            {
                endedGame -= value;
            }
        }

        public BoardManager(Panel board, TextBox playerName, PictureBox mark)
        {
            this.Board = board;
            this.PlayerName = playerName;
            this.PlayerMark = mark;

            this.Player = new List<Player>()
            {
                new Player("Player 1", Image.FromFile(Application.StartupPath + "\\Resources\\P1.png")),
                new Player("Player 2", Image.FromFile(Application.StartupPath + "\\Resources\\P2.png"))
            };
        }
        //Tạo bàn cờ mới
        public void CreateBoard()
        {
            Board.Enabled = true; 
            Board.Controls.Clear();

            CurrentPlayer = 0;

            ChangePlayer();

            Matrix = new List<List<Button>>();

            Button oldButton = new Button() { Width = 0, Location = new Point(0, 0) };
            for (int i = 0; i < BoardProp.boardHeight; i++)
            {
                for (int j = 0; j < BoardProp.boardWidth; j++)
                {
                    Matrix.Add(new List<Button>());

                    Button btn = new Button()
                    {
                        Width = BoardProp.pieceWidth,
                        Height = BoardProp.boardHeight,
                        Location = new Point(oldButton.Location.X + oldButton.Width, oldButton.Location.Y),
                        BackgroundImageLayout = ImageLayout.Stretch,
                        Tag = i.ToString()
                    };

                    btn.Click += btn_Click;
                    Board.Controls.Add(btn);
                    Matrix[i].Add(btn);
                    oldButton = btn;
                }
                oldButton.Location = new Point(0, oldButton.Location.Y + BoardProp.pieceHeight);
                oldButton.Width = 0;
                oldButton.Height = 0;
            }
        }
        //Đánh quân cờ
        void btn_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            ChangePlayer();

            if (playerMarked != null)
                playerMarked(this, new ButtonClickEvent(GetPoint(btn)));

            if (isEndGame(btn))
            {
                EndGame();
            }
        }

        public void OtherPlayerMove(Point point)
        {
            Button btn = Matrix[point.Y][point.X];

            if (btn.BackgroundImage != null)
                return;

            Mark(btn);

            CurrentPlayer = CurrentPlayer == 1 ? 0 : 1;

            ChangePlayer();

            if (isEndGame(btn))
            {
                EndGame();
            }
        }
        public void EndGame()
        {
            if (endedGame!= null)
                endedGame(this, new EventArgs());
        }
        #endregion
    #region Logic
        //------------------------Logic------------------------
        //Kiểm tra nếu có người thắng cuộc
        private bool isEndGame(Button btn)
        {
            return EndHorizontal(btn) || EndVertical(btn) || EndPrimary(btn) || EndSub(btn);
        }

        private Point GetPoint(Button btn)
        {
            int vertical = Convert.ToInt32(btn.Tag);
            int horizontal = Matrix[vertical].IndexOf(btn);
            Point point = new Point(horizontal, vertical);
            return point;
        }
        //Kiểm tra nếu có 5 quân cờ hàng ngang
        private bool EndHorizontal(Button btn)
        {
            Point point = GetPoint(btn);

            int countLeft = 0;
            for (int i = point.X; i >= 0; i--)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countLeft++;
                }
                else
                    break;
            }

            int countRight = 0;
            for (int i = point.X + 1; i < BoardProp.boardWidth; i++)
            {
                if (Matrix[point.Y][i].BackgroundImage == btn.BackgroundImage)
                {
                    countRight++;
                }
                else
                    break;
            }
            return countLeft + countRight == 5;
        }
        //Kiểm tra nếu có 5 quân cờ hàng dọc
        private bool EndVertical(Button btn)
        {
            Point point = GetPoint(btn);

            int countTop = 0;
            for (int i = point.Y; i >= 0; i--)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = point.Y + 1; i < BoardProp.boardHeight; i++)
            {
                if (Matrix[i][point.X].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        //Kiểm tra nếu có 5 quân cờ nằm theo đường chéo (/)
        private bool EndPrimary(Button btn)
        {
            Point point = GetPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X - i < 0 || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= BoardProp.boardWidth - point.X; i++)
            {
                if (point.Y + i >= BoardProp.boardHeight || point.X + i >= BoardProp.boardWidth)
                    break;

                if (Matrix[point.Y + i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        //Kiểm tra nếu có 5 quân cờ nằm theo đường chéo (\)
        private bool EndSub(Button btn)
        {
            Point point = GetPoint(btn);

            int countTop = 0;
            for (int i = 0; i <= point.X; i++)
            {
                if (point.X + i > BoardProp.boardWidth || point.Y - i < 0)
                    break;

                if (Matrix[point.Y - i][point.X + i].BackgroundImage == btn.BackgroundImage)
                {
                    countTop++;
                }
                else
                    break;
            }

            int countBottom = 0;
            for (int i = 1; i <= BoardProp.boardWidth - point.X; i++)
            {
                if (point.Y + i >= BoardProp.boardHeight || point.X - i < 0)
                    break;

                if (Matrix[point.Y + i][point.X - i].BackgroundImage == btn.BackgroundImage)
                {
                    countBottom++;
                }
                else
                    break;
            }
            return countTop + countBottom == 5;
        }
        //Thay đổi ảnh của ô được chọn bằng quân cờ của người chơi
        public void Mark(Button btn)
        {
            btn.BackgroundImage = Player[CurrentPlayer].Mark;
        }
        //Đổi người chơi
        public void ChangePlayer()
        {
            PlayerName.Text = Player[CurrentPlayer].Name;
            PlayerMark.Image = Player[CurrentPlayer].Mark;
        }
        public class ButtonClickEvent : EventArgs
        {
            private Point clickedPoint;

            public Point ClickedPoint
            {
                get { return clickedPoint; }
                set { clickedPoint = value; }
            }

            public ButtonClickEvent(Point point)
            {
                this.ClickedPoint = point;
            }
        }
        #endregion
    }
}
