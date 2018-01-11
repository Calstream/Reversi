using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace reversisharp
{
    public partial class GameWindow : Form
    {
        PictureBox[] board;
        bool humans_turn = false;
        List<possible_move> frontier;
        reversi game;
        public GameWindow()
        {
            InitializeComponent();
            frontier = new List<possible_move>();
            board = new PictureBox[64];
            panel1.BackColor = Color.Black;

			//List<int> b = new List<int>{ 1, 2,3,4,11,16, 18,19,24,25, 26,27,30,32, 35 };
			//List<int> w = new List<int> { 0,8,9,10,12,13,17,20,21,28,29, 36, 37 };
			//game = new reversi(b,w);
            
			for (int i = 0; i < 64; ++i)
			{
				board[i] = new PictureBox();
				board[i].Parent = panel1;
				board[i].Size = new Size(50, 50);
				int x = i / 8;
				int y = i % 8;
				board[i].Location = new Point( y * 50 + y + 1, x * 50 + x + 1);
				board[i].Click += cellClick;
				board[i].BackColor = Color.Azure;
				board[i].Tag = i;
			}
		}

		

		private void reset()
        {
            game = new reversi();
            computer_score.Text = game.black.Count.ToString();
            your_score.Text = game.white.Count.ToString();
            for (int i = 0; i < 64; ++i)
			{
				board[i].BackgroundImage = null;

				///////////////////////////////////////////////////////////////////////////
				//board[i].Image = new Bitmap(board[i].Width, board[i].Height);
				//using (Font myFont = new Font("Arial", 14))
				//{
				//	Bitmap b = board[i].Image as Bitmap;
				//	Graphics g = Graphics.FromImage(b);
				//	g.DrawString(board[i].Tag.ToString(), myFont, Brushes.Green, new Point(2, 2));
				//}
				////////////////////////////////////////////////////////////////////////////

			}
			foreach(var x in game.black)			
				board[x].BackgroundImage = Properties.Resources.black;
			foreach (var x in game.white)
				board[x].BackgroundImage = Properties.Resources.white;
			
			for (int i = 0; i < 64; ++i)
                board[i].Refresh();
            computer_score.Text = "2";
            your_score.Text = "2";
            turn.Text = "";
        }

        private void cellClick(object sender, EventArgs e)
        {
            if (!humans_turn)
                return;
            PictureBox p = sender as PictureBox;
            int tag = (int)p.Tag;

            
            

            if (frontier.Find(f => f.pos == tag).flipped == null) // not a valid move
                return;
            foreach (var x in frontier)
            {
                board[x.pos].BackgroundImage = null;
                board[x.pos].Refresh();
            }
            game = game.make_move(frontier.Find(f => f.pos == tag));
            display_state();
            computer_score.Text = game.black.Count.ToString();
            your_score.Text = game.white.Count.ToString();

            Computer:

            humans_turn = false;
            turn.Text = "Computer's turn!";
            turn.ForeColor = Color.Maroon;
            turn.Invalidate();
            turn.Update();
            computer_move();
            display_state();
            computer_score.Text = game.black.Count.ToString();
            your_score.Text = game.white.Count.ToString();

            frontier = game.get_frontier(true);
            if (frontier == null)
            {
                if (game.game_over())
                {
                    gameover();
                    return;
                }
                MessageBox.Show("you have no moves"); //you have no moves
                
                goto Computer;
            }
            foreach (var x in frontier)
                board[x.pos].BackgroundImage = Properties.Resources.hint;
            humans_turn = true;
            turn.Text = "Your turn!";
            turn.ForeColor = Color.Green;
            
        }

        private void gameover()
        {
            int y;
            int c;
            Int32.TryParse(your_score.Text, out y);
            Int32.TryParse(computer_score.Text, out c);
            if (c > y)
            {
                MessageBox.Show("YOU LOST");
                turn.Text = "YOU LOST";
                turn.ForeColor = Color.Maroon;
            }
            else if (c < y)
            {
                MessageBox.Show("YOU WON");
                turn.Text = "YOU WON";
                turn.ForeColor = Color.Green;
            }
            else 
            {
                MessageBox.Show("It's a draw");
                turn.Text = "It's a draw";
                turn.ForeColor = Color.Blue;
            }
        }

        private void computer_move()
        {
            frontier = game.get_frontier(false);
            if (frontier == null)
            {
                    if (game.game_over())
                    {
                        gameover();
                        return;
                    }
				MessageBox.Show("computer has no moves"); // computer has no moves
				return;
			}

            possible_move m = game.ai_make_move((int)ab_depth.Value); 
            board[m.pos].BackgroundImage = Properties.Resources.hint;
            board[m.pos].Refresh();
            System.Threading.Thread.Sleep(100);
            game = game.make_move(m);
            display_state();
            computer_score.Text = game.black.Count.ToString();
            your_score.Text = game.white.Count.ToString();
        }

        private void start_Click(object sender, EventArgs e)
        {
			reset();
			frontier = game.get_frontier(true);
			foreach (var x in frontier) 
				board[x.pos].BackgroundImage = Properties.Resources.hint;
			humans_turn = true;
			turn.Text = "Your turn!";
			turn.ForeColor = Color.Green;
		}

        private void display_state()
        {
            for (int i = 0; i < 64; ++i)
            {
                if (game.state[i] == 0)
                {
                    board[i].BackgroundImage = Properties.Resources.black;
                    board[i].Refresh();
                }
                else if (game.state[i] == 1)
                {
                    board[i].BackgroundImage = Properties.Resources.white;
                    board[i].Refresh();
                }
            }
        }
    }
}
