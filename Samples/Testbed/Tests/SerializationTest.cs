/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using System.IO;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class SerializationTest : Test
    {
        private bool _save = true;
        private float _time;

        public SerializationTest()
        {
            Body ground = World.CreateEdge(new Vector2(-20, 0), new Vector2(20, 0));

            //Friction and distance joint
            {
                Body bodyA = World.CreateBody(new Vector2(10, 25), 0, BodyType.Dynamic);
                bodyA.CreateCircle(1, 1.5f);

                Body bodyB = World.CreateBody(new Vector2(-1, 25), 0, BodyType.Dynamic);
                bodyB.CreateRectangle(1, 1, 1, Vector2.Zero);

                FrictionJoint frictionJoint = JointFactory.CreateFrictionJoint(World, bodyB, ground, Vector2.Zero);
                frictionJoint.CollideConnected = true;
                frictionJoint.MaxForce = 100;

                JointFactory.CreateDistanceJoint(World, bodyA, bodyB);
            }

            //Wheel joint
            {
                Vertices vertices = new Vertices(6);
                vertices.Add(new Vector2(-1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, -0.5f));
                vertices.Add(new Vector2(1.5f, 0.0f));
                vertices.Add(new Vector2(0.0f, 0.9f));
                vertices.Add(new Vector2(-1.15f, 0.9f));
                vertices.Add(new Vector2(-1.5f, 0.2f));

                Body carBody = World.CreateBody(new Vector2(0, 1), 0, BodyType.Dynamic);
                Fixture carFixture = carBody.CreatePolygon(vertices, 1);

                Body wheel1 = World.CreateBody(new Vector2(-1.0f, 0.35f), 0, BodyType.Dynamic);
                Fixture wheel1Fixture = wheel1.CreateCircle(0.4f, 1);
                wheel1Fixture.Friction = 0.9f;

                Body wheel2 = World.CreateBody(new Vector2(1.0f, 0.4f), 0, BodyType.Dynamic);
                Fixture wheel2Fixture = wheel2.CreateCircle(0.4f, 1);
                wheel2Fixture.Friction = 0.9f;

                Vector2 axis = new Vector2(0.0f, 1.0f);

                WheelJoint spring1 = JointFactory.CreateWheelJoint(World, carBody, wheel1, axis);
                spring1.MotorSpeed = 0.0f;
                spring1.MaxMotorTorque = 20.0f;
                spring1.MotorEnabled = true;
                spring1.Frequency = 4;
                spring1.DampingRatio = 0.7f;

                WheelJoint spring2 = JointFactory.CreateWheelJoint(World, carBody, wheel2, axis);
                spring2.MotorSpeed = 0.0f;
                spring2.MaxMotorTorque = 10.0f;
                spring2.MotorEnabled = false;
                spring2.Frequency = 4;
                spring2.DampingRatio = 0.7f;
            }

            //Prismatic joint
            {
                Body body = World.CreateBody(new Vector2(-10.0f, 10.0f), 0.5f * MathHelper.Pi, BodyType.Dynamic);
                body.CreateRectangle(2, 2, 5, Vector2.Zero);

                Vector2 axis = new Vector2(2.0f, 1.0f);
                axis.Normalize();

                PrismaticJoint joint = JointFactory.CreatePrismaticJoint(World, ground, body, Vector2.Zero, axis);
                joint.MotorSpeed = 5.0f;
                joint.MaxMotorForce = 1000.0f;
                joint.MotorEnabled = true;
                joint.LowerLimit = -10.0f;
                joint.UpperLimit = 20.0f;
                joint.LimitEnabled = true;
            }

            // Pulley joint
            {
                Body body1 = World.CreateBody(new Vector2(-10.0f, 16.0f),0, BodyType.Dynamic);
                body1.CreateRectangle(2, 4, 5, Vector2.Zero);

                Body body2 = World.CreateBody(new Vector2(10.0f, 16.0f),0, BodyType.Dynamic);
                body2.CreateRectangle(2, 4, 5, Vector2.Zero);

                Vector2 anchor1 = new Vector2(-10.0f, 16.0f + 2.0f);
                Vector2 anchor2 = new Vector2(10.0f, 16.0f + 2.0f);
                Vector2 worldAnchor1 = new Vector2(-10.0f, 16.0f + 2.0f + 12.0f);
                Vector2 worldAnchor2 = new Vector2(10.0f, 16.0f + 2.0f + 12.0f);

                JointFactory.CreatePulleyJoint(World, body1, body2, anchor1, anchor2, worldAnchor1, worldAnchor2, 1.5f, true);
            }

            //Revolute joint
            {
                Body ball = World.CreateBody(new Vector2(5.0f, 30.0f), 0, BodyType.Dynamic);
                ball.CreateCircle(3.0f, 5.0f);

                Body polygonBody = World.CreateBody(new Vector2(10, 10), 0, BodyType.Dynamic);
                polygonBody.CreateRectangle(20, 0.4f, 2, Vector2.Zero);
                polygonBody.IsBullet = true;

                RevoluteJoint joint = JointFactory.CreateRevoluteJoint(World, ground, polygonBody, new Vector2(10, 0));
                joint.LowerLimit = -0.25f * MathHelper.Pi;
                joint.UpperLimit = 0.0f * MathHelper.Pi;
                joint.LimitEnabled = true;
            }

            //Weld joint
            {
                PolygonShape shape = new PolygonShape(PolygonTools.CreateRectangle(0.5f, 0.125f), 20);

                Body prevBody = ground;
                for (int i = 0; i < 10; ++i)
                {
                    Body body = World.CreateBody(new Vector2(-14.5f + 1.0f * i, 5.0f), 0, BodyType.Dynamic);
                    body.CreateFixture(shape);

                    Vector2 anchor = new Vector2(0.5f, 0);

                    if (i == 0)
                        anchor = new Vector2(-15f, 5);

                    JointFactory.CreateWeldJoint(World, prevBody, body, anchor, new Vector2(-0.5f, 0));
                    prevBody = body;
                }
            }

            //Rope joint
            {
                World.CreateChain(new Vector2(-10, 10), new Vector2(-20, 10), 0.1f, 0.5f, 10, 0.1f, true);
            }

            //Angle joint
            {
                Body bA = World.CreateBody(new Vector2(-5, 4), (float)(Math.PI / 3), BodyType.Dynamic);
                Fixture fA = bA.CreateRectangle(4, 4, 1, Vector2.Zero);

                Body bB = World.CreateBody(new Vector2(5, 4), 0, BodyType.Dynamic);
                Fixture fB = bB.CreateRectangle(4, 4, 1, Vector2.Zero);

                AngleJoint joint = new AngleJoint(bA, bB);
                joint.TargetAngle = (float)Math.PI / 2;
                World.Add(joint);
            }

            //Motor joint
            {
                Body body = World.CreateBody(new Vector2(0, 35), 0, BodyType.Dynamic);
                Fixture fixture = body.CreateRectangle(4, 1, 2, Vector2.Zero);
                fixture.Friction = 0.6f;

                MotorJoint motorJoint = JointFactory.CreateMotorJoint(World, ground, body);
                motorJoint.MaxForce = 1000.0f;
                motorJoint.MaxTorque = 1000.0f;
                motorJoint.LinearOffset = new Vector2(0, 35);
                motorJoint.AngularOffset = (float)(Math.PI / 3f);
            }
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            _time += gameTime.ElapsedGameTime.Milliseconds;

            if (_time >= 300)
            {
                _time = 0;
                if (_save)
                {
                    using (Stream stream = new FileStream("SerializationTest.xml", FileMode.Create))
                    {
                        WorldSerializer.Serialize(World, stream);
                    }
                }
                else
                {
                    using (Stream stream = new FileStream("SerializationTest.xml", FileMode.Open))
                    {
                        World = WorldSerializer.Deserialize(stream);
                    }
                    base.Initialize(); //To initialize the debug view
                }

                _save = !_save;
            }

            base.Update(settings, gameTime);
        }

    }
}