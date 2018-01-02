/* Original source Farseer Physics Engine:
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
    internal class SimpleDemo7 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private Sprite _obstacle;
        private Body[] _obstacles = new Body[4];
        private Ragdoll _ragdoll;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Ragdoll";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to combine physics objects to create a ragdoll.");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate ragdoll: left and right triggers");
            sb.AppendLine("  - Move ragdoll: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate ragdoll: left and right arrows");
            sb.AppendLine("  - Move ragdoll: A,S,D,W");
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
            _ragdoll = new Ragdoll(World, ScreenManager, Vector2.Zero);

            LoadObstacles();
            SetUserAgent(_ragdoll.Body, 1000f, 400f);
        }

        private void LoadObstacles()
        {
            for (int i = 0; i < 4; i++)
            {
                _obstacles[i] = World.CreateRectangle(5f, 1.5f, 1f);
                _obstacles[i].BodyType = BodyType.Static;
            }

            _obstacles[0].Position = new Vector2(-9f, -5f);
            _obstacles[1].Position = new Vector2(-8f, 7f);
            _obstacles[2].Position = new Vector2(9f, -7f);
            _obstacles[3].Position = new Vector2(7f, 5f);

            // create sprite based on body
            _obstacle = new Sprite(ScreenManager.Assets.TextureFromShape(_obstacles[0].FixtureList[0].Shape, MaterialType.Dots, Color.SandyBrown, 0.8f));
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.BatchEffect.View = Camera.View;
            ScreenManager.BatchEffect.Projection = Camera.Projection;
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            for (int i = 0; i < 4; ++i)
            {
                ScreenManager.SpriteBatch.Draw(_obstacle.Texture, _obstacles[i].Position, null, Color.White, _obstacles[i].Rotation, _obstacle.Origin, new Vector2(5f, 1.5f) * _obstacle.TexelSize, SpriteEffects.FlipVertically, 0f);
            }
            _ragdoll.Draw();
            ScreenManager.SpriteBatch.End();
            _border.Draw();
            base.Draw(gameTime);
        }
    }
}