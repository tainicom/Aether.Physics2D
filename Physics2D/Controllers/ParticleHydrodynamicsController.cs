// Copyright (c) 2019 Kastellanos Nikolaos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
#if XNAAPI
using Vector2 = Microsoft.Xna.Framework.Vector2;
#endif

namespace tainicom.Aether.Physics2D.Controllers
{
    public class ParticleHydrodynamicsController : Controller
    {
        /// <summary>Smoothing length</summary>
        /// <remarks>this is the Influence radius</remarks>
        protected float h;

        protected float inv_h;
        protected float sq_h;

        #region Settings

        /// <summary>Stiffness (κ)</summary>
        public float k = 40.0f;
        /// <summary>Stiffness (κNear)</summary>
        public float kNear = 100.0f;
        /// <summary>Rest Density (ρ0)</summary>
        public float RestDensity = 1.0f;

        /// <summary>(σ)</summary>
        public float ViscositySigma = 0.0f;
        /// <summary>(β)</summary>
        public float ViscosityBeta = 0.3f;

        #endregion

        protected HashGrid hashGrid;

        #region Particle Data
        protected int particleCount = 0;
        
        protected Vector2[] Position;
        Vector2[] PositionPrev;
        Vector2[] Velocity;
        protected float[] Density;
        protected float[] DensityNear;
        protected float[] Pressure;
        protected float[] PressureNear;

        #endregion

        public int ParticleCount { get { return particleCount; } }

        public HashGrid Grid { get { return hashGrid; } }
        
        public ParticleHydrodynamicsController(float h = 1f, int initialParticlesCapacity = 256) : base()
        {
            Enabled = true;

            this.h = h;
            this.inv_h = 1f / h;
            this.sq_h = h * h;

            hashGrid = new HashGrid(this, h);


            Position = new Vector2[initialParticlesCapacity];
            PositionPrev = new Vector2[initialParticlesCapacity];
            Velocity = new Vector2[initialParticlesCapacity];
            Density = new float[initialParticlesCapacity];
            DensityNear = new float[initialParticlesCapacity];
            Pressure = new float[initialParticlesCapacity];
            PressureNear = new float[initialParticlesCapacity];
        }

        public void AddParticle(Vector2 position)
        {
            AddParticle(position, Vector2.Zero);
        }

        public Vector2 GetParticlePosition(int index)
        {
            return this.Position[index];
        }

        public void AddParticle(Vector2 position, Vector2 velocity)
        {
            if (particleCount == Position.Length)
            {
                var oldSize = Position.Length;
                var newSize = oldSize + 64;
                Array.Resize(ref Position, newSize);
                Array.Resize(ref PositionPrev, newSize);
                Array.Resize(ref Velocity, newSize);
                Array.Resize(ref Density, newSize);
                Array.Resize(ref DensityNear, newSize);
                Array.Resize(ref Pressure, newSize);
                Array.Resize(ref PressureNear, newSize);
            }

            Position[particleCount] = position;
            PositionPrev[particleCount] = position;
            Velocity[particleCount] = velocity;
            particleCount++;
        }
        
        public override void Update(float dt)
        {
            if (dt == 0 || !Enabled)
                return;

            ComputeVelocity(dt);

            ApplyGravity(dt);

            if (ViscosityBeta != 0 || ViscositySigma != 0)
            {
                hashGrid.Update(); // Broad phase
                ApplyViscosity(dt);
            }

            ApplyVelocity(dt);
            
            hashGrid.Update(); // Broad phase 
            DoubleDensityRelaxation(dt);

            ResolveCollisions(dt);
        }
        
        private void ComputeVelocity(float dt)
        {
            float inv_dt = 1f / dt;

            Vector2 velocity;
            for (int i = 0; i < particleCount; i++)
            {
                Vector2.Subtract(ref Position[i], ref PositionPrev[i], out velocity);
                Vector2.Multiply(ref velocity, inv_dt, out Velocity[i]);
            }
        }

        private void ApplyGravity(float dt)
        {
            var dgravity = dt * World.Gravity;

            for (int i = 0; i < particleCount; i++)
                Vector2.Add(ref Velocity[i], ref dgravity, out Velocity[i]);
        }

        private void ApplyViscosity(float dt)
        {
            var pll = hashGrid.ParticleLinkedList;
            foreach (var gridCellKey in hashGrid.GridCellDictionary.Keys)
            {
                int startingParticleNodeIndexT, startingParticleNodeIndexTR, startingParticleNodeIndexR, startingParticleNodeIndexBR;
                if(!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X,     gridCellKey.Y + 1), out startingParticleNodeIndexT))
                    startingParticleNodeIndexT = -1;
                if(!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y + 1), out startingParticleNodeIndexTR))
                    startingParticleNodeIndexTR = -1;
                if(!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y    ), out startingParticleNodeIndexR))
                    startingParticleNodeIndexR = -1;
                if(!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y - 1), out startingParticleNodeIndexBR))
                    startingParticleNodeIndexBR = -1;

                int startingParticleNodeIndex = hashGrid.GridCellDictionary[gridCellKey];
                for (int iNode = startingParticleNodeIndex; iNode != -1; iNode = pll[iNode].NextNodeIndex)
                {
                    int i = pll[iNode].ParticleIndex;
                    var jNode = pll[iNode].NextNodeIndex;
                    ApplyViscosity(dt, i, jNode); // ApplyViscosity to particles in the same cell

                    // we need to check only half of the neibobour cells for pairs. we check only top + right most 
                    // the other pairs will be updated from the bottom + left most cells
                    if (startingParticleNodeIndexT != -1)
                        ApplyViscosity(dt, i, startingParticleNodeIndexT);
                    if (startingParticleNodeIndexTR != -1)
                        ApplyViscosity(dt, i, startingParticleNodeIndexTR);
                    if (startingParticleNodeIndexR != -1)
                        ApplyViscosity(dt, i, startingParticleNodeIndexR);
                    if (startingParticleNodeIndexBR != -1)
                        ApplyViscosity(dt, i, startingParticleNodeIndexBR);
                }
            }
        }

        private void ApplyViscosity(float dt, int i, int startingParticleNodeIndex)
        {
            float u;
            var dt_over2 = dt * 0.5f;
            var dt_over2_inv_h = dt_over2 * inv_h;
            Vector2 rij, velocityNormal;

            var pll = hashGrid.ParticleLinkedList;

            for (int jNode = startingParticleNodeIndex; jNode != -1; jNode = pll[jNode].NextNodeIndex)
            {
                int j = pll[jNode].ParticleIndex;

                Vector2.Subtract(ref Position[j], ref Position[i], out rij);

                float sq_distance = rij.LengthSquared();
                if ((sq_distance > this.sq_h))
                    continue;

                float distance = (float)Math.Sqrt(sq_distance);
                Vector2.Subtract(ref Velocity[i], ref Velocity[j], out velocityNormal);
                Vector2.Divide(ref rij, distance, out rij);

                Vector2.Dot(ref velocityNormal, ref rij, out u);
                if (u <= 0.0f)
                    continue;
                
                //float q = distance * inv_h; // q = distance / h;
                //float oneMinus_q = (1 - q);
                //float I = dt * oneMinus_q * (ViscositySigma * u + ViscosityBeta * u * u);
                //Vector2.Multiply(ref rij, (I * 0.5f), out rij);

                float I_over2 = (dt_over2 - dt_over2_inv_h * distance) * (ViscositySigma * u + ViscosityBeta * u * u);
                Vector2.Multiply(ref rij, I_over2, out rij);

                Vector2.Subtract(ref Velocity[i], ref rij, out Velocity[i]);
                Vector2.Add(     ref Velocity[j], ref rij, out Velocity[j]);
            }
        }

        private void ApplyVelocity(float dt)
        {
            var tmpPosition = PositionPrev;
            PositionPrev = Position;
            Position = tmpPosition;
            
            Vector2 dtv;
            for (int i = 0; i < particleCount; i++)
            {
                Vector2.Multiply(ref Velocity[i], dt, out dtv);
                Vector2.Add(ref PositionPrev[i], ref dtv, out Position[i]);
            }
        }

        private void DoubleDensityRelaxation(float dt)
        {
            ComputeDensity(dt);

            ComputePressure();

            ApplyPressure(dt);
        }
        
        protected virtual void ComputeDensity(float dt)
        {
            Array.Clear(Density, 0, particleCount);
            Array.Clear(DensityNear, 0, particleCount);

            var pll = hashGrid.ParticleLinkedList;
            foreach (var gridCellKey in hashGrid.GridCellDictionary.Keys)
            {
                int startingParticleNodeIndexT, startingParticleNodeIndexTR, startingParticleNodeIndexR, startingParticleNodeIndexBR;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X, gridCellKey.Y + 1), out startingParticleNodeIndexT))
                    startingParticleNodeIndexT = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y + 1), out startingParticleNodeIndexTR))
                    startingParticleNodeIndexTR = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y), out startingParticleNodeIndexR))
                    startingParticleNodeIndexR = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y - 1), out startingParticleNodeIndexBR))
                    startingParticleNodeIndexBR = -1;

                int startingParticleNodeIndex = hashGrid.GridCellDictionary[gridCellKey];
                for (int iNode = startingParticleNodeIndex; iNode != -1; iNode = pll[iNode].NextNodeIndex)
                {
                    int i = pll[iNode].ParticleIndex;

                    var jNode = pll[iNode].NextNodeIndex;
                    ComputeDensity(dt, i, jNode); // ComputeDensity to particles in the same cell
                    
                    // we need to check only half of the neibobour cells for pairs. we check only top + right most 
                    // the other pairs will be updated from the bottom + left most cells
                    if (startingParticleNodeIndexT != -1)
                        ComputeDensity(dt, i, startingParticleNodeIndexT);
                    if (startingParticleNodeIndexTR != -1)
                        ComputeDensity(dt, i, startingParticleNodeIndexTR);
                    if (startingParticleNodeIndexR != -1)
                        ComputeDensity(dt, i, startingParticleNodeIndexR);
                    if (startingParticleNodeIndexBR != -1)
                        ComputeDensity(dt, i, startingParticleNodeIndexBR);
                }
            }
        }

        private void ComputeDensity(float dt, int i, int startingParticleNodeIndex)
        {
            Vector2 rij;

            var pll = hashGrid.ParticleLinkedList;

            for (int jNode = startingParticleNodeIndex; jNode != -1; jNode = pll[jNode].NextNodeIndex)
            {
                int j = pll[jNode].ParticleIndex;

                Vector2.Subtract(ref Position[j], ref Position[i], out rij);

                float sq_distance = rij.LengthSquared();
                if ((sq_distance > this.sq_h))
                    continue;

                float distance = (float)Math.Sqrt(sq_distance);
                float q = distance * inv_h; // q = distance / h;

                float oneMinus_q = (1 - q);

                float oneMinus_q2 = oneMinus_q * oneMinus_q;
                Density[i] += oneMinus_q2;
                Density[j] += oneMinus_q2;
                float oneMinus_q3 = oneMinus_q2 * oneMinus_q; 
                DensityNear[i] += oneMinus_q3;
                DensityNear[j] += oneMinus_q3;
            }
        }
        
        private void ComputePressure()
        {
            // Compute Pressure
            for (int i = 0; i < particleCount; i++)
            {
                Pressure[i] = (k * (Density[i] - RestDensity));
                PressureNear[i] = (kNear * DensityNear[i]);
            }
        }

        protected virtual void ApplyPressure(float dt)
        {
            var pll = hashGrid.ParticleLinkedList;
            foreach (var gridCellKey in hashGrid.GridCellDictionary.Keys)
            {
                int startingParticleNodeIndexT, startingParticleNodeIndexTR, startingParticleNodeIndexR, startingParticleNodeIndexBR;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X, gridCellKey.Y + 1), out startingParticleNodeIndexT))
                    startingParticleNodeIndexT = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y + 1), out startingParticleNodeIndexTR))
                    startingParticleNodeIndexTR = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y), out startingParticleNodeIndexR))
                    startingParticleNodeIndexR = -1;
                if (!hashGrid.GridCellDictionary.TryGetValue(new GridCell(gridCellKey.X + 1, gridCellKey.Y - 1), out startingParticleNodeIndexBR))
                    startingParticleNodeIndexBR = -1;

                int startingParticleNodeIndex = hashGrid.GridCellDictionary[gridCellKey];
                for (int iNode = startingParticleNodeIndex; iNode != -1; iNode = pll[iNode].NextNodeIndex)
                {
                    int i = pll[iNode].ParticleIndex;

                    var jNode = pll[iNode].NextNodeIndex;
                    ApplyPressure(dt, i, jNode); // ComputeDensity to particles in the same cell
                    
                    // we need to check only half of the neibobour cells for pairs. we check only top + right most 
                    // the other pairs will be updated from the bottom + left most cells
                    if (startingParticleNodeIndexT != -1)
                        ApplyPressure(dt, i, startingParticleNodeIndexT);
                    if (startingParticleNodeIndexTR != -1)
                        ApplyPressure(dt, i, startingParticleNodeIndexTR);
                    if (startingParticleNodeIndexR != -1)
                        ApplyPressure(dt, i, startingParticleNodeIndexR);
                    if (startingParticleNodeIndexBR != -1)
                        ApplyPressure(dt, i, startingParticleNodeIndexBR);
                }
            }
        }

        private void ApplyPressure(float dt, int i, int startingParticleNodeIndex)
        {
            var dt2 = dt * dt;
            Vector2 rij;

            Vector2 dx;
            
            var pll = hashGrid.ParticleLinkedList;

            for (int jNode = startingParticleNodeIndex; jNode != -1; jNode = pll[jNode].NextNodeIndex)
            {
                int j = pll[jNode].ParticleIndex;

                Vector2.Subtract(ref Position[j], ref Position[i], out rij);
                float sq_distance = rij.LengthSquared();
                if ((sq_distance > this.sq_h))
                    continue;

                float distance = (float)Math.Sqrt(sq_distance);
                float q = distance * inv_h; // q = distance / h;
                float oneMinus_q = (1 - q);

                Vector2.Divide(ref rij, distance, out rij); // //normalized rij

                float D = (dt2 * (Pressure[i] * oneMinus_q + PressureNear[i] * oneMinus_q * oneMinus_q));
                Vector2.Multiply(ref rij, (D * 0.5f), out rij);

                Vector2.Subtract(ref Position[i], ref rij, out Position[i]);
                Vector2.Add(     ref Position[j], ref rij, out Position[j]);
            }
        }

        float _query_dt;
        GridCell _query_gridCell;
        private void ResolveCollisions(float dt)
        {
            foreach (var gridCell in hashGrid.GridCellDictionary.Keys)
            {
                var lowerBound = new Vector2(hashGrid.cellSize * gridCell.X-h, hashGrid.cellSize * gridCell.Y-h);
                var upperBound = lowerBound + new Vector2(hashGrid.cellSize+h);
                var cellAABB = new AABB(ref lowerBound, ref upperBound);

                // save dt and gridCell for Query callback.
                _query_dt = dt;
                _query_gridCell = gridCell;

                World.ContactManager.BroadPhase.Query(ResolveCollisionsCallback, ref cellAABB);
            }
        }

        private bool ResolveCollisionsCallback(int proxyId)
        {
            FixtureProxy proxy = World.ContactManager.BroadPhase.GetProxy(proxyId);
            ResolveCollisions(_query_dt, proxy.Fixture, ref _query_gridCell);
            return true;
        }

        protected virtual void ResolveCollisions(float dt, Fixture fixture, ref GridCell gridCell)
        {
            if (fixture.IsSensor)
                return;

            var shape = fixture.Shape;
            switch (shape.ShapeType)
            {
                case ShapeType.Circle:
                    ResolveCollisions(dt, ref gridCell, fixture, (CircleShape)shape);
                    break;
                case ShapeType.Edge:
                    ResolveCollisions(dt, ref gridCell, fixture, (EdgeShape)shape);
                    break;
                case ShapeType.Polygon:
                    ResolveCollisions(dt, ref gridCell, fixture, (PolygonShape)shape);
                    break;
                case ShapeType.Chain:
                    ResolveCollisions(dt, ref gridCell, fixture, (ChainShape)shape);
                    break;
            }
        }

        private void ResolveCollisions(float dt, ref GridCell gridCell, Fixture fixture, CircleShape circle)
        {
            var dt2 = dt * dt;
            Vector2 rij;

            var a = fixture.Body.GetWorldPoint(circle.Position);
            var r = circle.Radius;

            var pll = hashGrid.ParticleLinkedList;
            int startingParticleNodeIndex = hashGrid.GridCellDictionary[gridCell];
            for (int iNode = startingParticleNodeIndex; iNode != -1; iNode = pll[iNode].NextNodeIndex)
            {
                int i = pll[iNode].ParticleIndex;
                var c = Position[i];

                var ac = c - a;
                float sq_acdistance = ac.LengthSquared();

                var maxDist = r + h;
                if (sq_acdistance > maxDist * maxDist)
                    continue;

                float acdistance = (float)Math.Sqrt(sq_acdistance);
                Vector2.Divide(ref ac, acdistance, out rij);

                float distance = Math.Max(0f, acdistance - r);
                float q = distance * inv_h; // q = distance / h;

                float oneMinus_q = (1 - q);

                float D =  (dt2 * (Pressure[i] * oneMinus_q + PressureNear[i] * oneMinus_q * oneMinus_q));
                
                //  when (acdistance < r) then particle is inside body
                D += Math.Max(0f, r - acdistance);
                
                if (fixture.Body.BodyType == BodyType.Dynamic)
                {
                    Vector2.Multiply(ref rij, (D * 0.5f), out rij);
                    fixture.Body.ApplyForce(-rij);
                    Vector2.Add(ref Position[i], ref rij, out Position[i]);
                }
                else
                {
                    Vector2.Multiply(ref rij, (D), out rij);
                    Vector2.Add(ref Position[i], ref rij, out Position[i]);
                }
            }
        }

        private void ResolveCollisions(float dt, ref GridCell gridCell, Fixture fixture, EdgeShape edge)
        {
            var a = edge.Vertex1;
            var b = edge.Vertex2;
            var ab = b - a;
            
            var pll = hashGrid.ParticleLinkedList;
            int startingParticleNodeIndex = hashGrid.GridCellDictionary[gridCell];
            for (int iNode = startingParticleNodeIndex; iNode != -1; iNode = pll[iNode].NextNodeIndex)
            {
                int i = pll[iNode].ParticleIndex;
                var c = Position[i];

/*
                //  find collision between particle path and edge
                var d = PositionPrev[i];
                float det = (b.X - a.X) * (d.Y - c.Y) - (d.X - c.X) * (b.Y - a.Y);
                if (det == 0) // lines are parallel
                    continue;

                float lambda = ((d.Y - c.Y) * (d.X - a.X) + (c.X - d.X) * (d.Y - a.Y)) / det;
                float gamma = ((a.Y - b.Y) * (d.X - a.X) + (b.X - a.X) * (d.Y - a.Y)) / det;
                if (lambda < 0  || 1 < lambda)
                    continue;
                if (gamma < 0 || 1 < gamma)
                    continue;

                Vector2 collisionPoint = a + ab * lambda;
                Vector2 collisionPoint2 = d + (dc-d) * lambda;


                Vector2 vi;
                Vector2.Subtract(ref c, ref d, out vi);

                Vector2 vp = Vector2.Zero;
                if (fixture.Body.BodyType == BodyType.Dynamic)
                {
                    vp = fixture.Body.GetLinearVelocityFromLocalPoint(ref collisionPoint);
                }

                var v_relative = vi - vp;
                var n = MathUtils.Rot90(ref ab);
                n.Normalize();
                //n = Vector2.Normalize(vi);
                var vnorm = - Vector2.Dot(v_relative, n) * n;
                var vtang = v_relative - vnorm;
                var I = vnorm - 0.7f * vtang;
                I = -I;
                Vector2.Subtract(ref c, ref I, out Position[i]);                
                continue;
*/
                 
                // calculate square distance
                float sq_distance;
                var ac = c - a;
                float e = Vector2.Dot(ac, ab);
                if (e <= 0)
                {
                    continue;
                    //sq_distance = Vector2.Dot(ac, ac);
                    //rij = ac;
                }
                else
                {
                    float f = Vector2.Dot(ab, ab);
                    if (e >= f)
                    {
                        continue;
                        //var bc = c - b;
                        //sq_distance = Vector2.Dot(bc, bc);
                        //rij = bc;
                    }
                    else
                    {
                        sq_distance = Vector2.Dot(ac, ac) - e * e / f;
                    }
                }

                if (sq_distance > sq_h)
                     continue;
                if (sq_distance <= 0)
                     continue;

                float distance = (float)Math.Sqrt(sq_distance);
                float q = distance * inv_h; // q = distance / h;

                float oneMinus_q = (1 - q);


                var rij = MathUtils.Rot90(ref ab);
                Vector2.Divide(ref rij, distance, out rij);

                float D = (dt * 0.3f * (oneMinus_q * oneMinus_q));
                
                if (fixture.Body.BodyType == BodyType.Dynamic)
                {
                    Vector2.Multiply(ref rij, (D * 0.5f), out rij);
                    fixture.Body.ApplyForce(-rij);
                    Vector2.Add(ref Position[i], ref rij, out Position[i]);
                }
                else
                {
                    //Vector2.Multiply(ref rij, (D), out rij);
                    Vector2.Multiply(ref rij, (D / 2f), out rij);
                    Vector2.Add(ref Position[i], ref rij, out Position[i]);
                }
             }
        }
        
        private void ResolveCollisions(float dt, ref GridCell gridCell, Fixture fixture, PolygonShape edge)
        {
        }

        private void ResolveCollisions(float dt, ref GridCell gridCell, Fixture fixture, ChainShape edge)
        {
        }


        public struct GridCell : IEquatable<GridCell>
        {
            public int X, Y;

            public GridCell(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public bool Equals(GridCell other) { return (X == other.X && Y == other.Y); }

            public override int GetHashCode() { return unchecked (X + Y << 16); }

            public override string ToString() { return String.Format("{{X:{0}, Y:{1}}}", X, Y); }
        }

        public class HashGrid
        {
            private ParticleHydrodynamicsController controller;
            private float h;
            internal float cellSize;
            private float inv_gridSize;

            public Dictionary<GridCell, int> GridCellDictionary;

            public float CellSize { get { return cellSize; } }

            internal struct ParticleLinkedListNode
            {
                public int ParticleIndex;
                public int NextNodeIndex;

                public override string ToString() { return String.Format("{{ParticleIndex:{0}, NextNodeIndex:{1}}}", ParticleIndex, NextNodeIndex); }
            }

            internal ParticleLinkedListNode[] ParticleLinkedList;
            int ParticleLinkedListCount;

            public HashGrid(ParticleHydrodynamicsController particleHydrodynamicsController, float h)
            {
                // TODO: Complete member initialization
                this.controller = particleHydrodynamicsController;
                this.h = h;
                cellSize = h * 1f;
                Debug.Assert(cellSize >= h); // cell size must be at least the influence radious 'h'.
                inv_gridSize = 1f / cellSize;

                GridCellDictionary = new Dictionary<GridCell, int>();


                ParticleLinkedList = new ParticleLinkedListNode[256];
                ParticleLinkedListCount = 0;


            }

            int GetParticleNodeIndex()
            {
                return GetParticleNodeIndex(-1);
            }

            private int GetParticleNodeIndex(int particleIndex)
            {
                if (ParticleLinkedListCount == ParticleLinkedList.Length)
                {
                    var oldSize = ParticleLinkedList.Length;
                    var newSize = oldSize + 64;
                    Array.Resize(ref ParticleLinkedList, newSize);
                }

                int index = ParticleLinkedListCount;
                ParticleLinkedList[index].NextNodeIndex = -1;
                ParticleLinkedList[index].ParticleIndex = particleIndex;

                ParticleLinkedListCount++;
                return index;
            }

            private void Clear()
            {
                GridCellDictionary.Clear();
                ParticleLinkedListCount = 0;
            }

            internal void Update()
            {
                Clear();

                for (int i = 0; i < controller.ParticleCount; i++)
                    Add(i, ref controller.Position[i]);
            }

            private void Add(int particleIndex, ref Vector2 position)
            {
                var gridCell = new GridCell(
                    (int)Math.Floor(position.X * inv_gridSize),
                    (int)Math.Floor(position.Y * inv_gridSize)
                    );

                int particleNodeIndex = GetParticleNodeIndex(particleIndex);

                int firstParticleNode;
                if (!GridCellDictionary.TryGetValue(gridCell, out firstParticleNode))
                {
                    GridCellDictionary.Add(gridCell, particleNodeIndex);
                }
                else
                {
                    ParticleLinkedList[particleNodeIndex].NextNodeIndex = ParticleLinkedList[firstParticleNode].NextNodeIndex;
                    ParticleLinkedList[firstParticleNode].NextNodeIndex = particleNodeIndex;
                }

                return;
            }

        }

    }
    


}
