/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D13_TheoJansenWalker : PhysicsDemoScreen
    {
        private Border _border;
        private TheoJansenWalker _walker;
        private Body[] _circles;

        private Sprite _grain;

        #region Demo description
        public override string GetTitle()
        {
            return "Theo Jansen's Strandbeast";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how complex mechanical structures can be realized.");
            sb.AppendLine("http://www.strandbeest.com/");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Switch walker direction: A button");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Switch walker direction: Space");
            sb.AppendLine("  - Exit to demo selection: Escape");
            sb.AppendLine();
            sb.AppendLine("Mouse / Touchscreen");
            sb.Append("  - Switch walker direction: Right click");
#endif
            return sb.ToString();
        }
        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            HasCursor = false;

            World.Gravity = new Vector2(0, -9.82f);

            _border = new Border(World, LineBatch, Framework.GraphicsDevice);

            CircleShape shape = new CircleShape(0.25f, 1);
            _grain = new Sprite(ContentWrapper.CircleTexture(0.25f, ContentWrapper.Gold, ContentWrapper.Grey, 24f));

            _circles = new Body[48];
            for (int i = 0; i < _circles.Length; i++)
            {
                _circles[i] = World.CreateBody();
                _circles[i].BodyType = BodyType.Dynamic;
                _circles[i].Position = new Vector2(-24f + 1f * i, -10f);
                _circles[i].CreateFixture(shape);
            }

            _walker = new TheoJansenWalker(World, Vector2.Zero);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewButtonPress(Buttons.A) || input.IsNewMouseButtonPress(MouseButtons.RightButton) || input.IsNewKeyPress(Keys.Space))
                _walker.Reverse();

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            BatchEffect.View = Camera.View;
            BatchEffect.Projection = Camera.Projection;

            SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, BatchEffect);
            for (int i = 0; i < _circles.Length; i++)
            {   
                SpriteBatch.Draw(_grain.Texture, _circles[i].Position, null, Color.White, _circles[i].Rotation, _grain.Origin, new Vector2(2f * 0.25f) * _grain.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            SpriteBatch.End();
                        
            _walker.Draw(SpriteBatch, BatchEffect, LineBatch, Camera);
            _border.Draw(Camera.Projection, Camera.View);

            base.Draw(gameTime);
        }
    }
}