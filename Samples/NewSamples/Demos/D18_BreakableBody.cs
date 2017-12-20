/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.PhysicsLogic;
using tainicom.Aether.Physics2D.Content;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D18_BreakableBody : PhysicsDemoScreen
    {
        private Border _border;
        private List<Sprite> _breakableSprite;
        private Sprite _completeSprite;
        private BreakableBody[] _breakableCookie = new BreakableBody[3];
        private static bool _isLoaded = false;

        #region Demo description
        public override string GetTitle()
        {
            return "Breakable body and explosions";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows a breakable cookie, imported from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explosion (at cursor): B button");
            sb.AppendLine("  - Move cursor: Left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: Left thumbstick");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse");
            sb.AppendLine("  - Explosion (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.Append("  - Drag grabbed object: Move mouse");
#endif
            return sb.ToString();
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            BodyContainer bodyContainer = Framework.Content.Load<BodyContainer>("Pipeline/BreakableBody");
            BodyTemplate bodyTemplate = bodyContainer["Cookie"];

            if (!_isLoaded) //we flip vertices once.
            {
                foreach (FixtureTemplate f in bodyTemplate.Fixtures)
                {
                    var shape = (PolygonShape)f.Shape;
                    shape.Vertices.Scale(new Vector2(1f, -1f)); // flip Vert
                    shape.Vertices.Translate(new Vector2(-5.33f, 5.33f));
                }
                _isLoaded = true;
            }
            
            for (int i = 0; i < 3; i++)
            {
                List<Shape> shapes = new List<Shape>();
                foreach (FixtureTemplate f in bodyTemplate.Fixtures)
                {   
                    shapes.Add(f.Shape);
                }
                _breakableCookie[i] = new tainicom.Aether.Physics2D.Common.PhysicsLogic.BreakableBody(World, shapes);

                _breakableCookie[i].Strength = 120f;
                _breakableCookie[i].MainBody.Position = new Vector2(-15.0f + 15f * i, 0f);
            }

            _breakableSprite = new List<Sprite>();
            List<Texture2D> textures = ContentWrapper.BreakableTextureFragments(_breakableCookie[0], "Cookie", 24f);
            for (int i = 0; i < _breakableCookie[0].Parts.Count; i++)
            {
                AABB bounds;
                Transform transform;
                _breakableCookie[0].Parts[i].Body.GetTransform(out transform);
                _breakableCookie[0].Parts[i].Shape.ComputeAABB(out bounds, ref transform, 0);
                Vector2 origin = 24f * (_breakableCookie[0].Parts[i].Body.Position - bounds.LowerBound);
                _breakableSprite.Add(new Sprite(textures[i], origin));
            }
            _completeSprite = new Sprite(ContentWrapper.GetTexture("Cookie"));
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            for (int i = 0; i < 3; i++)
            {
                if (_breakableCookie[i].State == BreakableBody.BreakableBodyState.ShouldBreak)
                {
                    // save MouseJoint position
                    Vector2? worldAnchor = null;
                    for (JointEdge je = _breakableCookie[i].MainBody.JointList; je != null; je = je.Next)
                    {
                        if (je.Joint == _fixedMouseJoint)
                        {
                            worldAnchor = _fixedMouseJoint.WorldAnchorA;
                            break;
                        }
                    }

                    // break body
                    _breakableCookie[i].Update();

                    // restore MouseJoint
                    if (worldAnchor != null && _fixedMouseJoint == null)
                    {
                        var ficture = World.TestPoint(worldAnchor.Value);
                        if (ficture != null)
                        {
                            _fixedMouseJoint = new FixedMouseJoint(ficture.Body, worldAnchor.Value);
                            _fixedMouseJoint.MaxForce = 1000.0f * ficture.Body.Mass;
                            World.Add(_fixedMouseJoint);
                        }
                    }
                }
                else
                {
                    _breakableCookie[i].Update();
                }
            }
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
                input.IsNewButtonPress(Buttons.B))
            {
                Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

                Vector2 min = cursorPos - new Vector2(10, 10);
                Vector2 max = cursorPos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                                {
                                    Vector2 fv = fixture.Body.Position - cursorPos;
                                    fv.Normalize();
                                    fv *= 80;
                                    fixture.Body.ApplyLinearImpulse(ref fv);
                                    return true;
                                }, ref aabb);
            }

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            for (int i = 0; i < 3; i++)
            {
                if (_breakableCookie[i].State == BreakableBody.BreakableBodyState.Broken)
                {
                    for (int j = 0; j < _breakableCookie[i].Parts.Count; j++)
                    {
                        var part = _breakableCookie[i].Parts[j];
                        var shape = (PolygonShape)part.Shape;
                        var aabb = shape.Vertices.GetAABB();
                        var box = new Vector2(aabb.Width, aabb.Height);
                        SpriteBatch.Draw(_breakableSprite[j].Texture, part.Body.Position, null, Color.White, part.Body.Rotation, _breakableSprite[j].Origin, box * _breakableSprite[j].TexelSize, SpriteEffects.FlipVertically, 0f);
                    }
                }
                else
                {
                    SpriteBatch.Draw(_completeSprite.Texture, _breakableCookie[i].MainBody.Position, null, Color.White, _breakableCookie[i].MainBody.Rotation, _completeSprite.Origin, new Vector2(10.66f) * _completeSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
                }
            }
            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}