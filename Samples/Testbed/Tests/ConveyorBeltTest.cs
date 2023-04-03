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

using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class ConveyorBeltTest : Test
    {
        private Fixture _platform;

        public ConveyorBeltTest()
        {
            // Ground
            {
                Body ground = World.CreateBody();
                ground.CreateEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));
            }

            // Platform
            {
                Body body = World.CreateBody(new Vector2(-5, 5));
                _platform = body.CreateRectangle(20, 1f, 1, Vector2.Zero);
                _platform.Friction = 0.8f;
            }

            // Boxes
            for (int i = 0; i < 5; ++i)
            {
                Body body = World.CreateRectangle(1f, 1f, 20, new Vector2(-10.0f + 2.0f * i, 7.0f));
                body.BodyType = BodyType.Dynamic;
            }
        }

        protected override void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            base.PreSolve(contact, ref oldManifold);

            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;

            if (fixtureA == _platform)
            {
                contact.TangentSpeed = 5.0f;
            }

            if (fixtureB == _platform)
            {
                contact.TangentSpeed = -5.0f;
            }
        }

    }
}