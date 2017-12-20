/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class TheoJansenWalker
    {
        private Body _chassis;
        private Body _wheel;
        private Body[] _leftShoulders;
        private Body[] _leftLegs;
        private Body[] _rightShoulders;
        private Body[] _rightLegs;

        private Sprite _body;
        private Sprite _engine;
        private Sprite _leftShoulder;
        private Sprite _leftLeg;
        private Sprite _rightShoulder;
        private Sprite _rightLeg;

        private RevoluteJoint _motorJoint;
        private List<DistanceJoint> _walkerJoints = new List<DistanceJoint>();

        private bool _motorOn;
        private float _motorSpeed;

        private Vector2 _position;

        private Color[] _walkerColors = { ContentWrapper.Brown, ContentWrapper.Orange, ContentWrapper.Gold };

        public TheoJansenWalker(World world, Vector2 position)
        {
            _position = position;
            _motorSpeed = 2.0f;
            _motorOn = true;

            _leftShoulders = new Body[3];
            _leftLegs = new Body[3];

            _rightShoulders = new Body[3];
            _rightLegs = new Body[3];

            Vector2 pivot = new Vector2(0f, 0.8f);

            // Chassis
            PolygonShape box = new PolygonShape(1f);
            box.Vertices = PolygonTools.CreateRectangle(2.5f, 1.0f);
            _body = new Sprite(ContentWrapper.TextureFromShape(box, _walkerColors[0], ContentWrapper.Black, 24f));

            _chassis = world.CreateBody();
            _chassis.BodyType = BodyType.Dynamic;
            _chassis.Position = pivot + _position;

            Fixture bodyFixture = _chassis.CreateFixture(box);
            bodyFixture.CollisionGroup = -1;

            // Wheel
            CircleShape circle = new CircleShape(1.6f, 1f);
            _engine = new Sprite(ContentWrapper.TextureFromShape(circle, "Stripe", _walkerColors[1] * 0.6f, _walkerColors[2] * 0.8f, ContentWrapper.Black, 3f, 24f));

            _wheel = world.CreateBody();
            _wheel.BodyType = BodyType.Dynamic;
            _wheel.Position = pivot + _position;

            Fixture wheelFixture = _wheel.CreateFixture(circle);
            wheelFixture.CollisionGroup = -1;

            // Physics
            _motorJoint = new RevoluteJoint(_wheel, _chassis, _chassis.Position, true);
            _motorJoint.CollideConnected = false;
            _motorJoint.MotorSpeed = _motorSpeed;
            _motorJoint.MaxMotorTorque = 400f;
            _motorJoint.MotorEnabled = _motorOn;
            world.Add(_motorJoint);

            Vector2 wheelAnchor = pivot + new Vector2(0f, -0.8f);

            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[0], out _leftLegs[0]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[0], out _rightLegs[0]);

            _wheel.SetTransform(_wheel.Position, 120f * MathHelper.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[1], out _leftLegs[1]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[1], out _rightLegs[1]);

            _wheel.SetTransform(_wheel.Position, -120f * MathHelper.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[2], out _leftLegs[2]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[2], out _rightLegs[2]);
            
            CreateLegTextures();
        }


        private void CreateLeg(World world, float direction, Vector2 wheelAnchor, out Body shoulder, out Body leg)
        {
            Vector2 p1 = new Vector2(5.4f * direction, -6.1f);
            Vector2 p2 = new Vector2(7.2f * direction, -1.2f);
            Vector2 p3 = new Vector2(4.3f * direction, -1.9f);
            Vector2 p4 = new Vector2(3.1f * direction, 0.8f);
            Vector2 p5 = new Vector2(6.0f * direction, 1.5f);
            Vector2 p6 = new Vector2(2.5f * direction, 3.7f);
            
            PolygonShape shoulderPolygon = new PolygonShape(1f);
            PolygonShape legPolygon = new PolygonShape(1f);

            if (direction > 0f)
            {
                legPolygon.Vertices = new Vertices(new[] { p1, p2, p3 });
                shoulderPolygon.Vertices = new Vertices(new[] { Vector2.Zero, p5 - p4, p6 - p4 });
            }
            else
            {
                legPolygon.Vertices = new Vertices(new[] { p1, p3, p2 });
                shoulderPolygon.Vertices = new Vertices(new[] { Vector2.Zero, p6 - p4, p5 - p4 });
            }

            leg = world.CreateBody();
            leg.BodyType = BodyType.Dynamic;
            leg.Position = _position;
            leg.AngularDamping = 10f;
            
            shoulder = world.CreateBody();
            shoulder.BodyType = BodyType.Dynamic;
            shoulder.Position = p4 + _position;
            shoulder.AngularDamping = 10f;
            
            Fixture legFixture = leg.CreateFixture(legPolygon);
            legFixture.CollisionGroup = -1;

            Fixture shoulderFixture = shoulder.CreateFixture(shoulderPolygon);
            shoulderFixture.CollisionGroup = -1;

            // Using a soft distancejoint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(leg, shoulder, p2 + _position, p5 + _position, true);
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10f;

            world.Add(djd);
            _walkerJoints.Add(djd);

            DistanceJoint djd2 = new DistanceJoint(leg, shoulder, p3 + _position, p4 + _position, true);
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10f;

            world.Add(djd2);
            _walkerJoints.Add(djd2);

            DistanceJoint djd3 = new DistanceJoint(leg, _wheel, p3 + _position, wheelAnchor + _position, true);
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10f;

            world.Add(djd3);
            _walkerJoints.Add(djd3);

            DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, p6 + _position, wheelAnchor + _position, true);
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10f;

            world.Add(djd4);
            _walkerJoints.Add(djd4);

            RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis, p4 + _position, true);
            world.Add(rjd);
        }

        private void CreateLegTextures()
        {
            Vector2 p1 = new Vector2(-5.4f, -6.1f);
            Vector2 p2 = new Vector2(-7.2f, -1.2f);
            Vector2 p3 = new Vector2(-4.3f, -1.9f);
            Vector2 p4 = Vector2.Zero;
            Vector2 p5 = new Vector2(-2.9f,  0.7f);
            Vector2 p6 = new Vector2( 0.6f,  2.9f);

            _leftShoulder = new Sprite(ContentWrapper.PolygonTexture(new[] { p4, p5, p6 }, Color.White * 0.6f, ContentWrapper.Black, 24f));
            _leftShoulder.Origin = ContentWrapper.CalculateOrigin(_leftShoulders[0], 24f);

            _leftLeg = new Sprite(ContentWrapper.PolygonTexture(new[] { p1, p3, p2 }, Color.White * 0.6f, ContentWrapper.Black, 24f));
            _leftLeg.Origin = ContentWrapper.CalculateOrigin(_leftLegs[0], 24f);

            p1.X *= -1f;
            p2.X *= -1f;
            p3.X *= -1f;
            p5.X *= -1f;
            p6.X *= -1f;

            _rightShoulder = new Sprite(ContentWrapper.PolygonTexture(new[] { p4, p6, p5 }, Color.White * 0.6f, ContentWrapper.Black, 24f));
            _rightShoulder.Origin = ContentWrapper.CalculateOrigin(_rightShoulders[0], 24f);

            _rightLeg = new Sprite(ContentWrapper.PolygonTexture(new[] { p1, p2, p3 }, Color.White * 0.6f, ContentWrapper.Black, 24f));
            _rightLeg.Origin = ContentWrapper.CalculateOrigin(_rightLegs[0], 24f);
        }

        public void Reverse()
        {
            _motorSpeed *= -1f;
            _motorJoint.MotorSpeed = _motorSpeed;
        }

        public void Draw(SpriteBatch batch, BasicEffect batchEffect, LineBatch lines, Camera2D camera)
        {
            batchEffect.View = camera.View;
            batchEffect.Projection = camera.Projection;

            batch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
            batch.Draw(_body.Texture, _chassis.Position, null, Color.White, _chassis.Rotation, _body.Origin, new Vector2(5f, 2.0f) * _body.TexelSize, SpriteEffects.FlipVertically, 0f);
            batch.End();

            for (int i = 0; i < 3; i++)
            {
                batch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
                batch.Draw(_leftLeg.Texture, _leftLegs[i].Position, null, _walkerColors[i], _leftLegs[i].Rotation, _leftLeg.Origin, new Vector2(2.9583f, 4.9583f) * _leftLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
                batch.Draw(_leftShoulder.Texture, _leftShoulders[i].Position, null, _walkerColors[i], _leftShoulders[i].Rotation, _leftShoulder.Origin, new Vector2(3.5833f, 2.9583f) * _leftShoulder.TexelSize, SpriteEffects.FlipVertically, 0f);
                batch.Draw(_rightLeg.Texture, _rightLegs[i].Position, null, _walkerColors[i], _rightLegs[i].Rotation, _rightLeg.Origin, new Vector2(2.9583f, 4.9583f) * _rightLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
                batch.Draw(_rightShoulder.Texture, _rightShoulders[i].Position, null, _walkerColors[i], _rightShoulders[i].Rotation, _rightShoulder.Origin, new Vector2(3.5833f, 2.9583f) * _rightShoulder.TexelSize, SpriteEffects.FlipVertically, 0f);
                batch.End();
                
                lines.Begin(camera.Projection, camera.View);
                for (int j = 0; j < 8; j++) // 4 joints pro for schleife...
                {
                    lines.DrawLine(_walkerJoints[8 * i + j].WorldAnchorA, _walkerJoints[8 * i + j].WorldAnchorB, ContentWrapper.Grey);
                }
                lines.End();
            }
            
            batch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
            batch.Draw(_engine.Texture, _wheel.Position, null, Color.White * 0.7f, _wheel.Rotation, _engine.Origin, new Vector2(2f * 1.6f) * _engine.TexelSize, SpriteEffects.FlipVertically, 0f);
            batch.End();
        }
    }
}