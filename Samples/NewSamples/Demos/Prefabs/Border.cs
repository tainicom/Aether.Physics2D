/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos.Prefabs
{
    public class Border
    {
        private Body _anchor;

        private BasicEffect _basicEffect;
        private VertexPositionColorTexture[] _borderVertices;
        private short[] _indexBuffer;

        private GraphicsDevice _graphics;
        private LineBatch _lines;

        public Border(World world, LineBatch lines, GraphicsDevice graphics)
        {
            _graphics = graphics;
            _lines = lines;

            // Physics
            var vp = graphics.Viewport;
            float height = 30f; // 30 meters height
            float width = height * vp.AspectRatio;
            width -= 1.5f; // 1.5 meters border
            height -= 1.5f;
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            Vertices borders = new Vertices(4);
            borders.Add(new Vector2(-halfWidth, halfHeight));  // Lower left
            borders.Add(new Vector2(halfWidth, halfHeight));   // Lower right
            borders.Add(new Vector2(halfWidth, -halfHeight));  // Upper right
            borders.Add(new Vector2(-halfWidth, -halfHeight)); // Upper left

            _anchor = world.CreateLoopShape(borders);
            _anchor.SetCollisionCategories(Category.All);
            _anchor.SetCollidesWith(Category.All);

            // GFX
            _basicEffect = new BasicEffect(graphics);
            _basicEffect.VertexColorEnabled = true;
            _basicEffect.TextureEnabled = true;

            _borderVertices = new[] {
                new VertexPositionColorTexture(new Vector3(-halfWidth, -halfHeight, 0f),Color.White, new Vector2(-halfWidth, -halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth, -halfHeight, 0f),Color.White, new Vector2(halfWidth, -halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth, halfHeight, 0f), Color.White,new Vector2(halfWidth, halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth, halfHeight, 0f),Color.White, new Vector2(-halfWidth, halfHeight) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth - 2f, -halfHeight - 2f, 0f),Color.White, new Vector2(-halfWidth - 2f, -halfHeight - 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth + 2f, -halfHeight - 2f, 0f),Color.White, new Vector2(halfWidth + 2f, -halfHeight - 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(halfWidth + 2f, halfHeight + 2f, 0f), Color.White,new Vector2(halfWidth + 2f, halfHeight + 2f) / 5.25f),
                new VertexPositionColorTexture(new Vector3(-halfWidth - 2f, halfHeight + 2f, 0f), Color.White,new Vector2(-halfWidth - 2f, halfHeight + 2f) / 5.25f)
           };

            _indexBuffer = new short[] { 0, 5, 4, 0, 1, 5, 1, 6, 5, 1, 2, 6, 2, 7, 6, 2, 3, 7, 3, 4, 7, 3, 0, 4 };
        }

        public void Draw(Matrix projection, Matrix view)
        {
            _graphics.SamplerStates[0] = SamplerState.AnisotropicWrap;
            _graphics.RasterizerState = RasterizerState.CullNone;

            _basicEffect.Projection = projection;
            _basicEffect.View = view;
            _basicEffect.Texture = ContentWrapper.GetTexture("Blank");
            _basicEffect.DiffuseColor = ContentWrapper.Black.ToVector3();
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _borderVertices, 0, 8, _indexBuffer, 0, 8);

            _basicEffect.Texture = ContentWrapper.GetTexture("Stripe");
            _basicEffect.DiffuseColor = ContentWrapper.Grey.ToVector3();
            _basicEffect.CurrentTechnique.Passes[0].Apply();
            _graphics.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _borderVertices, 0, 8, _indexBuffer, 0, 8);

            _lines.Begin(projection, view);
            _lines.DrawLineShape(_anchor.FixtureList[0].Shape);
            _lines.End();
        }
    }
}