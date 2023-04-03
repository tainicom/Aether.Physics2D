// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class SPH2Test : Test
    {
        private SpriteBatch _spriteBatch;
        private Texture2D _pixel;
        Random _random = new Random();
        int _particles = 1024;
        private ParticleHydrodynamicsController _controller;
        

        public SPH2Test()
        {
            World = new World();

            World.CreateEdge(new Vector2(-34f, -8f), new Vector2( 34f, -8f));
            World.CreateEdge(new Vector2(-34f, 48f), new Vector2(-34f, -8f));
            World.CreateEdge(new Vector2( 34f, -8f), new Vector2( 34f, 48f));
            World.CreateEdge(new Vector2( 34f, 48f), new Vector2(-34f, 48f));

            World.CreateEdge(new Vector2(-20f, -4f), new Vector2(-24f, -4f));
            World.CreateEdge(new Vector2(-20f, 38f), new Vector2(-20f, -4f));
            World.CreateEdge(new Vector2(-24f, -4f), new Vector2(-24f, 38f));
            World.CreateEdge(new Vector2(-24f, 38f), new Vector2(-20f, 38f));
            
            World.CreateCircle(3, 0.0005f, new Vector2(-6f, -2f), BodyType.Dynamic);
            World.CreateCircle(3, 0.0001f, new Vector2( 6f, -2f), BodyType.Dynamic);
            World.CreateCircle(3, 1.0000f, new Vector2(18f, -2f), BodyType.Static);
        }

        public override void Initialize()
        {
            base.Initialize();
            
            _spriteBatch = new SpriteBatch(GameInstance.GraphicsDevice);
            _pixel = new Texture2D(GameInstance.GraphicsDevice, 1, 1);
            _pixel.SetData<Color>(new[] {Color.White});
            
            _controller = new ParticleHydrodynamicsController(2.0f, 2048);
            World.Add(_controller);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            for (int i = 0; _particles > 0 && i < 4; i++)
            {
                var pos = new Vector2(-32/2 + 32.0f * (float)_random.NextDouble(), 10.0f + 20.0f * (float)_random.NextDouble());
                _controller.AddParticle(pos);
                _particles--;
            }
         
            _spriteBatch.Begin();
            var color = Color.FromNonPremultiplied(0, 0,255, 64);
            //for (int i = 0; i < _controller.ParticleCount; i++)
            //{
            //    var pos = _controller.GetParticlePosition(i);
            //    _spriteBatch.Draw(_pixel, GameInstance.ConvertWorldToScreen(pos), null, color, 0, new Vector2(0.5f), 16f, SpriteEffects.None, 0);
            //}
            for (int i = 0; i < _controller.ParticleCount; i++)
            {
                var pos = _controller.GetParticlePosition(i);
                _spriteBatch.Draw(_pixel, GameInstance.ConvertWorldToScreen(pos), null, Color.Blue, 0, new Vector2(0.5f), 4f, SpriteEffects.None, 0);
            }
            _spriteBatch.End();
            
            this.DebugView.BeginCustomDraw(GameInstance.Projection, GameInstance.View);
            foreach (var gridCell in _controller.Grid.GridCellDictionary.Keys)
            {
                var lowerBound = new Vector2(_controller.Grid.CellSize * gridCell.X, _controller.Grid.CellSize * gridCell.Y);
                var upperBound = lowerBound + new Vector2(_controller.Grid.CellSize);
                var cellAABB = new tainicom.Aether.Physics2D.Collision.AABB(ref lowerBound, ref upperBound);
                this.DebugView.DrawAABB(ref cellAABB, Color.FromNonPremultiplied(255, 0, 0, 64));
            }
            this.DebugView.EndCustomDraw();

            DrawString("Particles: " + _controller.ParticleCount);

            base.Update(settings, gameTime);
        }

        public override void Mouse(InputState input)
        {
            var state = input.MouseState;
            var oldState = input.PrevMouseState;
            if (GameInstance.IsActive && state.LeftButton == ButtonState.Pressed)
            {
                Vector2 mousePosition = GameInstance.ConvertScreenToWorld(state.X, state.Y);
                Vector2 oldMousePosition = GameInstance.ConvertScreenToWorld(oldState.X, oldState.Y);
                Vector2 mouseVelocity = mousePosition - oldMousePosition;
                
                _controller.AddParticle(new Vector2(mousePosition.X, mousePosition.Y), mouseVelocity);
                _controller.AddParticle(new Vector2(mousePosition.X + 1, mousePosition.Y), mouseVelocity);
                _controller.AddParticle(new Vector2(mousePosition.X - 1, mousePosition.Y), mouseVelocity);
                _controller.AddParticle(new Vector2(mousePosition.X, mousePosition.Y + 1), mouseVelocity);
                _controller.AddParticle(new Vector2(mousePosition.X, mousePosition.Y - 1), mouseVelocity);
            }

            base.Mouse(input);
        }
        
    }
}