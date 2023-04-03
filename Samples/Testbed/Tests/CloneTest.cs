﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class CloneTest : Test
    {
        public CloneTest()
        {
            //Ground
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Body box = World.CreateBody( new Vector2(10, 10), 0, BodyType.Dynamic);
            box.SleepingAllowed = false;
            box.LinearDamping = 1;
            box.AngularDamping = 0.5f;
            box.AngularVelocity = 0.5f;
            box.LinearVelocity = new Vector2(0, 10);
            var bfixture = box.CreateRectangle(5, 5, 5, Vector2.Zero);
            bfixture.Restitution = 0.8f;
            bfixture.Friction = 0.9f;

            //This clones the body and all attached fixtures
            Body boxClone1 = box.DeepClone();

            //Swiching the body type to static will reset all forces. This will affect the next clone.
            boxClone1.BodyType = BodyType.Static;
            boxClone1.Position += new Vector2(-10, 0);

            Body boxClone2 = boxClone1.DeepClone();
            boxClone2.BodyType = BodyType.Dynamic;
            boxClone2.Position += new Vector2(-10, 0);
        }

        public override void Initialize()
        {
            Texture2D polygonTexture = GameInstance.Content.Load<Texture2D>("Texture");
            uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];
            polygonTexture.GetData(data);

            Vertices verts = PolygonTools.CreatePolygon(data, polygonTexture.Width);

            Vector2 scale = new Vector2(0.07f, -0.07f);
            verts.Scale(ref scale);

            Vector2 centroid = -verts.GetCentroid();
            verts.Translate(ref centroid);

            Body compund = World.CreateCompoundPolygon(Triangulate.ConvexPartition(verts, TriangulationAlgorithm.Bayazit), 1);
            compund.Position = new Vector2(-25, 30);

            Body b = compund.DeepClone();
            b.Position = new Vector2(20, 30);
            b.BodyType = BodyType.Dynamic;

            base.Initialize();
        }

    }
}