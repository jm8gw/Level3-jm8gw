using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Reflection.Emit;
using System;
using System.IO;
using System.Drawing;

/* Jeffrey Mouritzen (jm8gw)
 * CS 4730
 * 
 * Reference Credit: Much of this game was designed and built with the help of the Platform2D MonoGame Tutorial/Example game, 
 * which I was told I was allowed to refer to for help.
 * https://github.com/MonoGame/MonoGame.Samples/tree/3.8.1/Platformer2D
 */


namespace Level3_jm8gw
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        Vector2 baseScreenSize = new Vector2(800, 480);
        int backbufferWidth, backbufferHeight;
        private Matrix globalTransformation;


        private const int numberOfLevels = 3;

        private KeyboardState keyboardState;


        private int levelIndex = -1;
        private Level level;
        private bool wasContinuePressed;
        private SpriteFont defaultFont;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.IsFullScreen = false;


            _graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
            
        }


        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            this.Content.RootDirectory = "Content";

            defaultFont = Content.Load<SpriteFont>("DefaultFont");

            ScalePresentationArea();

            LoadNextLevel();
        }

        public void ScalePresentationArea()
        {
            //Work out how much we need to scale our graphics to fill the screen
            backbufferWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            backbufferHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
            float horScaling = backbufferWidth / baseScreenSize.X;
            float verScaling = backbufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
            System.Diagnostics.Debug.WriteLine("Screen Size - Width[" + GraphicsDevice.PresentationParameters.BackBufferWidth + "] Height [" + GraphicsDevice.PresentationParameters.BackBufferHeight + "]");
        }




        protected override void Update(GameTime gameTime)
        {
            if (backbufferHeight != GraphicsDevice.PresentationParameters.BackBufferHeight ||
                backbufferWidth != GraphicsDevice.PresentationParameters.BackBufferWidth)
            {
                ScalePresentationArea();
            }

            HandleInput(gameTime);


            level.Update(gameTime, keyboardState, Window.CurrentOrientation);


            base.Update(gameTime);
        }


        private void HandleInput(GameTime gameTime)
        {
            // get input states
            keyboardState = Keyboard.GetState();

            bool continuePressed = keyboardState.IsKeyDown(Keys.Space);


            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive){
                    level.StartNewLife();
                }
                else if (level.ReachedExit) { 
                    LoadNextLevel();
                }
            }

            wasContinuePressed = continuePressed;

        }


        private void LoadNextLevel()
        {
            
            levelIndex = (levelIndex + 1) % numberOfLevels;

            // Unload the content for the current level before loading next
            if (level != null)
                level.Dispose();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex); 
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex);
        }


        private void ReloadCurrentLevel()
        {
            levelIndex -= 1; //--levelIndex
            LoadNextLevel();
        }

        
        
        


        protected override void Draw(GameTime gameTime)
        {
            _graphics.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, globalTransformation);

            // TODO: Add your drawing code here

            level.Draw(gameTime, _spriteBatch);

            _spriteBatch.DrawString(defaultFont, "BANANAS: " + level.BananaCount.ToString(), new Vector2(20, 20), Microsoft.Xna.Framework.Color.Black);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}