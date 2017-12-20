/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D07_Friction : PhysicsDemoScreen
    {
        private Border _border;
        private Body _ramps;
        private Body[] _rectangle = new Body[5];
        private Sprite _rectangleSprite;

        #region Demo description
        public override string GetTitle()
        {
            return "Friction";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies with varying friction.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.Append("  - Drag grabbed object: Move mouse");
#endif
            return sb.ToString();
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -20f);

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            _ramps = World.CreateBody();
            _ramps.CreateEdge(new Vector2(-20f, 11.2f), new Vector2(10f, 3.8f));
            _ramps.CreateEdge(new Vector2(12f, 5.6f), new Vector2(12f, 3.2f));

            _ramps.CreateEdge(new Vector2(-10f, -4.4f), new Vector2(20f, 1.4f));
            _ramps.CreateEdge(new Vector2(-12f, -2.6f), new Vector2(-12f, -5f));

            _ramps.CreateEdge(new Vector2(-20f, -6.8f), new Vector2(10f, -11.5f));

            float[] friction = { 0.75f, 0.45f, 0.28f, 0.17f, 0.0f };
            for (int i = 0; i < 5; i++)
            {
                _rectangle[i] = World.CreateRectangle(1.5f, 1.5f, 1f);
                _rectangle[i].BodyType = BodyType.Dynamic;
                _rectangle[i].Position = new Vector2(-18f + 5.2f * i, 13.0f - 1.282f * i);
                _rectangle[i].SetFriction(friction[i]);
            }

            // create sprite based on body
            _rectangleSprite = new Sprite(ContentWrapper.TextureFromShape(_rectangle[0].FixtureList[0].Shape, "Square", ContentWrapper.Green, ContentWrapper.Lime, ContentWrapper.Black, 1f, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);

            for (int i = 0; i < 5; ++i)
            {
                SpriteBatch.Draw(_rectangleSprite.Texture, _rectangle[i].Position, null,
                             Color.White, _rectangle[i].Rotation, _rectangleSprite.Origin, new Vector2(1.5f, 1.5f) * _rectangleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
            }

            SpriteBatch.End();
            LineBatch.Begin(Camera.Projection, Camera.View);

            foreach (Fixture f in _ramps.FixtureList)
            {
                LineBatch.DrawLineShape(f.Shape, ContentWrapper.Teal);
            }

            LineBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}