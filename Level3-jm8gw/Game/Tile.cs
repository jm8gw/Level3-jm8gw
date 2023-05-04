#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/* Jeffrey Mouritzen (jm8gw)
 * CS 4730
 * 
 * Reference Credit: Much of this game was designed and built with the help of the Platform2D MonoGame Tutorial/Example game, 
 * which I was told I was allowed to refer to for help.
 * https://github.com/MonoGame/MonoGame.Samples/tree/3.8.1/Platformer2D
 */

namespace Level3_jm8gw
{

    enum TileCollision
    {

        Passable = 0, // blocks completely without collisions (collectables)

        Impassable = 1, // blocks with collsions on all sides

        Platform = 2, // blocks with collisions on top only
    }

    struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;

        public const int Width = 40;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        // construct tile
        public Tile(Texture2D texture, TileCollision collision)
        {
            Texture = texture;
            Collision = collision;
        }
    }
}