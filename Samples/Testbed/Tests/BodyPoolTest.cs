// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class BodyPoolTest : Test
    {
        Stack<Body> bodyPool = new Stack<Body>();

        private BodyPoolTest()
        {
            //Ground body
            Body ground = World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
            ground.OnCollision += OnCollision;
            ground.OnSeparation += OnSeparation;
        }

        private bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            return true;
        }

        private void OnSeparation(Fixture sender, Fixture other, Contact contact)
        {           
            if (other.Body.World.IsLocked)
            {
                other.Body.World.RemoveAsync(other.Body);
                bodyPool.Push(other.Body);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Body body;

            // make sure Bodies are removed from World.
            // Alternatively, you can push bodies to the pool from World.BodyRemoved event.
            World.ProcessChanges();

            if (bodyPool.Count > 0)
            {
                body = bodyPool.Pop();

                // reset values                
                body.Position = new Vector2(0, 10); // reset posititon to avoid colision with Ground
                body.LinearVelocity = Vector2.Zero;
                body.AngularVelocity = 0f;

                World.Add(body);
            }
            else
                body = World.CreateCircle(0.4f, 1);

            body.Position = new Vector2(Rand.RandomFloat(-35, 35), 10);
            body.BodyType = BodyType.Dynamic;
            body.SetRestitution(1f);

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new BodyPoolTest();
        }
    }
}