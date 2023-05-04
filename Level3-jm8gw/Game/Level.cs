using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Microsoft.Xna.Framework.Input;

namespace Level3_jm8gw
{

    class Level : IDisposable
    {
        // Structure of levels
        private Tile[,] tiles;
        private Texture2D[] layers;
        // entities drawn on top of layer
        private const int EntityLayer = 0;

        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Banana> bananas = new List<Banana>();

        public int BananaCount
        {
            get { return bananaCount; }
        }
        int bananaCount;

        // locations        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);


        // level game state (arbitrary)
        private Random random = new Random(866453);

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        // level constructor
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {

            content = new ContentManager(serviceProvider, "Content");

            LoadTiles(fileStream);

            // Load background layer textures
            //layers = new Texture2D[3];
            layers = new Texture2D[1];
            for (int i = 0; i < layers.Length; ++i)
            {
                // for level variety
                int segmentIndex = levelIndex;
                layers[i] = Content.Load<Texture2D>("Backgrounds/Layer" + i + "_" + segmentIndex);
            }

        }


        private void LoadTiles(Stream fileStream)
        {
            // Load the level
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }

            // tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // load each tile.
                    char tileType = lines[y][x];

                    if (tileType == '.')
                    {
                        tiles[x, y] = new Tile(null, TileCollision.Passable);
                    }
                    else if (tileType == 'X') // endpoint
                    {
                        tiles[x, y] = LoadExitTile(x, y);
                    }
                    else if (tileType == 'B') //  collectable
                    {
                        tiles[x, y] = LoadBanana(x, y);
                    }
                    else if (tileType == '-') // basic floating platform
                    {
                        tiles[x, y] = LoadTile("PlatformBasic", TileCollision.Platform);
                    }
                    else if (tileType == '~') // platform (purple)
                    {
                        tiles[x, y] = LoadTile("PurpleTile", TileCollision.Platform);
                    }
                    else if (tileType == ':') // passable (green)
                    {
                        tiles[x, y] = LoadTile("GreenTile", TileCollision.Passable);
                    }
                    else if (tileType == '1') // start point
                    {
                        tiles[x, y] = LoadStartTile(x, y);
                    }
                    else if (tileType == '#') // impassable (red)
                    {
                        tiles[x, y] = LoadTile("RedTile", TileCollision.Impassable);
                    }
                    else
                    {
                        throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
                    }
                }
            }
        }


        private Tile LoadTile(string name, TileCollision collision)
        {

            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }

        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        private Tile LoadStartTile(int x, int y)
        {
            start = new Vector2(GetBounds(x, y).X + GetBounds(x, y).Width / 2.0f, GetBounds(x, y).Bottom);

            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadExitTile(int x, int y)
        {
            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        private Tile LoadBanana(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            bananas.Add(new Banana(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        // unload level content
        public void Dispose()
        {
            Content.Unload();
        }

        // get collision information
        public TileCollision GetCollision(int x, int y)
        {
            // cannot escape past the level
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Except for top and bottom
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }




        public void Update(
                GameTime gameTime,
                KeyboardState keyboardState,
                DisplayOrientation orientation)
        {
            // Pause while the player is dead
            if (!Player.IsAlive)
            {
                // Still want to perform physics
                Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // some logic here idc
            }
            else
            {
                Player.Update(gameTime, keyboardState, orientation);
                UpdateBananas(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    Player.OnKilled();



                // The player has reached the exit
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    reachedExit = true;
                }
            }


        }

        // for collectables
        private void UpdateBananas(GameTime gameTime)
        {
            for (int i = 0; i < bananas.Count; ++i)
            {
                Banana banana = bananas[i];
        
                banana.Update(gameTime);
        
                if (banana.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    bananas.RemoveAt(i--);
                    bananaCount += 1;
                }
            }
        }

        public void StartNewLife()
        {
            Player.Reset(start);
        }


        // Draw EVERYTHING
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            for (int i = 0; i <= EntityLayer; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);

            DrawTiles(spriteBatch);

            foreach (Banana banana in bananas)
                banana.Draw(gameTime, spriteBatch);

            Player.Draw(gameTime, spriteBatch);

            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
        }

        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile pos
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If tiles are visible
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }


    }
}
