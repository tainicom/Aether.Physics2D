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
        public Vector2 Origin { get; set; }

        private Texture2D _image;
        public Texture2D Image
        {
            get { return _image; }
            set
            {
                _image = value;
                Origin = new Vector2(_image.Width / 2f, _image.Height / 2f);
            }
        }

        public Sprite(Texture2D image, Vector2 origin)
        {
            _image = image;
            Origin = origin;
        }

        public Sprite(Texture2D image)
        {
            Image = image;
        }
    }
}