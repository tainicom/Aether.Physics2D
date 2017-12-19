﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private SpriteBatch _batch;
        private float _bodyRadius;

        public Objects(World world, ScreenManager screenManager, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type)
        {
            _batch = screenManager.SpriteBatch;
            _bodyRadius = radius;
            _bodies = new List<Body>(count);

            CollidesWith = Category.All;
            CollisionCategories = Category.All;

            for (int i = 0; i < count; ++i)
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

            for (int i = 0; i < _bodies.Count; ++i)
            {
                Body body = _bodies[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.SetRestitution(.7f);
                body.SetFriction(.2f);
                body.SetCollisionCategories(CollisionCategories);
                body.SetCollidesWith(CollidesWith);
            }

            //GFX
            AssetCreator creator = screenManager.Assets;
            switch (type)
            {
                case ObjectType.Circle:
                    _object = new Sprite(creator.CircleTexture(radius, MaterialType.Dots, Color.DarkRed, 0.8f, 24f));
                    break;
                case ObjectType.Rectangle:
                    _object = new Sprite(creator.TextureFromVertices(PolygonTools.CreateRectangle(radius / 2f, radius / 2f), MaterialType.Dots, Color.Blue, 0.8f, 24f));
                    break;
                case ObjectType.Star:
                    _object = new Sprite(creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 0f, 1f), MaterialType.Dots, Color.Yellow, 0.8f, 24f));
                    break;
                case ObjectType.Gear:
                    _object = new Sprite(creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 100f, 1f), MaterialType.Dots, Color.DarkGreen, 0.8f, 24f));
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

        public void Draw()
        {
            

            for (int i = 0; i < _bodies.Count; ++i)
            {
                _batch.Draw(_object.Texture, _bodies[i].Position, null, Color.White, _bodies[i].Rotation, _object.Origin, new Vector2(2f*_bodyRadius) * _object.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
        }
    }
}