/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class BuoyancyTest : Test
    {
        private BuoyancyTest()
        {
            World.Gravity = new Vector2(0, -9.82f);

            World.CreateEdge(new Vector2(-40, 0), new Vector2(40, 0));

            float offset = 5;
            for (int i = 0; i < 3; i++)
            {
                Body rectangle = World.CreateRectangle(2, 2, 1, new Vector2(-30 + offset, 20));
                rectangle.Rotation = Rand.RandomFloat(0, 3.14f);
                rectangle.BodyType = BodyType.Dynamic;
                offset += 7;
            }

            for (int i = 0; i < 3; i++)
            {
                Body rectangle = World.CreateCircle(1, 1, new Vector2(-30 + offset, 20));
                rectangle.Rotation = Rand.RandomFloat(0, 3.14f);
                rectangle.BodyType = BodyType.Dynamic;
                offset += 7;
            }

            AABB container = new AABB(new Vector2(0, 10), 60, 10);
            BuoyancyController buoyancy = new BuoyancyController(container, 2, 2, 1, World.Gravity);
            World.Add(buoyancy);
        }

        internal static Test Create()
        {
            return new BuoyancyTest();
        }
    }
}