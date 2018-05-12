/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class DeletionTest : Test
    {
        private DeletionTest()
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
                other.Body.World.RemoveAsync(other.Body);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Body body = World.CreateCircle(0.4f, 1);
            body.Position = new Vector2(Rand.RandomFloat(-35, 35), 10);
            body.BodyType = BodyType.Dynamic;
            body.SetRestitution(1f);

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new DeletionTest();
        }
    }
}