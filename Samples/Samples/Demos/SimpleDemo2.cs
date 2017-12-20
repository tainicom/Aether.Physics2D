/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class SimpleDemo2 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private Sprite _rectangleSprite;
        private Vector2 _rectangleSize = new Vector2(4f, 4f);
        private Body _rectangles;
        private Vector2 _offset = new Vector2(2f, 0f);

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Body with two fixtures";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with two attached fixtures and shapes.");
            sb.AppendLine("A fixture binds a shape to a body and adds material");
            sb.AppendLine("properties such as density, friction, and restitution.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate object: left and right triggers");
            sb.AppendLine("  - Move object: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate Object: left and right arrows");
            sb.AppendLine("  - Move Object: A,S,D,W");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, ScreenManager, Camera);

            Vertices rect1 = PolygonTools.CreateRectangle(_rectangleSize.X/2f, _rectangleSize.Y/2f);
            Vertices rect2 = PolygonTools.CreateRectangle(_rectangleSize.X/2f, _rectangleSize.Y/2f);

            rect1.Translate(-_offset);
            rect2.Translate(_offset);

            List<Vertices> vertices = new List<Vertices>(2);
            vertices.Add(rect1);
            vertices.Add(rect2);

            _rectangles = World.CreateCompoundPolygon(vertices, 1f);
            _rectangles.BodyType = BodyType.Dynamic;

            SetUserAgent(_rectangles, 200f, 200f);

            // create sprite based on rectangle fixture
            _rectangleSprite = new Sprite(ScreenManager.Assets.TextureFromVertices(rect1, MaterialType.Squares,
                                                                                   Color.Orange, 1f, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.BatchEffect.View = Camera.View;
            ScreenManager.BatchEffect.Projection = Camera.Projection;
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            // draw first rectangle
            ScreenManager.SpriteBatch.Draw(_rectangleSprite.Texture, _rectangles.Position, null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin + _offset * _rectangleSprite.Size / _rectangleSize, _rectangleSize * _rectangleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);

            // draw second rectangle
            ScreenManager.SpriteBatch.Draw(_rectangleSprite.Texture, _rectangles.Position, null, Color.White, _rectangles.Rotation, _rectangleSprite.Origin - _offset * _rectangleSprite.Size / _rectangleSize, _rectangleSize * _rectangleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
            ScreenManager.SpriteBatch.End();
            _border.Draw();
            base.Draw(gameTime);
        }
    }
}