/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Ragdoll
    {
        private const float ArmDensity = 10;
        private const float LegDensity = 15;
        private const float LimbAngularDamping = 7;

        private Body _body;
        private Sprite _face;
        private Body _head;
        private Sprite _lowerArm;

        private Body _lowerLeftArm;
        private Body _lowerLeftLeg;
        private Sprite _lowerLeg;
        private Body _lowerRightArm;
        private Body _lowerRightLeg;
        private Sprite _torso;
        private Sprite _upperArm;

        private Body _upperLeftArm;
        private Body _upperLeftLeg;
        private Sprite _upperLeg;
        private Body _upperRightArm;
        private Body _upperRightLeg;

        private SpriteBatch _batch;

        public Ragdoll(World world, ScreenManager screenManager, Vector2 position)
        {
            _batch = screenManager.SpriteBatch;

            CreateBody(world, position);
            CreateJoints(world);

            CreateGFX(screenManager.Assets);
        }

        public Body Body
        {
            get { return _body; }
        }

        //Torso
        private void CreateBody(World world, Vector2 position)
        {
            //Head
            _head = world.CreateCircle(0.9f, 10f);
            _head.BodyType = BodyType.Dynamic;
            _head.AngularDamping = LimbAngularDamping;
            _head.Mass = 2f;
            _head.Position = position;

            //Body
            _body = world.CreateRoundedRectangle(2f, 4f, 0.5f, 0.7f, 2, 10f);
            _body.BodyType = BodyType.Dynamic;
            _body.Mass = 2f;
            _body.Position = position + new Vector2(0f, -3f);

            //Left Arm
            _lowerLeftArm = world.CreateCapsule(1f, 0.45f, ArmDensity);
            _lowerLeftArm.BodyType = BodyType.Dynamic;
            _lowerLeftArm.AngularDamping = LimbAngularDamping;
            _lowerLeftArm.Mass = 2f;
            _lowerLeftArm.Rotation = -1.4f;
            _lowerLeftArm.Position = position + new Vector2(-4f, -2.2f);

            _upperLeftArm = world.CreateCapsule(1f, 0.45f, ArmDensity);
            _upperLeftArm.BodyType = BodyType.Dynamic;
            _upperLeftArm.AngularDamping = LimbAngularDamping;
            _upperLeftArm.Mass = 2f;
            _upperLeftArm.Rotation = -1.4f;
            _upperLeftArm.Position = position + new Vector2(-2f, -1.8f);

            //Right Arm
            _lowerRightArm = world.CreateCapsule(1f, 0.45f, ArmDensity);
            _lowerRightArm.BodyType = BodyType.Dynamic;
            _lowerRightArm.AngularDamping = LimbAngularDamping;
            _lowerRightArm.Mass = 2f;
            _lowerRightArm.Rotation = 1.4f;
            _lowerRightArm.Position = position + new Vector2(4f, -2.2f);

            _upperRightArm = world.CreateCapsule(1f, 0.45f, ArmDensity);
            _upperRightArm.BodyType = BodyType.Dynamic;
            _upperRightArm.AngularDamping = LimbAngularDamping;
            _upperRightArm.Mass = 2f;
            _upperRightArm.Rotation = 1.4f;
            _upperRightArm.Position = position + new Vector2(2f, -1.8f);

            //Left Leg
            _lowerLeftLeg = world.CreateCapsule(1f, 0.5f, LegDensity);
            _lowerLeftLeg.BodyType = BodyType.Dynamic;
            _lowerLeftLeg.AngularDamping = LimbAngularDamping;
            _lowerLeftLeg.Mass = 2f;
            _lowerLeftLeg.Position = position + new Vector2(-0.6f, -8f);

            _upperLeftLeg = world.CreateCapsule(1f, 0.5f, LegDensity);
            _upperLeftLeg.BodyType = BodyType.Dynamic;
            _upperLeftLeg.AngularDamping = LimbAngularDamping;
            _upperLeftLeg.Mass = 2f;
            _upperLeftLeg.Position = position + new Vector2(-0.6f, -6f);

            //Right Leg
            _lowerRightLeg = world.CreateCapsule(1f, 0.5f, LegDensity);
            _lowerRightLeg.BodyType = BodyType.Dynamic;
            _lowerRightLeg.AngularDamping = LimbAngularDamping;
            _lowerRightLeg.Mass = 2f;
            _lowerRightLeg.Position = position + new Vector2(0.6f, -8f);

            _upperRightLeg = world.CreateCapsule(1f, 0.5f, LegDensity);
            _upperRightLeg.BodyType = BodyType.Dynamic;
            _upperRightLeg.AngularDamping = LimbAngularDamping;
            _upperRightLeg.Mass = 2f;
            _upperRightLeg.Position = position + new Vector2(0.6f, -6f);
        }

        private void CreateJoints(World world)
        {
            const float dampingRatio = 1f;
            const float frequency = 25f;

            //head -> body
            DistanceJoint jHeadBody = new DistanceJoint(_head, _body,
                                                        new Vector2(0f, -1f),
                                                        new Vector2(0f, 2f));
            jHeadBody.CollideConnected = true;
            jHeadBody.DampingRatio = dampingRatio;
            jHeadBody.Frequency = frequency;
            jHeadBody.Length = 0.025f;
            world.Add(jHeadBody);

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftArm = new DistanceJoint(_lowerLeftArm, _upperLeftArm,
                                                       new Vector2(0f, 1f),
                                                       new Vector2(0f, -1f));
            jLeftArm.CollideConnected = true;
            jLeftArm.DampingRatio = dampingRatio;
            jLeftArm.Frequency = frequency;
            jLeftArm.Length = 0.02f;
            world.Add(jLeftArm);

            //upperLeftArm -> body
            DistanceJoint jLeftArmBody = new DistanceJoint(_upperLeftArm, _body,
                                                           new Vector2(0f, 1f),
                                                           new Vector2(-1f, 1.5f));
            jLeftArmBody.CollideConnected = true;
            jLeftArmBody.DampingRatio = dampingRatio;
            jLeftArmBody.Frequency = frequency;
            jLeftArmBody.Length = 0.02f;
            world.Add(jLeftArmBody);

            //lowerRightArm -> upperRightArm
            DistanceJoint jRightArm = new DistanceJoint(_lowerRightArm, _upperRightArm,
                                                        new Vector2(0f, 1f),
                                                        new Vector2(0f, -1f));
            jRightArm.CollideConnected = true;
            jRightArm.DampingRatio = dampingRatio;
            jRightArm.Frequency = frequency;
            jRightArm.Length = 0.02f;
            world.Add(jRightArm);

            //upperRightArm -> body
            DistanceJoint jRightArmBody = new DistanceJoint(_upperRightArm, _body,
                                                            new Vector2(0f, 1f),
                                                            new Vector2(1f, 1.5f));

            jRightArmBody.CollideConnected = true;
            jRightArmBody.DampingRatio = dampingRatio;
            jRightArmBody.Frequency = 25;
            jRightArmBody.Length = 0.02f;
            world.Add(jRightArmBody);

            //lowerLeftLeg -> upperLeftLeg
            DistanceJoint jLeftLeg = new DistanceJoint(_lowerLeftLeg, _upperLeftLeg,
                                                       new Vector2(0f, 1.1f),
                                                       new Vector2(0f, -1f));
            jLeftLeg.CollideConnected = true;
            jLeftLeg.DampingRatio = dampingRatio;
            jLeftLeg.Frequency = frequency;
            jLeftLeg.Length = 0.05f;
            world.Add(jLeftLeg);

            //upperLeftLeg -> body
            DistanceJoint jLeftLegBody = new DistanceJoint(_upperLeftLeg, _body,
                                                           new Vector2(0f, 1.1f),
                                                           new Vector2(-0.8f, -1.9f));
            jLeftLegBody.CollideConnected = true;
            jLeftLegBody.DampingRatio = dampingRatio;
            jLeftLegBody.Frequency = frequency;
            jLeftLegBody.Length = 0.02f;
            world.Add(jLeftLegBody);

            //lowerRightleg -> upperRightleg
            DistanceJoint jRightLeg = new DistanceJoint(_lowerRightLeg, _upperRightLeg,
                                                        new Vector2(0f, 1.1f),
                                                        new Vector2(0f, -1f));
            jRightLeg.CollideConnected = true;
            jRightLeg.DampingRatio = dampingRatio;
            jRightLeg.Frequency = frequency;
            jRightLeg.Length = 0.05f;
            world.Add(jRightLeg);

            //upperRightleg -> body
            DistanceJoint jRightLegBody = new DistanceJoint(_upperRightLeg, _body,
                                                            new Vector2(0f, 1.1f),
                                                            new Vector2(0.8f, -1.9f));
            jRightLegBody.CollideConnected = true;
            jRightLegBody.DampingRatio = dampingRatio;
            jRightLegBody.Frequency = frequency;
            jRightLegBody.Length = 0.02f;
            world.Add(jRightLegBody);
        }

        private void CreateGFX(AssetCreator assets)
        {
            _face = new Sprite(assets.CircleTexture(0.9f, MaterialType.Squares, Color.Gray, 1f, 24f));
            _torso = new Sprite(assets.TextureFromVertices(PolygonTools.CreateRoundedRectangle(2f, 4f, 0.5f, 0.7f, 2), MaterialType.Squares, Color.LightSlateGray, 0.8f, 24f));

            _upperArm = new Sprite(assets.TextureFromVertices(PolygonTools.CreateCapsule(1.9f, 0.45f, 16), MaterialType.Squares, Color.DimGray, 0.8f, 24f));
            _lowerArm = new Sprite(assets.TextureFromVertices(PolygonTools.CreateCapsule(1.9f, 0.45f, 16), MaterialType.Squares, Color.DarkSlateGray, 0.8f, 24f));

            _upperLeg = new Sprite(assets.TextureFromVertices(PolygonTools.CreateCapsule(2f, 0.5f, 16), MaterialType.Squares, Color.DimGray, 0.8f, 24f));
            _lowerLeg = new Sprite(assets.TextureFromVertices(PolygonTools.CreateCapsule(2f, 0.5f, 16), MaterialType.Squares, Color.DarkSlateGray, 0.8f, 24f));
        }

        public void Draw()
        {
            _batch.Draw(_lowerLeg.Texture, _lowerLeftLeg.Position, null, Color.White, _lowerLeftLeg.Rotation, _lowerLeg.Origin, new Vector2(2f * 0.5f, 2f * 0.5f + 1f) * _lowerLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_lowerLeg.Texture, _lowerRightLeg.Position, null, Color.White, _lowerRightLeg.Rotation, _lowerLeg.Origin, new Vector2(2f * 0.5f, 2f * 0.5f + 1f) * _lowerLeg.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_upperLeg.Texture, _upperLeftLeg.Position, null, Color.White, _upperLeftLeg.Rotation, _upperLeg.Origin, new Vector2(2f * 0.5f, 2f * 0.5f + 1f) * _upperLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_upperLeg.Texture, _upperRightLeg.Position, null, Color.White, _upperRightLeg.Rotation, _upperLeg.Origin, new Vector2(2f * 0.5f, 2f * 0.5f + 1f) * _upperLeg.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_lowerArm.Texture, _lowerLeftArm.Position, null, Color.White, _lowerLeftArm.Rotation, _lowerArm.Origin, new Vector2(2f * 0.45f, 2f * 0.45f + 1f) * _lowerArm.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_lowerArm.Texture, _lowerRightArm.Position, null, Color.White, _lowerRightArm.Rotation, _lowerArm.Origin, new Vector2(2f * 0.45f, 2f * 0.45f + 1f) * _lowerArm.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_upperArm.Texture, _upperLeftArm.Position, null, Color.White, _upperLeftArm.Rotation, _upperArm.Origin, new Vector2(2f * 0.45f, 2f * 0.45f + 1f) * _upperArm.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_upperArm.Texture, _upperRightArm.Position, null, Color.White, _upperRightArm.Rotation, _upperArm.Origin, new Vector2(2f * 0.45f, 2f * 0.45f + 1f) * _upperArm.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_torso.Texture, _body.Position, null, Color.White, _body.Rotation, _torso.Origin, new Vector2(2f, 4f) * _torso.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_face.Texture, _head.Position, null, Color.White, _head.Rotation, _face.Origin, new Vector2(2f * 0.9f) * _face.TexelSize, SpriteEffects.FlipVertically, 0f);
        }
    }
}