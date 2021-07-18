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
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class MotorJointTest : Test
    {
        private MotorJoint _joint;
        private float _time;

        MotorJointTest()
        {
            Body ground = World.CreateBody();
            ground.CreateEdge(new Vector2(-20, 0), new Vector2(20, 0));

            // Define motorized body
            Body body = World.CreateBody(new Vector2(0, 8), 0, BodyType.Dynamic);
            var bfixture = body.CreateRectangle(4, 1, 2, Vector2.Zero);
            bfixture.Friction = 0.6f;

            _joint = new MotorJoint(ground, body);
            _joint.MaxForce = 1000.0f;
            _joint.MaxTorque = 1000.0f;

            World.Add(_joint);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (!settings.Pause && settings.Hz > 0.0f)
                _time += 1.0f / settings.Hz;

            Vector2 linearOffset = new Vector2();
            linearOffset.X = 6.0f * (float)Math.Sin(2.0f * _time);
            linearOffset.Y = 8.0f + 4.0f * (float)Math.Sin(1.0f * _time);

            float angularOffset = 4.0f * _time;

            _joint.LinearOffset = linearOffset;
            _joint.AngularOffset = angularOffset;
        }

        public static Test Create()
        {
            return new MotorJointTest();
        }
    }
}