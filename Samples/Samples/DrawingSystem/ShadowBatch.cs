// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Common.Maths;

namespace tainicom.Aether.Physics2D.Samples.DrawingSystem
{
    public class ShadowBatch
    {
        const int CircleSegments = 32;
        Complex circleSegmentRotation = Complex.FromAngle((float)(MathHelper.Pi * 2.0 / CircleSegments));

        private VertexPositionColor[] _vertices;

        public VertexPositionColor[] Vertices { get { return _vertices; } }
        public int VerticesCount { get; private set; }

        public ShadowBatch(int bufferSize = 512)
        {
            _vertices = new VertexPositionColor[bufferSize * 3];
            Clear();
        }

        public void Clear()
        {
            VerticesCount = 0;
        }

        private void GrowBuffer()
        {
            var newSize = _vertices.Length + 512 * 3;
            Array.Resize(ref _vertices, newSize);
        }

        public int AddVertex(Vector3 position)
        {
            if (VerticesCount >= _vertices.Length)
                GrowBuffer();

            _vertices[VerticesCount].Position = position;
            _vertices[VerticesCount].Color = Color.White;
            return VerticesCount++;
        }

        public void AddSegment(Vector2 start, Vector2 end, bool flipNormal = false)
        {
            if (flipNormal)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }

            float hh = 1f / 2f; // half height

            AddVertex(new Vector3(start, -hh));
            AddVertex(new Vector3(end, -hh));
            AddVertex(new Vector3(end, hh));

            AddVertex(new Vector3(end, hh));
            AddVertex(new Vector3(start, hh));
            AddVertex(new Vector3(start, -hh));
        }
        
        private void AddCicle(Vector2 center, float radius, bool flipNormal = false)
        {
            Vector2 v2 = new Vector2(radius, 0);
            var center_vS = center + v2;

            for (int i = 0; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = v2;
                v2 = Complex.Multiply(ref v1, ref circleSegmentRotation);

                // Draw Circle
                AddSegment(center + v1, center + v2, flipNormal);
            }
            // Close Circle
            AddSegment(center + v2, center_vS, flipNormal);
        }

        public void AddShape(Fixture fixture, bool flipNormal = false)
        {
            Transform xf;
            fixture.Body.GetTransform(out xf);

            switch (fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        var circle = (CircleShape)fixture.Shape;
                        Vector2 center = Transform.Multiply(circle.Position, ref xf);
                        AddCicle(center, circle.Radius, flipNormal);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;

                        Vector2 vS = Transform.Multiply(poly.Vertices[0], ref xf);
                        Vector2 v2 = vS;
                        for (int i = 1; i < poly.Vertices.Count; ++i)
                        {
                            Vector2 v1 = v2;
                            v2 = Transform.Multiply(poly.Vertices[i], ref xf);
                            AddSegment(v1, v2, flipNormal);
                        }
                        AddSegment(v2, vS, flipNormal);
                    }
                    break;

                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = Transform.Multiply(edge.Vertex1, ref xf);
                        Vector2 v2 = Transform.Multiply(edge.Vertex2, ref xf);
                        AddSegment(v1, v2);
                        AddSegment(v2, v1); // double face
                    }
                    break;

                case ShapeType.Chain:
                    {
                        ChainShape chain = (ChainShape)fixture.Shape;
                        for (int i = 0; i < chain.Vertices.Count - 1; ++i)
                        {
                            Vector2 v1 = Transform.Multiply(chain.Vertices[i], ref xf);
                            Vector2 v2 = Transform.Multiply(chain.Vertices[i + 1], ref xf);
                            AddSegment(v1, v2);
                            AddSegment(v2, v1); // double face
                        }
                    }
                    break;
            }
            return;
        }

        public void AddBody(Body body, bool flipNormal = false)
        {
            foreach (Fixture f in body.FixtureList)
                AddShape(f, flipNormal);
        }

        internal void AddWorld(World world, bool flipNormal = false)
        {
            foreach (Body b in world.BodyList)
                AddBody(b, flipNormal);
        }
    }
}
