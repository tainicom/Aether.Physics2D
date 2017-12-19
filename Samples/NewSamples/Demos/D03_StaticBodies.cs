﻿/* Original source Farseer Physics Engine:
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
    internal class D03_StaticBodies : PhysicsDemoScreen
    {
        private Agent _agent;
        private Border _border;
        private Sprite _obstacle;
        private Body[] _obstacles = new Body[5];

        #region Demo description
        public override string GetTitle()
        {
            return "Multiple fixtures and static bodies";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a single body with multiple attached fixtures");
            sb.AppendLine("and different shapes attached.");
            sb.AppendLine("Several static bodies are placed as obstacles in the environment.");
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

            World.Gravity = new Vector2(0f, -20f);

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            _agent = new Agent(World, new Vector2(-6.9f, 11f));

            // Obstacles
            for (int i = 0; i < 5; i++)
            {
                _obstacles[i] = World.CreateRectangle(5f, 1f, 1f);
                _obstacles[i].BodyType = BodyType.Static;
                _obstacles[i].SetRestitution(0.2f);
                _obstacles[i].SetFriction(0.2f);
            }

            _obstacles[0].Position = new Vector2(-5f, -9f);
            _obstacles[1].Position = new Vector2(15f, -6f);
            _obstacles[2].Position = new Vector2(10f, 3f);
            _obstacles[3].Position = new Vector2(-10f, 9f);
            _obstacles[4].Position = new Vector2(-17f, 0f);

            // create sprite based on body
            _obstacle = new Sprite(ContentWrapper.TextureFromShape(_obstacles[0].FixtureList[0].Shape, "Stripe", ContentWrapper.Gold, ContentWrapper.Black, ContentWrapper.Black, 1.5f, 24f));

            SetUserAgent(_agent.Body, 1000f, 400f);
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(0, null, null, null, RasterizerState.CullNone, BatchEffect);

            for (int i = 0; i < 5; ++i)
            {
                SpriteBatch.Draw(_obstacle.Texture, _obstacles[i].Position, null, Color.White, _obstacles[i].Rotation, _obstacle.Origin, new Vector2(5f, 1f) * _obstacle.TexelSize, SpriteEffects.FlipVertically, 0f);
            }

            _agent.Draw(SpriteBatch);
            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}