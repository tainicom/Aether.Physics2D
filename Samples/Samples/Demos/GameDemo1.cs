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
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class GameDemo1 : PhysicsGameScreen, IDemoScreen
    {
        private float _acceleration;
        private Body _board;
        private List<Body> _boxes;


        private List<Body> _bridgeSegments;
        private Body _car;
        private Body _ground;

        private Body _wheelBack;
        private Body _wheelFront;
        private WheelJoint _springBack;
        private WheelJoint _springFront;

        private Sprite _bridge;
        private Sprite _carBody;
        private Sprite _box;
        private Sprite _teeter;
        private Sprite _wheel;

        private const float MaxSpeed = 50.0f;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Racing Car";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -10f);

            HasCursor = false;
            EnableCameraControl = true;
            HasVirtualStick = true;

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

                PolygonShape box = new PolygonShape(PolygonTools.CreateRectangle(20.0f/2f, 0.5f/2f), 1);
                _teeter =
                    new Sprite(ScreenManager.Assets.TextureFromShape(box, MaterialType.Pavement, Color.LightGray, 1.2f));

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
                PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(1.0f, 0.125f), 1f);
                _bridge = new Sprite(ScreenManager.Assets.TextureFromShape(shape, MaterialType.Dots, Color.SandyBrown, 1f));

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
                PolygonShape box = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.5f), 1f);
                _box = new Sprite(ScreenManager.Assets.TextureFromShape(box, MaterialType.Squares, Color.SaddleBrown, 2f));

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

                PolygonShape chassis = new PolygonShape(vertices, 2);
                CircleShape wheelShape = new CircleShape(0.5f, 0.8f);

                _car = World.CreateBody();
                _car.BodyType = BodyType.Dynamic;
                _car.Position = new Vector2(0.0f, 1.0f);
                _car.CreateFixture(chassis);

                _wheelBack = World.CreateBody();
                _wheelBack.BodyType = BodyType.Dynamic;
                _wheelBack.Position = new Vector2(-1.709f, 0.78f);
                _wheelBack.CreateFixture(wheelShape);
                _wheelBack.SetFriction(0.9f);

                wheelShape.Density = 1;
                _wheelFront = World.CreateBody();
                _wheelFront.BodyType = BodyType.Dynamic;
                _wheelFront.Position = new Vector2(1.54f, 0.8f);
                _wheelFront.CreateFixture(wheelShape);

                Vector2 axis = new Vector2(0.0f, 1.2f);
                _springBack = new WheelJoint(_car, _wheelBack, _wheelBack.Position, axis, true);
                _springBack.MotorSpeed = 0.0f;
                _springBack.MaxMotorTorque = 20.0f;
                _springBack.MotorEnabled = true;
                _springBack.Frequency = 4.0f;
                _springBack.DampingRatio = 0.7f;
                World.Add(_springBack);

                _springFront = new WheelJoint(_car, _wheelFront, _wheelFront.Position, axis, true);
                _springFront.MotorSpeed = 0.0f;
                _springFront.MaxMotorTorque = 10.0f;
                _springFront.MotorEnabled = false;
                _springFront.Frequency = 4.0f;
                _springFront.DampingRatio = 0.7f;
                World.Add(_springFront);

                _carBody = new Sprite(ScreenManager.Content.Load<Texture2D>("Samples/car"), AssetCreator.CalculateOrigin(_car, 24f));
                _wheel = new Sprite(ScreenManager.Content.Load<Texture2D>("Samples/wheel"));
            }

            Camera.MinRotation = -0.05f;
            Camera.MaxRotation = 0.05f;

            Camera.TrackingBody = _car;
            Camera.EnableTracking = true;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _springBack.MotorSpeed = Math.Sign(_acceleration) * MathHelper.SmoothStep(0f, MaxSpeed, Math.Abs(_acceleration));
            if (Math.Abs(_springBack.MotorSpeed) < MaxSpeed * 0.06f)
            {
                _springBack.MotorEnabled = false;
            }
            else
            {
                _springBack.MotorEnabled = true;
            }
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.VirtualState.ThumbSticks.Left.X > 0.5f)
                _acceleration = Math.Min(_acceleration + (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), 1f);
            else if (input.VirtualState.ThumbSticks.Left.X < -0.5f)
                _acceleration = Math.Max(_acceleration - (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds), -1f);
            else if (input.VirtualState.Buttons.A == ButtonState.Pressed)
                _acceleration = 0f;
            else
                _acceleration -= Math.Sign(_acceleration) * (float)(2.0 * gameTime.ElapsedGameTime.TotalSeconds);

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.BatchEffect.View = Camera.View;
            ScreenManager.BatchEffect.Projection = Camera.Projection;
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            // draw car
            ScreenManager.SpriteBatch.Draw(_wheel.Texture, _wheelBack.Position, null, Color.White, _wheelBack.Rotation, _wheel.Origin, new Vector2(0.5f) * _wheel.TexelSize, SpriteEffects.FlipVertically, 0f);
            ScreenManager.SpriteBatch.Draw(_wheel.Texture, _wheelFront.Position, null, Color.White, _wheelFront.Rotation, _wheel.Origin, new Vector2(0.5f) * _wheel.TexelSize, SpriteEffects.FlipVertically, 0f);
            ScreenManager.SpriteBatch.Draw(_carBody.Texture, _car.Position, null, Color.White, _car.Rotation, _carBody.Origin, new Vector2(5f, 1.27f) * _carBody.TexelSize, SpriteEffects.FlipVertically, 0f);

            // draw teeter
            ScreenManager.SpriteBatch.Draw(_teeter.Texture, _board.Position, null, Color.White, _board.Rotation, _teeter.Origin, new Vector2(20f, 0.5f) * _teeter.TexelSize, SpriteEffects.FlipVertically, 0f);

            // draw bridge
            for (int i = 0; i < _bridgeSegments.Count; ++i)
            {
                ScreenManager.SpriteBatch.Draw(_bridge.Texture, _bridgeSegments[i].Position, null, Color.White, _bridgeSegments[i].Rotation, _bridge.Origin, new Vector2(2.0f, 0.25f) * _bridge.TexelSize, SpriteEffects.FlipVertically, 0f);
            }

            // draw boxes
            for (int i = 0; i < _boxes.Count; ++i)
            {
                ScreenManager.SpriteBatch.Draw(_box.Texture, _boxes[i].Position, null, Color.White, _boxes[i].Rotation, _box.Origin, new Vector2(1f, 1f) * _box.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            ScreenManager.SpriteBatch.End();

            ScreenManager.LineBatch.Begin(Camera.Projection, Camera.View);
            // draw ground
            for (int i = 0; i < _ground.FixtureList.Count; ++i)
            {
                ScreenManager.LineBatch.DrawLineShape(_ground.FixtureList[i].Shape, Color.Black);
            }
            ScreenManager.LineBatch.End();
            base.Draw(gameTime);
        }
    }
}