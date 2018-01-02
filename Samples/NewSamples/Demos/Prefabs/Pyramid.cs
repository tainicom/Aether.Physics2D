/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Pyramid
    {
        private Sprite _box;
        private List<Body> _boxes;

        public Pyramid(World world, Vector2 position, int count, float density)
        {
            Vertices rect = PolygonTools.CreateRectangle(1f/2f, 1f/2f);
            PolygonShape shape = new PolygonShape(rect, density);

            Vector2 rowStart = position;
            rowStart.Y -= 0.5f + count * 1.1f;

            Vector2 deltaRow = new Vector2(-0.625f, -1.1f);
            const float spacing = 1.25f;

            // Physics
            _boxes = new List<Body>();

            for (int i = 0; i < count; i++)
            {
                Vector2 pos = rowStart;

                for (int j = 0; j < i + 1; j++)
                {
                    Body body = world.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = pos;
                    body.CreateFixture(shape);
                    _boxes.Add(body);

                    pos.X += spacing;
                }
                rowStart += deltaRow;
            }

            //GFX
            _box = new Sprite(ContentWrapper.PolygonTexture(rect, "Square", ContentWrapper.Blue, ContentWrapper.Gold, ContentWrapper.Black, 1f, 24f));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Body body in _boxes)
            {
                batch.Draw(_box.Texture, body.Position, null, Color.White, body.Rotation, _box.Origin, new Vector2(1f, 1f) * _box.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
        }
    }
}