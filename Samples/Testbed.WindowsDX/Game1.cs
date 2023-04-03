//   Copyright 2021 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using tainicom.Aether.Physics2D.Samples.Testbed.Tests;

namespace tainicom.Aether.Physics2D.Samples.Testbed
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {

        private GraphicsDeviceManager _graphics;
        Point windowedSize = new Point(1024, 768);
        private InputState _inputState = new InputState();

        public Matrix Projection;
        public Matrix View;
        private Vector2 _viewCenter;
        private float _viewZoom;
        private Vector2 _mouseMoveBeginViewCenter;
        private MouseState? _mouseMoveBeginState;

        private TestEntry _entry;
        private Test _test;
        private int _testIndex;

        private GameSettings _settings = new GameSettings();
        private ControlPanel _controlPanel;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _graphics.GraphicsProfile = GraphicsProfile.Reach;
            _graphics.PreferMultiSampling = true;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;

            IsMouseVisible = true;
            IsFixedTimeStep = true;

        }

        public float ViewZoom
        {
            get { return _viewZoom; }
            set
            {
                _viewZoom = value;
                UpdateProjection();
            }
        }

        public Vector2 ViewCenter
        {
            get { return _viewCenter; }
            set
            {
                _viewCenter = value;
                UpdateView();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            _controlPanel = new ControlPanel(this);
            Components.Add(_controlPanel);

            base.Initialize();

            //Set window defaults. Parent game can override in constructor
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += WindowClientSizeChanged;


            //Default projection and view
            ResetCamera();

            _testIndex = MathUtils.Clamp(_testIndex, 0, TestEntries.TestList.Length - 1);
            StartTest(_testIndex);
            UpdateProjection();
        }


        private void StartTest(int index)
        {
            // save previous flags
            DebugViewFlags prevFlags = DebugViewFlags.None;
            if (_test != null)
                prevFlags = _test.DebugView.Flags;

            _entry = TestEntries.TestList[index];
            _test = _entry.CreateTest();
            _test.GameInstance = this;
            _test.Initialize();

            // re-enable previous flags
            _test.DebugView.Flags |= prevFlags;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // clear graphics here because some tests already draw during update
            GraphicsDevice.Clear(Color.Black);

            _inputState.Update(this.IsActive);

            // Allows the game to exit
            if (_inputState.IsButtonPressed(Buttons.Back))
                Exit();
            
            if (_inputState.IsKeyReleased(Keys.F11))
                ToggleFullscreen();

            if (_inputState.IsKeyDown(Keys.Z)) // Press 'z' to zoom out.
                ViewZoom = Math.Min((float)Math.Pow(Math.E, -0.05f) * ViewZoom, 20.0f);
            else if (_inputState.IsKeyDown(Keys.X)) // Press 'x' to zoom in.
                ViewZoom = Math.Max((float)Math.Pow(Math.E, +0.05f) * ViewZoom, 0.02f);
            else if (_inputState.IsKeyDown(Keys.Subtract)) // Press '-' to zoom out.
                ViewZoom = Math.Min((float)Math.Pow(Math.E, -0.05f) * ViewZoom, 20.0f);
            else if (_inputState.IsKeyDown(Keys.Add)) // Press '+' to zoom in.
                ViewZoom = Math.Max((float)Math.Pow(Math.E, +0.05f) * ViewZoom, 0.02f);
            else if (_inputState.ScrollWheelDelta != 0) // Mouse Wheel to Zoom.
            {
                var wheelDelta = _inputState.ScrollWheelDelta /120f;
                var zoomFactor = (float)Math.Pow(Math.E, 0.05f * wheelDelta);
                ViewZoom = Math.Min(Math.Max(zoomFactor * ViewZoom, 0.02f), 20.0f);
            }
            else if (_inputState.IsKeyPressed(Keys.R)) // Press 'r' to reset.
                Restart();
            else if (_inputState.IsKeyPressed(Keys.P) || _inputState.IsButtonPressed(Buttons.Start))
                _settings.Pause = !_settings.Pause;
            else if (_inputState.IsKeyPressed(Keys.I) || _inputState.IsButtonPressed(Buttons.LeftShoulder)) // Press I to prev test.
            {
                --_testIndex;
                _testIndex = (_testIndex+ TestEntries.TestList.Length) % TestEntries.TestList.Length;

                StartTest(_testIndex);
                ResetCamera();
                UpdateProjection();
            }
            else if (_inputState.IsKeyPressed(Keys.O) || _inputState.IsButtonPressed(Buttons.RightShoulder)) // Press O to next test.
            {
                ++_testIndex;
                _testIndex = _testIndex % TestEntries.TestList.Length;

                StartTest(_testIndex);
                ResetCamera();
                UpdateProjection();
            }
            

            if (_inputState.IsKeyDown(Keys.Left)) // Press left to pan left.
                ViewCenter += -Vector2.UnitX * 0.5f;
            else if (_inputState.IsKeyDown(Keys.Right)) // Press right to pan right.
                ViewCenter +=  Vector2.UnitX * 0.5f;
            if (_inputState.IsKeyDown(Keys.Down)) // Press down to pan down.
                ViewCenter += -Vector2.UnitY * 0.5f;
            else if (_inputState.IsKeyDown(Keys.Up)) // Press up to pan up.
                ViewCenter +=  Vector2.UnitY * 0.5f;
            if (_inputState.IsKeyPressed(Keys.Home)) // Press home to reset the view.
                ResetCamera();
            else if (_inputState.IsKeyPressed(Keys.F2))
                ToggleDebugDrawFlag(DebugViewFlags.DebugPanel);
            else if (_inputState.IsKeyPressed(Keys.F3))
                ToggleDebugDrawFlag(DebugViewFlags.PerformanceGraph);
            else if (_inputState.IsKeyPressed(Keys.F4))
                ToggleDebugDrawFlag(DebugViewFlags.Shape);
            else if (_inputState.IsKeyPressed(Keys.F5))
                ToggleDebugDrawFlag(DebugViewFlags.AABB);
            else if (_inputState.IsKeyPressed(Keys.F6))
                ToggleDebugDrawFlag(DebugViewFlags.CenterOfMass);
            else if (_inputState.IsKeyPressed(Keys.F7))
                ToggleDebugDrawFlag(DebugViewFlags.Joint);
            else if (_inputState.IsKeyPressed(Keys.F8))
            {
                ToggleDebugDrawFlag(DebugViewFlags.ContactPoints);
                ToggleDebugDrawFlag(DebugViewFlags.ContactNormals);
            }
            else if (_inputState.IsKeyPressed(Keys.F9))
                ToggleDebugDrawFlag(DebugViewFlags.PolygonPoints);
            else
            {
                if (_test != null)
                    _test.Keyboard(_inputState);
            }

            if (_test != null)
                _test.Mouse(_inputState);

            // move camera with Right mouse button
            if (_inputState.IsRightButtonPressed())
            {
                _mouseMoveBeginState = _inputState.MouseState;
                _mouseMoveBeginViewCenter = ViewCenter;
                IsMouseVisible = false;
            }
            if (_inputState.IsRightButtonReleased())
            {
                _mouseMoveBeginState = null;
                _mouseMoveBeginViewCenter = Vector2.Zero;
                IsMouseVisible = true;
            }
            if (_mouseMoveBeginState.HasValue)
            {
                var beginView = Matrix.CreateLookAt(new Vector3(_mouseMoveBeginViewCenter, 1), new Vector3(_mouseMoveBeginViewCenter, 0), Vector3.Up);

                Vector3 beginPos = new Vector3(_mouseMoveBeginState.Value.X, _mouseMoveBeginState.Value.Y, 0f);
                Vector3 pos = new Vector3(_inputState.MouseState.X, _inputState.MouseState.Y, 0f);

#if MG
                if (Mouse.IsRawInputAvailable)
                {
                    beginPos = new Vector3(_mouseMoveBeginState.Value.RawX, _mouseMoveBeginState.Value.RawY, 0f);
                    pos = new Vector3(_inputState.MouseState.RawX, _inputState.MouseState.RawY, 0f);
                }
#endif

                beginPos = GraphicsDevice.Viewport.Unproject(beginPos, Projection, beginView, Matrix.Identity);
                pos = GraphicsDevice.Viewport.Unproject(pos, Projection, beginView, Matrix.Identity);                    
                Vector3 offset = beginPos - pos;

                ViewCenter = _mouseMoveBeginViewCenter + new Vector2(offset.X, offset.Y);
            }

            if (_test != null && _inputState.GamePadState.IsConnected)
                _test.Gamepad(_inputState);

            base.Update(gameTime);

            if (_test != null)
            {
                _test.TextLine = 30;
                _test.Update(_settings, gameTime);
            }

            _test.DebugView.UpdatePerformanceGraph(_test.World.UpdateTime);
        }

        private void ToggleFullscreen()
        {
            if (!_graphics.IsFullScreen)
            {
                windowedSize.X = GraphicsDevice.PresentationParameters.BackBufferWidth;
                windowedSize.Y = GraphicsDevice.PresentationParameters.BackBufferHeight;
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.IsFullScreen = true;
                _graphics.ApplyChanges();
            }
            else
            {
                _graphics.PreferredBackBufferWidth = windowedSize.X;
                _graphics.PreferredBackBufferHeight = windowedSize.Y;
                _graphics.IsFullScreen = false;
                _graphics.ApplyChanges();
            }
        }

        private void ToggleDebugDrawFlag(DebugViewFlags flag)
        {
            if ((_test.DebugView.Flags & flag) == flag)
                _test.DebugView.RemoveFlags(flag);
            else
                _test.DebugView.AppendFlags(flag);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            _test.DrawTitle(50, 15, _entry.Name);

            _test.DrawDebugView(gameTime, ref Projection, ref View);

            base.Draw(gameTime);
        }

        private void ResetCamera()
        {
            ViewZoom = 1f;
            ViewCenter = new Vector2(0.0f, 20.0f);
        }

        private void UpdateProjection()
        {
            Vector2 bounds = new Vector2(84, 64)/2f;
            if (_test != null)
                bounds = _test.Bounds/2f;

            var lower = -new Vector2(bounds.Y * GraphicsDevice.Viewport.AspectRatio, bounds.Y) / ViewZoom;
            var upper =  new Vector2(bounds.Y * GraphicsDevice.Viewport.AspectRatio, bounds.Y) / ViewZoom;

            // L/R/B/T
            Projection = Matrix.CreateOrthographicOffCenter(lower.X, upper.X, lower.Y, upper.Y, 0f, 2f);
        }

        private void UpdateView()
        {
            View = Matrix.CreateLookAt(new Vector3(ViewCenter, 1), new Vector3(ViewCenter, 0), Vector3.Up);
        }

        public Vector2 ConvertWorldToScreen(Vector2 position)
        {
            Vector3 temp = GraphicsDevice.Viewport.Project(new Vector3(position, 0), Projection, View, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }

        public Vector2 ConvertScreenToWorld(int x, int y)
        {
            Vector3 temp = GraphicsDevice.Viewport.Unproject(new Vector3(x, y, 0), Projection, View, Matrix.Identity);
            return new Vector2(temp.X, temp.Y);
        }

        private void Restart()
        {
            StartTest(_testIndex);
        }

        private void WindowClientSizeChanged(object sender, EventArgs e)
        {
            //We want to keep aspec ratio. Recalcuate the projection matrix.
            UpdateProjection();
        }
    }
}