/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Maths;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Samples.DrawingSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class AdvancedDemo6 : PhysicsGameScreen, IDemoScreen
    {
        private Agent _agent;
        private Border _border;
        private Objects _staticCircles;
        private Objects _staticRectangles;
        private Objects _circles0;
        private Objects _circles1;
        private Objects _rectangles1;
        private Objects _rectangles0;
        private Objects _gears;
        private Objects _stars;

        private ShadowBatch _batch;
        public Effect _lightEffect;
        private ShadowMap2D _mainLight;
        private ShadowMap2D[] _starLights;
        private ShadowMap2D[] _gearLights;
        private Sprite _backgroundQuad;        

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Lights and Shadows";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to draw Lights and Shadows from the World geometry.");
            sb.AppendLine("In this demo:");
            sb.AppendLine("  - Static Circles and Boxes cast shadows and are self lit.");
            sb.AppendLine("  - Circles cast shadows and are self lit.");
            sb.AppendLine("  - Boxes cast shadows and receive light.");
            sb.AppendLine("  - Stars and Gears emit light and cast shadows.");
            sb.AppendLine("  - The agent emit light and cast shadows.");
            sb.AppendLine("  - The floor receive light.");

            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Rotate agent: left and right triggers");
            sb.AppendLine("  - Move agent: right thumbstick");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Rotate agent: left and right arrows");
            sb.AppendLine("  - Move agent: A,S,D,W");
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

            World.Gravity = Vector2.Zero;

            _border = new Border(World, ScreenManager, Camera);

            _agent = new Agent(World, ScreenManager, Vector2.Zero);
            _agent.Body.IsBullet = true;


            Vector2 startPosition = new Vector2(-20f, 11f);
            Vector2 endPosition = new Vector2(20, 11f);
            _circles0 = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.3f, ObjectType.Circle);
            
            startPosition = new Vector2(-20, 9f);
            endPosition = new Vector2(20, 9f);
            _staticRectangles = new Objects(World, ScreenManager, startPosition, endPosition, 15, 1.2f, ObjectType.Rectangle);
            _staticRectangles.SetBodyType(BodyType.Static);

            startPosition = new Vector2(-20f, 7f);
            endPosition = new Vector2(20, 7f);
            _circles1 = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.3f, ObjectType.Circle);
            
            startPosition = new Vector2(-20, -7f);
            endPosition = new Vector2(20, -7f);
            _rectangles1 = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.6f, ObjectType.Rectangle);

            startPosition = new Vector2(-20f, -9f);
            endPosition = new Vector2(20, -9f);
            _staticCircles = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.6f, ObjectType.Circle);
            _staticCircles.SetBodyType(BodyType.Static);
            
            startPosition = new Vector2(-20, -11f);
            endPosition = new Vector2(20, -11f);
            _rectangles0 = new Objects(World, ScreenManager, startPosition, endPosition, 15, 0.6f, ObjectType.Rectangle);
            
            startPosition = new Vector2(-22, -7);
            endPosition = new Vector2(-22, 7);
            _gears = new Objects(World, ScreenManager, startPosition, endPosition, 8, 0.15f, ObjectType.Gear, 0.15f);
            
            startPosition = new Vector2(22, -7);
            endPosition = new Vector2(22, 7);
            _stars = new Objects(World, ScreenManager, startPosition, endPosition, 8, 0.15f, ObjectType.Star, 0.15f);
            
            SetUserAgent(_agent.Body, 1000f, 400f);

            PolygonShape box = new PolygonShape(PolygonTools.CreateRectangle(53.33f / 2f, 30f / 2f), 1);
            _backgroundQuad = new Sprite(ScreenManager.Assets.TextureFromShape(box, MaterialType.Pavement, Color.LightGray, 1.0f));


            // shadow lighting            
            _lightEffect = ScreenManager.Content.Load<Effect>("Effects/LightEffect");

            _mainLight = new ShadowMap2D(ScreenManager.GraphicsDevice, this.DebugView, _agent.Body, 14f, 512);

            _starLights = new ShadowMap2D[_stars.BodyList.Count];
            for (int i = 0; i < _stars.BodyList.Count; i++)
                _starLights[i] = new ShadowMap2D(ScreenManager.GraphicsDevice, this.DebugView, _stars.BodyList[i], 3f, 64);

            _gearLights = new ShadowMap2D[_gears.BodyList.Count];
            for (int i = 0; i < _gears.BodyList.Count; i++)
                _gearLights[i] = new ShadowMap2D(ScreenManager.GraphicsDevice, this.DebugView, _gears.BodyList[i], 3f, 64);

            _batch = new ShadowBatch();

            return;
        }

        public override void PreDraw(GameTime gameTime)
        {
            // update geometry batch
            _batch.Clear();            
            foreach (Body b in _staticCircles.BodyList)
                _batch.AddBody(b);
            foreach (Body b in _staticRectangles.BodyList)
                _batch.AddBody(b);
            foreach (Body b in _circles0.BodyList)
                _batch.AddBody(b);
            foreach (Body b in _circles1.BodyList)
                _batch.AddBody(b);
            foreach (Body b in _rectangles1.BodyList)
                _batch.AddBody(b, true); // flip facing normal do prevent self shadow
            foreach (Body b in _rectangles0.BodyList)
                _batch.AddBody(b, true); // flip facing normal do prevent self shadow
            _batch.AddBody(_border.Anchor);            
            _batch.AddBody(_agent.Body);
            foreach (Body b in _gears.BodyList)
                _batch.AddBody(b);
            foreach (Body b in _stars.BodyList)
                _batch.AddBody(b);
            
            // draw shadow maps
            for (int i = 0; i < _starLights.Length; i++)
                _starLights[i].RenderShadowMap(_batch, false, 0.16f);
            for (int i = 0; i < _gearLights.Length; i++)
                _gearLights[i].RenderShadowMap(_batch, false, 0.31f);
            _mainLight.RenderShadowMap(_batch, false, 0.4f);
        }
        
        public override void Draw(GameTime gameTime)
        {
            ScreenManager.BatchEffect.View = Camera.View;
            ScreenManager.BatchEffect.Projection = Camera.Projection;

            ScreenManager.GraphicsDevice.Clear(Color.Black); //black background
            

            // Draw fade background
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            ScreenManager.SpriteBatch.Draw(_backgroundQuad.Texture, Vector2.Zero, null, Color.White * 0.2f, 0f, _backgroundQuad.Origin, new Vector2(53.33f, 30f) * _backgroundQuad.TexelSize, SpriteEffects.FlipVertically, 0f);
            ScreenManager.SpriteBatch.End();


            // Draw Lights
            // This draws each light with each body. To improve performance you could
            // render a light map in PreDraw() and use iy here with DualTextureEffect.
            _mainLight.SetLightEffect(_lightEffect, Camera.View, Camera.Projection);
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, _lightEffect);
            ScreenManager.SpriteBatch.Draw(_backgroundQuad.Texture, Vector2.Zero, null, Color.Red, 0f, _backgroundQuad.Origin, new Vector2(53.33f, 30f) * _backgroundQuad.TexelSize, SpriteEffects.FlipVertically, 0f);
            _rectangles1.Draw();
            _rectangles0.Draw();
            ScreenManager.SpriteBatch.End();

            for (int i = 0; i < _starLights.Length; i++)
            {
                _starLights[i].SetLightEffect(_lightEffect, Camera.View, Camera.Projection);
                ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, RasterizerState.CullNone, _lightEffect);
                ScreenManager.SpriteBatch.Draw(_backgroundQuad.Texture, Vector2.Zero, null, Color.DarkGoldenrod, 0f, _backgroundQuad.Origin, new Vector2(53.33f, 30f) * _backgroundQuad.TexelSize, SpriteEffects.FlipVertically, 0f);
                _rectangles1.Draw();
                _rectangles0.Draw();
                ScreenManager.SpriteBatch.End();                
            }

            for (int i = 0; i < _gearLights.Length; i++)
            {
                _gearLights[i].SetLightEffect(_lightEffect, Camera.View, Camera.Projection);
                ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, RasterizerState.CullNone, _lightEffect);
                ScreenManager.SpriteBatch.Draw(_backgroundQuad.Texture, Vector2.Zero, null, Color.LightGreen, 0f, _backgroundQuad.Origin, new Vector2(53.33f, 30f) * _backgroundQuad.TexelSize, SpriteEffects.FlipVertically, 0f);
                _rectangles1.Draw();
                _rectangles0.Draw();
                ScreenManager.SpriteBatch.End();
            }
            
            ScreenManager.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, RasterizerState.CullNone, ScreenManager.BatchEffect);
            _agent.Draw();
            _circles0.Draw();
            _circles1.Draw();
            _staticCircles.Draw(); // static circles
            _staticRectangles.Draw(); // static rectangles
            _stars.Draw();
            _gears.Draw();
            ScreenManager.SpriteBatch.End();

            _border.Draw();

            // Draw Shadow Maps of _mainLight.
            ScreenManager.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null);
            ScreenManager.SpriteBatch.Draw(_mainLight.ShadowMapU, new Rectangle(20, 20, 256, 8), Color.White);
            ScreenManager.SpriteBatch.Draw(_mainLight.ShadowMapR, new Rectangle(20, 30, 256, 8), Color.White);
            ScreenManager.SpriteBatch.Draw(_mainLight.ShadowMapD, new Rectangle(20, 40, 256, 8), Color.White);
            ScreenManager.SpriteBatch.Draw(_mainLight.ShadowMapL, new Rectangle(20, 50, 256, 8), Color.White);
            //ScreenManager.SpriteBatch.Draw(_mainLight.ShadowMapU, new Rectangle(20, 50, 256, 128), Color.White);
            ScreenManager.SpriteBatch.End();

            base.Draw(gameTime);
        }

    }
}