/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D02_MultipleFixtures : PhysicsDemoScreen
    {
        private Border _border;
        private Sprite _rectangleSprite;
        private Vector2 _rectangleSize = new Vector2(4f, 4f);
        private Body _rectangles;
        private Vector2 _offset = new Vector2(2f, 0f);

        #region Demo description
        public override string GetTitle()
        {
            return "Single body with two fixtures";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with two attached fixtures and shapes.");
            sb.AppendLine("A fixture binds a shape to a body and adds material properties such");
            sb.AppendLine("as density, friction, and restitution.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: Left and right trigger");
            sb.AppendLine("  - Move object: Right thumbstick");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate object: Q, E");
            sb.AppendLine("  - Move object: W, S, A, D");
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

            World.Gravity = Vector2.Zero;

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            Vertices rectangle1 = PolygonTools.CreateRectangle(_rectangleSize.X/2f, _rectangleSize.Y/2f);
            Vertices rectangle2 = PolygonTools.CreateRectangle(_rectangleSize.X/2f, _rectangleSize.Y/2f);

            rectangle1.Translate(-_offset);
            rectangle2.Translate(_offset);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rectangle1);
            vertices.Add(rectangle2);

            _rectangles = World.CreateCompoundPolygon(vertices, 1f);
            _rectangles.BodyType = BodyType.Dynamic;

            SetUserAgent(_rectangles, 200f, 200f);

            // create sprite based on rectangle fixture
            _rectangleSprite = new Sprite(ContentWrapper.PolygonTexture(rectangle1, "Square", ContentWrapper.Blue, ContentWrapper.Gold, ContentWrapper.Black, 1f, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            // draw first rectangle
            SpriteBatch.Draw(_rectangleSprite.Texture, _rectangles.Position, null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin + _offset * _rectangleSprite.Size / _rectangleSize, _rectangleSize * _rectangleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);

            // draw second rectangle
            SpriteBatch.Draw(_rectangleSprite.Texture, _rectangles.Position, null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin - _offset * _rectangleSprite.Size / _rectangleSize, _rectangleSize * _rectangleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}