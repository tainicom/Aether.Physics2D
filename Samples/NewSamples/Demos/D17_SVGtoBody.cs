/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Content;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Collision.Shapes;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D17_SVGtoBody : PhysicsDemoScreen
    {
        #region Demo description
        public override string GetTitle()
        {
            return "SVG Importer to bodies";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to load bodies from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
#endif
            return sb.ToString();
        }
        #endregion

        private BodyContainer _BodyContainer;
        private Border _border;
        private Body _heartBody;
        private Body _clubBody;
        private Body _spadeBody;
        private Body _diamondBody;
        private Sprite _heart;
        private Sprite _club;
        private Sprite _spade;
        private Sprite _diamond;
        private bool _flipped = false;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -10f);
            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            _BodyContainer = Framework.Content.Load<BodyContainer>("Pipeline/Body");
            
            _heart = new Sprite(ContentWrapper.GetTexture("Heart"));
            _club = new Sprite(ContentWrapper.GetTexture("Club"));
            _spade = new Sprite(ContentWrapper.GetTexture("Spade"));
            _diamond = new Sprite(ContentWrapper.GetTexture("Diamond"));

            if (!_flipped)
            {
                foreach (var b in _BodyContainer.Values)
                {
                    foreach (var f in b.Fixtures)
                    {
                        var shape = (PolygonShape)f.Shape;
                        shape.Vertices.Scale(new Vector2(1f, -1f)); // flip Vertices
                    }
                }
                
                _flipped = true;
            }
            
            _heartBody = _BodyContainer["Heart"].Create(World);
            _clubBody = _BodyContainer["Club"].Create(World);
            _spadeBody = _BodyContainer["Spade"].Create(World);
            _diamondBody = _BodyContainer["Diamond"].Create(World);
            
            _heart.Origin = ContentWrapper.CalculateOrigin(_heartBody, 24f);
            _club.Origin = ContentWrapper.CalculateOrigin(_clubBody, 24f);
            _spade.Origin = ContentWrapper.CalculateOrigin(_spadeBody, 24f);
            _diamond.Origin = ContentWrapper.CalculateOrigin(_diamondBody, 24f);
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;
            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            SpriteBatch.Draw(_heart.Texture, _heartBody.Position, null, Color.White, _heartBody.Rotation, _heart.Origin, new Vector2(5.3f) * _heart.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.Draw(_club.Texture, _clubBody.Position, null, Color.White, _clubBody.Rotation, _club.Origin, new Vector2(5.3f) * _club.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.Draw(_spade.Texture, _spadeBody.Position, null, Color.White, _spadeBody.Rotation, _spade.Origin, new Vector2(5.3f) * _spade.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.Draw(_diamond.Texture, _diamondBody.Position, null, Color.White, _diamondBody.Rotation, _diamond.Origin, new Vector2(5.3f) * _diamond.TexelSize, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.End();

            _border.Draw(Camera.Projection, Camera.View);
            base.Draw(gameTime);
        }
    }
}