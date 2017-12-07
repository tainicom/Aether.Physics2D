// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private BasicEffect _spriteBatchEffect;
        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;
        private SpriteFont _font;

        private World _world;

        private Body _circleBody;
        private Body _groundBody;
        private const float _circleBodyRadius = 0.75f;
        private Vector2 _groundBodySize = new Vector2(8f, 1f);


        private Texture2D _circleSprite;
        private Texture2D _groundSprite;
        private Vector2 _circleSpriteSize;
        private Vector2 _groundSpriteSize;
        private Vector2 _circleSpriteOrigin;
        private Vector2 _groundSpriteOrigin;

        // Simple camera controls
        private Matrix _projection;
        private Matrix _view;
        private Vector3 _cameraPosition;



#if !JOYSTICK
        const string Text = "Press A or D to rotate the ball\n" +
                            "Press Space to jump\n" +
                            "Use arrow keys to move the camera";
#else
                const string Text = "Use left stick to move\n" +
                                    "Press A to jump\n" +
                                    "Use right stick to move camera\n";
#endif

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.GraphicsProfile = GraphicsProfile.Reach;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 480;

            Content.RootDirectory = "Content";

            //Create a world
            _world = new World();
        }

        protected override void LoadContent()
        {
            // Initialize camera controls 
            _cameraPosition = Vector3.Zero;

            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _spriteBatchEffect = new BasicEffect(_graphics.GraphicsDevice);
            _spriteBatchEffect.TextureEnabled = true;
            
             _font = Content.Load<SpriteFont>("font");

            // Load sprites
            _circleSprite = Content.Load<Texture2D>("CircleSprite"); //  96px x 96px => 1.5m x 1.5m
            _groundSprite = Content.Load<Texture2D>("GroundSprite"); // 512px x 64px =>   8m x 1m
                        
            _groundSpriteSize = new Vector2(_groundSprite.Width, _groundSprite.Height);
            _circleSpriteSize = new Vector2(_circleSprite.Width, _circleSprite.Height);

            /* We need XNA to draw the ground and circle at the center of the shapes */
            _groundSpriteOrigin = _groundSpriteSize / 2f;
            _circleSpriteOrigin = _circleSpriteSize / 2f;
                        
            /* Circle */
            Vector2 circlePosition = new Vector2(0, 1.5f);

            // Create the circle fixture
            _circleBody = _world.CreateCircle(_circleBodyRadius, 1f, circlePosition, BodyType.Dynamic);

            // Give it some bounce and friction
            _circleBody.SetRestitution(0.3f);
            _circleBody.SetFriction(0.5f);

            /* Ground */
            Vector2 groundPosition = new Vector2(0, -1.25f);

            // Create the ground fixture
            _groundBody = _world.CreateRectangle(_groundBodySize.X, _groundBodySize.Y, 1f, groundPosition);
            _groundBody.IsStatic = true;
            _groundBody.SetRestitution(0.3f);
            _groundBody.SetFriction(0.5f);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            HandleGamePad(gameTime);
            HandleKeyboard(gameTime);

            // update camera View Projection
            var vp = GraphicsDevice.Viewport;
            var cameraZoomFactor = 12.5f / vp.Width; // zoom out to about ~12.5 meters wide
            _projection = Matrix.CreateOrthographic(vp.Width * cameraZoomFactor, vp.Height * cameraZoomFactor, 0f, -1f);
            _view = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + Vector3.Forward, Vector3.Up);

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalMilliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void HandleGamePad(GameTime gameTime)
        {
            GamePadState padState = GamePad.GetState(0);

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                    _circleBody.ApplyLinearImpulse(new Vector2(0, -10));

                _circleBody.ApplyForce(padState.ThumbSticks.Left);
                _cameraPosition.X -= padState.ThumbSticks.Right.X;
                _cameraPosition.Y += padState.ThumbSticks.Right.Y;


                _oldPadState = padState;
            }
        }

        private void HandleKeyboard(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            var vp = GraphicsDevice.Viewport;

            // Move camera
            if (state.IsKeyDown(Keys.Left))
                _cameraPosition.X -= totalSeconds * (vp.Width * 0.01f);

            if (state.IsKeyDown(Keys.Right))
                _cameraPosition.X += totalSeconds * (vp.Width * 0.01f);

            if (state.IsKeyDown(Keys.Up))
                _cameraPosition.Y += totalSeconds * (vp.Width * 0.01f);

            if (state.IsKeyDown(Keys.Down))
                _cameraPosition.Y -= totalSeconds * (vp.Width * 0.01f);


            // We make it possible to rotate the circle body
            if (state.IsKeyDown(Keys.A))
                _circleBody.ApplyTorque(10);

            if (state.IsKeyDown(Keys.D))
                _circleBody.ApplyTorque(-10);

            if (state.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
                _circleBody.ApplyLinearImpulse(new Vector2(0, 10));

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            _oldKeyState = state;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //Draw circle and ground
            _spriteBatchEffect.Projection = _projection;
            _spriteBatchEffect.View = _view;
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullClockwise, _spriteBatchEffect);
            _spriteBatch.Draw(_circleSprite, _circleBody.Position, null, Color.White, _circleBody.Rotation, _circleSpriteOrigin, new Vector2(_circleBodyRadius * 2f) / _circleSpriteSize, SpriteEffects.FlipVertically, 0f);
            _spriteBatch.Draw(_groundSprite, _groundBody.Position, null, Color.White, 0f, _groundSpriteOrigin, _groundBodySize / _groundSpriteSize, SpriteEffects.FlipVertically, 0f);
            _spriteBatch.End();

            // Display instructions
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, Text, new Vector2(14f, 14f), Color.Black);
            _spriteBatch.DrawString(_font, Text, new Vector2(12f, 12f), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}