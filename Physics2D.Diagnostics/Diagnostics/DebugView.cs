// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Maths;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Diagnostics
{
    /// <summary>
    /// A debug view shows you what happens inside the physics engine. You can view
    /// bodies, joints, fixtures and more.
    /// </summary>
    public class DebugView : DebugViewBase, IDisposable
    {
        //Drawing
        private IPrimitiveBatch _primitiveBatch;
        private SpriteBatch _batch;
        private SpriteFont _font;
        private GraphicsDevice _device;
        private Vector2[] _tempVertices = new Vector2[Settings.MaxPolygonVertices];
        private List<StringData> _stringData;

        private Matrix _localProjection;
        private Matrix _localView;
        private Matrix _localWorld;

        //Shapes
        public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);
        public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);
        public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);
        public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);
        public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);
        public Color TextColor = Color.White;

        //Contacts
        private int _pointCount;
        private const int MaxContactPoints = 2048;
        private ContactPoint[] _points = new ContactPoint[MaxContactPoints];

        //Debug panel
        public Vector2 DebugPanelPosition = new Vector2(55, 100);
        private TimeSpan _min;
        private TimeSpan _max;
        private TimeSpan _avg;
        private StringBuilder _graphSbMax = new StringBuilder();   
        private StringBuilder _graphSbAvg = new StringBuilder();   
        private StringBuilder _graphSbMin = new StringBuilder();   
        private StringBuilder _debugPanelSbObjects = new StringBuilder();        
        private StringBuilder _debugPanelSbUpdate = new StringBuilder();

        //Performance graph
        private bool _updatePerformanceGraphCalled = false;
        public bool AdaptiveLimits = true;
        public int ValuesToGraph = 500;
        public TimeSpan MinimumValue;
        public TimeSpan MaximumValue = TimeSpan.FromMilliseconds(10);
        private List<TimeSpan> _graphValues = new List<TimeSpan>(500);
        public Rectangle PerformancePanelBounds = new Rectangle(330, 100, 200, 100);
        private Vector2[] _background = new Vector2[4];
        public bool Enabled = true;
        
        public const int CircleSegments = 32;
        private Complex circleSegmentRotation = Complex.FromAngle((float)(Math.PI * 2.0 / CircleSegments));

        public DebugView(World world)
            : base(world)
        {
            world.ContactManager.PreSolve += PreSolve;

            //Default flags
            AppendFlags(DebugViewFlags.Shape);
            AppendFlags(DebugViewFlags.Controllers);
            AppendFlags(DebugViewFlags.Joint);
        }

        #region IDisposable Members

        public void Dispose()
        {
            World.ContactManager.PreSolve -= PreSolve;
        }

        #endregion

        private void PreSolve(Contact contact, ref Manifold oldManifold)
        {
            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                Manifold manifold = contact.Manifold;

                if (manifold.PointCount == 0)
                    return;

                Fixture fixtureA = contact.FixtureA;

                FixedArray2<PointState> state1, state2;
                Collision.Collision.GetPointStates(out state1, out state2, ref oldManifold, ref manifold);

                FixedArray2<Vector2> points;
                Vector2 normal;
                contact.GetWorldManifold(out normal, out points);

                for (int i = 0; i < manifold.PointCount && _pointCount < MaxContactPoints; ++i)
                {
                    if (fixtureA == null)
                        _points[i] = new ContactPoint();

                    ContactPoint cp = _points[_pointCount];
                    cp.Position = points[i];
                    cp.Normal = normal;
                    cp.State = state2[i];
                    _points[_pointCount] = cp;
                    ++_pointCount;
                }
            }
        }

        /// <summary>
        /// Call this to draw shapes and other debug draw data.
        /// </summary>
        private void DrawDebugData()
        {
            if ((Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf = b.GetTransform();
                    foreach (Fixture f in b.FixtureList)
                    {
                        if (b.Enabled == false)
                            DrawShape(f, xf, InactiveShapeColor);
                        else if (b.BodyType == BodyType.Static)
                            DrawShape(f, xf, StaticShapeColor);
                        else if (b.BodyType == BodyType.Kinematic)
                            DrawShape(f, xf, KinematicShapeColor);
                        else if (b.Awake == false)
                            DrawShape(f, xf, SleepingShapeColor);
                        else
                            DrawShape(f, xf, DefaultShapeColor);
                    }
                }
            }

            if ((Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
            {
                const float axisScale = 0.3f;

                for (int i = 0; i < _pointCount; ++i)
                {
                    ContactPoint point = _points[i];

                    if (point.State == PointState.Add)
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
                    else if (point.State == PointState.Persist)
                        DrawPoint(point.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));

                    if ((Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
                    {
                        Vector2 p1 = point.Position;
                        Vector2 p2 = p1 + axisScale * point.Normal;
                        DrawSegment(p1, p2, new Color(0.4f, 0.9f, 0.4f));
                    }
                }

                _pointCount = 0;
            }

            if ((Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
            {
                foreach (Body body in World.BodyList)
                {
                    foreach (Fixture f in body.FixtureList)
                    {
                        PolygonShape polygon = f.Shape as PolygonShape;
                        if (polygon != null)
                        {
                            Transform xf = body.GetTransform();

                            for (int i = 0; i < polygon.Vertices.Count; i++)
                            {
                                Vector2 tmp = Transform.Multiply(polygon.Vertices[i], ref xf);
                                DrawPoint(tmp, 0.1f, Color.Red);
                            }
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
            {
                foreach (Joint j in World.JointList)
                {
                    DrawJoint(j);
                }
            }

            if ((Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
            {
                Color color = new Color(0.9f, 0.3f, 0.9f);
                IBroadPhase bp = World.ContactManager.BroadPhase;

                foreach (Body body in World.BodyList)
                {
                    if (body.Enabled == false)
                        continue;

                    foreach (Fixture f in body.FixtureList)
                    {
                        for (int t = 0; t < f.ProxyCount; ++t)
                        {
                            FixtureProxy proxy = f.Proxies[t];
                            AABB aabb;
                            bp.GetFatAABB(proxy.ProxyId, out aabb);

                            DrawAABB(ref aabb, color);
                        }
                    }
                }
            }

            if ((Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
            {
                foreach (Body b in World.BodyList)
                {
                    Transform xf = b.GetTransform();
                    xf.p = b.WorldCenter;
                    DrawTransform(ref xf);
                }
            }

            if ((Flags & DebugViewFlags.Controllers) == DebugViewFlags.Controllers)
            {
                for (int i = 0; i < World.ControllerList.Count; i++)
                {
                    Controller controller = World.ControllerList[i];

                    BuoyancyController buoyancy = controller as BuoyancyController;
                    if (buoyancy != null)
                    {
                        AABB container = buoyancy.Container;
                        DrawAABB(ref container, Color.LightBlue);
                    }
                }
            }

            if ((Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
                DrawDebugPanel();
        }

        private void DrawPerformanceGraph()
        {
            float x = PerformancePanelBounds.X;
            float deltaX = PerformancePanelBounds.Width / (float)ValuesToGraph;
            float yScale = PerformancePanelBounds.Bottom - (float)PerformancePanelBounds.Top;

            // we must have at least 2 values to start rendering
            if (_graphValues.Count > 2)
            {
                _min = TimeSpan.MaxValue;
                _max = TimeSpan.Zero;
                _avg = TimeSpan.Zero;
                for (int i = 0; i<_graphValues.Count; i++)
                {
                    var val = _graphValues[i];
                    _min = TimeSpan.FromTicks(Math.Min(_min.Ticks, val.Ticks));
                    _max = TimeSpan.FromTicks(Math.Max(_max.Ticks, val.Ticks));
                    _avg += val;
                }
                _avg = TimeSpan.FromTicks(_avg.Ticks / _graphValues.Count);

                if (AdaptiveLimits)
                {
                    MaximumValue = _max;
                    MinimumValue = TimeSpan.Zero;
                }

                // start at last value (newest value added)
                // continue until no values are left
                for (int i = _graphValues.Count - 1; i > 0; i--)
                {
                    float y1 = PerformancePanelBounds.Bottom - (((yScale * _graphValues[i].Ticks) / (MaximumValue - MinimumValue).Ticks));
                    float y2 = PerformancePanelBounds.Bottom - (((yScale * _graphValues[i - 1].Ticks) / (MaximumValue - MinimumValue).Ticks));

                    Vector2 x1 = new Vector2(MathHelper.Clamp(x, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(y1, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));
                    Vector2 x2 = new Vector2(MathHelper.Clamp(x + deltaX, PerformancePanelBounds.Left, PerformancePanelBounds.Right), MathHelper.Clamp(y2, PerformancePanelBounds.Top, PerformancePanelBounds.Bottom));

                    DrawSegment(x1, x2, Color.LightGreen);

                    x += deltaX;
                }
            }

            _graphSbMax.Clear(); _graphSbAvg.Clear(); _graphSbMin.Clear();
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Top, _graphSbMax.Append("Max: ").AppendNumber((float)_max.TotalMilliseconds, 3).Append(" ms"));
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Center.Y - 7, _graphSbAvg.Append("Avg: ").AppendNumber((float)_avg.TotalMilliseconds, 3).Append(" ms"));
            DrawString(PerformancePanelBounds.Right + 10, PerformancePanelBounds.Bottom - 15, _graphSbMin.Append("Min: ").AppendNumber((float)_min.TotalMilliseconds, 3).Append(" ms"));

            //Draw background.
            _background[0] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y);
            _background[1] = new Vector2(PerformancePanelBounds.X, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[2] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y + PerformancePanelBounds.Height);
            _background[3] = new Vector2(PerformancePanelBounds.X + PerformancePanelBounds.Width, PerformancePanelBounds.Y);

            DrawSolidPolygon(_background, 4, Color.DarkGray, true);
        }

        public void UpdatePerformanceGraph(TimeSpan updateTime)
        {
            _graphValues.Add(updateTime);

            if (_graphValues.Count > ValuesToGraph + 1)
                _graphValues.RemoveAt(0);

            _updatePerformanceGraphCalled = true;
        }

        private void DrawDebugPanel()
        {
            int fixtureCount = 0;
            for (int i = 0; i < World.BodyList.Count; i++)
            {
                fixtureCount += World.BodyList[i].FixtureList.Count;
            }

            int x = (int)DebugPanelPosition.X;
            int y = (int)DebugPanelPosition.Y;
            
            _debugPanelSbObjects.Clear();
            _debugPanelSbObjects.Append("Objects:").AppendLine();
            _debugPanelSbObjects.Append("- Bodies:   ").AppendNumber(World.BodyList.Count).AppendLine();
            _debugPanelSbObjects.Append("- Fixtures: ").AppendNumber(fixtureCount).AppendLine();
            _debugPanelSbObjects.Append("- Contacts: ").AppendNumber(World.ContactCount).AppendLine();
            _debugPanelSbObjects.Append("- Proxies:  ").AppendNumber(World.ProxyCount).AppendLine();  
            _debugPanelSbObjects.Append("- Joints:   ").AppendNumber(World.JointList.Count).AppendLine();
            _debugPanelSbObjects.Append("- Controllers: ").AppendNumber(World.ControllerList.Count).AppendLine();
            DrawString(x, y, _debugPanelSbObjects);
            
            _debugPanelSbUpdate.Clear();
            _debugPanelSbUpdate.Append("Update time:").AppendLine();
            _debugPanelSbUpdate.Append("- Body:    ").AppendNumber(  (float)World.SolveUpdateTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            _debugPanelSbUpdate.Append("- Contact: ").AppendNumber(  (float)World.ContactsUpdateTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            _debugPanelSbUpdate.Append("- CCD:     ").AppendNumber(  (float)World.ContinuousPhysicsTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            _debugPanelSbUpdate.Append("- Joint:   ").AppendNumber(  (float)World.Island.JointUpdateTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            _debugPanelSbUpdate.Append("- Controller:").AppendNumber((float)World.ControllersUpdateTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            _debugPanelSbUpdate.Append("- Total:   ").AppendNumber(  (float)World.UpdateTime.TotalMilliseconds, 3).Append(" ms").AppendLine();
            DrawString(x + 110, y, _debugPanelSbUpdate);
        }

        public void DrawAABB(ref AABB aabb, Color color)
        {
            Vector2[] verts = new Vector2[4];
            verts[0] = new Vector2(aabb.LowerBound.X, aabb.LowerBound.Y);
            verts[1] = new Vector2(aabb.UpperBound.X, aabb.LowerBound.Y);
            verts[2] = new Vector2(aabb.UpperBound.X, aabb.UpperBound.Y);
            verts[3] = new Vector2(aabb.LowerBound.X, aabb.UpperBound.Y);

            DrawPolygon(verts, 4, color);
        }

        private void DrawJoint(Joint joint)
        {
            if (!joint.Enabled)
                return;

            Body b1 = joint.BodyA;
            Body b2 = joint.BodyB;
            Transform xf1 = b1.GetTransform();

            Vector2 x2 = Vector2.Zero;

            // WIP David
            if (!joint.IsFixedType())
            {
                Transform xf2 = b2.GetTransform();
                x2 = xf2.p;
            }

            Vector2 p1 = joint.WorldAnchorA;
            Vector2 p2 = joint.WorldAnchorB;
            Vector2 x1 = xf1.p;

            Color color = new Color(0.5f, 0.8f, 0.8f);

            switch (joint.JointType)
            {
                case JointType.Distance:
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.Pulley:
                    PulleyJoint pulley = (PulleyJoint)joint;
                    Vector2 s1 = b1.GetWorldPoint(pulley.LocalAnchorA);
                    Vector2 s2 = b2.GetWorldPoint(pulley.LocalAnchorB);
                    DrawSegment(p1, p2, color);
                    DrawSegment(p1, s1, color);
                    DrawSegment(p2, s2, color);
                    break;
                case JointType.FixedMouse:
                    DrawPoint(p1, 0.5f, new Color(0.0f, 1.0f, 0.0f));
                    DrawSegment(p1, p2, new Color(0.8f, 0.8f, 0.8f));
                    break;
                case JointType.Revolute:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);

                    DrawSolidCircle(p2, 0.1f, Vector2.Zero, Color.Red);
                    DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color.Blue);
                    break;
                case JointType.FixedAngle:
                    //Should not draw anything.
                    break;
                case JointType.FixedRevolute:
                    DrawSegment(x1, p1, color);
                    DrawSolidCircle(p1, 0.1f, Vector2.Zero, Color.Pink);
                    break;
                case JointType.FixedLine:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.FixedDistance:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.FixedPrismatic:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    break;
                case JointType.Gear:
                    DrawSegment(x1, x2, color);
                    break;
                default:
                    DrawSegment(x1, p1, color);
                    DrawSegment(p1, p2, color);
                    DrawSegment(x2, p2, color);
                    break;
            }
        }

        public void DrawShape(Fixture fixture, Transform xf, Color color)
        {
            switch (fixture.Shape.ShapeType)
            {
                case ShapeType.Circle:
                    {
                        CircleShape circle = (CircleShape)fixture.Shape;

                        Vector2 center = Transform.Multiply(circle.Position, ref xf);
                        float radius = circle.Radius;
                        Vector2 axis = xf.q.ToVector2();

                        DrawSolidCircle(center, radius, axis, color);
                    }
                    break;

                case ShapeType.Polygon:
                    {
                        PolygonShape poly = (PolygonShape)fixture.Shape;
                        int vertexCount = poly.Vertices.Count;
                        Debug.Assert(vertexCount <= Settings.MaxPolygonVertices);

                        for (int i = 0; i < vertexCount; ++i)
                        {
                            _tempVertices[i] = Transform.Multiply(poly.Vertices[i], ref xf);
                        }

                        DrawSolidPolygon(_tempVertices, vertexCount, color);
                    }
                    break;


                case ShapeType.Edge:
                    {
                        EdgeShape edge = (EdgeShape)fixture.Shape;
                        Vector2 v1 = Transform.Multiply(edge.Vertex1, ref xf);
                        Vector2 v2 = Transform.Multiply(edge.Vertex2, ref xf);
                        DrawSegment(v1, v2, color);
                    }
                    break;

                case ShapeType.Chain:
                    {
                        ChainShape chain = (ChainShape)fixture.Shape;

                        for (int i = 0; i < chain.Vertices.Count - 1; ++i)
                        {
                            Vector2 v1 = Transform.Multiply(chain.Vertices[i], ref xf);
                            Vector2 v2 = Transform.Multiply(chain.Vertices[i + 1], ref xf);
                            DrawSegment(v1, v2, color);
                        }
                    }
                    break;
            }
        }

        public override void DrawPolygon(Vector2[] vertices, int count, Color color, bool closed = true)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            for (int i = 0; i < count - 1; i++)
            {
                _primitiveBatch.AddVertex(ref vertices[i], color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(ref vertices[i + 1], color, PrimitiveType.LineList);
            }
            if (closed)
            {
                _primitiveBatch.AddVertex(ref vertices[count - 1], color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(ref vertices[0], color, PrimitiveType.LineList);
            }
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, Color color)
        {
            DrawSolidPolygon(vertices, count, color);
        }

        public void DrawSolidPolygon(Vector2[] vertices, int count, Color color, bool outline = true)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            if (count == 2)
            {
                DrawPolygon(vertices, count, color);
                return;
            }

            Color colorFill = color * (outline ? 0.5f : 1.0f);

            for (int i = 1; i < count - 1; i++)
            {
                _primitiveBatch.AddVertex(ref vertices[0], colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(ref vertices[i], colorFill, PrimitiveType.TriangleList);
                _primitiveBatch.AddVertex(ref vertices[i + 1], colorFill, PrimitiveType.TriangleList);
            }

            if (outline)
                DrawPolygon(vertices, count, color);
        }

        public override void DrawCircle(Vector2 center, float radius, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            Vector2 v2 = new Vector2(radius, 0);
            var center_v2 = center + v2;
            var center_vS = center_v2;

            for (int i = 0; i < CircleSegments - 1; i++)
            {
                Vector2 v1 = v2;
                var center_v1 = center_v2;
                Complex.Multiply(ref v1, ref circleSegmentRotation, out v2);
                Vector2.Add(ref center, ref v2, out center_v2);

                _primitiveBatch.AddVertex(ref center_v1, color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(ref center_v2, color, PrimitiveType.LineList);
            }
            // Close Circle
            _primitiveBatch.AddVertex(ref center_v2, color, PrimitiveType.LineList);
            _primitiveBatch.AddVertex(ref center_vS, color, PrimitiveType.LineList);
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            Vector2 v2 = new Vector2(radius, 0);
            var center_v2 = center + v2;
            var center_vS = center_v2;

            Color colorFill = color * 0.5f;

            for (int i = 0; i < CircleSegments-1; i++)
            {
                Vector2 v1 = v2;
                var center_v1 = center_v2;
                Complex.Multiply(ref v1, ref circleSegmentRotation, out v2);
                Vector2.Add(ref center, ref v2, out center_v2);

                // Draw Circle
                _primitiveBatch.AddVertex(ref center_v1, color, PrimitiveType.LineList);
                _primitiveBatch.AddVertex(ref center_v2, color, PrimitiveType.LineList);

                // Draw Solid Circle
                if (i > 0)
                {
                    _primitiveBatch.AddVertex(ref center_vS, colorFill, PrimitiveType.TriangleList);
                    _primitiveBatch.AddVertex(ref center_v1, colorFill, PrimitiveType.TriangleList);
                    _primitiveBatch.AddVertex(ref center_v2, colorFill, PrimitiveType.TriangleList);
            }
            }
            // Close Circle
            _primitiveBatch.AddVertex(ref center_v2, color, PrimitiveType.LineList);
            _primitiveBatch.AddVertex(ref center_vS, color, PrimitiveType.LineList);

            DrawSegment(center, center + axis * radius, color);
        }

        public override void DrawSegment(Vector2 start, Vector2 end, Color color)
        {
            if (!_primitiveBatch.IsReady())
                throw new InvalidOperationException("BeginCustomDraw must be called before drawing anything.");

            _primitiveBatch.AddVertex(ref start, color, PrimitiveType.LineList);
            _primitiveBatch.AddVertex(ref end, color, PrimitiveType.LineList);
        }

        public override void DrawTransform(ref Transform transform)
        {
            const float axisScale = 0.4f;
            Vector2 p1 = transform.p;

            var xAxis = transform.q.ToVector2();
            Vector2 p2 = p1 + axisScale * xAxis;
            DrawSegment(p1, p2, Color.Red);
            
            var yAxis = new Vector2(-transform.q.Imaginary, transform.q.Real);
            p2 = p1 + axisScale * yAxis;
            DrawSegment(p1, p2, Color.Green);
        }

        public void DrawPoint(Vector2 p, float size, Color color)
        {
            Vector2[] verts = new Vector2[4];
            float hs = size / 2.0f;
            verts[0] = p + new Vector2(-hs, -hs);
            verts[1] = p + new Vector2(hs, -hs);
            verts[2] = p + new Vector2(hs, hs);
            verts[3] = p + new Vector2(-hs, hs);

            DrawSolidPolygon(verts, 4, color, true);
        }

        public void DrawString(int x, int y, string text)
        {
            DrawString(new Vector2(x, y), text);
        }

        public void DrawString(Vector2 position, string text)
        {
            _stringData.Add(new StringData(position, text, TextColor));
        }

        public void DrawString(int x, int y, StringBuilder text)
        {
            DrawString(new Vector2(x, y), text);
        }

        public void DrawString(Vector2 position, StringBuilder text)
        {
            _stringData.Add(new StringData(position, text, TextColor));
        }

        public void DrawArrow(Vector2 start, Vector2 end, float length, float width, bool drawStartIndicator, Color color)
        {
            // Draw connection segment between start- and end-point
            DrawSegment(start, end, color);

            // Precalculate halfwidth
            float halfWidth = width / 2;

            // Create directional reference
            Vector2 rotation = (start - end);
            rotation.Normalize();

            // Calculate angle of directional vector
            float angle = (float)Math.Atan2(rotation.X, -rotation.Y);
            // Create matrix for rotation
            Matrix rotMatrix = Matrix.CreateRotationZ(angle);
            // Create translation matrix for end-point
            Matrix endMatrix = Matrix.CreateTranslation(end.X, end.Y, 0);

            // Setup arrow end shape
            Vector2[] verts = new Vector2[3];
            verts[0] = new Vector2(0, 0);
            verts[1] = new Vector2(-halfWidth, -length);
            verts[2] = new Vector2(halfWidth, -length);

            // Rotate end shape
            Vector2.Transform(verts, ref rotMatrix, verts);
            // Translate end shape
            Vector2.Transform(verts, ref endMatrix, verts);

            // Draw arrow end shape
            DrawSolidPolygon(verts, 3, color, false);

            if (drawStartIndicator)
            {
                // Create translation matrix for start
                Matrix startMatrix = Matrix.CreateTranslation(start.X, start.Y, 0);
                // Setup arrow start shape
                Vector2[] baseVerts = new Vector2[4];
                baseVerts[0] = new Vector2(-halfWidth, length / 4);
                baseVerts[1] = new Vector2(halfWidth, length / 4);
                baseVerts[2] = new Vector2(halfWidth, 0);
                baseVerts[3] = new Vector2(-halfWidth, 0);

                // Rotate start shape
                Vector2.Transform(baseVerts, ref rotMatrix, baseVerts);
                // Translate start shape
                Vector2.Transform(baseVerts, ref startMatrix, baseVerts);
                // Draw start shape
                DrawSolidPolygon(baseVerts, 4, color, false);
            }
        }

        public void BeginCustomDraw(Matrix projection, Matrix view,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            BeginCustomDraw(ref projection, ref view, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void BeginCustomDraw(Matrix projection, Matrix view, Matrix world,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            BeginCustomDraw(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void BeginCustomDraw(ref Matrix projection, ref Matrix view,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            Matrix world = Matrix.Identity;
            _primitiveBatch.Begin(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void BeginCustomDraw(ref Matrix projection, ref Matrix view, ref Matrix world,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            _primitiveBatch.Begin(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void EndCustomDraw()
        {
            _primitiveBatch.End();
        }

        public void RenderDebugData(Matrix projection, Matrix view,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            RenderDebugData(ref projection, ref view, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void RenderDebugData(Matrix projection, Matrix view, Matrix world,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            RenderDebugData(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void RenderDebugData(ref Matrix projection, ref Matrix view,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            if (!Enabled)
                return;

            Matrix world = Matrix.Identity;
            RenderDebugData(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void RenderDebugData(ref Matrix projection, ref Matrix view, ref Matrix world,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            if (!Enabled)
                return;

            if (!_updatePerformanceGraphCalled)
                UpdatePerformanceGraph(World.UpdateTime);
            _updatePerformanceGraphCalled = false;


            //Nothing is enabled - don't draw the debug view.
            if (Flags == 0)
                return;

            _primitiveBatch.Begin(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
            DrawDebugData();
            _primitiveBatch.End();

            if ((Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
            {
                _primitiveBatch.Begin(ref _localProjection, ref _localView, ref _localWorld, blendState, samplerState, depthStencilState, rasterizerState, alpha);
                DrawPerformanceGraph();
                _primitiveBatch.End();
            }

            // begin the sprite batch effect
            _batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // draw any strings we have
            for (int i = 0; i < _stringData.Count; i++)
            {
                if (_stringData[i].Text != null)
                    _batch.DrawString(_font, _stringData[i].Text, _stringData[i].Position, _stringData[i].Color);
                else
                    _batch.DrawString(_font, _stringData[i].stringBuilderText, _stringData[i].Position, _stringData[i].Color);
            }

            // end the sprite batch effect
            _batch.End();

            _stringData.Clear();
        }

        public void RenderDebugData(ref Matrix projection,
                                    BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            if (!Enabled)
                return;

            Matrix view = Matrix.Identity;
            Matrix world = Matrix.Identity;
            RenderDebugData(ref projection, ref view, ref world, blendState, samplerState, depthStencilState, rasterizerState, alpha);
        }

        public void LoadContent(GraphicsDevice device, ContentManager content, IPrimitiveBatch primitiveBatch = null)
        {
            _device = device;
            // Create a new SpriteBatch, which can be used to draw textures.
            _batch = new SpriteBatch(_device);
            _primitiveBatch = (primitiveBatch != null) ? primitiveBatch : new PrimitiveBatch(_device, 1000);
            _font = content.Load<SpriteFont>("DiagnosticsFont");
            _stringData = new List<StringData>();

            _localProjection = Matrix.CreateOrthographicOffCenter(0f, _device.Viewport.Width, _device.Viewport.Height, 0f, 0f, 1f);
            _localView = Matrix.Identity;
            _localWorld = Matrix.Identity;
        }

        #region Nested type: ContactPoint

        private struct ContactPoint
        {
            public Vector2 Normal;
            public Vector2 Position;
            public PointState State;
        }

        #endregion

        #region Nested type: StringData

        private struct StringData
        {
            public readonly Color Color;
            public readonly string Text;
            public readonly StringBuilder stringBuilderText;
            public readonly Vector2 Position;

            public StringData(Vector2 position, string text, Color color)
            {
                Position = position;
                Text = text;
                stringBuilderText = null;
                Color = color;
            }

            public StringData(Vector2 position, StringBuilder text, Color color)
            {
                Position = position;
                Text = null;
                stringBuilderText = text;
                Color = color;
            }
        }

        #endregion
    }
}