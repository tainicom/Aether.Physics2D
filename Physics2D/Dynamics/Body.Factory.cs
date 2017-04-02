/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Dynamics
{
    /// <summary>
    /// An easy to use factory for creating bodies
    /// </summary>
    public partial class Body
    {
        /// <summary>
        /// Creates a fixture and attach it to this body.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// Warning: This function is locked during callbacks.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <param name="userData">Application specific data</param>
        /// <returns></returns>
        public virtual Fixture CreateFixture(Shape shape, object userData = null)
        {
            return new Fixture(this, shape, userData);
        }

        public Fixture CreateEdge(Vector2 start, Vector2 end, object userData = null)
        {
            EdgeShape edgeShape = new EdgeShape(start, end);
            return CreateFixture(edgeShape, userData);
        }

        public Fixture CreateChainShape(Vertices vertices, object userData = null)
        {
            ChainShape shape = new ChainShape(vertices);
            return CreateFixture(shape, userData);
        }

        public Fixture CreateLoopShape(Vertices vertices, object userData = null)
        {
            ChainShape shape = new ChainShape(vertices, true);
            return CreateFixture(shape, userData);
        }

        public Fixture CreateRectangle(float width, float height, float density, Vector2 offset, object userData = null)
        {
            Vertices rectangleVertices = PolygonTools.CreateRectangle(width / 2, height / 2);
            rectangleVertices.Translate(ref offset);
            PolygonShape rectangleShape = new PolygonShape(rectangleVertices, density);
            return CreateFixture(rectangleShape, userData);
        }

        public Fixture CreateCircle(float radius, float density, object userData = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            return CreateFixture(circleShape, userData);
        }

        public Fixture CreateCircle(float radius, float density, Vector2 offset, object userData = null)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException("radius", "Radius must be more than 0 meters");

            CircleShape circleShape = new CircleShape(radius, density);
            circleShape.Position = offset;
            return CreateFixture(circleShape, userData);
        }

        public Fixture CreatePolygon(Vertices vertices, float density, object userData = null)
        {
            if (vertices.Count <= 1)
                throw new ArgumentOutOfRangeException("vertices", "Too few points to be a polygon");

            PolygonShape polygon = new PolygonShape(vertices, density);
            return CreateFixture(polygon, userData);
        }

        public Fixture CreateEllipse(float xRadius, float yRadius, int edges, float density, object userData = null)
        {
            if (xRadius <= 0)
                throw new ArgumentOutOfRangeException("xRadius", "X-radius must be more than 0");

            if (yRadius <= 0)
                throw new ArgumentOutOfRangeException("yRadius", "Y-radius must be more than 0");

            Vertices ellipseVertices = PolygonTools.CreateEllipse(xRadius, yRadius, edges);
            PolygonShape polygonShape = new PolygonShape(ellipseVertices, density);
            return CreateFixture(polygonShape, userData);
        }

        public List<Fixture> CreateCompoundPolygon(List<Vertices> list, float density, object userData = null)
        {
            List<Fixture> res = new List<Fixture>(list.Count);

            //Then we create several fixtures using the body
            foreach (Vertices vertices in list)
            {
                if (vertices.Count == 2)
                {
                    EdgeShape shape = new EdgeShape(vertices[0], vertices[1]);
                    res.Add(CreateFixture(shape, userData));
                }
                else
                {
                    PolygonShape shape = new PolygonShape(vertices, density);
                    res.Add(CreateFixture(shape, userData));
                }
            }

            return res;
        }

        public Fixture CreateLineArc(float radians, int sides, float radius, bool closed, object userData = null)
        {
            Vertices arc = PolygonTools.CreateArc(radians, sides, radius);
            arc.Rotate((MathHelper.Pi - radians) / 2);
            return closed ? CreateLoopShape(arc, userData) : CreateChainShape(arc, userData);
        }

        public List<Fixture> CreateSolidArc(float density, float radians, int sides, float radius, object userData = null)
        {
            Vertices arc = PolygonTools.CreateArc(radians, sides, radius);
            arc.Rotate((MathHelper.Pi - radians) / 2);

            //Close the arc
            arc.Add(arc[0]);

            List<Vertices> triangles = Triangulate.ConvexPartition(arc, TriangulationAlgorithm.Earclip);

            return CreateCompoundPolygon(triangles, density, userData);
        }
    }
}