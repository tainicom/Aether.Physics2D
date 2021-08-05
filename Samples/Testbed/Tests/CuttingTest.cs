/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.PolygonManipulation;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class CuttingTest : Test
    {
        private const float MoveAmount = 0.1f;

        private const int Count = 20;
        private Vector2 _end = new Vector2(6, 5);
        private Vector2 _start = new Vector2(-6, 5);
        private bool _switched;

        private CuttingTest()
        {
            //Ground
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 5);

            Vector2 x = new Vector2(-7.0f, 0.75f);
            Vector2 deltaX = new Vector2(0.5625f, 1.25f);
            Vector2 deltaY = new Vector2(1.125f, 0.0f);

            for (int i = 0; i < Count; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < Count; ++j)
                {
                    Body body = World.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.CreateFixture(shape);

                    y += deltaY;
                }

                x += deltaX;
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            DrawString("Press A,S,W,D move endpoint");
            
            DrawString("Press Enter to cut");
            
            DrawString("Press TAB to change endpoint");
            

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            DebugView.DrawSegment(_start, _end, Color.Red);
            DebugView.EndCustomDraw();

            List<Fixture> fixtures = new List<Fixture>();
            List<Vector2> entryPoints = new List<Vector2>();
            List<Vector2> exitPoints = new List<Vector2>();

            //Get the entry points
            World.RayCast((f, p, n, fr) =>
                              {
                                  fixtures.Add(f);
                                  entryPoints.Add(p);
                                  return 1;
                              }, _start, _end);

            //Reverse the ray to get the exitpoints
            World.RayCast((f, p, n, fr) =>
                              {
                                  exitPoints.Add(p);
                                  return 1;
                              }, _end, _start);

            DrawString("Fixtures: " + fixtures.Count);

            DebugView.BeginCustomDraw(ref GameInstance.Projection, ref GameInstance.View);
            foreach (Vector2 entryPoint in entryPoints)
            {
                DebugView.DrawPoint(entryPoint, 0.5f, Color.Yellow);
            }

            foreach (Vector2 exitPoint in exitPoints)
            {
                DebugView.DrawPoint(exitPoint, 0.5f, Color.PowderBlue);
            }
            DebugView.EndCustomDraw();

            base.Update(settings, gameTime);
        }

        public override void Keyboard(InputState input)
        {
            if (input.IsKeyPressed(Keys.Tab))
                _switched = !_switched;

            if (input.IsKeyPressed(Keys.Enter))
                CuttingTools.Cut(World, _start, _end);

            if (_switched)
            {
                if (input.IsKeyDown(Keys.A))
                    _start.X -= MoveAmount;

                if (input.IsKeyDown(Keys.S))
                    _start.Y -= MoveAmount;

                if (input.IsKeyDown(Keys.W))
                    _start.Y += MoveAmount;

                if (input.IsKeyDown(Keys.D))
                    _start.X += MoveAmount;
            }
            else
            {
                if (input.IsKeyDown(Keys.A))
                    _end.X -= MoveAmount;

                if (input.IsKeyDown(Keys.S))
                    _end.Y -= MoveAmount;

                if (input.IsKeyDown(Keys.W))
                    _end.Y += MoveAmount;

                if (input.IsKeyDown(Keys.D))
                    _end.X += MoveAmount;
            }

            base.Keyboard(input);
        }

        public override void Gamepad(InputState input)
        {
            _start.X += input.GamePadState.ThumbSticks.Left.X / 5;
            _start.Y += input.GamePadState.ThumbSticks.Left.Y / 5;

            _end.X += input.GamePadState.ThumbSticks.Right.X / 5;
            _end.Y += input.GamePadState.ThumbSticks.Right.Y / 5;

            if (input.IsButtonPressed(Buttons.A))
                CuttingTools.Cut(World, _start, _end);

            base.Gamepad(input);
        }

        public static CuttingTest Create()
        {
            return new CuttingTest();
        }
    }
}