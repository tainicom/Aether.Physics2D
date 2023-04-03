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

using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class SphereStackTest : Test
    {
        private const int Count = 10;
        private Body[] _bodies = new Body[Count];

        public SphereStackTest()
        {
            //Ground
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            {
                CircleShape shape = new CircleShape(1.0f, 1);

                for (int i = 0; i < Count; ++i)
                {
                    _bodies[i] = World.CreateBody();
                    _bodies[i].BodyType = BodyType.Dynamic;
                    _bodies[i].Position = new Vector2(0.0f, 4.0f + 3.0f * i);

                    _bodies[i].CreateFixture(shape);

                    _bodies[i].LinearVelocity = new Vector2(0.0f, -50.0f);
                }
            }
        }

    }
}