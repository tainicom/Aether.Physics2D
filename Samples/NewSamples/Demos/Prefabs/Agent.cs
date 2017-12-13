/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Agent
    {
        private Body _agentBody;
        private Sprite _box;
        private Sprite _knob;
        private Category _collidesWith;
        private Category _collisionCategories;
        private float _offset;

        public Agent(World world, Vector2 position)
        {
            _collidesWith = Category.All;
            _collisionCategories = Category.All;

            _agentBody = world.CreateBody(position);
            _agentBody.BodyType = BodyType.Dynamic;

            //Center
            _agentBody.CreateCircle(0.5f, 0.5f);

            //Left arm
            _agentBody.CreateRectangle(1.5f, 0.4f, 1f, new Vector2(-1f, 0f));
            _agentBody.CreateCircle(0.5f, 0.5f, new Vector2(-2f, 0f));

            //Right arm
            _agentBody.CreateRectangle(1.5f, 0.4f, 1f, new Vector2(1f, 0f));
            _agentBody.CreateCircle(0.5f, 0.5f, new Vector2(2f, 0f));

            //Top arm
            _agentBody.CreateRectangle(0.4f, 1.5f, 1f, new Vector2(0f, 1f));
            _agentBody.CreateCircle(0.5f, 0.5f, new Vector2(0f, 2f));

            //Bottom arm
            _agentBody.CreateRectangle(0.4f, 1.5f, 1f, new Vector2(0f, -1f));
            _agentBody.CreateCircle(0.5f, 0.5f, new Vector2(0f, -2f));

            //GFX
            _box = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateRectangle(1.75f, 0.2f), Color.White, ContentWrapper.Black));
            _knob = new Sprite(ContentWrapper.CircleTexture(0.5f, "Square", ContentWrapper.Black, ContentWrapper.Gold, ContentWrapper.Black, 1f));

            _offset = ConvertUnits.ToDisplayUnits(2f);
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;
                Body.SetCollisionCategories(value);
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;
                Body.SetCollidesWith(value);
            }
        }

        public Body Body
        {
            get { return _agentBody; }
        }

        public void Draw(SpriteBatch batch)
        {
            //cross
            batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _box.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_box.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation + MathHelper.Pi / 2f, _box.Origin, 1f, SpriteEffects.None, 0f);
            
            //knobs
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _knob.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _knob.Origin + new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _knob.Origin - new Vector2(0f, _offset), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _knob.Origin + new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
            batch.Draw(_knob.Image, ConvertUnits.ToDisplayUnits(_agentBody.Position), null, Color.White, _agentBody.Rotation, _knob.Origin - new Vector2(_offset, 0f), 1f, SpriteEffects.None, 0f);
        }
    }
}