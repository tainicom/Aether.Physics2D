/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.ScreenSystem
{
    public class LogoScreen : GameScreen
    {
        private TimeSpan _duration;
        private Texture2D _logoTexture;
        private Vector2 _logoPosition;

        public LogoScreen(TimeSpan duration)
        {
            _duration = duration;
            HasCursor = false;
            TransitionOffTime = TimeSpan.FromSeconds(0.6);
        }

        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            _logoTexture = this.Framework.Content.Load<Texture2D>("Common/logo");

            Viewport viewport = Framework.GraphicsDevice.Viewport;

            _logoPosition = new Vector2((viewport.Width - _logoTexture.Width) / 2f - 100f, (viewport.Height - _logoTexture.Height) / 2f);
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsMenuSelect() || input.IsMenuCancel())
            {
                _duration = TimeSpan.Zero;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            _duration -= gameTime.ElapsedGameTime;
            if (_duration <= TimeSpan.Zero)
            {
                ExitScreen();
            }

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(GameTime gameTime)
        {
            Framework.GraphicsDevice.Clear(Color.White);

            SpriteBatch.Begin();
            SpriteBatch.Draw(_logoTexture, _logoPosition, Color.White);
            SpriteBatch.End();
        }
    }
}