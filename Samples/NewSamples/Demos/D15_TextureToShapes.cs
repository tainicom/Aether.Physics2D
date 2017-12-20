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
using tainicom.Aether.Physics2D.Collision;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D15_TextureToShapes : PhysicsDemoScreen
    {
        private Border _border;
        private Body _compound;
        private Vector2 _polygonSize;
        private Sprite _objectSprite;

        #region Demo description
        public override string GetTitle()
        {
            return "Texture to collision shapes";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to create collision shapes from a texture.");
            sb.AppendLine("These are added to a single body with multiple fixtures.");
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

            List<Vertices> tracedObject = Framework.Content.Load<List<Vertices>>("Pipeline/Object");

            //scale the vertices from graphics space to sim space
            Vector2 vertScale = new Vector2(1f / 24f);
            AABB aabb = new AABB();
            foreach (Vertices vertices in tracedObject)
            {
                vertices.Scale(new Vector2(1f, -1f));
                vertices.Translate(new Vector2(0f, 0f));

                var vaabb = vertices.GetAABB();
                aabb.Combine(ref vaabb);
            }
            _polygonSize = new Vector2(aabb.Width, aabb.Height);

            // Create a single body with multiple fixtures
            _compound = World.CreateCompoundPolygon(tracedObject, 1f, Vector2.Zero, 0, BodyType.Dynamic);

            SetUserAgent(_compound, 200f, 200f);
            _objectSprite = new Sprite(ContentWrapper.GetTexture("Logo"), ContentWrapper.CalculateOrigin(_compound, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            SpriteBatch.Draw(_objectSprite.Texture, _compound.Position, null, Color.White, _compound.Rotation, _objectSprite.Origin, _polygonSize * _objectSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}