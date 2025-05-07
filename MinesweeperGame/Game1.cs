using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MinesweeperGame
{
    public class Game1 : Game
    {
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        int rows = 10, cols = 10, cellSize = 40, mineCount = 15;
        bool[,] mines;
        bool[,] revealed;
        int[,] adjacentMines;
        SpriteFont font;
        Texture2D cellTexture;
        MouseState prevMouse;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 400;
            _graphics.PreferredBackBufferHeight = 400;
        }

        protected override void Initialize()
        {
            mines = new bool[rows, cols];
            revealed = new bool[rows, cols];
            adjacentMines = new int[rows, cols];
            PlaceMines();
            CalculateAdjacents();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("DefaultFont");
            cellTexture = new Texture2D(GraphicsDevice, 1, 1);
            cellTexture.SetData(new[] { Color.White });
        }

        void PlaceMines()
        {
            var rand = new System.Random();
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
            MouseState mouse = Mouse.GetState();

            if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
            {
                int col = mouse.X / cellSize;
                int row = mouse.Y / cellSize;
                if (row >= 0 && row < rows && col >= 0 && col < cols)
                {
                    if (mines[row, col])
                    {
                        // Reveal all mines
                        for (int r = 0; r < rows; r++)
                            for (int c = 0; c < cols; c++)
                                if (mines[r, c]) revealed[r, c] = true;
                    }
                    else
                    {
                        RevealCell(row, col);
                    }
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
            {
                for (int dr = -1; dr <= 1; dr++)
                    for (int dc = -1; dc <= 1; dc++)
                        if (!(dr == 0 && dc == 0))
                            RevealCell(row + dr, col + dc);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    Rectangle rect = new Rectangle(c * cellSize, r * cellSize, cellSize - 2, cellSize - 2);
                    Color color = revealed[r, c] ? Color.LightGray : Color.Gray;
                    _spriteBatch.Draw(cellTexture, rect, color);

                    if (revealed[r, c])
                    {
                        if (mines[r, c])
                        {
                            _spriteBatch.DrawString(font, "*", new Vector2(c * cellSize + 10, r * cellSize + 5), Color.Red);
                        }
                        else if (adjacentMines[r, c] > 0)
                        {
                            _spriteBatch.DrawString(font, adjacentMines[r, c].ToString(), new Vector2(c * cellSize + 10, r * cellSize + 5), Color.Black);
                        }
                    }
                }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
