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

using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    /// <summary>
    /// This tests distance joints, body destruction, and joint destruction.
    /// </summary>
    public class WebTest : Test
    {
        private Body[] _bodies = new Body[4];
        private Joint[] _joints = new Joint[8];

        private int _removedBodies;
        private int _removedJoints;

        private WebTest()
        {
            World.JointRemoved += JointRemovedFired;
            World.BodyRemoved += BodyRemovedFired;

            Body ground = World.CreateEdge(new Vector2(-40, 0), new Vector2(40, 0));

            {
                _bodies[0] = World.CreateRectangle(1f, 1f, 5, new Vector2(-5.0f, 5.0f));
                _bodies[0].BodyType = BodyType.Dynamic;

                _bodies[1] = World.CreateRectangle(1f, 1f, 5, new Vector2(5.0f, 5.0f));
                _bodies[1].BodyType = BodyType.Dynamic;

                _bodies[2] = World.CreateRectangle(1f, 1f, 5, new Vector2(5.0f, 15.0f));
                _bodies[2].BodyType = BodyType.Dynamic;

                _bodies[3] = World.CreateRectangle(1f, 1f, 5, new Vector2(-5.0f, 15.0f));
                _bodies[3].BodyType = BodyType.Dynamic;

                DistanceJoint dj = new DistanceJoint(ground, _bodies[0], new Vector2(-10.0f, 0.0f), new Vector2(-0.5f, -0.5f));
                _joints[0] = dj;
                dj.Frequency = 2.0f;
                dj.DampingRatio = 0.0f;
                World.Add(_joints[0]);

                DistanceJoint dj1 = new DistanceJoint(ground, _bodies[1], new Vector2(10.0f, 0.0f), new Vector2(0.5f, -0.5f));
                _joints[1] = dj1;
                dj1.Frequency = 2.0f;
                dj1.DampingRatio = 0.0f;
                World.Add(_joints[1]);

                DistanceJoint dj2 = new DistanceJoint(ground, _bodies[2], new Vector2(10.0f, 20.0f), new Vector2(0.5f, 0.5f));
                _joints[2] = dj2;
                dj2.Frequency = 2.0f;
                dj2.DampingRatio = 0.0f;
                World.Add(_joints[2]);

                DistanceJoint dj3 = new DistanceJoint(ground, _bodies[3], new Vector2(-10.0f, 20.0f), new Vector2(-0.5f, 0.5f));
                _joints[3] = dj3;
                dj3.Frequency = 2.0f;
                dj3.DampingRatio = 0.0f;
                World.Add(_joints[3]);

                DistanceJoint dj4 = new DistanceJoint(_bodies[0], _bodies[1], new Vector2(0.5f, 0.0f), new Vector2(-0.5f, 0.0f));
                _joints[4] = dj4;
                dj4.Frequency = 2.0f;
                dj4.DampingRatio = 0.0f;
                World.Add(_joints[4]);

                DistanceJoint dj5 = new DistanceJoint(_bodies[1], _bodies[2], new Vector2(0.0f, 0.5f), new Vector2(0.0f, -0.5f));
                _joints[5] = dj5;
                dj5.Frequency = 2.0f;
                dj5.DampingRatio = 0.0f;
                World.Add(_joints[5]);

                DistanceJoint dj6 = new DistanceJoint(_bodies[2], _bodies[3], new Vector2(-0.5f, 0.0f), new Vector2(0.5f, 0.0f));
                _joints[6] = dj6;
                dj6.Frequency = 2.0f;
                dj6.DampingRatio = 0.0f;
                World.Add(_joints[6]);

                DistanceJoint dj7 = new DistanceJoint(_bodies[3], _bodies[0], new Vector2(0.0f, -0.5f), new Vector2(0.0f, 0.5f));
                _joints[7] = dj7;
                dj7.Frequency = 2.0f;
                dj7.DampingRatio = 0.0f;
                World.Add(_joints[7]);
            }
        }

        private void BodyRemovedFired(World sender, Body body)
        {
            _removedBodies++;
        }

        private void JointRemovedFired(World sender, Joint joint)
        {
            if (joint is DistanceJoint)
                _removedJoints++;
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            if (keyboardManager.IsNewKeyPress(Keys.B))
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (_bodies[i] != null)
                    {
                        World.Remove(_bodies[i]);
                        _bodies[i] = null;
                        break;
                    }
                }
            }

            if (keyboardManager.IsNewKeyPress(Keys.J))
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (_joints[i] != null)
                    {
                        World.Remove(_joints[i]);
                        _joints[i] = null;
                        break;
                    }
                }
            }

            base.Keyboard(keyboardManager);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("This demonstrates a soft distance joint.");

            DrawString("Press: (b) to delete a body, (j) to delete a joint");

            DrawString("Bodies removed: " + _removedBodies);

            DrawString("Joints removed: " + _removedJoints);
        }

        protected override void JointRemoved(World sender, Joint joint)
        {
            for (int i = 0; i < 8; ++i)
            {
                if (_joints[i] == joint)
                {
                    _joints[i] = null;
                    break;
                }
            }

            base.JointRemoved(sender, joint);
        }

        internal static Test Create()
        {
            return new WebTest();
        }
    }
}