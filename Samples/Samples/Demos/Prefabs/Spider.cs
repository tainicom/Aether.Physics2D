/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Spider
    {
        private const float SpiderBodyRadius = 0.65f;
        private bool _kneeFlexed;
        private float _kneeTargetAngle = 0.4f;
        private AngleJoint _leftKneeAngleJoint;
        private AngleJoint _leftShoulderAngleJoint;
        private Sprite _lowerLeg;
        private Vector2 _lowerLegSize = new Vector2(1.8f, 0.3f);

        private AngleJoint _rightKneeAngleJoint;
        private AngleJoint _rightShoulderAngleJoint;
        private float _s;
        private SpriteBatch _batch;
        private bool _shoulderFlexed;
        private float _shoulderTargetAngle = 0.2f;
        private Sprite _torso;
        private Sprite _upperLeg;
        private Vector2 _upperLegSize = new Vector2(1.8f, 0.3f);

        private Body _circle;
        private Body _leftLower;
        private Body _leftUpper;
        private Body _rightLower;
        private Body _rightUpper;

        public Spider(World world, ScreenManager screenManager, Vector2 position)
        {
            _batch = screenManager.SpriteBatch;

            //Load bodies
            _circle = world.CreateCircle(SpiderBodyRadius, 0.1f, position);
            _circle.BodyType = BodyType.Dynamic;

            //Left upper leg
            _leftUpper = world.CreateRectangle(_upperLegSize.X, _upperLegSize.Y, 0.1f, _circle.Position - new Vector2(SpiderBodyRadius, 0f) - new Vector2(_upperLegSize.X / 2f, 0f));
            _leftUpper.BodyType = BodyType.Dynamic;

            //Left lower leg
            _leftLower = world.CreateRectangle(_lowerLegSize.X, _lowerLegSize.Y, 0.1f, _circle.Position - new Vector2(SpiderBodyRadius, 0f) - new Vector2(_upperLegSize.X, 0f) - new Vector2(_lowerLegSize.X / 2f, 0f));
            _leftLower.BodyType = BodyType.Dynamic;

            //Right upper leg
            _rightUpper = world.CreateRectangle(_upperLegSize.X, _upperLegSize.Y, 0.1f, _circle.Position + new Vector2(SpiderBodyRadius, 0f) + new Vector2(_upperLegSize.X / 2f, 0f));
            _rightUpper.BodyType = BodyType.Dynamic;

            //Right lower leg
            _rightLower = world.CreateRectangle(_lowerLegSize.X, _lowerLegSize.Y, 0.1f, _circle.Position + new Vector2(SpiderBodyRadius, 0f) + new Vector2(_upperLegSize.X, 0f) + new Vector2(_lowerLegSize.X / 2f, 0f));
            _rightLower.BodyType = BodyType.Dynamic;

            //Create joints
            JointFactory.CreateRevoluteJoint(world, _circle, _leftUpper, new Vector2(_upperLegSize.X / 2f, 0f));
            _leftShoulderAngleJoint = JointFactory.CreateAngleJoint(world, _circle, _leftUpper);
            _leftShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _circle, _rightUpper, new Vector2(-_upperLegSize.X / 2f, 0f));
            _rightShoulderAngleJoint = JointFactory.CreateAngleJoint(world, _circle, _rightUpper);
            _rightShoulderAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _leftUpper, _leftLower, new Vector2(_lowerLegSize.X / 2f, 0f));
            _leftKneeAngleJoint = JointFactory.CreateAngleJoint(world, _leftUpper, _leftLower);
            _leftKneeAngleJoint.MaxImpulse = 3f;

            JointFactory.CreateRevoluteJoint(world, _rightUpper, _rightLower, new Vector2(-_lowerLegSize.X / 2f, 0f));
            _rightKneeAngleJoint = JointFactory.CreateAngleJoint(world, _rightUpper, _rightLower);
            _rightKneeAngleJoint.MaxImpulse = 3;

            //GFX
            AssetCreator creator = screenManager.Assets;
            _torso = new Sprite(creator.CircleTexture(SpiderBodyRadius, MaterialType.Waves, Color.Gray, 1f, 24f));
            _upperLeg = new Sprite(creator.TextureFromShape(_leftUpper.FixtureList[0].Shape, MaterialType.Blank, Color.DimGray, 1f));
            _lowerLeg = new Sprite(creator.TextureFromShape(_leftLower.FixtureList[0].Shape, MaterialType.Blank, Color.DarkSlateGray, 1f));
        }

        public void Update(GameTime gameTime)
        {
            _s += gameTime.ElapsedGameTime.Milliseconds;
            if (_s > 4000)
            {
                _s = 0;

                _kneeFlexed = !_kneeFlexed;
                _shoulderFlexed = !_shoulderFlexed;

                if (_kneeFlexed)
                    _kneeTargetAngle = 1.4f;
                else
                    _kneeTargetAngle = 0.4f;

                if (_kneeFlexed)
                    _shoulderTargetAngle = 1.2f;
                else
                    _shoulderTargetAngle = 0.2f;
            }

            _leftKneeAngleJoint.TargetAngle = _kneeTargetAngle;
            _rightKneeAngleJoint.TargetAngle = -_kneeTargetAngle;

            _leftShoulderAngleJoint.TargetAngle = _shoulderTargetAngle;
            _rightShoulderAngleJoint.TargetAngle = -_shoulderTargetAngle;
        }

        public void Draw()
        {
            _batch.Draw(_lowerLeg.Texture, _leftLower.Position, null, Color.White, _leftLower.Rotation, _lowerLeg.Origin, _lowerLegSize * _lowerLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_lowerLeg.Texture, _rightLower.Position, null, Color.White, _rightLower.Rotation, _lowerLeg.Origin, _lowerLegSize * _lowerLeg.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_upperLeg.Texture, _leftUpper.Position, null, Color.White, _leftUpper.Rotation, _upperLeg.Origin, _upperLegSize * _upperLeg.TexelSize, SpriteEffects.FlipVertically, 0f);
            _batch.Draw(_upperLeg.Texture, _rightUpper.Position, null, Color.White, _rightUpper.Rotation, _upperLeg.Origin, _upperLegSize * _upperLeg.TexelSize, SpriteEffects.FlipVertically, 0f);

            _batch.Draw(_torso.Texture, _circle.Position, null, Color.White, _circle.Rotation, _torso.Origin, new Vector2(2f * SpiderBodyRadius) * _torso.TexelSize, SpriteEffects.FlipVertically, 0f);
        }
    }
}