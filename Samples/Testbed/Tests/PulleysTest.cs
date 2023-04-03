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
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class PulleysTest : Test
    {
        private PulleyJoint _joint1;
        const float Y = 16.0f;
        const float L = 12.0f;
        const float A = 1.0f;
        const float B = 2.0f;

        public PulleysTest()
        {
            //Ground
            Body ground = World.CreateBody();
            ground.CreateCircle(2, 0, new Vector2(-10.0f, Y + B + L));
            ground.CreateCircle(2, 0, new Vector2(10.0f, Y + B + L));

            {
                PolygonShape shape = new PolygonShape(5);
                shape.Vertices = PolygonTools.CreateRectangle(A, B);

                Body body1 = World.CreateBody();
                body1.BodyType = BodyType.Dynamic;
                body1.Position = new Vector2(-10.0f, Y);
                body1.CreateFixture(shape);

                Body body2 = World.CreateBody();
                body2.BodyType = BodyType.Dynamic;
                body2.Position = new Vector2(10.0f, Y);

                body2.CreateFixture(shape);

                Vector2 anchor1 = new Vector2(-10.0f, Y + B);
                Vector2 anchor2 = new Vector2(10.0f, Y + B);
                Vector2 groundAnchor1 = new Vector2(-10.0f, Y + B + L);
                Vector2 groundAnchor2 = new Vector2(10.0f, Y + B + L);
                _joint1 = new PulleyJoint(body1, body2, anchor1, anchor2, groundAnchor1, groundAnchor2, 1.5f, true);
                World.Add(_joint1);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            float ratio = _joint1.Ratio;
            float l = _joint1.LengthA + ratio * _joint1.LengthB;
            DrawString(string.Format("L1 + {0:n} * L2 = {1:n}", ratio, l));

        }

    }
}