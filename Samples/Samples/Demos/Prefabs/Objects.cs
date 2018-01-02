// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
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
        public List<Body> BodyList { get; private set; }
        private Sprite _object;
        private SpriteBatch _batch;
        private float _bodyRadius;

        public Objects(World world, ScreenManager screenManager, Vector2 startPosition, Vector2 endPosition, int count, float radius, ObjectType type, float toothHeight = 1f)
        {
            _batch = screenManager.SpriteBatch;
            _bodyRadius = radius;
            BodyList = new List<Body>(count);


            for (int i = 0; i < count; i++)
            {
                switch (type)
                {
                    case ObjectType.Circle:
                        BodyList.Add(world.CreateCircle(radius, 1f));
                        break;
                    case ObjectType.Rectangle:
                        BodyList.Add(world.CreateRectangle(radius, radius, 1f));
                        _bodyRadius = radius/2f;
                        break;
                    case ObjectType.Star:
                        BodyList.Add(world.CreateGear(radius, 10, 0f, toothHeight, 1f));
                        _bodyRadius = radius * 2.7f;
                        break;
                    case ObjectType.Gear:
                        BodyList.Add(world.CreateGear(radius, 10, 100f, toothHeight, 1f));
                        _bodyRadius = radius * 2.7f;
                        break;
                }
            }

            var collidesWith = Category.All;
            var collisionCategories = Category.All;

            for (int i = 0; i < BodyList.Count; i++)
            {
                Body body = BodyList[i];
                body.BodyType = BodyType.Dynamic;
                body.Position = Vector2.Lerp(startPosition, endPosition, i / (float)(count - 1));
                body.SetRestitution(0.7f);
                body.SetFriction(0.2f);
                body.SetCollisionCategories(collisionCategories);
                body.SetCollidesWith(collidesWith);
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
                    _object = new Sprite(creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 0f, toothHeight), MaterialType.Dots, Color.Yellow, 0.8f, 24f));
                    break;
                case ObjectType.Gear:
                    _object = new Sprite(creator.TextureFromVertices(PolygonTools.CreateGear(radius, 10, 100f, toothHeight), MaterialType.Dots, Color.DarkGreen, 0.8f, 24f));
                    break;
            }
        }

        public void SetBodyType(BodyType bodyType)
        {
                foreach (Body body in BodyList)
                    body.BodyType = bodyType;
        }

        public void SetCollisionCategories(Category collisionCategories)
        {
            foreach (Body body in BodyList)
                body.SetCollisionCategories(collisionCategories);
        }

        public void SetCollidesWith(Category collidesWith)
        {
            foreach (Body body in BodyList)
                body.SetCollidesWith(collidesWith);
        }
        
        public void Draw()
        {
            foreach (Body body in BodyList)
            {
                _batch.Draw(_object.Texture, body.Position, null, Color.White, body.Rotation, _object.Origin, new Vector2(2f * _bodyRadius) * _object.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
        }
    }
}