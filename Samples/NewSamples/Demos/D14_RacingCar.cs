/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using System.Collections.Generic;
using System.Text;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D14_RacingCar : PhysicsDemoScreen
    {
        private float _acceleration;
        private Body _board;
        private Sprite _box;
        private List<Body> _boxes;

        private Sprite _bridge;
        private List<Body> _bridgeSegments;
        private Body _car;
        private Sprite _carBody;
        private Body _ground;
        private float _hzBack;
        private float _hzFront;
        private float _maxSpeed;

        private WheelJoint _springBack;
        private WheelJoint _springFront;
        private Sprite _teeter;
        private Sprite _wheel;
        private Body _wheelBack;
        private Body _wheelFront;
        private float _zeta;

        #region Demo description
        public override string GetTitle()
        {
            return "Racing car";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a side scrolling car on a race track.");
            sb.AppendLine("The car uses two wheel joints, which combine a revolute and");
            sb.AppendLine("a (soft) distance joint for the tire suspension.");
            sb.AppendLine("The track is composed of several edge shapes and different");
            sb.AppendLine("obstacles are attached to the track.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Accelerate / reverse: Left thumbstick");
            sb.AppendLine("  - Break: A button");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Accelerate / reverse: D / A");
            sb.AppendLine("  - Break: S");
            sb.Append("  - Exit to demo selection: Escape");
#endif
            return sb.ToString();
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -10f);

            HasCursor = false;
            EnableCameraControl = true;

            _hzFront = 8.5f;
            _hzBack = 5.0f;
            _zeta = 0.85f;
            _maxSpeed = 50.0f;

            // terrain
            _ground = World.CreateBody();
            {
                Vertices terrain = new Vertices();
                terrain.Add(new Vector2(-20f, 5f));
                terrain.Add(new Vector2(-20f, 0f));
                terrain.Add(new Vector2(20f, 0f));
                terrain.Add(new Vector2(25f, 0.25f));
                terrain.Add(new Vector2(30f, 1f));
                terrain.Add(new Vector2(35f, 4f));
                terrain.Add(new Vector2(40f, 0f));
                terrain.Add(new Vector2(45f, 0f));
                terrain.Add(new Vector2(50f, -1f));
                terrain.Add(new Vector2(55f, -2f));
                terrain.Add(new Vector2(60f, -2f));
                terrain.Add(new Vector2(65f, -1.25f));
                terrain.Add(new Vector2(70f, 0f));
                terrain.Add(new Vector2(75f, 0.3f));
                terrain.Add(new Vector2(80f, 1.5f));
                terrain.Add(new Vector2(85f, 3.5f));
                terrain.Add(new Vector2(90f, 0f));
                terrain.Add(new Vector2(95f, -0.5f));
                terrain.Add(new Vector2(100f, -1f));
                terrain.Add(new Vector2(105f, -2f));
                terrain.Add(new Vector2(110f, -2.5f));
                terrain.Add(new Vector2(115f, -1.3f));
                terrain.Add(new Vector2(120f, 0f));
                terrain.Add(new Vector2(160f, 0f));
                terrain.Add(new Vector2(159f, -10f));
                terrain.Add(new Vector2(201f, -10f));
                terrain.Add(new Vector2(200f, 0f));
                terrain.Add(new Vector2(240f, 0f));
                terrain.Add(new Vector2(250f, 5f));
                terrain.Add(new Vector2(250f, -10f));
                terrain.Add(new Vector2(270f, -10f));
                terrain.Add(new Vector2(270f, 0));
                terrain.Add(new Vector2(310f, 0));
                terrain.Add(new Vector2(310f, 5));

                for (int i = 0; i < terrain.Count - 1; ++i)
                    _ground.CreateEdge(terrain[i], terrain[i + 1]);

                _ground.SetFriction(0.6f);
            }

            // teeter board
            {
                _board = World.CreateBody();
                _board.BodyType = BodyType.Dynamic;
                _board.Position = new Vector2(140.0f, 1.0f);

                PolygonShape box = new PolygonShape(1f);
                box.Vertices = PolygonTools.CreateRectangle(20.0f/2f, 0.5f/2f);
                _teeter = new Sprite(ContentWrapper.TextureFromShape(box, "Stripe", ContentWrapper.Gold, ContentWrapper.Black, ContentWrapper.Black, 1f, 24f));

                _board.CreateFixture(box);

                RevoluteJoint teeterAxis = JointFactory.CreateRevoluteJoint(World, _ground, _board, Vector2.Zero);
                teeterAxis.LowerLimit = -8.0f * MathHelper.Pi / 180.0f;
                teeterAxis.UpperLimit = 8.0f * MathHelper.Pi / 180.0f;
                teeterAxis.LimitEnabled = true;

                _board.ApplyAngularImpulse(100.0f);
            }

            // bridge
            {
                _bridgeSegments = new List<Body>();

                const int segmentCount = 20;
                PolygonShape shape = new PolygonShape(1f);
                shape.Vertices = PolygonTools.CreateRectangle(1.0f, 0.125f);

                _bridge = new Sprite(ContentWrapper.TextureFromShape(shape, ContentWrapper.Gold, ContentWrapper.Black, 24f));

                Body prevBody = _ground;
                for (int i = 0; i < segmentCount; ++i)
                {
                    Body body = World.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = new Vector2(161f + 2f * i, -0.125f);
                    Fixture fix = body.CreateFixture(shape);
                    fix.Friction = 0.6f;
                    JointFactory.CreateRevoluteJoint(World, prevBody, body, -Vector2.UnitX);

                    prevBody = body;
                    _bridgeSegments.Add(body);
                }
                JointFactory.CreateRevoluteJoint(World, _ground, prevBody, Vector2.UnitX);
            }

            // boxes
            {
                _boxes = new List<Body>();
                PolygonShape box = new PolygonShape(1f);
                box.Vertices = PolygonTools.CreateRectangle(0.5f, 0.5f);
                _box = new Sprite(ContentWrapper.TextureFromShape(box, "Square", ContentWrapper.Sky, ContentWrapper.Sunset, ContentWrapper.Black, 1f, 24f));

                Body body = World.CreateBody();
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, 0.5f);
                body.CreateFixture(box);
                _boxes.Add(body);

                body = World.CreateBody();
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, 1.5f);
                body.CreateFixture(box);
                _boxes.Add(body);

                body = World.CreateBody();
                body.BodyType = BodyType.Dynamic;
                body.Position = new Vector2(220f, 2.5f);
                body.CreateFixture(box);
                _boxes.Add(body);
            }

            // car
            {
                Vertices vertices = new Vertices(8);
                vertices.Add(new Vector2(-2.5f, -0.08f));
                vertices.Add(new Vector2(-2.375f, 0.46f));
                vertices.Add(new Vector2(-0.58f, 0.92f));
                vertices.Add(new Vector2(0.46f, 0.92f));
                vertices.Add(new Vector2(2.5f, 0.17f));
                vertices.Add(new Vector2(2.5f, -0.205f));
                vertices.Add(new Vector2(2.3f, -0.33f));
                vertices.Add(new Vector2(-2.25f, -0.35f));

                PolygonShape chassis = new PolygonShape(vertices, 2f);

                _car = World.CreateBody();
                _car.BodyType = BodyType.Dynamic;
                _car.Position = new Vector2(0.0f, 1.0f);
                _car.CreateFixture(chassis);

                _wheelBack = World.CreateBody();
                _wheelBack.BodyType = BodyType.Dynamic;
                _wheelBack.Position = new Vector2(-1.709f, 0.78f);
                Fixture fix = _wheelBack.CreateFixture(new CircleShape(0.5f, 0.8f));
                fix.Friction = 0.9f;

                _wheelFront = World.CreateBody();
                _wheelFront.BodyType = BodyType.Dynamic;
                _wheelFront.Position = new Vector2(1.54f, 0.8f);
                _wheelFront.CreateFixture(new CircleShape(0.5f, 1f));

                Vector2 axis = new Vector2(0.0f, 1.2f);
                _springBack = new WheelJoint(_car, _wheelBack, _wheelBack.Position, axis, true);
                _springBack.MotorSpeed = 0.0f;
                _springBack.MaxMotorTorque = 20.0f;
                _springBack.MotorEnabled = true;
                _springBack.Frequency = _hzBack;
                _springBack.DampingRatio = _zeta;
                World.Add(_springBack);

                _springFront = new WheelJoint(_car, _wheelFront, _wheelFront.Position, axis, true);
                _springFront.MotorSpeed = 0.0f;
                _springFront.MaxMotorTorque = 10.0f;
                _springFront.MotorEnabled = false;
                _springFront.Frequency = _hzFront;
                _springFront.DampingRatio = _zeta;
                World.Add(_springFront);

                // GFX
                _carBody = new Sprite(ContentWrapper.GetTexture("Car"), ContentWrapper.CalculateOrigin(_car, 24f));
                _wheel = new Sprite(ContentWrapper.GetTexture("Wheel"));
            }

            Camera.MinRotation = -0.05f;
            Camera.MaxRotation = 0.05f;

            Camera.TrackingBody = _car;
            Camera.EnableTracking = true;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _springBack.MotorSpeed = Math.Sign(_acceleration) * MathHelper.SmoothStep(0f, _maxSpeed, Math.Abs(_acceleration));

            if (Math.Abs(_springBack.MotorSpeed) < _maxSpeed * 0.06f)
                _springBack.MotorEnabled = false;
            else
                _springBack.MotorEnabled = true;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.GamePadState.ThumbSticks.Left.X > 0.5f || input.KeyboardState.IsKeyDown(Keys.D))
                _acceleration = Math.Min(_acceleration + (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), 1f);
            else if (input.GamePadState.ThumbSticks.Left.X < -0.5f || input.KeyboardState.IsKeyDown(Keys.A))
                _acceleration = Math.Max(_acceleration - (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), -1f);
            else if (input.IsNewButtonPress(Buttons.A) || input.IsNewKeyRelease(Keys.S))
                _acceleration = 0f;
            else
                _acceleration -= Math.Sign(_acceleration) * (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds);

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            
            // draw car
            SpriteBatch.Draw(_wheel.Texture, _wheelBack.Position, null, Color.White, _wheelBack.Rotation, _wheel.Origin, new Vector2(0.5f) * _wheel.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.Draw(_wheel.Texture, _wheelFront.Position, null, Color.White, _wheelFront.Rotation, _wheel.Origin, new Vector2(0.5f) * _wheel.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.Draw(_carBody.Texture, _car.Position, null, Color.White, _car.Rotation, _carBody.Origin, new Vector2(5f, 1.27f) * _carBody.TexelSize, SpriteEffects.FlipVertically, 0f);
            
            // draw teeter
            SpriteBatch.Draw(_teeter.Texture, _board.Position, null, Color.White, _board.Rotation, _teeter.Origin, new Vector2(20f, 0.5f) * _teeter.TexelSize, SpriteEffects.FlipVertically, 0f);
            
            // draw bridge
            for (int i = 0; i < _bridgeSegments.Count; ++i)
            {
                SpriteBatch.Draw(_bridge.Texture, _bridgeSegments[i].Position, null, Color.White, _bridgeSegments[i].Rotation, _bridge.Origin, new Vector2(2.0f, 0.25f) * _bridge.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            
            // draw boxes
            for (int i = 0; i < _boxes.Count; ++i)
            {
                SpriteBatch.Draw(_box.Texture, _boxes[i].Position, null, Color.White, _boxes[i].Rotation, _box.Origin, new Vector2(1f, 1f) * _box.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            SpriteBatch.End();

            LineBatch.Begin(Camera.Projection, Camera.View);
            
            // draw ground
            for (int i = 0; i < _ground.FixtureList.Count; ++i)
            {
                LineBatch.DrawLineShape(_ground.FixtureList[i].Shape, Color.Black);
            }
            LineBatch.End();
            base.Draw(gameTime);
        }
    }
}