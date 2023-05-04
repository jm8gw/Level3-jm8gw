#region File Description
//-----------------------------------------------------------------------------
// Gem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

/* Jeffrey Mouritzen (jm8gw)
 * CS 4730
 * 
 * Reference Credit: Much of this game was designed and built with the help of the Platform2D MonoGame Tutorial/Example game, 
 * which I was told I was allowed to refer to for help.
 * https://github.com/MonoGame/MonoGame.Samples/tree/3.8.1/Platformer2D
 */


namespace Level3_jm8gw
{

    class Banana
    {
        private Texture2D texture;
        private Vector2 origin;
        private SoundEffect collectedSound;


        // The gem is animated from a base position along the Y axis.
        private Vector2 basePosition;
        private float bounce;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        // banana position
        public Vector2 Position
        {
            get
            {
                return basePosition + new Vector2(0.0f, bounce);
            }
        }


        // using a circle helps when distinguishing collisions
        public Circle BoundingCircle
        {
            get
            {
                return new Circle(Position, Tile.Width / 3.0f);
            }
        }


        public Banana(Level level, Vector2 position)
        {
            this.level = level;
            this.basePosition = position;

            LoadContent();
        }

        public void LoadContent()
        {
            texture = Level.Content.Load<Texture2D>("Sprites/Banana");
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);
        }

        // gives the banan that enticing jumping up and down
        public void Update(GameTime gameTime)
        {

            // bounce along a sine curve over time           
            double t = gameTime.TotalGameTime.TotalSeconds * 3.0f + Position.X * -0.75f;
            bounce = (float)Math.Sin(t) * 0.18f * texture.Height;
        }



        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, Position, null, Color.Yellow, 0.0f, origin, 1.0f, SpriteEffects.None, 0.0f);
        }
    }

    struct Circle
    {

        public Vector2 Center;
        public float Radius;

        public Circle(Vector2 position, float radius)
        {
            Center = position;
            Radius = radius;
        }
        // collision logice
        public bool Intersects(Rectangle rectangle)
        {
            Vector2 v = new Vector2(MathHelper.Clamp(Center.X, rectangle.Left, rectangle.Right), MathHelper.Clamp(Center.Y, rectangle.Top, rectangle.Bottom));

            Vector2 direction = Center - v;
            float distanceSquared = direction.LengthSquared();

            return ((distanceSquared > 0) && (distanceSquared < Radius * Radius));
        }
    }
}