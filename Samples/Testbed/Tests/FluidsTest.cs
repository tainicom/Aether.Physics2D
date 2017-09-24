/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
*/

using System;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Fluids;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class FluidsTest : Test
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;

        private FluidsTest()
        {
            World = new World(new Vector2(0f, -10f));

            Random random = new Random();

            for (int i = 0; i < 2000; i++)
            {
                World.Fluid.AddParticle(new Vector2(-14.0f + 28.0f * (float)random.NextDouble(), 10.0f + 20.0f * (float)random.NextDouble()));
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            //DebugView.AppendFlags(DebugViewFlags.DebugPanel);
            //DebugView.AppendFlags(DebugViewFlags.PerformanceGraph);

            _spriteBatch = new SpriteBatch(GameInstance.GraphicsDevice);
            _pixel = GameInstance.Content.Load<Texture2D>("Pixel");
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _spriteBatch.Begin();

            foreach (var fluidParticle in World.Fluid.Particles)
            {
                _spriteBatch.Draw(_pixel, GameInstance.ConvertWorldToScreen(new Vector2(fluidParticle.Position.X, fluidParticle.Position.Y)), Color.White);
            }
            _spriteBatch.End();

            DrawString("Particles: " + World.Fluid.Particles.Count);
            
            base.Update(settings, gameTime);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            if (state.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);

                for (int i = 0; i < 5; i++)
                {
                    World.Fluid.AddParticle(new Vector2(mousePosition.X + i, mousePosition.Y));
                }
            }

            base.Mouse(state, oldState);
        }

        internal static Test Create()
        {
            return new FluidsTest();
        }
    }
}