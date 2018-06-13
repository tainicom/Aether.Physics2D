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
        private SpriteFont _font;

        private Texture2D _playerTexture;
        private Texture2D _groundTexture;
        private Vector2 _playerTextureSize;
        private Vector2 _groundTextureSize;
        private Vector2 _playerTextureOrigin;
        private Vector2 _groundTextureOrigin;

        private KeyboardState _oldKeyState;
        private GamePadState _oldPadState;

        // Simple camera controls
        private Vector3 _cameraPosition = new Vector3(0, 1.70f, 0); // camera is 1.7 meters above the ground
        float cameraViewWidth = 12.5f; // camera is 12.5 meters wide.

        // physics
        private World _world;
        private Body _playerBody;
        private Body _groundBody;
        private float _playerBodyRadius = 1.5f / 2f; // player diameter is 1.5 meters
        private Vector2 _groundBodySize = new Vector2(8f, 1f); // ground is 8x1 meters


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

        }

        protected override void Initialize()
        {
            base.Initialize();
            
            //Create a world
            _world = new World();

            /* Circle */
            Vector2 playerPosition = new Vector2(0, _playerBodyRadius);

            // Create the player fixture
            _playerBody = _world.CreateCircle(_playerBodyRadius, 1f, playerPosition);
            _playerBody.BodyType = BodyType.Dynamic;

            // Give it some bounce and friction
            _playerBody.SetRestitution(0.3f);
            _playerBody.SetFriction(0.5f);


            /* Ground */
            Vector2 groundPosition = new Vector2(0, -(_groundBodySize.Y/2f));

            // Create the ground fixture
            _groundBody = _world.CreateRectangle(_groundBodySize.X, _groundBodySize.Y, 1f, groundPosition);
            _groundBody.BodyType = BodyType.Static;

            _groundBody.SetRestitution(0.3f);
            _groundBody.SetFriction(0.5f);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            // We use a BasicEffect to pass our view/projection in _spriteBatch
            _spriteBatchEffect = new BasicEffect(_graphics.GraphicsDevice);
            _spriteBatchEffect.TextureEnabled = true;
            
             _font = Content.Load<SpriteFont>("font");

            // Load sprites
            _playerTexture = Content.Load<Texture2D>("CircleSprite");
            _groundTexture = Content.Load<Texture2D>("GroundSprite");

            // We scale the ground and player textures at body dimensions
            _playerTextureSize = new Vector2(_playerTexture.Width, _playerTexture.Height);
            _groundTextureSize = new Vector2(_groundTexture.Width, _groundTexture.Height);

            // We draw the ground and player textures at the center of the shapes
            _playerTextureOrigin = _playerTextureSize / 2f;
            _groundTextureOrigin = _groundTextureSize / 2f;
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
            
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            //We update the world
            _world.Step((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }

        private void HandleGamePad(GameTime gameTime)
        {
            GamePadState padState = GamePad.GetState(0);
            float totalSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (padState.IsConnected)
            {
                if (padState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                if (padState.Buttons.A == ButtonState.Pressed && _oldPadState.Buttons.A == ButtonState.Released)
                    _playerBody.ApplyLinearImpulse(new Vector2(0, -10));

                _playerBody.ApplyForce(padState.ThumbSticks.Left);
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
                _cameraPosition.X -= totalSeconds * cameraViewWidth;

            if (state.IsKeyDown(Keys.Right))
                _cameraPosition.X += totalSeconds * cameraViewWidth;

            if (state.IsKeyDown(Keys.Up))
                _cameraPosition.Y += totalSeconds * cameraViewWidth;

            if (state.IsKeyDown(Keys.Down))
                _cameraPosition.Y -= totalSeconds * cameraViewWidth;


            // We make it possible to rotate the player body
            if (state.IsKeyDown(Keys.A))
                _playerBody.ApplyTorque(10);

            if (state.IsKeyDown(Keys.D))
                _playerBody.ApplyTorque(-10);

            if (state.IsKeyDown(Keys.Space) && _oldKeyState.IsKeyUp(Keys.Space))
                _playerBody.ApplyLinearImpulse(new Vector2(0, 10));

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
            
            // Update camera View and Projection.
            var vp = GraphicsDevice.Viewport;
            _spriteBatchEffect.View = Matrix.CreateLookAt(_cameraPosition, _cameraPosition + Vector3.Forward, Vector3.Up);
            _spriteBatchEffect.Projection = Matrix.CreateOrthographic(cameraViewWidth, cameraViewWidth / vp.AspectRatio, 0f, -1f);
            
            // Draw player and ground. 
            // Our View/Projection requires RasterizerState.CullClockwise and SpriteEffects.FlipVertically.
            _spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullClockwise, _spriteBatchEffect);
            _spriteBatch.Draw(_playerTexture, _playerBody.Position, null, Color.White, _playerBody.Rotation, _playerTextureOrigin, new Vector2(_playerBodyRadius * 2f) / _playerTextureSize, SpriteEffects.FlipVertically, 0f);
            _spriteBatch.Draw(_groundTexture, _groundBody.Position, null, Color.White, _groundBody.Rotation, _groundTextureOrigin, _groundBodySize / _groundTextureSize, SpriteEffects.FlipVertically, 0f);
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