/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Text;
using tainicom.Aether.Physics2D.Content;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class D16_SVGtoPolygon : PhysicsDemoScreen
    {
        #region Demo description
        public override string GetTitle()
        {
            return "SVG Importer to polygons";
        }

        public override string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("This demo shows how to load vertices from a SVG.");
            sb.AppendLine();
            sb.AppendLine("GamePad:");
            sb.Append("  - Exit to demo selection: Back button");
#if WINDOWS
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to demo selection: Escape");
#endif
            return sb.ToString();
        }
        #endregion

        private PolygonContainer _farseerPoly;

        public override void LoadContent()
        {
            base.LoadContent();

            World.Gravity = Vector2.Zero;

            if (_farseerPoly == null) // flip once
            {
                _farseerPoly = Framework.Content.Load<PolygonContainer>("Pipeline/Polygon");

                foreach (Polygon p in _farseerPoly.Values)
                    p.Vertices.Scale(new Vector2(1f, -1f)); // flip Vertices
            }

        }

        public override void Draw(GameTime gameTime)
        {
            DebugView.BeginCustomDraw(Camera.Projection, Camera.View);
            foreach (Polygon p in _farseerPoly.Values)
            {
                DebugView.DrawPolygon(p.Vertices.ToArray(), p.Vertices.Count, Color.Black, p.Closed);
            }
            DebugView.EndCustomDraw();

            base.Draw(gameTime);
        }
    }
}