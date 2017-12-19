﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D08_DistanceAngleJoint : PhysicsDemoScreen
    {
        private Border _border;
        private Body _obstacles;
        private Body[] _angleBody = new Body[3];
        private Body[] _distanceBody = new Body[4];

        private Sprite _angleCube;
        private Sprite _distanceCube;

        #region Demo description
        public override string GetTitle()
        {
            return "Distance & angle joints";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodies connected by distance and angle joints.");
            sb.AppendLine("Orange bodies are forced to have the same angle at all times.");
            sb.AppendLine();
            sb.AppendLine("Striped bodies are forced to have the same distance at all times.");
            sb.AppendLine("Two of them have a rigid distance joint.");
            sb.AppendLine("The other two have a soft (spring-like) distance joint.");
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

            _obstacles = World.CreateBody();
            _obstacles.CreateEdge(new Vector2(-16f, 1f), new Vector2(-14f, -1f));
            _obstacles.CreateEdge(new Vector2(-14f, -1f), new Vector2(-12f, 1f));

            _obstacles.CreateEdge(new Vector2(14f, 1f), new Vector2(12f, -5f));
            _obstacles.CreateEdge(new Vector2(14f, 1f), new Vector2(16f, -5f));

            _angleBody[0] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _angleBody[0].BodyType = BodyType.Dynamic;
            _angleBody[0].SetFriction(0.7f);
            _angleBody[0].Position = new Vector2(-15f, 5f);
            _angleBody[1] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _angleBody[1].BodyType = BodyType.Dynamic;
            _angleBody[1].SetFriction(0.7f);
            _angleBody[1].Position = new Vector2(-18f, -5f);
            _angleBody[2] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _angleBody[2].BodyType = BodyType.Dynamic;
            _angleBody[2].SetFriction(0.7f);
            _angleBody[2].Position = new Vector2(-10f, -5f);

            World.Add(new AngleJoint(_angleBody[0], _angleBody[1]));
            World.Add(new AngleJoint(_angleBody[0], _angleBody[2]));

            _distanceBody[0] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _distanceBody[0].BodyType = BodyType.Dynamic;
            _distanceBody[0].SetFriction(0.7f);
            _distanceBody[0].Position = new Vector2(11.5f, 4f);
            _distanceBody[1] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _distanceBody[1].BodyType = BodyType.Dynamic;
            _distanceBody[1].SetFriction(0.7f);
            _distanceBody[1].Position = new Vector2(16.5f, 4f);
            _distanceBody[2] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _distanceBody[2].BodyType = BodyType.Dynamic;
            _distanceBody[2].SetFriction(0.7f);
            _distanceBody[2].Position = new Vector2(11.5f, 6f);
            _distanceBody[3] = World.CreateRectangle(1.5f, 1.5f, 1f);
            _distanceBody[3].BodyType = BodyType.Dynamic;
            _distanceBody[3].SetFriction(0.7f);
            _distanceBody[3].Position = new Vector2(16.5f, 6f);

            DistanceJoint softDistance = new DistanceJoint(_distanceBody[0], _distanceBody[1], Vector2.Zero, Vector2.Zero, false);
            softDistance.DampingRatio = 0.3f;
            softDistance.Frequency = 5f;
            World.Add(softDistance);
            World.Add(new DistanceJoint(_distanceBody[2], _distanceBody[3], Vector2.Zero, Vector2.Zero, false));

            // create sprites based on bodies
            _angleCube = new Sprite(ContentWrapper.TextureFromShape(_angleBody[0].FixtureList[0].Shape, "Square", ContentWrapper.Gold, ContentWrapper.Orange, ContentWrapper.Grey, 1f, 24f));
            _distanceCube = new Sprite(ContentWrapper.TextureFromShape(_distanceBody[0].FixtureList[0].Shape, "Stripe", ContentWrapper.Red, ContentWrapper.Blue, ContentWrapper.Grey, 4f, 24f));
        }

        public override void Draw(GameTime gameTime)
        {
            LineBatch.Begin(Camera.Projection, Camera.View);
            foreach (Fixture f in _obstacles.FixtureList)
            {
                LineBatch.DrawLine(_distanceBody[0].Position, _distanceBody[1].Position, ContentWrapper.Black);
                LineBatch.DrawLine(_distanceBody[2].Position, _distanceBody[3].Position, ContentWrapper.Black);
            }
            LineBatch.End();

            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(0, null, null, null, RasterizerState.CullNone, BatchEffect);
            for (int i = 0; i < 3; i++)
            {
                SpriteBatch.Draw(_angleCube.Texture, _angleBody[i].Position, null, Color.White, _angleBody[i].Rotation, _angleCube.Origin, new Vector2(1.5f, 1.5f) * _angleCube.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            for (int i = 0; i < 4; i++)
            {
                SpriteBatch.Draw(_distanceCube.Texture, _distanceBody[i].Position, null, Color.White, _distanceBody[i].Rotation, _distanceCube.Origin, new Vector2(1.5f, 1.5f) * _distanceCube.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            SpriteBatch.End();

            LineBatch.Begin(Camera.Projection, Camera.View);            
            foreach (Fixture f in _obstacles.FixtureList)
            {
                LineBatch.DrawLineShape(f.Shape, ContentWrapper.Black);
            }
            LineBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}