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
using System.IO;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public class Test
    {
        internal DebugView DebugView;
        internal int StepCount;
        internal World World;
        private FixedMouseJoint _fixedMouseJoint;
        internal int TextLine;

        protected Test()
        {
            World = new World(new Vector2(0.0f, -10.0f));

            TextLine = 30;

            World.JointRemoved += JointRemoved;
            World.ContactManager.PreSolve += PreSolve;
            World.ContactManager.PostSolve += PostSolve;

            StepCount = 0;
        }

        public Game1 GameInstance { protected get; set; }

        public virtual void Initialize()
        {
            DebugView = new DebugView(World);
            DebugView.LoadContent(GameInstance.GraphicsDevice, GameInstance.Content);
        }

        protected virtual void JointRemoved(World sender, Joint joint)
        {
            if (_fixedMouseJoint == joint)
                _fixedMouseJoint = null;
        }

        public void DrawTitle(int x, int y, string title)
        {
            DebugView.DrawString(x, y, title);
        }

        public virtual void DrawDebugView(GameTime gameTime, ref Matrix projection, ref Matrix view)
        {
            DebugView.RenderDebugData(ref projection, ref view);
        }

        public virtual void Update(GameSettings settings, GameTime gameTime)
        {
            float timeStep = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f));

            if (settings.Pause)
            {
                if (settings.SingleStep)
                    settings.SingleStep = false;
                else
                    timeStep = 0.0f;

                DrawString("****PAUSED****");
            }
            else
                World.Step(timeStep);

            if (timeStep > 0.0f)
                ++StepCount;
        }

        public virtual void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.F11))
            {
                using (Stream stream = new FileStream("out.xml", FileMode.Create))
                {
                    WorldSerializer.Serialize(World, stream);
                }
            }

            if (keyboardManager.IsNewKeyPress(Keys.F12))
            {
                using (Stream stream = new FileStream("out.xml", FileMode.Open))
                {
                    World = WorldSerializer.Deserialize(stream);
                }
                Initialize();
            }
        }

        public virtual void Gamepad(GamePadState state, GamePadState oldState)
        {
        }

        public virtual void Mouse(MouseState state, MouseState oldState)
        {
            Vector2 position = GameInstance.ConvertScreenToWorld(state.X, state.Y);

            if (state.LeftButton == ButtonState.Released && oldState.LeftButton == ButtonState.Pressed)
                MouseUp();
            else if (state.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
                MouseDown(position);

            MouseMove(position);
        }

        private void MouseDown(Vector2 p)
        {
            if (_fixedMouseJoint != null)
                return;

            Fixture fixture = World.TestPoint(p);

            if (fixture != null)
            {
                Body body = fixture.Body;
                _fixedMouseJoint = new FixedMouseJoint(body, p);
                _fixedMouseJoint.MaxForce = 1000.0f * body.Mass;
                World.Add(_fixedMouseJoint);
                body.Awake = true;
            }
        }

        private void MouseUp()
        {
            if (_fixedMouseJoint != null)
            {
                World.Remove(_fixedMouseJoint);
                _fixedMouseJoint = null;
            }
        }

        private void MouseMove(Vector2 p)
        {
            if (_fixedMouseJoint != null)
                _fixedMouseJoint.WorldAnchorB = p;
        }

        protected virtual void PreSolve(Contact contact, ref Manifold oldManifold)
        {
        }

        protected virtual void PostSolve(Contact contact, ContactVelocityConstraint impulse)
        {
        }

#if WINDOWS
        protected Vertices LoadDataFile(string filename)
        {
            string[] lines = File.ReadAllLines(filename);

            Vertices vertices = new Vertices(lines.Length);

            foreach (string line in lines)
            {
                string[] split = line.Split(' ');
                vertices.Add(new Vector2(float.Parse(split[0]), float.Parse(split[1])));
            }

            return vertices;
        }
#endif

        protected void DrawString(string text)
        {
            DebugView.DrawString(50, TextLine, text);
            TextLine += 15;
        }
    }
}