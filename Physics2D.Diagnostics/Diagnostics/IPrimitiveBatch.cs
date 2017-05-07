// Copyright (c) 2017 Kastellanos Nikolaos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Diagnostics
{
    public interface IPrimitiveBatch
    {        
        void Begin(ref Matrix projection, ref Matrix view, ref Matrix world, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, float alpha);

        void End();
        bool IsReady();

        void AddVertex(Vector2 vector2, Color color, PrimitiveType primitiveType);
    }
}
