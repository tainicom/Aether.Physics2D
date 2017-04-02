/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.DrawingSystem
{
    public struct Sprite
    {
        public Vector2 Origin;
        public Texture2D Texture;

        public Sprite(Texture2D texture, Vector2 origin)
        {
            Texture = texture;
            Origin = origin;
        }

        public Sprite(Texture2D sprite)
        {
            Texture = sprite;
            Origin = new Vector2(sprite.Width / 2f, sprite.Height / 2f);
        }
    }
}