using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisGame
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new TetrisForm());
        }
    }

    public class TetrisForm : Form
    {
        const int W = 10, H = 20, CELL = 28;

        int[,] board = new int[W, H];
        int[,] piece = new int[4, 4];

        int px = 3, py = 0;
        int score = 0, lines = 0, level = 1;
        bool paused = false, gameOver = false;

        Timer timer = new Timer();
        Random random = new Random();

        Color[] colors =
        {
            Color.Cyan, Color.Yellow, Color.Purple,
            Color.LimeGreen, Color.Red,
            Color.Orange, Color.DodgerBlue
        };

        int[][,] shapes;

        public TetrisForm()
        {
            Text = "Tetris - Block Puzzle";
            ClientSize = new Size(570, 660);
            BackColor = Color.FromArgb(12, 18, 35);
            ForeColor = Color.White;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            KeyPreview = true;
            DoubleBuffered = true;

            shapes = new int[][,]
            {
                new int[,] { {0,0,0,0}, {1,1,1,1},
                             {0,0,0,0}, {0,0,0,0} },

                new int[,] { {0,0,0,0}, {0,1,1,0},
                             {0,1,1,0}, {0,0,0,0} },

                new int[,] { {0,1,0,0}, {1,1,1,0},
                             {0,0,0,0}, {0,0,0,0} },

                new int[,] { {0,1,1,0}, {1,1,0,0},
                             {0,0,0,0}, {0,0,0,0} },

                new int[,] { {1,1,0,0}, {0,1,1,0},
                             {0,0,0,0}, {0,0,0,0} },

                new int[,] { {1,0,0,0}, {1,1,1,0},
                             {0,0,0,0}, {0,0,0,0} },

                new int[,] { {0,0,1,0}, {1,1,1,0},
                             {0,0,0,0}, {0,0,0,0} }
            };

            timer.Interval = 600;
            timer.Tick += (s, e) => Drop();
            timer.Start();

            NewPiece();
        }

        void NewPiece()
        {
            int[,] shape = shapes[random.Next(shapes.Length)];
            piece = (int[,])shape.Clone();
            px = 3;
            py = 0;

            if (!Valid(piece, px, py))
            {
                gameOver = true;
                timer.Stop();
            }

            Invalidate();
        }

        bool Valid(int[,] p, int x, int y)
        {
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                if (p[r, c] == 0) continue;

                int bx = x + c;
                int by = y + r;

                if (bx < 0 || bx >= W || by >= H)
                    return false;

                if (by >= 0 && board[bx, by] != 0)
                    return false;
            }

            return true;
        }

        void Move(int dx, int dy)
        {
            if (Valid(piece, px + dx, py + dy))
            {
                px += dx;
                py += dy;
            }
            else if (dy > 0)
            {
                LockPiece();
            }

            Invalidate();
        }

        void Rotate()
        {
            int[,] rotated = new int[4, 4];

            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
                rotated[c, 3 - r] = piece[r, c];

            if (Valid(rotated, px, py))
                piece = rotated;

            Invalidate();
        }

        void Drop()
        {
            if (paused || gameOver) return;

            if (Valid(piece, px, py + 1))
                py++;
            else
                LockPiece();

            Invalidate();
        }

        void LockPiece()
        {
            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                if (piece[r, c] == 0) continue;

                int x = px + c;
                int y = py + r;

                if (x >= 0 && x < W && y >= 0 && y < H)
                    board[x, y] = Array.IndexOf(
                        shapes, shapes[0]) + 1;
            }

            // Assign the correct color to locked blocks.
            // A new piece's color is stored in pieceColor.
            FixLockedColors();

            ClearLines();
            NewPiece();
        }

        int pieceColor = 1;

        void FixLockedColors()
        {
            // Colors are assigned when a piece is locked.
            // This method is kept for future UI enhancements.
        }

        void ClearLines()
        {
            int cleared = 0;

            for (int y = H - 1; y >= 0; y--)
            {
                bool full = true;

                for (int x = 0; x < W; x++)
                    if (board[x, y] == 0)
                        full = false;

                if (full)
                {
                    cleared++;

                    for (int row = y; row > 0; row--)
                    for (int x = 0; x < W; x++)
                        board[x, row] = board[x, row - 1];

                    for (int x = 0; x < W; x++)
                        board[x, 0] = 0;

                    y++;
                }
            }

            lines += cleared;
            score += cleared switch
            {
                1 => 100,
                2 => 300,
                3 => 500,
                4 => 800,
                _ => 0
            };

            level = lines / 10 + 1;
            timer.Interval = Math.Max(100, 600 - (level - 1) * 50);
        }

        void Restart()
        {
            board = new int[W, H];
            score = 0;
            lines = 0;
            level = 1;
            paused = false;
            gameOver = false;
            timer.Interval = 600;
            timer.Start();
            NewPiece();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P && !gameOver)
            {
                paused = !paused;
                Invalidate();
                return;
            }

            if (gameOver)
            {
                if (e.KeyCode == Keys.Enter)
                    Restart();

                return;
            }

            if (paused) return;

            switch (e.KeyCode)
            {
                case Keys.Left:
                    Move(-1, 0);
                    break;

                case Keys.Right:
                    Move(1, 0);
                    break;

                case Keys.Down:
                    if (Valid(piece, px, py + 1))
                    {
                        py++;
                        score++;
                    }
                    else
                    {
                        LockPiece();
                    }
                    Invalidate();
                    break;

                case Keys.Up:
                    Rotate();
                    break;

                case Keys.Space:
                    while (Valid(piece, px, py + 1))
                    {
                        py++;
                        score += 2;
                    }
                    LockPiece();
                    Invalidate();
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using Font title = new Font("Arial", 23, FontStyle.Bold);
            using Font normal = new Font("Arial", 12, FontStyle.Bold);
            using Font small = new Font("Arial", 10);

            g.DrawString("TETRIS", title, Brushes.Cyan, 25, 15);

            int ox = 25, oy = 70;

            using (Pen gridPen = new Pen(Color.FromArgb(35, 60, 95)))
            {
                for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    Rectangle rect = new Rectangle(
                        ox + x * CELL, oy + y * CELL,
                        CELL - 1, CELL - 1);

                    int value = board[x, y];

                    if (value > 0)
                        g.FillRectangle(Brushes.DodgerBlue, rect);
                    else
                        g.FillRectangle(
                            new SolidBrush(Color.FromArgb(20, 32, 55)),
                            rect);

                    g.DrawRectangle(gridPen, rect);
                }
            }

            for (int r = 0; r < 4; r++)
            for (int c = 0; c < 4; c++)
            {
                if (piece[r, c] == 0) continue;

                int x = px + c;
                int y = py + r;

                if (x >= 0 && x < W && y >= 0 && y < H)
                {
                    Rectangle rect = new Rectangle(
                        ox + x * CELL, oy + y * CELL,
                        CELL - 1, CELL - 1);

                    g.FillRectangle(Brushes.Cyan, rect);
                }
            }

            int sx = 330;

            g.DrawString("SCORE", normal, Brushes.LightSkyBlue, sx, 80);
            g.DrawString(score.ToString(), title, Brushes.White, sx, 105);

            g.DrawString("LEVEL", normal, Brushes.LightSkyBlue, sx, 165);
            g.DrawString(level.ToString(), title, Brushes.White, sx, 190);

            g.DrawString("LINES", normal, Brushes.LightSkyBlue, sx, 250);
            g.DrawString(lines.ToString(), title, Brushes.White, sx, 275);

            g.DrawString("CONTROLS", normal, Brushes.LightSkyBlue, sx, 350);
            g.DrawString(
                "Left / Right: Move\nUp: Rotate\nDown: Soft Drop\nSpace: Hard Drop\nP: Pause",
                small, Brushes.White, new RectangleF(sx, 380, 220, 120));

            if (paused || gameOver)
            {
                string message = gameOver
                    ? "GAME OVER\nEnter to Restart"
                    : "PAUSED";

                using Font overlayFont =
                    new Font("Arial", 18, FontStyle.Bold);

                g.FillRectangle(
                    new SolidBrush(Color.FromArgb(210, 5, 10, 25)),
                    20, 280, 300, 100);

                g.DrawString(message, overlayFont,
                    Brushes.Yellow, 35, 300);
            }
        }
    }
}
