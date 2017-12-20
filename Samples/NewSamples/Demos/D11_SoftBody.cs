/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D11_SoftBody : PhysicsDemoScreen
    {
        private Border _border;

        private List<Body> _bridgeBodies;
        private List<Body> _softBodies;

        private Sprite _bridgeBox;
        private Sprite _softBodyBox;
        private Sprite _softBodyCircle;

        #region Demo description
        public override string GetTitle()
        {
            return "Soft body & path generator";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how a soft body and a bridge can be created, using");
            sb.AppendLine("the path generator and bodies connected with revolute joints.");
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

            World.Gravity = new Vector2(0, -9.82f);

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            // Bridge
            // We make a path using 2 points.
            Path bridgePath = new Path();
            bridgePath.Add(new Vector2(-15, -5));
            bridgePath.Add(new Vector2(15, -5));
            bridgePath.Closed = false;

            Vertices box = PolygonTools.CreateRectangle(0.25f/2f, 1.0f/2f);
            PolygonShape shape = new PolygonShape(box, 20);

            _bridgeBodies = PathManager.EvenlyDistributeShapesAlongPath(World, bridgePath, shape, BodyType.Dynamic, 29);

            // Attach the first and last fixtures to the world
            Body anchor = World.CreateBody(Vector2.Zero);
            anchor.BodyType = BodyType.Static;
            World.Add(new RevoluteJoint(_bridgeBodies[0], anchor, _bridgeBodies[0].Position - new Vector2(0.5f, 0f), true));
            World.Add(new RevoluteJoint(_bridgeBodies[_bridgeBodies.Count - 1], anchor, _bridgeBodies[_bridgeBodies.Count - 1].Position + new Vector2(0.5f, 0f), true));

            PathManager.AttachBodiesWithRevoluteJoint(World, _bridgeBodies, new Vector2(0f, -0.5f), new Vector2(0f, 0.5f), false, true);

            // Soft body
            // We make a rectangular path.
            Path rectanglePath = new Path();
            rectanglePath.Add(new Vector2(-6, 11));
            rectanglePath.Add(new Vector2(-6, -1));
            rectanglePath.Add(new Vector2(6, -1));
            rectanglePath.Add(new Vector2(6, 11));
            rectanglePath.Closed = true;

            // Creating two shapes. A circle to form the circle and a rectangle to stabilize the soft body.
            Shape[] shapes = new Shape[2];
            shapes[0] = new PolygonShape(PolygonTools.CreateRectangle(1f/2f, 1f/2f, new Vector2(-0.1f, 0f), 0f), 1f);
            shapes[1] = new CircleShape(0.5f, 1f);

            // We distribute the shapes in the rectangular path.
            _softBodies = PathManager.EvenlyDistributeShapesAlongPath(World, rectanglePath, shapes, BodyType.Dynamic, 30);

            // Attach the bodies together with revolute joints. The rectangular form will converge to a circular form.
            PathManager.AttachBodiesWithRevoluteJoint(World, _softBodies, new Vector2(0f, 0.5f), new Vector2(0f, -0.5f), true, true);

            // GFX
            _bridgeBox = new Sprite(ContentWrapper.TextureFromShape(shape, ContentWrapper.Orange, ContentWrapper.Brown, 24f));
            _softBodyBox = new Sprite(ContentWrapper.TextureFromShape(shapes[0], ContentWrapper.Green, ContentWrapper.Black, 24f));
            _softBodyBox.Origin += new Vector2(2.4f, 0f);
            _softBodyCircle = new Sprite(ContentWrapper.TextureFromShape(shapes[1], ContentWrapper.Lime, ContentWrapper.Grey, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            
            foreach (Body body in _softBodies)
            {
                SpriteBatch.Draw(_softBodyBox.Texture, body.Position, null, Color.White, body.Rotation, _softBodyBox.Origin, new Vector2(1f, 1f) * _softBodyBox.TexelSize, SpriteEffects.None, 0f);
            }

            foreach (Body body in _softBodies)
            {
                SpriteBatch.Draw(_softBodyCircle.Texture, body.Position, null, Color.White, body.Rotation, _softBodyCircle.Origin, new Vector2(1f, 1f) * _softBodyCircle.TexelSize, SpriteEffects.None, 0f);
            }

            foreach (Body body in _bridgeBodies)
            {
                SpriteBatch.Draw(_bridgeBox.Texture, body.Position, null, Color.White, body.Rotation, _bridgeBox.Origin, new Vector2(0.25f, 1f) * _bridgeBox.TexelSize, SpriteEffects.None, 0f);
            }

            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}