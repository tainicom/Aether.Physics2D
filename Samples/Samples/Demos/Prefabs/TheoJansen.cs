/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class TheoJansenWalker
    {
        private Sprite _body;
        private Body _chassis;
        private Sprite _engine;
        private Sprite _leftLeg;
        private Body[] _leftLegs;
        private Sprite _leftShoulder;
        private Body[] _leftShoulders;
        private RevoluteJoint _motorJoint;
        private float _motorSpeed;
        private Vector2 _position;
        private Sprite _rightLeg;
        private Body[] _rightLegs;

        private Sprite _rightShoulder;
        private Body[] _rightShoulders;
        private SpriteBatch _spriteBatch;
        private LineBatch _lineBatch;
        private Camera2D _camera;

        private List<DistanceJoint> _walkerJoints;
        private Body _wheel;

        public TheoJansenWalker(World world, ScreenManager screenManager, Camera2D camera, Vector2 position)
        {
            _position = position;
            _motorSpeed = 2.0f;
            _spriteBatch = screenManager.SpriteBatch;
            _lineBatch = screenManager.LineBatch;
            _camera = camera;

            _walkerJoints = new List<DistanceJoint>();

            _leftShoulders = new Body[3];
            _rightShoulders = new Body[3];
            _leftLegs = new Body[3];
            _rightLegs = new Body[3];

            Vector2 pivot = new Vector2(0f, 0.8f);

            CreateChassis(world, pivot, screenManager.Assets);

            Vector2 wheelAnchor = pivot + new Vector2(0f, -0.8f);

            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[0], out _leftLegs[0]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[0], out _rightLegs[0]);

            _wheel.SetTransform(_wheel.Position, 120f * MathHelper.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[1], out _leftLegs[1]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[1], out _rightLegs[1]);

            _wheel.SetTransform(_wheel.Position, -120f * MathHelper.Pi / 180f);
            CreateLeg(world, -1f, wheelAnchor, out _leftShoulders[2], out _leftLegs[2]);
            CreateLeg(world, 1f, wheelAnchor, out _rightShoulders[2], out _rightLegs[2]);


            CreateLegTextures(screenManager.Assets);
        }

        private void CreateChassis(World world, Vector2 pivot, AssetCreator assets)
        {
            {
                PolygonShape shape = new PolygonShape(1f);
                shape.Vertices = PolygonTools.CreateRectangle(5f/2f, 2.0f/2f);

                _body = new Sprite(assets.TextureFromShape(shape, MaterialType.Blank, Color.Beige, 1f));

                _chassis = world.CreateBody();
                _chassis.BodyType = BodyType.Dynamic;
                _chassis.Position = pivot + _position;

                Fixture fixture = _chassis.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                CircleShape shape = new CircleShape(1.6f, 1f);
                _engine = new Sprite(assets.TextureFromShape(shape, MaterialType.Waves, Color.Beige * 0.8f, 1f));

                _wheel = world.CreateBody();
                _wheel.BodyType = BodyType.Dynamic;
                _wheel.Position = pivot + _position;

                Fixture fixture = _wheel.CreateFixture(shape);
                fixture.CollisionGroup = -1;
            }

            {
                _motorJoint = new RevoluteJoint(_wheel, _chassis, _wheel.GetLocalPoint(_chassis.Position), Vector2.Zero);
                _motorJoint.CollideConnected = false;
                _motorJoint.MotorSpeed = _motorSpeed;
                _motorJoint.MaxMotorTorque = 400f;
                _motorJoint.MotorEnabled = true;
                world.Add(_motorJoint);
            }
        }

        public void Reverse()
        {
            _motorSpeed *= -1f;
            _motorJoint.MotorSpeed = _motorSpeed;
        }

        private void CreateLeg(World world, float direction, Vector2 wheelAnchor, out Body shoulder, out Body leg)
        {
            Vector2 p1 = new Vector2(5.4f * direction, -6.1f);
            Vector2 p2 = new Vector2(7.2f * direction, -1.2f);
            Vector2 p3 = new Vector2(4.3f * direction, -1.9f);
            Vector2 p4 = new Vector2(3.1f * direction, 0.8f);
            Vector2 p5 = new Vector2(6.0f * direction, 1.5f);
            Vector2 p6 = new Vector2(2.5f * direction, 3.7f);

            PolygonShape shoulderPolygon;
            PolygonShape legPolygon;

            Vertices vertices = new Vertices(3);

            if (direction > 0f)
            {
                vertices.Add(p1);
                vertices.Add(p2);
                vertices.Add(p3);
                shoulderPolygon = new PolygonShape(vertices, 1);

                vertices[0] = Vector2.Zero;
                vertices[1] = p5 - p4;
                vertices[2] = p6 - p4;
                legPolygon = new PolygonShape(vertices, 2);
            }
            else
            {
                vertices.Add(p1);
                vertices.Add(p3);
                vertices.Add(p2);
                shoulderPolygon = new PolygonShape(vertices, 1);

                vertices[0] = Vector2.Zero;
                vertices[1] = p6 - p4;
                vertices[2] = p5 - p4;
                legPolygon = new PolygonShape(vertices, 2);
            }

            leg = world.CreateBody();
            leg.BodyType = BodyType.Dynamic;
            leg.Position = _position;
            leg.AngularDamping = 10f;


            shoulder = world.CreateBody();
            shoulder.BodyType = BodyType.Dynamic;
            shoulder.Position = p4 + _position;
            shoulder.AngularDamping = 10f;


            Fixture f1 = leg.CreateFixture(shoulderPolygon);
            f1.CollisionGroup = -1;

            Fixture f2 = shoulder.CreateFixture(legPolygon);
            f2.CollisionGroup = -1;

            // Using a soft distanceraint can reduce some jitter.
            // It also makes the structure seem a bit more fluid by
            // acting like a suspension system.
            DistanceJoint djd = new DistanceJoint(leg, shoulder, leg.GetLocalPoint(p2 + _position), shoulder.GetLocalPoint(p5 + _position));
            djd.DampingRatio = 0.5f;
            djd.Frequency = 10f;

            world.Add(djd);
            _walkerJoints.Add(djd);

            DistanceJoint djd2 = new DistanceJoint(leg, shoulder, leg.GetLocalPoint(p3 + _position), shoulder.GetLocalPoint(p4 + _position));
            djd2.DampingRatio = 0.5f;
            djd2.Frequency = 10f;

            world.Add(djd2);
            _walkerJoints.Add(djd2);

            DistanceJoint djd3 = new DistanceJoint(leg, _wheel, leg.GetLocalPoint(p3 + _position), _wheel.GetLocalPoint(wheelAnchor + _position));
            djd3.DampingRatio = 0.5f;
            djd3.Frequency = 10f;

            world.Add(djd3);
            _walkerJoints.Add(djd3);

            DistanceJoint djd4 = new DistanceJoint(shoulder, _wheel, shoulder.GetLocalPoint(p6 + _position), _wheel.GetLocalPoint(wheelAnchor + _position));
            djd4.DampingRatio = 0.5f;
            djd4.Frequency = 10f;

            world.Add(djd4);
            _walkerJoints.Add(djd4);

            Vector2 anchor = p4 - new Vector2(0f, 0.8f);
            RevoluteJoint rjd = new RevoluteJoint(shoulder, _chassis, shoulder.GetLocalPoint(_chassis.GetWorldPoint(anchor)), anchor);
            world.Add(rjd);
        }

        private void CreateLegTextures(AssetCreator assets)
        {
            Vector2 p1 = new Vector2(-5.4f, -6.1f);
            Vector2 p2 = new Vector2(-7.2f, -1.2f);
            Vector2 p3 = new Vector2(-4.3f, -1.9f);
            Vector2 p4 = Vector2.Zero;
            Vector2 p5 = new Vector2(-2.9f,  0.7f);
            Vector2 p6 = new Vector2( 0.6f,  2.9f);

            Vertices vertices = new Vertices(3);

            vertices.Add(p1);
            vertices.Add(p3);
            vertices.Add(p2);
            _leftLeg = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.IndianRed * 0.8f, 1f, 24f));

            vertices[0] = p4;
            vertices[1] = p5;
            vertices[2] = p6;
            _leftShoulder = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.Beige * 0.8f, 1f, 24f));

            p1.X *= -1f;
            p2.X *= -1f;
            p3.X *= -1f;
            p5.X *= -1f;
            p6.X *= -1f;

            vertices[0] = p1;
            vertices[1] = p2;
            vertices[2] = p3;
            _rightLeg = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.IndianRed * 0.8f, 1f, 24f));

            vertices[0] = p4;
            vertices[1] = p6;
            vertices[2] = p5;
            _rightShoulder = new Sprite(assets.TextureFromVertices(vertices, MaterialType.Blank, Color.Beige * 0.8f, 1f, 24f));

            _leftShoulder.Origin = AssetCreator.CalculateOrigin(_leftShoulders[0], 24f);
            _leftLeg.Origin = AssetCreator.CalculateOrigin(_leftLegs[0], 24f);
            _rightShoulder.Origin = AssetCreator.CalculateOrigin(_rightShoulders[0], 24f);
            _rightLeg.Origin = AssetCreator.CalculateOrigin(_rightLegs[0], 24f);
        }

        public void Draw(BasicEffect batchEffect, Camera2D camera)
        {
            batchEffect.View = camera.View;
            batchEffect.Projection = camera.Projection;

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
            _spriteBatch.Draw(_body.Texture, _chassis.Position, null, Color.White, _chassis.Rotation, _body.Origin, new Vector2(5f, 2.0f) * _body.TexelSize, SpriteEffects.FlipVertically, 0f);
            _spriteBatch.End();
            
            for (int i = 0; i < 3; ++i)
            {
                _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
                _spriteBatch.Draw(_leftLeg.Texture, _leftLegs[i].Position, null, Color.White, _leftLegs[i].Rotation, _leftLeg.Origin, new Vector2(2.9583f, 4.9583f) * _leftLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
                _spriteBatch.Draw(_leftShoulder.Texture, _leftShoulders[i].Position, null, Color.White, _leftShoulders[i].Rotation, _leftShoulder.Origin, new Vector2(3.5833f, 2.9583f) * _leftShoulder.TexelSize, SpriteEffects.FlipVertically, 0f);
                _spriteBatch.Draw(_rightLeg.Texture, _rightLegs[i].Position, null, Color.White, _rightLegs[i].Rotation, _rightLeg.Origin, new Vector2(2.9583f, 4.9583f) * _rightLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
                _spriteBatch.Draw(_rightShoulder.Texture, _rightShoulders[i].Position, null, Color.White, _rightShoulders[i].Rotation, _rightShoulder.Origin, new Vector2(3.5833f, 2.9583f) * _rightShoulder.TexelSize, SpriteEffects.FlipVertically, 0f);
                _spriteBatch.End();
                
                _lineBatch.Begin(_camera.Projection, _camera.View);
                for (int j = 0; j < 8; j++) 
                {
                    _lineBatch.DrawLine(_walkerJoints[8 * i + j].WorldAnchorA, _walkerJoints[8 * i + j].WorldAnchorB, Color.DarkRed);
                }
                _lineBatch.End();
            }

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, batchEffect);
            _spriteBatch.Draw(_engine.Texture, _wheel.Position, null, Color.White * 0.7f, _wheel.Rotation, _engine.Origin, new Vector2(2f * 1.6f) * _engine.TexelSize, SpriteEffects.FlipVertically, 0f);
            _spriteBatch.End();
        }
    }
}