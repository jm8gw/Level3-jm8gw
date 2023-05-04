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