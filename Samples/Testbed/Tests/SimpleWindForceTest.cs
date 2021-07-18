﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    internal class SimpleWindForceTest : Test
    {
        private SimpleWindForce _simpleWind;

        private float _strength;

        private SimpleWindForceTest()
        {
            _simpleWind = new SimpleWindForce();
            _simpleWind.Direction = new Vector2(0.7f, 0.2f);
            _simpleWind.Variation = 1.0f;
            _simpleWind.Strength = 5;
            _simpleWind.Position = new Vector2(0, 20);
            _simpleWind.DecayStart = 5f;
            _simpleWind.DecayEnd = 10f;
            _simpleWind.DecayMode = AbstractForceController.DecayModes.Step;
            _simpleWind.ForceType = AbstractForceController.ForceTypes.Point;

            _strength = 1.0f;

            World.Add(_simpleWind);
            World.Gravity = Vector2.Zero;

            const int countX = 10;
            const int countY = 10;

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countY; y++)
                {
                    Body currentBody = World.CreateBody(new Vector2(x * 2 - countX, y * 2 + 5), 0.6f, BodyType.Dynamic);
                    var currentFixture = currentBody.CreateRectangle(1f, 1f, 5f, Vector2.Zero);
                    //Body currentBody = World.CreateBody(new Vector2(x - countX, y + 5), 0.6f, BodyType.Dynamic);
                    //var currentFixture = currentBody.CreateCircle(0.2f, 10f);
                    currentFixture.Friction=0.5f;
                    //currentFixture.CollidesWith = Category.Cat10;
                }
            }

            Body floor = World.CreateRectangle(100, 1, 1, new Vector2(0, 0));
            Body ceiling = World.CreateRectangle(100, 1, 1, new Vector2(0, 40));
            Body right = World.CreateRectangle(1, 100, 1, new Vector2(35, 0));
            Body left = World.CreateRectangle(1, 100, 1, new Vector2(-35, 0));

            foreach (var fixture in floor.FixtureList)
                fixture.Friction = 0.2f;
            foreach (var fixture in ceiling.FixtureList)
                fixture.Friction = 0.2f;
            foreach (var fixture in right.FixtureList)
                fixture.Friction = 0.2f;
            foreach (var fixture in left.FixtureList)
                fixture.Friction = 0.2f;
        }

        public void DrawPointForce()
        {
            DebugView.DrawPoint(_simpleWind.Position, 2, Color.Red);
            DebugView.DrawCircle(_simpleWind.Position, _simpleWind.DecayStart, Color.Green);
            DebugView.DrawCircle(_simpleWind.Position, _simpleWind.DecayEnd, Color.Red);
        }

        public void DrawLineForce()
        {
            Vector2 drawVector;
            drawVector = _simpleWind.Direction;
            drawVector.Normalize();
            drawVector *= _strength;
            DebugView.DrawArrow(_simpleWind.Position, _simpleWind.Position + drawVector, 2, 1f, true, Color.Red);
        }

        public void DrawNoneForce()
        {
            DebugView.DrawPoint(_simpleWind.Position, 2, Color.Red);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("SimpleWindForce | Mouse: Direction | Left-Click: Position | W/S: Variation");
            DrawString("Wind Strength:" + _simpleWind.Strength);
            DrawString("Variation:" + _simpleWind.Variation);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            //DebugView.DrawSegment(SimpleWind.Position, SimpleWind.Direction-SimpleWind.Position, Color.Red);
            DrawPointForce();
            DebugView.EndCustomDraw();
            base.Update(settings, gameTime);
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsKeyDown(Keys.Q))
                _strength += 1f;

            if (keyboardManager.IsKeyDown(Keys.A))
                _strength -= 1f;

            if (keyboardManager.IsKeyDown(Keys.W))
                _simpleWind.Variation += 0.1f;

            if (keyboardManager.IsKeyDown(Keys.S))
                _simpleWind.Variation -= 0.1f;

            base.Keyboard(keyboardManager);
        }

        public override void Mouse(MouseState state, MouseState oldState)
        {
            //base.Mouse(state, oldState);
            Vector2 mouseWorld = GameInstance.ConvertScreenToWorld(state.X, state.Y);
            _simpleWind.Direction = mouseWorld - _simpleWind.Position;
            _simpleWind.Strength = _strength;

            if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
            {
                _simpleWind.Position = mouseWorld;
                _simpleWind.Direction = mouseWorld + new Vector2(0, 1);
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(state.X, state.Y + 10);
            }
        }

        internal static Test Create()
        {
            return new SimpleWindForceTest();
        }
    }
}