/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class AdvancedDemo3 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private TheoJansenWalker _walker;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Theo Jansen's walker";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Switch walker direction: B button");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Switch walker direction: Space");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Switch walker direction: Right click");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            HasCursor = false;

            World.Gravity = new Vector2(0, -9.82f);

            _border = new Border(World, ScreenManager, Camera);
            _walker = new TheoJansenWalker(World, ScreenManager, Camera, Vector2.Zero);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewButtonPress(Buttons.B) || input.IsNewMouseButtonPress(MouseButtons.RightButton) || input.IsNewKeyPress(Keys.Space))
                _walker.Reverse();

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _walker.Draw(ScreenManager.BatchEffect, Camera);
            _border.Draw();
            base.Draw(gameTime);
        }
    }
}