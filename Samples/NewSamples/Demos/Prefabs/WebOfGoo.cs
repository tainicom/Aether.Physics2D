/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class WebOfGoo
    {
        private World _world;
        private float _radius;

        private const float Breakpoint = 100f;

        private List<List<Body>> _ringBodys;
        private List<DistanceJoint> _ringJoints;

        private Sprite _goo;
        private Sprite _link;

        public WebOfGoo(World world, Vector2 position, float radius, int rings, int sides)
        {
            _world = world;
            _radius = radius;

            _ringBodys = new List<List<Body>>(rings);
            _ringJoints = new List<DistanceJoint>();

            for (int i = 1; i < rings; i++)
            {
                Vertices vertices = PolygonTools.CreateCircle(i * 2.9f, sides);
                vertices.Translate(ref position);
                List<Body> bodies = new List<Body>(sides);

                //Create the first goo
                Body previous = world.CreateCircle(radius, 0.2f, vertices[0]);
                previous.BodyType = BodyType.Dynamic;

                bodies.Add(previous);

                //Connect the first goo to the next
                for (int j = 1; j < vertices.Count; j++)
                {
                    Body current = world.CreateCircle(radius, 0.2f, vertices[j]);
                    current.BodyType = BodyType.Dynamic;

                    DistanceJoint joint = new DistanceJoint(previous, current, Vector2.Zero, Vector2.Zero);
                    joint.Frequency = 4.0f;
                    joint.DampingRatio = 0.5f;
                    joint.Breakpoint = Breakpoint;
                    world.Add(joint);
                    _ringJoints.Add(joint);

                    previous = current;
                    bodies.Add(current);
                }

                //Connect the first and the last goo
                DistanceJoint jointClose = new DistanceJoint(bodies[0], bodies[bodies.Count - 1], Vector2.Zero, Vector2.Zero);
                jointClose.Frequency = 4.0f;
                jointClose.DampingRatio = 0.5f;
                jointClose.Breakpoint = Breakpoint;
                world.Add(jointClose);
                _ringJoints.Add(jointClose);

                _ringBodys.Add(bodies);
            }

            //Create an outer ring
            Vertices frame = PolygonTools.CreateCircle(rings * 2.9f - 0.9f, sides);
            frame.Translate(ref position);

            Body anchor = world.CreateBody(position);
            anchor.BodyType = BodyType.Static;

            //Attach the outer ring to the anchor
            for (int i = 0; i < _ringBodys[rings - 2].Count; i++)
            {
                DistanceJoint joint = new DistanceJoint(anchor, _ringBodys[rings - 2][i], frame[i], _ringBodys[rings - 2][i].Position, true);
                joint.Frequency = 8.0f;
                joint.DampingRatio = 0.5f;
                joint.Breakpoint = Breakpoint;
                world.Add(joint);
                _ringJoints.Add(joint);
            }

            //Interconnect the rings
            for (int i = 1; i < _ringBodys.Count; i++)
            {
                for (int j = 0; j < sides; j++)
                {
                    DistanceJoint joint = new DistanceJoint(_ringBodys[i - 1][j], _ringBodys[i][j], Vector2.Zero, Vector2.Zero);
                    joint.Frequency = 4.0f;
                    joint.DampingRatio = 0.5f;
                    joint.Breakpoint = Breakpoint;
                    world.Add(joint);
                    _ringJoints.Add(joint);
                }
            }

            _link = new Sprite(ContentWrapper.GetTexture("Link"));
            _goo = new Sprite(ContentWrapper.GetTexture("Goo"));
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (DistanceJoint j in _ringJoints)
            {
                if (j.Enabled)
                {
                    Vector2 pos = (j.WorldAnchorA + j.WorldAnchorB) / 2f;
                    Vector2 AtoB = j.WorldAnchorB - j.WorldAnchorA;
                    float angle = (float)MathUtils.VectorAngle(Vector2.UnitX, AtoB);
                    float distance = AtoB.Length() + _radius * 2f / 3f;

                    batch.Draw(_link.Texture, pos, null, Color.White, angle, _link.Origin, new Vector2(distance, _radius) * _link.TexelSize, SpriteEffects.FlipVertically, 0f);
                }
            }

            foreach (List<Body> bodyList in _ringBodys)
            {
                foreach (Body body in bodyList)
                {
                    batch.Draw(_goo.Texture, body.Position, null, Color.White, 0f, _goo.Origin, new Vector2(2f * _radius) * _goo.TexelSize, SpriteEffects.FlipVertically, 0f);
                }
            }
        }
    }
}