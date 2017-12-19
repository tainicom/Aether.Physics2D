/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.MediaSystem
{
    public class Sprite
    {
        public readonly Texture2D Texture;
        public readonly Vector2 Size;
        public readonly Vector2 TexelSize;
        public Vector2 Origin;
                
        public Sprite(Texture2D texture, Vector2 origin)
        {
            Texture = texture;
            Size = new Vector2(Texture.Width, Texture.Height);
            TexelSize = Vector2.One / Size;
            Origin = origin;
        }

        public Sprite(Texture2D texture)
        {
            Texture = texture;
            Size = new Vector2(Texture.Width, Texture.Height);
            TexelSize = Vector2.One / Size;
            Origin = Size / 2f;
        }
    }
}