﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class SimpleDemo8 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private Body[] _circle = new Body[6];
        private Sprite _circleSprite;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Restitution";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows several bodys with varying restitution.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = new Vector2(0f, -20f);

            _border = new Border(World, ScreenManager, Camera);

            Vector2 position = new Vector2(-15f, 8f);
            float restitution = 0f;

            for (int i = 0; i < 6; ++i)
            {
                _circle[i] = World.CreateCircle(1.5f, 1f, position);
                _circle[i].BodyType = BodyType.Dynamic;
                _circle[i].SetRestitution(restitution);
                position.X += 6f;
                restitution += 0.2f;
            }

            // create sprite based on body
            _circleSprite = new Sprite(ScreenManager.Assets.TextureFromShape(_circle[0].FixtureList[0].Shape, MaterialType.Waves, Color.Brown, 1f));
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.BatchEffect.View = Camera.View;
            ScreenManager.BatchEffect.Projection = Camera.Projection;
            ScreenManager.SpriteBatch.Begin(0, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            for (int i = 0; i < 6; ++i)
            {
                ScreenManager.SpriteBatch.Draw(_circleSprite.Texture, _circle[i].Position, null, Color.White, _circle[i].Rotation, _circleSprite.Origin, new Vector2(2f * 1.5f) * _circleSprite.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            ScreenManager.SpriteBatch.End();
            _border.Draw();
            base.Draw(gameTime);
        }
    }
}