/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class QuadTreeTest : Test
    {
        private Vector2 _worldSize;

        public override void Initialize()
        {
            Settings.VelocityIterations = 2;
            Settings.PositionIterations = 4;

            GameInstance.ViewCenter = Vector2.Zero;

            _worldSize = 2 * GameInstance.ConvertScreenToWorld(GameInstance.Window.ClientBounds.Width, 0);

            //Create a World using QuadTree constructor
            var worldSpan = new AABB(-_worldSize / 2, _worldSize / 2);
            World = new World(new QuadTreeBroadPhase(worldSpan));

            //Create a World using DynamicTree constructor
            //World = new World();

            //
            //set up gravity
            //
            World.Gravity = new Vector2(0.0f, -10.0f);

            //
            //set up border
            //

            float halfWidth = _worldSize.X / 2 - 2f;
            float halfHeight = _worldSize.Y / 2 - 2f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, halfHeight));
            borders.Add(new Vector2(halfWidth, -halfHeight));
            borders.Add(new Vector2(-halfWidth, -halfHeight));

            Body anchor = World.CreateLoopShape(borders);
            anchor.SetCollisionCategories(Category.All);
            anchor.SetCollidesWith(Category.All);

            //
            //box
            //

            Vertices bigbox = PolygonTools.CreateRectangle(3f, 3f);
            PolygonShape bigshape = new PolygonShape(bigbox, 5);

            Body bigbody = World.CreateBody();
            bigbody.BodyType = BodyType.Dynamic;
            bigbody.Position = Vector2.UnitX * 25;
            bigbody.CreateFixture(bigshape);

            World.Remove(bigbody);

            //
            //populate
            //
            const int rad = 12;
            const float a = 0.6f;
            const float sep = 0.000f;

            Vector2 cent = Vector2.Zero;

            for (int y = -rad; y <= +rad; y++)
            {
                int xrad = (int)Math.Round(Math.Sqrt(rad * rad - y * y));
                for (int x = -xrad; x <= +xrad; x++)
                {
                    Vector2 pos = cent + new Vector2(x * (2 * a + sep), y * (2 * a + sep));
                    Body cBody = World.CreateCircle(a, 55, pos);
                    cBody.BodyType = BodyType.Dynamic;
                }
            }

            base.Initialize();

        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            GameInstance.ViewCenter = Vector2.Zero;

            base.Update(settings, gameTime);
        }

        internal static Test Create()
        {
            return new QuadTreeTest();
        }
    }
}