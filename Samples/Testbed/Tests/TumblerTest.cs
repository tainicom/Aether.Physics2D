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
    public class TumblerTest : Test
    {
        private const int Count = 200;
        private int _count;

        TumblerTest()
        {
            Body ground = World.CreateBody();

            Body tumblerBody = World.CreateBody(new Vector2(0, 10));
            tumblerBody.SleepingAllowed = false;
            tumblerBody.BodyType = BodyType.Dynamic;

            tumblerBody.CreateRectangle(1, 20, 5, new Vector2(10, 0));
            tumblerBody.CreateRectangle(1, 20, 5, new Vector2(-10, 0));
            tumblerBody.CreateRectangle(20, 1, 5, new Vector2(0, 10));
            tumblerBody.CreateRectangle(20, 1, 5, new Vector2(0, -10));

            RevoluteJoint joint = JointFactory.CreateRevoluteJoint(World, ground, tumblerBody, new Vector2(0, 10), Vector2.Zero);
            joint.ReferenceAngle = 0.0f;
            joint.MotorSpeed = 0.05f * MathHelper.Pi;
            joint.MaxMotorTorque = 1e8f;
            joint.MotorEnabled = true;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            
            if (_count < Count)
            {
                Body box = World.CreateRectangle(0.125f * 2, 0.125f * 2, 1, new Vector2(0, 10));
                box.BodyType = BodyType.Dynamic;
                ++_count;
            }
        }
        
        public static Test Create()
        {
            return new TumblerTest();
        }
    }
}