﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public enum ObjectType
    {
        Circle,
        Rectangle,
        Gear,
        Star
    }

    public class Objects
    {
        private List<Body> _bodies;
        private Category _collidesWith;
        private Category _collisionCategories;
        private Sprite _object;
        private float _bodyRadius;

        public Objects(World world, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _bodies = new List<Body>(count);
            _bodyRadius = radius;
            CollidesWith = Category.All;
            CollisionCategories = Category.All;

            // Physics
            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case ObjectType.Circle:
                        _bodies.Add(world.CreateCircle(radius, 1f));
                        break;
                    case ObjectType.Rectangle:
                        _bodies.Add(world.CreateRectangle(radius, radius, 1f));
                        _bodyRadius = radius/2f;
                        break;
                    case ObjectType.Star:
                        _bodies.Add(world.CreateGear(radius, 10, 0f, 1f, 1f));
                        _bodyRadius = radius * 2.7f;
                        break;
                    case ObjectType.Gear:
                        _bodies.Add(world.CreateGear(radius, 10, 100f, 1f, 1f));
                        _bodyRadius = radius * 2.7f;
                        break;
                }
            }

            for (int i = 0; i < _bodies.Count; i++)
            {
                Body body = _bodies[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.SetRestitution(0.7f);
                body.SetFriction(0.2f);
            }

            //GFX
            switch (type)
            {
                case ObjectType.Circle:
                    _object = new Sprite(ContentWrapper.CircleTexture(radius, ContentWrapper.Gold, ContentWrapper.Grey, 24f));
                    break;
                case ObjectType.Rectangle:
                    _object = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateRectangle(radius / 2f, radius / 2f), ContentWrapper.Red, ContentWrapper.Grey, 24f));
                    break;
                case ObjectType.Star:
                    _object = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateGear(radius, 10, 0f, 1f), ContentWrapper.Brown, ContentWrapper.Black, 24f));
                    break;
                case ObjectType.Gear:
                    _object = new Sprite(ContentWrapper.PolygonTexture(PolygonTools.CreateGear(radius, 10, 100f, 1f), ContentWrapper.Orange, ContentWrapper.Grey, 24f));
                    break;
            }
        }

        public Category CollisionCategories
        {
            get { return _collisionCategories; }
            set
            {
                _collisionCategories = value;

                foreach (Body body in _bodies)
                    body.SetCollisionCategories(_collisionCategories);
            }
        }

        public Category CollidesWith
        {
            get { return _collidesWith; }
            set
            {
                _collidesWith = value;

                foreach (Body body in _bodies)
                    body.SetCollidesWith(_collidesWith);
            }
        }

        public void Draw(SpriteBatch batch)
        {
            foreach (Body body in _bodies)
            {
                batch.Draw(_object.Texture, body.Position, null, Color.White, body.Rotation, _object.Origin, new Vector2(2f * _bodyRadius) * _object.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
        }
    }
}