// Copyright (c) 2021 Kastellanos Nikolaos

using System;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    // This test uses directly the BroadPhase for collision tests
    // and implements a custom physics symulation.
    public class BoidsTest : Test
    {
        const float maxDistanceFlock = 75;
        const float maxDistanceAlign = 75;
        const float maxDistanceAvoid = 30;
        const float maxDistancePredator = 100;

        const float flockForce = .0004f;
        const float AlignForce = .004f;
        const float AvoidForce = .003f;
        const float PredatorForce = .001f;

        float minSpeed = 1.5f;
        float maxSpeed = 5;

        const int BoidCount = 128;
        
        public override Vector2 Bounds { get { return new Vector2(1024, 768); } }

        // Boids
        public readonly Vector2[] _pos = new Vector2[BoidCount];
        public readonly Vector2[] _vel = new Vector2[BoidCount];
        public readonly Vector2[] _acc = new Vector2[BoidCount];
        public readonly int[] _pidfl = new int[BoidCount];
        public readonly int[] _pidal = new int[BoidCount];
        public readonly int[] _pidav = new int[BoidCount];
        
        IBroadPhase<int> bpfl = new DynamicTreeBroadPhase<int>();
        IBroadPhase<int> bpal = new DynamicTreeBroadPhase<int>();
        IBroadPhase<int> bpav = new DynamicTreeBroadPhase<int>();
        private readonly Random Rand = new Random();

        public BoidsTest()
        {
            for (int i = 0; i < BoidCount; i++)
            {
                _pos[i] = -(Bounds / 2f) + new Vector2(
                    (float)Rand.NextDouble() * Bounds.X,
                    (float)Rand.NextDouble() * Bounds.Y);
                _vel[i] = new Vector2(
                    (float)Rand.NextDouble() - 0.5f,
                    (float)Rand.NextDouble() - 0.5f);

                var boidPosition = new Vector2(_pos[i].X, _pos[i].Y);

                var aabb = new AABB(boidPosition, maxDistanceFlock * 2, maxDistanceFlock * 2);
                _pidfl[i] = bpfl.AddProxy(ref aabb);
                bpfl.SetProxy(_pidfl[i], ref i);

                aabb = new AABB(boidPosition, maxDistanceAlign * 2, maxDistanceAlign * 2);
                _pidal[i] = bpal.AddProxy(ref aabb);
                bpal.SetProxy(_pidal[i], ref i);

                aabb = new AABB(boidPosition, maxDistanceAvoid * 2, maxDistanceAvoid * 2);
                _pidav[i] = bpav.AddProxy(ref aabb);
                bpav.SetProxy(_pidav[i], ref i);
            }

        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Advance(gameTime);

            float minSpeedsq = (float)Math.Pow(minSpeed, 2);
            float maxSpeedsq = (float)Math.Pow(maxSpeed, 2);

            for (int i = 0; i < BoidCount; i += 1)
            {
                // apply force
                _vel[i] += _acc[i];
                // apply velocity
                _pos[i] += _vel[i] * 30f * dt;
            }

            for (int i = 0; i < BoidCount; i++)
            {
                var boidPosition = new Vector2(_pos[i].X, _pos[i].Y);
                var boidVelocity = new Vector2(_vel[i].X, _vel[i].Y);

                //update aabb
                var aabb = new AABB(boidPosition, maxDistanceFlock * 2, maxDistanceFlock * 2);
                bpfl.MoveProxy(_pidfl[i], ref aabb, boidVelocity);

                aabb = new AABB(boidPosition, maxDistanceAlign * 2, maxDistanceAlign * 2);
                bpal.MoveProxy(_pidal[i], ref aabb, boidVelocity);

                aabb = new AABB(boidPosition, maxDistanceAvoid * 2, maxDistanceAvoid * 2);
                bpav.MoveProxy(_pidav[i], ref aabb, boidVelocity);


                SpeedLimit(i, minSpeed, maxSpeed, minSpeedsq, maxSpeedsq);
            }

            bool bounceOffWalls = true;
            bool wrapAroundEdges = false;
            if (bounceOffWalls)
            {
                for (int i = 0; i < BoidCount; i++)

                    BounceOffWalls(i);
            }
            else if (wrapAroundEdges)
            {
                for (int i = 0; i < BoidCount; i++)
                    WrapAround(i);
            }

            base.Update(settings, gameTime);


        }
        
        public override void Mouse(InputState input)
        {
            Vector2 worldPosition = GameInstance.ConvertScreenToWorld(input.MouseState.X, input.MouseState.Y);
            Predator(worldPosition);
        }

        private Vector2 ToLocal(Point mousePos)
        {
            //TODO: Unproject
            return mousePos.ToVector2();
        }

        public void Advance(GameTime gameTime)
        {
         
            // reset forces
            for (int i = 0; i < BoidCount; i++)
                _acc[i] = Vector2.Zero;

            Flock();
            Align();
            Avoid();
        }

        private void Flock()
        {
            float frc = flockForce;
            float maxDistancesq = (float)Math.Pow(maxDistanceFlock, 2);

            bpfl.UpdatePairs((pid, otherpid) =>
            {
                int i = bpfl.GetProxy(pid);
                int otheri = bpfl.GetProxy(otherpid);
                Vector2 d = _pos[otheri] - _pos[i];
                var distancesq = d.X * d.X + d.Y * d.Y;
                if (distancesq < maxDistancesq)
                {
                    _acc[i] += d * frc;
                    _acc[otheri] += -d * frc;
                }
            });
        }

        private void Align()
        {
            float frc = AlignForce;
            float maxDistancesq = (float)Math.Pow(maxDistanceAlign, 2);

            bpal.UpdatePairs((pid, otherpid) =>
            {
                int i = bpfl.GetProxy(pid);
                int otheri = bpal.GetProxy(otherpid);
                Vector2 dp = _pos[otheri] - _pos[i];
                var distancesq = dp.X * dp.X + dp.Y * dp.Y;
                if (distancesq < maxDistancesq)
                {
                    Vector2 dv = _vel[otheri] - _vel[i];
                    _acc[i] += dv * frc;
                    _acc[otheri] += -dv * frc;
                }
            });
        }

        private void Avoid()
        {
            float frc = AvoidForce;
            float maxDistance = maxDistanceAvoid;
            float maxDistancesq = (float)Math.Pow(maxDistanceAvoid, 2);

            bpav.UpdatePairs((pid, otherpid) =>
            {
                int i = bpfl.GetProxy(pid);
                int otheri = bpav.GetProxy(otherpid);
                Vector2 d = _pos[otheri] - _pos[i];
                var distancesq = d.X * d.X + d.Y * d.Y;
                if (distancesq < maxDistancesq)
                {
                    float distance = (float)Math.Sqrt(distancesq);
                    float closeness = maxDistance - distance;
                    _acc[i] += -d * closeness * frc;
                    _acc[otheri] += d * closeness * frc;
                }
            });
        }

        private void Predator(Vector2 predPos)
        {
            float frc = PredatorForce;
            float maxDistance = maxDistancePredator;
            float maxDistancesq = (float)Math.Pow(maxDistancePredator, 2);

            for (int i = 0; i < BoidCount; i++)
            {
                Vector2 d = predPos - _pos[i];
                float distancesq = d.X * d.X + d.Y * d.Y;
                if (distancesq < maxDistancesq)
                {
                    float distance = (float)Math.Sqrt(distancesq);
                    float closeness = maxDistance - distance;
                    _acc[i] += -d * closeness * frc;
                }
            }
        }

        private void BounceOffWalls(int i)
        {
            float turn = .125f;

            if (_pos[i].X < -Bounds.X / 2f)
                _vel[i].X += turn;
            else if (_pos[i].X > +Bounds.X / 2f)
                _vel[i].X -= turn;
            if (_pos[i].Y < -Bounds.Y / 2f)
                _vel[i].Y += turn;
            else if (_pos[i].Y > +Bounds.Y / 2f)
                _vel[i].Y -= turn;
        }

        private void WrapAround(int i)
        {
            if (_pos[i].X < -Bounds.X / 2f)
                _pos[i].X += Bounds.X;
            else if (_pos[i].X > Bounds.X / 2f)
                _pos[i].X -= Bounds.X;
            if (_pos[i].Y < -Bounds.Y / 2f)
                _pos[i].Y += Bounds.Y;
            else if (_pos[i].Y > Bounds.Y / 2f)
                _pos[i].Y -= Bounds.Y;
        }

        public void SpeedLimit(int i, float minSpeed, float maxSpeed, float minSpeedsq, float maxSpeedsq)
        {
            var speedsq = _vel[i].LengthSquared();

            if (speedsq > 0 && speedsq < minSpeedsq)
            {
                _vel[i] *= (minSpeed / (float)Math.Sqrt(speedsq));
            }
            else if (speedsq > 0 && speedsq > maxSpeedsq)
            {
                _vel[i] *= (maxSpeed / (float)Math.Sqrt(speedsq));
            }
        }


        Vector2[] path =
        {
            new Vector2(0,3),
            new Vector2(10,0),
            new Vector2(0,-3)
        };

        public override void DrawDebugView(GameTime gameTime, ref Matrix projection, ref Matrix view)
        {
            var prj = projection;
            DebugView.BeginCustomDraw(ref prj, ref view);

            // render each boid
            for (int i = 0; i < BoidCount; i++)
            {
                var lensq = _vel[i].LengthSquared();
                if (lensq > 0)
                {
                    Vector2 orientation = _vel[i] / (float)Math.Sqrt(lensq);

                    Vector2 nose  = new Vector2(orientation.X, orientation.Y) * 10f;
                    Vector2 left  = new Vector2(orientation.Y, -orientation.X) * 3f;
                    Vector2 right = new Vector2(-orientation.Y, orientation.X) * 3f;
                    DebugView.DrawSegment(_pos[i] + left, _pos[i] + nose, Color.LightCyan);
                    DebugView.DrawSegment(_pos[i] + right, _pos[i] + nose, Color.LightCyan);
                }
            }

            DebugView.EndCustomDraw();

            base.DrawDebugView(gameTime, ref projection, ref view);
        }

    }
}
