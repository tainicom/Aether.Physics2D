// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;

namespace tainicom.Aether.Physics2D.Samples.DrawingSystem
{
    public class ShadowMap2D
    {
        public GraphicsDevice GraphicsDevice { get; private set; }
        public Body body { get; private set; }

        public float LightRadius { get; private set; }
        public int ShadowMapSize { get; private set; }

        // for this demo we use BasicEffect+Fog to generate the shadowMap.
        public BasicEffect _shadowEffect;

        public RenderTarget2D ShadowMapU { get; private set; }
        public RenderTarget2D ShadowMapR { get; private set; }
        public RenderTarget2D ShadowMapD { get; private set; }
        public RenderTarget2D ShadowMapL { get; private set; }

        public ShadowMap2D(GraphicsDevice graphicsDevice, DebugView debugView, Body body, float lightRadius, int shadowMapSize = 128)
        {
            this.GraphicsDevice = graphicsDevice;
            this.body = body;

            this.LightRadius = lightRadius;
            this.ShadowMapSize = shadowMapSize;

            _shadowEffect = new BasicEffect(GraphicsDevice);
            _shadowEffect.VertexColorEnabled = true;
            _shadowEffect.FogEnabled = true;
            _shadowEffect.FogColor = Color.Black.ToVector3();

            ShadowMapU = new RenderTarget2D(GraphicsDevice, ShadowMapSize, 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            ShadowMapR = new RenderTarget2D(GraphicsDevice, ShadowMapSize, 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            ShadowMapD = new RenderTarget2D(GraphicsDevice, ShadowMapSize, 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
            ShadowMapL = new RenderTarget2D(GraphicsDevice, ShadowMapSize, 1, false, SurfaceFormat.Color, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
        }

        public void RenderShadowMap(ShadowBatch batch, bool flipNormal = false, float nearPlane = 0.001f)
        {
            var pos = new Vector3(body.Position, 0.0f);
            _shadowEffect.FogStart = 0.0f;
            _shadowEffect.FogEnd = LightRadius;

            var proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.Pi / 2f, 1f, nearPlane, LightRadius);
            var viewU = Matrix.CreateLookAt(pos, pos + Vector3.Up, Vector3.Backward);
            var viewR = Matrix.CreateLookAt(pos, pos + Vector3.Right, Vector3.Backward);
            var viewD = Matrix.CreateLookAt(pos, pos + Vector3.Down, Vector3.Backward);
            var viewL = Matrix.CreateLookAt(pos, pos + Vector3.Left, Vector3.Backward);

#if XNA 
            proj = proj * Matrix.CreateTranslation(0f, 1.0f, 0f); // one pixel up
#endif

            RenderShadowMap(batch, flipNormal, ShadowMapU, ref proj, ref viewU);
            RenderShadowMap(batch, flipNormal, ShadowMapR, ref proj, ref viewR);
            RenderShadowMap(batch, flipNormal, ShadowMapD, ref proj, ref viewD);
            RenderShadowMap(batch, flipNormal, ShadowMapL, ref proj, ref viewL);

            GraphicsDevice.SetRenderTarget(null);
        }

        protected void RenderShadowMap(ShadowBatch batch, bool flipNormal, RenderTarget2D shadowTarget, ref Matrix proj, ref Matrix view)
        {
            GraphicsDevice.SetRenderTarget(shadowTarget);
            GraphicsDevice.Clear(Color.White);
            GraphicsDevice.Clear(Color.Black);

            _shadowEffect.Projection = proj;
            _shadowEffect.View = view;
            _shadowEffect.CurrentTechnique.Passes[0].Apply(); // CurrentTechnique is "LightDrawing"

            if (batch.VerticesCount >= 3)
            {
                int primitiveCount = batch.VerticesCount / 3;
                // submit the draw call to the graphics card
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.RasterizerState = (flipNormal) ? RasterizerState.CullCounterClockwise : RasterizerState.CullClockwise;
                GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, batch.Vertices, 0, primitiveCount);
            }

            return;
        }

        public void SetLightEffect(Effect effect, Matrix cameraView, Matrix cameraProjection)
        {
            effect.Parameters["WorldViewProjection"].SetValue(Matrix.Identity * cameraView * cameraProjection);
            effect.Parameters["LightRadius"].SetValue(LightRadius);
            effect.Parameters["LightPosition"].SetValue(body.Position);
            effect.Parameters["ShadowMapTexelSize"].SetValue(1f / ShadowMapSize);
            effect.Parameters["ShadowMapU"].SetValue(ShadowMapU);
            effect.Parameters["ShadowMapR"].SetValue(ShadowMapR);
            effect.Parameters["ShadowMapD"].SetValue(ShadowMapD);
            effect.Parameters["ShadowMapL"].SetValue(ShadowMapL);
        }

    }
}
