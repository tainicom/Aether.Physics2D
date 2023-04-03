﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class EdgeShapeBenchmarkTest : Test
    {
        private int _count;
        private PolygonShape _polyShape;

        public EdgeShapeBenchmarkTest()
        {
            // Ground body
            {
                Body ground = World.CreateBody();

                float x1 = -20.0f;
                float y1 = 2.0f * (float)Math.Cos(x1 / 10.0f * (float)Math.PI);
                for (int i = 0; i < 80; ++i)
                {
                    float x2 = x1 + 0.5f;
                    float y2 = 2.0f * (float)Math.Cos(x2 / 10.0f * (float)Math.PI);

                    EdgeShape shape = new EdgeShape(new Vector2(x1, y1), new Vector2(x2, y2));
                    ground.CreateFixture(shape);

                    x1 = x2;
                    y1 = y2;
                }
            }

            const float w = 1.0f;
            const float t = 2.0f;
            float b = w / (2.0f + (float)Math.Sqrt(t));
            float s = (float)Math.Sqrt(t) * b;

            Vertices vertices = new Vertices(8);
            vertices.Add(new Vector2(0.5f * s, 0.0f));
            vertices.Add(new Vector2(0.5f * w, b));
            vertices.Add(new Vector2(0.5f * w, b + s));
            vertices.Add(new Vector2(0.5f * s, w));
            vertices.Add(new Vector2(-0.5f * s, w));
            vertices.Add(new Vector2(-0.5f * w, b + s));
            vertices.Add(new Vector2(-0.5f * w, b));
            vertices.Add(new Vector2(-0.5f * s, 0.0f));

            _polyShape = new PolygonShape(vertices,20);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _count++;

            if (_count < 50)
            {
                const float x = 0;
                const float y = 15;

                Body body = World.CreateBody();

                body.Position = new Vector2(x, y);
                body.BodyType = BodyType.Dynamic;

                Fixture fixture = body.CreateFixture(_polyShape);
                fixture.Friction = 0.3f;
            }

            base.Update(settings, gameTime);
        }

    }
}