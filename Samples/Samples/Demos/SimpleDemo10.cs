/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class SimpleDemo10 : PhysicsGameScreen, IDemoScreen
    {
        private Agent _agent;
        private Border _border;
        private Objects _circles;
        private Objects _gears;
        private Objects _rectangles;
        private Objects _stars;

        private bool _is3D = false;
        private float _cameraBlend = 0;
        Matrix _view = Matrix.Identity;
        Matrix _proj = Matrix.Identity;
        
        #region IDemoScreen Members

        public string GetTitle()
        {
            return "3D Camera";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to setup a 3D camera.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Move agent: A,S,D,W");
            sb.AppendLine("  - Move left/right: Left-Ctrl and A,D");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, ScreenManager, Camera);

            _agent = new Agent(World, ScreenManager, Vector2.Zero);
            _agent.Body.AngularDamping = _agentAngularDamping;
            _agent.Body.LinearDamping = _agentLinearDamping;

            Vector2 startPosition = new Vector2(-20f, 11f);
            Vector2 endPosition = new Vector2(20, 11f);
            _circles = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.6f, ObjectType.Circle);
            
            startPosition = new Vector2(-20, -11f);
            endPosition = new Vector2(20, -11f);
            _rectangles = new Objects(World, ScreenManager, startPosition, endPosition, 15, 1.2f, ObjectType.Rectangle);
            
            startPosition = new Vector2(-20, -7);
            endPosition = new Vector2(-20, 7);
            _gears = new Objects(World, ScreenManager, startPosition, endPosition, 5, 0.6f, ObjectType.Gear);
            
            startPosition = new Vector2(20, -7);
            endPosition = new Vector2(20, 7);
            _stars = new Objects(World, ScreenManager, startPosition, endPosition, 5, 0.6f, ObjectType.Star);
            
            SetUserAgent(_agent.Body, _agentForce, _agentTorque);
        }

        private float _agentForce = 1000f;
        private float _agentTorque = 400f;
        private float _agentAngularDamping = 8.0f;
        private float _agentLinearDamping = 8.0f;
        
        public override void HandleInput(InputHelper input, GameTime gameTime)
        {            
            if (input.IsNewKeyPress(Keys.Space))
                _is3D = !_is3D;

            base.HandleInput(input, gameTime);
        }


        protected override void HandleCursor(InputHelper input)
        {
            //ConvertScreenToWorld
            var p0 = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(input.Cursor, 0), _proj, _view, Matrix.Identity);
            var p1 = ScreenManager.GraphicsDevice.Viewport.Unproject(new Vector3(input.Cursor, 1), _proj, _view, Matrix.Identity);
            var d = p1-p0;
            d.Normalize();
            var ray = new Ray(p0, p1-p0);
            var plane = new Plane(Vector3.Backward, 0);
            float? t = ray.Intersects(plane);
            p0 = ray.Position + ray.Direction * t.GetValueOrDefault(1f);
            Vector2 position = new Vector2(p0.X, p0.Y);

            if ((input.IsNewButtonPress(Buttons.A) || input.IsNewMouseButtonPress(MouseButtons.LeftButton)) && _fixedMouseJoint == null)
            {
                Fixture savedFixture = World.TestPoint(position);
                if (savedFixture != null)
                {
                    Body body = savedFixture.Body;
                    _fixedMouseJoint = new FixedMouseJoint(body, position);
                    _fixedMouseJoint.MaxForce = 50.0f * body.Mass;
                    World.Add(_fixedMouseJoint);
                    body.Awake = true;
                }
            }

            if ((input.IsNewButtonRelease(Buttons.A) || input.IsNewMouseButtonRelease(MouseButtons.LeftButton)) && _fixedMouseJoint != null)
            {
                World.Remove(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }

            if (_fixedMouseJoint != null)
                _fixedMouseJoint.WorldAnchorB = position;
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (_is3D && _cameraBlend < 1f)
                _cameraBlend = MathHelper.Clamp(_cameraBlend += 2.0f * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, 1f);
            if (!_is3D && _cameraBlend > 0f)
                _cameraBlend = MathHelper.Clamp(_cameraBlend -= 2.0f * (float)gameTime.ElapsedGameTime.TotalSeconds, 0f, 1f);

            // update camera
            _view = Camera.View;
            _proj = Camera.Projection;

            if (_cameraBlend > 0)
            {
                Transform xf = _agent.Body.GetTransform();
                var pos = xf.p;
                var forward = xf.q.ToVector2();
                var left = MathUtils.Rot90(ref forward);
                var pos3D = new Vector3(pos, 0);
                var forward3D = new Vector3(forward, 0);
                var left3D = new Vector3(left, 0);
                var up3D = Vector3.Backward;
                var height = up3D * 3.0f;

                var aspectRatio = ScreenManager.GraphicsDevice.Viewport.AspectRatio;
                var fpView = Matrix.CreateLookAt(pos3D + height - forward3D * 6f, pos3D + forward3D * 30f, up3D);
                var fpProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 4f, aspectRatio, 0.001f, 60f);

                _view = BlendMatrix(_view, fpView, _cameraBlend);
                _proj = BlendMatrix(_proj, fpProj, _cameraBlend);
            }

        }

        protected override void HandleUserAgent(InputHelper input)
        {
            var _userAgent = _agent.Body;

            Vector2 force = _agentForce * new Vector2(input.GamePadState.ThumbSticks.Right.X, -input.GamePadState.ThumbSticks.Right.Y);
            float torque = _agentTorque * (input.GamePadState.Triggers.Right - input.GamePadState.Triggers.Left);

            _userAgent.ApplyForce(force);
            _userAgent.ApplyTorque(torque);

            float forceAmount = _agentForce * 0.6f;

            force = Vector2.Zero;
            torque = 0;

            Transform xf = _userAgent.GetTransform();
            var forward = xf.q.ToVector2();
            var left = MathUtils.Rot90(ref forward);


            if (input.KeyboardState.IsKeyDown(Keys.W))
                force += forward * forceAmount;
            if (input.KeyboardState.IsKeyDown(Keys.S))
                force -= forward * forceAmount;
            if (input.KeyboardState.IsKeyDown(Keys.A) && !input.KeyboardState.IsKeyDown(Keys.LeftControl))
                torque += _agentTorque;
            if (input.KeyboardState.IsKeyDown(Keys.D) && !input.KeyboardState.IsKeyDown(Keys.LeftControl))
                torque -= _agentTorque;
            if (input.KeyboardState.IsKeyDown(Keys.A) && input.KeyboardState.IsKeyDown(Keys.LeftControl))
                force += left * forceAmount;
            if (input.KeyboardState.IsKeyDown(Keys.D) && input.KeyboardState.IsKeyDown(Keys.LeftControl))
                force -= left * forceAmount;
            
            _userAgent.ApplyForce(force);
            _userAgent.ApplyTorque(torque);
        }

        public override void Draw(GameTime gameTime)
        {   
            ScreenManager.BatchEffect.View = _view;
            ScreenManager.BatchEffect.Projection = _proj;
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            _agent.Draw();
            _circles.Draw();
            _rectangles.Draw();
            _stars.Draw();
            _gears.Draw();
            ScreenManager.SpriteBatch.End();
            _border.Draw();


            //base.Draw(gameTime);
            DebugView.RenderDebugData(_proj, _view);
        }

        private Matrix BlendMatrix(Matrix a, Matrix b, float amount)
        {
            return new Matrix(
                MathHelper.Lerp(a.M11, b.M11, amount),
                MathHelper.Lerp(a.M12, b.M12, amount),
                MathHelper.Lerp(a.M13, b.M13, amount),
                MathHelper.Lerp(a.M14, b.M14, amount),

                MathHelper.Lerp(a.M21, b.M21, amount),
                MathHelper.Lerp(a.M22, b.M22, amount),
                MathHelper.Lerp(a.M23, b.M23, amount),
                MathHelper.Lerp(a.M24, b.M24, amount),
                
                MathHelper.Lerp(a.M31, b.M31, amount),
                MathHelper.Lerp(a.M32, b.M32, amount),
                MathHelper.Lerp(a.M33, b.M33, amount),
                MathHelper.Lerp(a.M34, b.M34, amount),
                
                MathHelper.Lerp(a.M41, b.M41, amount),
                MathHelper.Lerp(a.M42, b.M42, amount),
                MathHelper.Lerp(a.M43, b.M43, amount),
                MathHelper.Lerp(a.M44, b.M44, amount)
                );
        }
    }
}