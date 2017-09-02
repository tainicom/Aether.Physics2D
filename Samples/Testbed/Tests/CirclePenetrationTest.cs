/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class CirclePenetrationTest : Test
    {
        private CirclePenetrationTest()
        {
            World.Gravity = Vector2.Zero;

            List<Vertices> borders = new List<Vertices>(4);

            const float borderWidth = 0.2f;
            const float width = 40f;
            const float height = 25f;

            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(-width, 0), 0));

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, borderWidth, new Vector2(0, -height), 0));

            //Right
            borders.Add(PolygonTools.CreateRectangle(borderWidth, height, new Vector2(width, 0), 0));

            Body body = World.CreateCompoundPolygon(borders, 1, new Vector2(0, 20));

            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.Restitution = 1f;
                fixture.Friction = 0;
            }

            Body circle = World.CreateCircle(0.32f, 1);
            circle.BodyType = BodyType.Dynamic;
            circle.SetRestitution(1f);
            circle.SetFriction(0);

            circle.ApplyLinearImpulse(new Vector2(200, 50));
        }

        internal static Test Create()
        {
            return new CirclePenetrationTest();
        }
    }
}