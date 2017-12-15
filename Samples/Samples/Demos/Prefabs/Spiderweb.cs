/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Spiderweb
    {
        private World _world;
        private float _radius;

        private Sprite _goo;
        private Sprite _link;


        public Spiderweb(World world, Body ground, Vector2 position, float radius, int rings, int sides)
        {
            _world = world;
            _radius = radius;

            const float breakpoint = 100f;


            List<List<Body>> ringBodys = new List<List<Body>>(rings);

            for (int i = 1; i < rings; ++i)
            {
                Vertices vertices = PolygonTools.CreateCircle(i * 2.9f, sides);
                List<Body> bodies = new List<Body>(sides);

                //Create the first goo
                Body prev = world.CreateCircle(radius, 0.2f, vertices[0]);
                prev.FixedRotation = true;
                prev.Position += position;
                prev.BodyType = BodyType.Dynamic;

                bodies.Add(prev);

                //Connect the first goo to the next
                for (int j = 1; j < vertices.Count; ++j)
                {
                    Body bod = world.CreateCircle(radius, 0.2f, vertices[j]);
                    bod.FixedRotation = true;
                    bod.BodyType = BodyType.Dynamic;
                    bod.Position += position;

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prev, bod, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                    dj.Breakpoint = breakpoint;

                    prev = bod;
                    bodies.Add(bod);
                }

                //Connect the first and the last goo
                DistanceJoint djEnd = JointFactory.CreateDistanceJoint(world, bodies[0], bodies[bodies.Count - 1], Vector2.Zero, Vector2.Zero);
                djEnd.Frequency = 4.0f;
                djEnd.DampingRatio = 0.5f;
                djEnd.Breakpoint = breakpoint;

                ringBodys.Add(bodies);
            }

            //Create an outer ring
            Vertices lastRing = PolygonTools.CreateCircle(rings * 2.9f, sides);
            lastRing.Translate(ref position);

            List<Body> lastRingBodies = ringBodys[ringBodys.Count - 1];

            //Fix each of the body of the outer ring
            for (int j = 0; j < lastRingBodies.Count; ++j)
            {
                lastRingBodies[j].BodyType = BodyType.Static;
            }

            //Interconnect the rings
            for (int i = 1; i < ringBodys.Count; i++)
            {
                List<Body> prev = ringBodys[i - 1];
                List<Body> current = ringBodys[i];

                for (int j = 0; j < prev.Count; j++)
                {
                    Body prevFixture = prev[j];
                    Body currentFixture = current[j];

                    DistanceJoint dj = JointFactory.CreateDistanceJoint(world, prevFixture, currentFixture, Vector2.Zero, Vector2.Zero);
                    dj.Frequency = 4.0f;
                    dj.DampingRatio = 0.5f;
                }
            }
        }

        public void LoadContent(ContentManager content)
        {
            _link = new Sprite(content.Load<Texture2D>("Samples/link"));
            _goo = new Sprite(content.Load<Texture2D>("Samples/goo"));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Joint j in _world.JointList)
            {
                if (j.Enabled && j.JointType != JointType.FixedMouse)
                {
                    Vector2 pos = (j.WorldAnchorA + j.WorldAnchorB) / 2f;
                    Vector2 AtoB = j.WorldAnchorB - j.WorldAnchorA;
                    float angle = (float)MathUtils.VectorAngle(Vector2.UnitX, AtoB);
                    float distance = AtoB.Length() + _radius * 2f/3f;

                    batch.Draw(_link.Texture, pos, null, Color.White, angle, _link.Origin, new Vector2(distance, _radius) * _link.TexelSize, SpriteEffects.FlipVertically, 0f);
                }
            }

            foreach (Body b in _world.BodyList)
            {
                if (b.Enabled && b.FixtureList.Count > 0 && b.FixtureList[0].Shape.ShapeType == ShapeType.Circle)
                {
                    batch.Draw(_goo.Texture, b.Position, null, Color.White, 0f, _goo.Origin, new Vector2(2f * _radius) * _goo.TexelSize, SpriteEffects.FlipVertically, 0f);
                }
            }
        }
    }
}