/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class CharacterCollisionTest : Test
    {
        private bool _collision;
        private Body _character;

        private CharacterCollisionTest()
        {
            //Ground body
            Body ground = World.CreateEdge(new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f));

            // Collinear edges with no adjacency information.
            // This shows the problematic case where a box shape can hit
            // an internal vertex.
            EdgeShape shape = new EdgeShape(new Vector2(-8.0f, 1.0f), new Vector2(-6.0f, 1.0f));
            ground.CreateFixture(shape);
            shape = new EdgeShape(new Vector2(-6.0f, 1.0f), new Vector2(-4.0f, 1.0f));
            ground.CreateFixture(shape);
            shape = new EdgeShape(new Vector2(-4.0f, 1.0f), new Vector2(-2.0f, 1.0f));
            ground.CreateFixture(shape);

            // Chain shape
            {
                Vertices vs = new Vertices(4);
                vs.Add(new Vector2(5.0f, 7.0f));
                vs.Add(new Vector2(6.0f, 8.0f));
                vs.Add(new Vector2(7.0f, 8.0f));
                vs.Add(new Vector2(8.0f, 7.0f));

                Body body = World.CreateChainShape(vs);
                body.Rotation = 0.25f * MathHelper.Pi;
            }

            // Square tiles. This shows that adjacency shapes may
            // have non-smooth collision. There is no solution
            // to this problem.
            PolygonShape tile = new PolygonShape(1);
            tile.Vertices = PolygonTools.CreateRectangle(1.0f, 1.0f, new Vector2(4.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);
            tile.Vertices = PolygonTools.CreateRectangle(1.0f, 1.0f, new Vector2(6.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);
            tile.Vertices = PolygonTools.CreateRectangle(1.0f, 1.0f, new Vector2(8.0f, 3.0f), 0.0f);
            ground.CreateFixture(tile);

            // Square made from an edge loop. Collision should be smooth.
            Vertices vertices = new Vertices(4);
            vertices.Add(new Vector2(-1.0f, 3.0f));
            vertices.Add(new Vector2(1.0f, 3.0f));
            vertices.Add(new Vector2(1.0f, 5.0f));
            vertices.Add(new Vector2(-1.0f, 5.0f));
            ground.CreateLoopShape(vertices);

            // Edge loop. Collision should be smooth.
            vertices = new Vertices(10);
            vertices.Add(new Vector2(0.0f, 0.0f));
            vertices.Add(new Vector2(6.0f, 0.0f));
            vertices.Add(new Vector2(6.0f, 2.0f));
            vertices.Add(new Vector2(4.0f, 1.0f));
            vertices.Add(new Vector2(2.0f, 2.0f));
            vertices.Add(new Vector2(0.0f, 2.0f));
            vertices.Add(new Vector2(-2.0f, 2.0f));
            vertices.Add(new Vector2(-4.0f, 3.0f));
            vertices.Add(new Vector2(-6.0f, 2.0f));
            vertices.Add(new Vector2(-6.0f, 0.0f));
            World.CreateLoopShape(vertices, new Vector2(-10, 4));

            // Square character 1
            Body squareCharacter = World.CreateRectangle(1, 1, 20, new Vector2(-3.0f, 8.0f));
            squareCharacter.BodyType = BodyType.Dynamic;
            squareCharacter.FixedRotation = true;
            squareCharacter.SleepingAllowed = false;

            squareCharacter.OnCollision += CharacterOnCollision;
            squareCharacter.OnSeparation += CharacterOnSeparation;

            // Square character 2
            Body squareCharacter2 = World.CreateRectangle(0.5f, 0.5f, 20, new Vector2(-5.0f, 5.0f));
            squareCharacter2.BodyType = BodyType.Dynamic;
            squareCharacter2.FixedRotation = true;
            squareCharacter2.SleepingAllowed = false;

            // Hexagon character
            float angle = 0.0f;
            const float delta = MathHelper.Pi / 3.0f;
            vertices = new Vertices(6);

            for (int i = 0; i < 6; ++i)
            {
                vertices.Add(new Vector2(0.5f * (float)Math.Cos(angle), 0.5f * (float)Math.Sin(angle)));
                angle += delta;
            }

            Body hexCharacter = World.CreatePolygon(vertices, 20, new Vector2(-5.0f, 8.0f));
            hexCharacter.BodyType = BodyType.Dynamic;
            hexCharacter.FixedRotation = true;
            hexCharacter.SleepingAllowed = false;

            // Circle character
            Body circleCharacter = World.CreateCircle(0.5f, 20, new Vector2(3.0f, 5.0f));
            circleCharacter.BodyType = BodyType.Dynamic;
            circleCharacter.FixedRotation = true;
            circleCharacter.SleepingAllowed = false;

            // Circle character
            _character = World.CreateCircle(0.25f, 20, new Vector2(-7.0f, 6.0f));
            _character.BodyType = BodyType.Dynamic;
            _character.SetFriction(1.0f);
            _character.SleepingAllowed = false;
        }

        private bool CharacterOnCollision(Fixture sender, Fixture other, Contact contact)
        {
            _collision = true;
            return true;
        }

        private void CharacterOnSeparation(Fixture sender, Fixture other, Contact contact)
        {
            _collision = false;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            Vector2 v = _character.LinearVelocity;
            v.X = -5.0f;
            _character.LinearVelocity = v;

            DrawString(_collision ? "OnCollision fired" : "OnSeparation fired");

            base.Update(settings, gameTime);
        }

        public static Test Create()
        {
            return new CharacterCollisionTest();
        }
    }
}