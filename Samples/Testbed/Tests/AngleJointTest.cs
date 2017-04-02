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
    public class AngleJointTest : Test
    {
        private AngleJointTest()
        {
            World.CreateEdge(new Vector2(-40, 0), new Vector2(40, 0));

            Body fA = World.CreateRectangle(4, 4, 1, new Vector2(-5, 4));
            fA.BodyType = BodyType.Dynamic;

            Body fB = World.CreateRectangle(4, 4, 1, new Vector2(5, 4));
            fB.BodyType = BodyType.Dynamic;

            AngleJoint joint = new AngleJoint(fA, fB);
            joint.TargetAngle = (float)Math.PI / 2;
            World.Add(joint);

            //Keep a body at a fixed angle without a joint 
            Body fC = World.CreateRectangle(4, 4, 1, new Vector2(10, 4));
            fC.BodyType = BodyType.Dynamic;
            fC.Rotation = (float)(Math.PI / 3);
            fC.FixedRotation = true; // Or set the Inertia to float.MaxValue
        }

        internal static Test Create()
        {
            return new AngleJointTest();
        }
    }
}