// Copyright (c) 2018 Kastellanos Nikolaos

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Diagnostics;
using System;
using System.Threading.Tasks;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class MultithreadWorldsTest : Test
    {
        World world2, world3, world4;
        DebugView debugView2, debugView3, debugView4;

        private MultithreadWorldsTest()
        {
            world2 = new Dynamics.World();
            world3 = new Dynamics.World();
            world4 = new Dynamics.World();

            CreateWorld(this.World); // initialize World synchronously in the main thread
            CreateWorld(world2); // initialize world2 synchronously in the main thread
            var cw3 = Task.Factory.StartNew(() => CreateWorld(world3)); // initialize world3 asynchronously in the threadpool
            var cw4 = Task.Factory.StartNew(() => CreateWorld(world4)); // initialize world4 asynchronously in the threadpool 

            cw3.Wait();
            cw4.Wait();
        }

        private void CreateWorld(World world)
        {
            world.Gravity = Vector2.Zero;

            List<Vertices> borders = new List<Vertices>(4);

            const float borderThickness = 0.1f;
            const float width = 16f;
            const float height = 9f;

            //Top
            borders.Add(PolygonTools.CreateRectangle(width, borderThickness, new Vector2(width, height), 0));
            //Bottom
            borders.Add(PolygonTools.CreateRectangle(width, borderThickness, new Vector2(width, -height), 0));

            //Left
            borders.Add(PolygonTools.CreateRectangle(borderThickness, height, new Vector2(0, 0), 0));
            //Right
            borders.Add(PolygonTools.CreateRectangle(borderThickness, height, new Vector2(width + width, 0), 0));

            Body body = world.CreateCompoundPolygon(borders, 1, new Vector2(0, 0));

            foreach (Fixture fixture in body.FixtureList)
            {
                fixture.Restitution = 1f;
                fixture.Friction = 0;
            }

            Body circle = world.CreateCircle(0.32f, 1);
            circle.BodyType = BodyType.Dynamic;
            circle.SetRestitution(1f);
            circle.SetFriction(0);

            circle.ApplyLinearImpulse(new Vector2(200, 50));
        }

        public override void Initialize()
        {
            base.Initialize();
            
            debugView2 = new Diagnostics.DebugView(world2);
            debugView3 = new Diagnostics.DebugView(world3);
            debugView4 = new Diagnostics.DebugView(world4);
            debugView2.LoadContent(GameInstance.GraphicsDevice, GameInstance.Content);
            debugView3.LoadContent(GameInstance.GraphicsDevice, GameInstance.Content);
            debugView4.LoadContent(GameInstance.GraphicsDevice, GameInstance.Content);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime); // update World synchronously in the main thread

            if (!settings.Pause)
            {
                float timeStep = Math.Min((float)gameTime.ElapsedGameTime.TotalSeconds, (1f / 30f));

                var st2 = Task.Factory.StartNew(() => world2.Step(timeStep)); // update world2 asynchronously in the threadpool
                var st3 = Task.Factory.StartNew(() => world3.Step(timeStep)); // update world3 asynchronously in the threadpool
                world4.Step(timeStep); // update world4 synchronously in the main thread

                st2.Wait();
                st3.Wait();
            }
        }

        public override void DrawDebugView(GameTime gameTime, ref Matrix projection, ref Matrix view)
        {
            base.DrawDebugView(gameTime, ref projection, ref view);

            var worldMtx = Matrix.CreateTranslation( -34, 0, 0);
            debugView2.RenderDebugData(ref projection, ref view, ref worldMtx);

            worldMtx = Matrix.CreateTranslation(0, 20, 0);
            debugView3.RenderDebugData(ref projection, ref view, ref worldMtx);

            worldMtx = Matrix.CreateTranslation(-34, 20, 0);
            debugView4.RenderDebugData(ref projection, ref view, ref worldMtx);
        }


        internal static Test Create()
        {
            return new MultithreadWorldsTest();
        }
    }
}
