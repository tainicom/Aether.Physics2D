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
    public class LockTest : Test
    {
        private Body _rectangle;

        private LockTest()
        {
            World.CreateEdge(new Vector2(-20, 0), new Vector2(20, 0));

            _rectangle = World.CreateRectangle(2, 2, 1);
            _rectangle.BodyType = BodyType.Dynamic;
            _rectangle.Position = new Vector2(0, 10);
            _rectangle.OnCollision += OnCollision;

            //Properties and methods that were checking for lock before
            //Body.Enabled
            //Body.LocalCenter
            //Body.Mass
            //Body.Inertia
            //Fixture.Remove()
            //Body.SetTransformIgnoreContacts()
            //Fixture()
        }

        private bool OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            //_rectangle.CreateFixture(_rectangle.Shape); //Calls the constructor in Fixture
            //_rectangle.Remove(_rectangle);
            //_rectangle.Inertia = 40;
            //_rectangle.LocalCenter = new Vector2(-1, -1);
            //_rectangle.Mass = 10;
            //_rectangle.World.Clear();
            _rectangle.Enabled = false;
            return false;
        }

        internal static Test Create()
        {
            return new LockTest();
        }
    }
}