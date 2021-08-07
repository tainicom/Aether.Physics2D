//   Copyright 2021 Kastellanos Nikolaos


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    internal class ControlPanel : DrawableGameComponent
    {
        private SpriteBatch _sb;
        private SpriteFont _font;

        protected readonly Game1 GameInstance;

        public ControlPanel(Game1 game) : base(game)
        {
            GameInstance = game;
        }

        public override void Initialize()
        {
            _sb = new SpriteBatch(GraphicsDevice);
            _font = GameInstance.Content.Load<SpriteFont>("DiagnosticsFont");
        }

        public override void Draw(GameTime gameTime)
        {
            KeyboardState ks = Keyboard.GetState();

            Color color = Color.White;
            Viewport vp = GraphicsDevice.Viewport;
            Vector2 pos = new Vector2(vp.Width - 180,20);

            _sb.Begin(SpriteSortMode.Deferred);
            
            _sb.DrawString(_font, "(F1) - Help", pos, color);
            pos.Y += _font.LineSpacing;
            pos.Y += _font.LineSpacing;

            if (ks.IsKeyDown(Keys.F1))
            {
                _sb.DrawString(_font, "(R) - Reset", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(I) - Prev test", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(O) - Next test", pos, color);
                pos.Y += _font.LineSpacing;

                _sb.DrawString(_font, "(Z/-) - Zoom out", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(Z/+) - Zoom in", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "MouseWheel - Zoom in/out", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "Arrow keys - Pan L/R/U/D", pos, color);
                pos.Y += _font.LineSpacing;

                pos.Y += _font.LineSpacing;

                _sb.DrawString(_font, "- Debug Draw -", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F2) - DebugPanel", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F3) - PerformanceGraph", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F4) - Shape", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F5) - AABB", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F6) - CenterOfMass", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F7) - Joint", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F8) - Contacts", pos, color);
                pos.Y += _font.LineSpacing;
                _sb.DrawString(_font, "(F9) - PolygonPoints", pos, color);
                pos.Y += _font.LineSpacing;
            }

            _sb.End();
        }
    }
}
