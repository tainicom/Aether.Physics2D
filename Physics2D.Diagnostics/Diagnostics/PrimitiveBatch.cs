// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Diagnostics
{
    public class PrimitiveBatch : IPrimitiveBatch, IDisposable
    {
        private const int DefaultBufferSize = 500;

        public const float DefaultTriangleListDepth = -0.1f;
        public const float DefaultLineListDepth     =  0.0f;

        // a basic effect, which contains the shaders that we will use to draw our
        // primitives.
        private BasicEffect _basicEffect;

        // the device that we will issue draw calls to.
        private GraphicsDevice _device;

        // states
        BlendState _blendState;
        SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState;	

        // hasBegun is flipped to true once Begin is called, and is used to make
        // sure users don't call End before Begin is called.
        private bool _hasBegun;

        private bool _isDisposed;
        private VertexPositionColor[] _lineVertices;
        private int _lineVertsCount;
        private VertexPositionColor[] _triangleVertices;
        private int _triangleVertsCount;

        public PrimitiveBatch(GraphicsDevice graphicsDevice, int bufferSize = DefaultBufferSize)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            _device = graphicsDevice;

            _triangleVertices = new VertexPositionColor[bufferSize - bufferSize % 3];
            _lineVertices = new VertexPositionColor[bufferSize - bufferSize % 2];

            // set up a new basic effect, and enable vertex colors.
            _basicEffect = new BasicEffect(graphicsDevice);
            _basicEffect.VertexColorEnabled = true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        public void SetProjection(ref Matrix projection)
        {
            _basicEffect.Projection = projection;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !_isDisposed)
            {
                if (_basicEffect != null)
                    _basicEffect.Dispose();

                _isDisposed = true;
            }
        }


        /// <summary>
        /// Begin is called to tell the PrimitiveBatch what kind of primitives will be
        /// drawn, and to prepare the graphics card to render those primitives.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <param name="view">The view.</param>
        public void Begin(  ref Matrix projection, ref Matrix view, ref Matrix world,
                            BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, float alpha = 1.0f)
        {
            if (_hasBegun)
                throw new InvalidOperationException("End must be called before Begin can be called again.");

            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.Default;
            _rasterizerState = rasterizerState ?? RasterizerState.CullNone;

            //tell our basic effect to begin.
            _basicEffect.Alpha = alpha;
            _basicEffect.Projection = projection;
            _basicEffect.View = view;
            _basicEffect.World = world;
            _basicEffect.CurrentTechnique.Passes[0].Apply();

            // flip the error checking boolean. It's now ok to call AddVertex, Flush,
            // and End.
            _hasBegun = true;
        }

        public bool IsReady()
        {
            return _hasBegun;
        }

        public int AddVertex(Vector3 position, Color color, PrimitiveType primitiveType)
        {
            return AddVertex(ref position, color, primitiveType);
        }

        public int AddVertex(ref Vector3 position, Color color, PrimitiveType primitiveType)
        {
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");

            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:                    
                    if (_triangleVertsCount >= _triangleVertices.Length)
                        FlushTriangles();

                    _triangleVertices[_triangleVertsCount].Position = position;
                    _triangleVertices[_triangleVertsCount].Color = color;
                    return _triangleVertsCount++;

                case PrimitiveType.LineList:
                    if (_lineVertsCount >= _lineVertices.Length)
                        FlushLines();

                    _lineVertices[_lineVertsCount].Position = position;
                    _lineVertices[_lineVertsCount].Color = color;
                    return _lineVertsCount++;

                default:
                    throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }
        }


        public int AddVertex(Vector2 position, Color color, PrimitiveType primitiveType)
        {
            return AddVertex(ref position, color, primitiveType);
        }

        public int AddVertex(ref Vector2 position, Color color, PrimitiveType primitiveType)
        {
            if (!_hasBegun)
                throw new InvalidOperationException("Begin must be called before AddVertex can be called.");

            switch (primitiveType)
            {
                case PrimitiveType.TriangleList:
                    if (_triangleVertsCount >= _triangleVertices.Length)
                        FlushTriangles();

                    _triangleVertices[_triangleVertsCount].Position.X = position.X;
                    _triangleVertices[_triangleVertsCount].Position.Y = position.Y;
                    _triangleVertices[_triangleVertsCount].Position.Z = DefaultTriangleListDepth;
                    _triangleVertices[_triangleVertsCount].Color = color;
                    return _triangleVertsCount++;

                case PrimitiveType.LineList:
                    if (_lineVertsCount >= _lineVertices.Length)
                        FlushLines();

                    _lineVertices[_lineVertsCount].Position.X = position.X;
                    _lineVertices[_lineVertsCount].Position.Y = position.Y;
                    _lineVertices[_lineVertsCount].Position.Z = DefaultLineListDepth;
                    _lineVertices[_lineVertsCount].Color = color;
                    return _lineVertsCount++;

                default:
                    throw new NotSupportedException("The specified primitiveType is not supported by PrimitiveBatch.");
            }
        }

        /// <summary>
        /// End is called once all the primitives have been drawn using AddVertex.
        /// it will call Flush to actually submit the draw call to the graphics card, and
        /// then tell the basic effect to end.
        /// </summary>
        public void End()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before End can be called.");
            }

            // Draw whatever the user wanted us to draw
            FlushTriangles();
            FlushLines();

            _hasBegun = false;
        }

        private void FlushTriangles()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (_triangleVertsCount >= 3)
            {
                int primitiveCount = _triangleVertsCount / 3;
                // submit the draw call to the graphics card
                _device.BlendState = _blendState;
                _device.SamplerStates[0] = _samplerState;
                _device.DepthStencilState = _depthStencilState;
                _device.RasterizerState = _rasterizerState;
                _device.DrawUserPrimitives(PrimitiveType.TriangleList, _triangleVertices, 0, primitiveCount);
                // clear count
                _triangleVertsCount = 0;
            }
        }

        private void FlushLines()
        {
            if (!_hasBegun)
            {
                throw new InvalidOperationException("Begin must be called before Flush can be called.");
            }
            if (_lineVertsCount >= 2)
            {
                int primitiveCount = _lineVertsCount / 2;
                // submit the draw call to the graphics card
                _device.BlendState = _blendState;
                _device.SamplerStates[0]    = _samplerState;
                _device.DepthStencilState   = _depthStencilState;
                _device.RasterizerState     = _rasterizerState;
                _device.DrawUserPrimitives(PrimitiveType.LineList, _lineVertices, 0, primitiveCount);
                // clear count
                _lineVertsCount = 0;
            }
        }
    }
}