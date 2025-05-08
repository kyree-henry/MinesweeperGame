using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MinesweeperGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        int rows = 16, cols = 30, cellSize = 35, mineCount = 70;
        bool[,] mines;
        bool[,] revealed;
        int[,] adjacentMines;

        SpriteFont font;
        Texture2D cellTexture;

        MouseState prevMouse;

        bool gameOver = false;
        bool win = false;
        double elapsedTime = 0;
        double highScore = double.MaxValue;
        bool scoreRecorded = false;
        Rectangle resetButtonRect;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = cols * cellSize;
            _graphics.PreferredBackBufferHeight = rows * cellSize + 40;
        }

        protected override void Initialize()
        {
            resetButtonRect = new Rectangle(10, rows * cellSize + 5, 80, 30);
            ResetGame();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("DefaultFont");
            cellTexture = new Texture2D(GraphicsDevice, 1, 1);
            cellTexture.SetData(new[] { Color.White });
        }

        void ResetGame()
        {
            mines = new bool[rows, cols];
            revealed = new bool[rows, cols];
            adjacentMines = new int[rows, cols];
            gameOver = false;
            win = false;
            elapsedTime = 0;
            scoreRecorded = false;
            PlaceMines();
            CalculateAdjacents();
        }

        void PlaceMines()
        {
            var rand = new Random();
            int placed = 0;
            while (placed < mineCount)
            {
                int r = rand.Next(rows);
                int c = rand.Next(cols);
                if (!mines[r, c])
                {
                    mines[r, c] = true;
                    placed++;
                }
            }
        }

        void CalculateAdjacents()
        {
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    if (!mines[r, c])
                        for (int dr = -1; dr <= 1; dr++)
                            for (int dc = -1; dc <= 1; dc++)
                            {
                                int nr = r + dr, nc = c + dc;
                                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols && mines[nr, nc])
                                    adjacentMines[r, c]++;
                            }
        }

        protected override void Update(GameTime gameTime)
        {
            var mouse = Mouse.GetState();

            // Reset button logic
            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released &&
                resetButtonRect.Contains(mouse.Position))
            {
                ResetGame();
            }

            if (!gameOver)
            {
                elapsedTime += gameTime.ElapsedGameTime.TotalSeconds;

                if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                {
                    int col = mouse.X / cellSize;
                    int row = mouse.Y / cellSize;
                    if (row >= 0 && row < rows && col >= 0 && col < cols)
                    {
                        if (mines[row, col])
                        {
                            // Reveal mines and end
                            for (int r = 0; r < rows; r++)
                                for (int c = 0; c < cols; c++)
                                    if (mines[r, c]) revealed[r, c] = true;
                            gameOver = true;
                            win = false;
                        }
                        else
                        {
                            RevealCell(row, col);
                            int count = 0;
                            for (int r = 0; r < rows; r++)
                                for (int c = 0; c < cols; c++)
                                    if (revealed[r, c]) count++;
                            if (count == rows * cols - mineCount)
                            {
                                gameOver = true;
                                win = true;
                            }
                        }
                    }
                }

                // Record high score when game ends
                if (gameOver && !scoreRecorded)
                {
                    if (elapsedTime < highScore)
                        highScore = elapsedTime;
                    scoreRecorded = true;
                }
            }

            prevMouse = mouse;
            base.Update(gameTime);
        }

        void RevealCell(int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= cols || revealed[row, col])
                return;

            revealed[row, col] = true;
            if (adjacentMines[row, col] == 0)
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                        if (!(dr == 0 && dc == 0)) RevealCell(row + dr, col + dc);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            // Draw grid
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var rect = new Rectangle(c * cellSize, r * cellSize, cellSize - 2, cellSize - 2);
                    var color = revealed[r, c] ? Color.LightGray : Color.Gray;
                    _spriteBatch.Draw(cellTexture, rect, color);

                    if (revealed[r, c])
                    {
                        if (mines[r, c])
                            _spriteBatch.DrawString(font, "*", new Vector2(c * cellSize + 10, r * cellSize + 5), Color.Red);
                        else if (adjacentMines[r, c] > 0)
                            _spriteBatch.DrawString(font, adjacentMines[r, c].ToString(), new Vector2(c * cellSize + 10, r * cellSize + 5), Color.Black);
                    }
                }

            // Draw UI background
            var uiBg = new Rectangle(0, rows * cellSize, cols * cellSize, 40);
            _spriteBatch.Draw(cellTexture, uiBg, Color.DarkSlateGray);

            // Draw reset button and timers
            _spriteBatch.Draw(cellTexture, resetButtonRect, Color.Bisque);
            _spriteBatch.DrawString(font, "Reset", new Vector2(resetButtonRect.X + 10, resetButtonRect.Y + 7), Color.Black);
            _spriteBatch.DrawString(font, $"Time: {elapsedTime:F1}s", new Vector2(100, rows * cellSize + 10), Color.White);
            _spriteBatch.DrawString(font, highScore < double.MaxValue ? $"Best: {highScore:F1}s" : "Best: --",
                new Vector2(250, rows * cellSize + 10), Color.White);

            // Draw game over overlay
            if (gameOver)
            {
                var overlay = new Rectangle(0, 0, cols * cellSize, rows * cellSize);
                _spriteBatch.Draw(cellTexture, overlay, Color.Black * 0.5f);
                string msg = win ? "You Win!" : "Game Over!";
                var size = font.MeasureString(msg);
                var pos = new Vector2((cols * cellSize - size.X) / 2, (rows * cellSize - size.Y) / 2);
                _spriteBatch.DrawString(font, msg, pos, Color.Yellow);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
