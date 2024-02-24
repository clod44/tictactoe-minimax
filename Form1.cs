using Microsoft.VisualBasic.Devices;
using System.Diagnostics;

namespace sosGame
{
    public partial class Form1 : Form
    {
        string[] board = { "X", "X", "X", "X", "X", "X", "X", "X", "X" };
        bool playerXisHuman = true;
        bool playerOisHuman = true;
        int xScore = 0;
        int yScore = 0;
        int botDifficulty = 1;
        //in this game the X is always the first one to play
        bool playerTurnX = true;
        Random r = new Random();    
        int gameStep = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void updatePhysicalBoard()
        {
            button_Place1.Text = board[0];
            button_Place2.Text = board[1];
            button_Place3.Text = board[2];
            button_Place4.Text = board[3];
            button_Place5.Text = board[4];
            button_Place6.Text = board[5];
            button_Place7.Text = board[6];
            button_Place8.Text = board[7];
            button_Place9.Text = board[8];
            gameProgressToolStripProgressBar.Value = Math.Clamp(gameStep*10, 0, 90);
            bool affirmation = isGameOver(board);
            if (affirmation)
            {
                if (CheckWinnerFor("X", board))
                {
                    MessageBox.Show("X" + (!playerXisHuman ? "(BOT)" : "(Human)") + " has won");
                    addScoreTo(1, "X");
                }
                else if (CheckWinnerFor("O", board))
                {
                    MessageBox.Show("O" + (!playerOisHuman ? "(BOT)" : "(Human)") + " has won");
                    addScoreTo(1, "O");
                }
                else MessageBox.Show("Tie");

                resetBoard();
            }
        }
        private void addScoreTo(int score, string player)
        {
            //X/Y: 0 - 0
            xScore = Math.Max(0, xScore + (player == "X" ? score : 0));
            yScore = Math.Max(0, yScore + (player == "O" ? score : 0));
            totalScoreToolStripStatusLabel.Text = "X/O: " + xScore + " - " + yScore;
        }
        private bool placeThing(int index, string thing, bool force=false)
        {
            if (force)
            {
                board[index] = thing;
                return true;
            }
            else if (board[index] == "")
            {
                board[index] = thing;
                return true;
            }
            else return false;   
        }

        private void resetBoard()
        {
            for (int i = 0; i < board.Length; i++) placeThing(i, "", true);
            gameStep = 0;
            playerTurnX = true;
            updatePhysicalBoard();
            toolStripStatusLabel.Text = $"First player is always X";
        }

        private void startGame()
        {
            playerTurnX = true;
            resetBoard();
        }
        private void pvPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playerXisHuman = true;
            playerOisHuman = true;
            startGame();
        }

        private void playerFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playerXisHuman = true;
            playerOisHuman = false;
            startGame();
        }

        private void bOTFirstToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playerXisHuman = false;
            playerOisHuman = true;
            startGame();
        }

        private void bvBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            playerXisHuman = false;
            playerOisHuman = false;
            startGame();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.button_Place2.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place3.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place4.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place5.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place6.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place7.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place8.Click += new System.EventHandler(this.button_Place1_Click);
            this.button_Place9.Click += new System.EventHandler(this.button_Place1_Click);
        }

        private void button_Place1_Click(object sender, EventArgs e)
        {
            string buttonName = (sender as Button).Name;
            //while converting the last letter of the Name into an integer,
            //we must convert it to an string (it was a char) in order to get the integer value instead of an ASCII code number.
            int buttonIndex = Convert.ToInt32(Convert.ToString(buttonName[buttonName.Length-1])) - 1;

            if (playerTurnX && playerXisHuman || !playerTurnX && playerOisHuman)
            {
                //human is placing
                bool legal = placeThing(buttonIndex, playerTurnX ? "X" : "O");
                if (legal)
                {
                    playerTurnX = !playerTurnX;
                    gameStep++;
                    updatePhysicalBoard();
                }
            }
            else
            {
                //BOT is placing
                string currentBotSymbol = playerTurnX ? "X" : "O";
                int moveIndex = botCalculate(currentBotSymbol, currentBotSymbol == "X" ? "O" : "X");
                bool affirmation = placeThing(moveIndex, currentBotSymbol);
                if (affirmation)
                {
                    playerTurnX = !playerTurnX;
                    gameStep++;
                    updatePhysicalBoard();
                }
                
            }
        }

        int minimaxCycles = 0;
        private int botCalculate(string playerSymbol, string enemySymbol)
        {


            //minimax resources
            //https://www.youtube.com/watch?v=trKjYdBASyQ   <-- damn
            //https://www.youtube.com/watch?v=l-hh51ncgDI
            //https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/


            if(botDifficulty == 0)
            {
                int maxTry = 100;
                bool found = false;
                int move = 0;
                while(maxTry > 0 && !found)
                {
                    maxTry++;
                    move = r.Next(9);
                    if (board[move] == "") found = true;
                }
                return move;
            }

            //bot difficulty is 1
            string[] newBoard = board;
            minimaxCycles = 0;

            //root maximizing
            int bestScore = int.MinValue;
            int bestMove = -1;
            for (int i = 0; i < 9; i++)
            {
                if (newBoard[i] == "")
                {
                    newBoard[i] = playerSymbol;
                    int score = Minimax(newBoard, playerSymbol, enemySymbol, false);
                    newBoard[i] = "";


                    if (bestScore < score)
                    {
                        bestScore = score;
                        bestMove = i;
                    }                    
                }
            }

            toolStripStatusLabel.Text = $"Checked:{minimaxCycles}";
            return bestMove;
        }

        private int Minimax(string[] newBoard, string playerSymbol, string enemySymbol, bool isMaximizing, int alpha = int.MinValue, int beta = int.MaxValue)
        {
            //check for terminating condutions
            if (isGameOver(newBoard))
            {
                if (CheckWinnerFor(playerSymbol, newBoard)) return 10;
                if (CheckWinnerFor(enemySymbol, newBoard)) return -10;
                return 0;
            }
            minimaxCycles++;

            //repeat for all available spots
            int bestScore = isMaximizing ? int.MinValue : int.MaxValue; 
            for (int i = 0; i < 9; i++)
            {
                if (newBoard[i] == "")
                {
                    newBoard[i] = isMaximizing ? playerSymbol : enemySymbol;
                    int score = Minimax(newBoard, playerSymbol, enemySymbol, !isMaximizing, alpha, beta);
                    newBoard[i] = "";

                    if (isMaximizing)                                               
                    {
                        bestScore = Math.Max(bestScore, score);
                        alpha = Math.Max(alpha, score);
                    }
                    else
                    {
                        bestScore = Math.Min(bestScore, score);
                        beta= Math.Min(beta, score);
                    }
                    //skip the branches with bad scores compared to the previous ones
                    if(beta <= alpha) break;
                }
            }

            return bestScore;
        }
   
        private bool CheckWinnerFor(string player, string[] theBoard)
        {
            // rows
            for (int i = 0; i < 9; i += 3) if (theBoard[i] == player && theBoard[i + 1] == player && theBoard[i + 2] == player) return true;
            // columns
            for (int i = 0; i < 3; i++) if (theBoard[i] == player && theBoard[i + 3] == player && theBoard[i + 6] == player) return true;
            // diagonals
            if (theBoard[0] == player && theBoard[4] == player && theBoard[8] == player) return true;
            if (theBoard[2] == player && theBoard[4] == player && theBoard[6] == player) return true;
           

            return false;
        }

        private bool isGameOver(string[] theBoard)
        {
            int theGameStep = 0;
            for (int i = 0; i < 9; i++) theGameStep += theBoard[i] != "" ? 1 : 0;

            return CheckWinnerFor("X", theBoard) || CheckWinnerFor("O", theBoard) || theGameStep==9;
        }

        private void resetScoresToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addScoreTo(int.MinValue, "X");
            addScoreTo(int.MinValue, "O");
        }

        private void dumbassToolStripMenuItem_Click(object sender, EventArgs e)
        {
            botDifficulty = 0;
        }

        private void veryGoodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            botDifficulty = 1;
        }
    }
}