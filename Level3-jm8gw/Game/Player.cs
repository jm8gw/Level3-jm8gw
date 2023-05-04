using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


/* Jeffrey Mouritzen (jm8gw)
 * CS 4730
 * 
 * Reference Credit: Much of this game was designed and built with the help of the Platform2D MonoGame Tutorial/Example game, 
 * which I was told I was allowed to refer to for help.
 * https://github.com/MonoGame/MonoGame.Samples/tree/3.8.1/Platformer2D
 */


namespace Level3_jm8gw
{
    class Player
    {

        //private Rectangle sprite;



        public Level Level
        {
            get { return level; }
        }
        Level level;


        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;


        // physics info
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        private float previousBottom;
        private Rectangle localBounds;
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        // movement input
        private float movement;

        // jumping?
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;


        // movement constants
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f;
        // shoutout to platformer2d for really getting the physics down nicely


        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        // bounds of player (for collisions)
        public Rectangle BoundingRectangle
        {
            get
            {
                Vector2 Origin = new Vector2(Texture.Width / 2.0f, Texture.Height);
                int left = (int)Math.Round(Position.X - Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }


        public Vector2 GetIntersectionDepth(Rectangle rectA, Rectangle rectB)
        {
            // half sizes
            float halfWidthA = rectA.Width / 2.0f;
            float halfHeightA = rectA.Height / 2.0f;
            float halfWidthB = rectB.Width / 2.0f;
            float halfHeightB = rectB.Height / 2.0f;

            // centers.
            Vector2 centerA = new Vector2(rectA.Left + halfWidthA, rectA.Top + halfHeightA);
            Vector2 centerB = new Vector2(rectB.Left + halfWidthB, rectB.Top + halfHeightB);

            // current and minimum-non-intersecting distances between centers
            float distanceX = centerA.X - centerB.X;
            float distanceY = centerA.Y - centerB.Y;
            float minDistanceX = halfWidthA + halfWidthB;
            float minDistanceY = halfHeightA + halfHeightB;

            // of not intersecting at all, return (0, 0).
            if (Math.Abs(distanceX) >= minDistanceX || Math.Abs(distanceY) >= minDistanceY)
                return new Vector2(0, 0);

            // intersection depths.
            float depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
            float depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;
            return new Vector2(depthX, depthY);
        }







        // player constructor
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
        }

        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; }
        }
        Texture2D texture;

        private SpriteEffects flip = SpriteEffects.None;

        public void LoadContent()
        {
            // no animations this time, I am not going through that again

             
            Texture = Level.Content.Load<Texture2D>("Sprites/Exit");

            // Calculate bounds within texture size.            
            int width = (int)(Texture.Height * 0.4);
            int left = (Texture.Width - width) / 2;
            int height = (int)(Texture.Height * 0.8);
            int top = Texture.Height - height;
            localBounds = new Rectangle(left, top, width, height);


        }



        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, orientation);

            ApplyPhysics(gameTime);

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }


        private void GetInput(
            KeyboardState keyboardState,
            DisplayOrientation orientation)
        {

            // If any digital horizontal movement input is found, override the analog movement.
            if (keyboardState.IsKeyDown(Keys.A))
            {
                movement = -1.0f;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                movement = 1.0f;
            }

            isJumping = keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.W);
        }

        public void OnKilled()
        {
            isAlive = false;
        }
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // velocity caluculation
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // ground drag
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // stop at top speed           
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            //  Collision physics
            HandleCollisions();

            // reset the velocity to zero if collision stopped player
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }


        private float DoJump(float velocityY, GameTime gameTime)
        {
            // jump desired
            if (isJumping)
            {
                // begin or continue jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                }

                // during ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // not jumping or jump cancelled
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }


        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }


        public void Draw(GameTime gameTime, SpriteBatch _spriteBatch)
        {

            // Flip the sprite
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.

            Vector2 Origin = new Vector2(Texture.Width / 2.0f, Texture.Height);

            Rectangle source = new Rectangle(Texture.Height, 0, Texture.Height, Texture.Height);

            _spriteBatch.Draw(Texture, Position, source, Color.White, 0.0f, Origin, 1.0f, flip, 0.0f);
        }

    }
}
