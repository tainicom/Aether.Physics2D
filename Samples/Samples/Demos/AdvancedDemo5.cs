/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Common.Decomposition;
using tainicom.Aether.Physics2D.Common.PhysicsLogic;
using tainicom.Aether.Physics2D.Common.PolygonManipulation;
using tainicom.Aether.Physics2D.Diagnostics;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Demos.Prefabs;
using tainicom.Aether.Physics2D.Samples.ScreenSystem;

namespace tainicom.Aether.Physics2D.Samples.Demos
{
    internal class AdvancedDemo5 : PhysicsGameScreen, IDemoScreen
    {
        private Border _border;
        private List<BreakableBody> _breakableBodies;

        #region IDemoScreen Members

        public string GetTitle()
        {
            return "Breakable bodies and explosions";
        }

        public string GetDetails()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TODO: Add sample description!");
            sb.AppendLine(string.Empty);
            sb.AppendLine("GamePad:");
            sb.AppendLine("  - Explode (at cursor): B button");
            sb.AppendLine("  - Move cursor: left thumbstick");
            sb.AppendLine("  - Grab object (beneath cursor): A button");
            sb.AppendLine("  - Drag grabbed object: left thumbstick");
            sb.AppendLine("  - Exit to menu: Back button");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Keyboard:");
            sb.AppendLine("  - Exit to menu: Escape");
            sb.AppendLine(string.Empty);
            sb.AppendLine("Mouse / Touchscreen");
            sb.AppendLine("  - Explode (at cursor): Right click");
            sb.AppendLine("  - Grab object (beneath cursor): Left click");
            sb.AppendLine("  - Drag grabbed object: move mouse / finger");
            return sb.ToString();
        }

        #endregion

        public override void LoadContent()
        {
            base.LoadContent();

            DebugView.AppendFlags(DebugViewFlags.Shape);

            World.Gravity = Vector2.Zero;

            _border = new Border(World, ScreenManager, Camera);
            _breakableBodies = new List<BreakableBody>();

            Texture2D alphabet = ScreenManager.Content.Load<Texture2D>("Samples/alphabet");

            uint[] data = new uint[alphabet.Width * alphabet.Height];
            alphabet.GetData(data);

            List<Vertices> list = PolygonTools.CreatePolygon(data, alphabet.Width, 3.5f, 20, true, true);
            
            for (int i = 0; i < list.Count; i++)
                list[i].Scale(new Vector2(1f,-1f)); // flip Vert

            float yOffset = -5f;
            float xOffset = -14f;
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 9)
                {
                    yOffset = 0f;
                    xOffset = -14f;
                }
                if (i == 18)
                {
                    yOffset = 5f;
                    xOffset = -12.25f;
                }
                Vertices polygon = list[i];
                Vector2 centroid = -polygon.GetCentroid();
                polygon.Translate(ref centroid);
                polygon = SimplifyTools.CollinearSimplify(polygon);
                polygon = SimplifyTools.ReduceByDistance(polygon, 4);
                List<Vertices> triangulated = Triangulate.ConvexPartition(polygon, TriangulationAlgorithm.Bayazit);

                Vector2 vertScale = new Vector2(13.916667f, 23.25f) / new Vector2(alphabet.Width, alphabet.Height);
                foreach (Vertices vertices in triangulated)
                    vertices.Scale(ref vertScale);

                var breakableBody = new BreakableBody(World, triangulated, 1);
                breakableBody.MainBody.Position = new Vector2(xOffset, yOffset);
                breakableBody.Strength = 100;
                _breakableBodies.Add(breakableBody);

                xOffset += 3.5f;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            foreach (var breakableBody in _breakableBodies)
            {
                if (breakableBody.State == BreakableBody.BreakableBodyState.ShouldBreak)
                {
                    // save MouseJoint position
                    Vector2? worldAnchor = null;
                    for (JointEdge je = breakableBody.MainBody.JointList; je != null; je = je.Next)
                    {
                        if (je.Joint == _fixedMouseJoint)
                        {
                            worldAnchor = _fixedMouseJoint.WorldAnchorA;
                            break;
                        }
                    }

                    // break body
                    breakableBody.Update();

                    // restore MouseJoint
                    if (worldAnchor != null && _fixedMouseJoint == null)
                    {
                        var ficture = World.TestPoint(worldAnchor.Value);
                        if (ficture != null)
                        {
                            _fixedMouseJoint = new FixedMouseJoint(ficture.Body, worldAnchor.Value);
                            _fixedMouseJoint.MaxForce = 1000.0f * ficture.Body.Mass;
                            World.Add(_fixedMouseJoint);
                        }
                    }
                }
                else
                {
                    breakableBody.Update();
                }
            }
        }

        public override void HandleInput(InputHelper input, GameTime gameTime)
        {
            if (input.IsNewMouseButtonPress(MouseButtons.RightButton) ||
                input.IsNewButtonPress(Buttons.B))
            {
                Vector2 cursorPos = Camera.ConvertScreenToWorld(input.Cursor);

                Vector2 min = cursorPos - new Vector2(10, 10);
                Vector2 max = cursorPos + new Vector2(10, 10);

                AABB aabb = new AABB(ref min, ref max);

                World.QueryAABB(fixture =>
                                    {
                                        Vector2 fv = fixture.Body.Position - cursorPos;
                                        fv.Normalize();
                                        fv *= 40;
                                        fixture.Body.ApplyLinearImpulse(ref fv);
                                        return true;
                                    }, ref aabb);
            }

            base.HandleInput(input, gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            _border.Draw();
            base.Draw(gameTime);
        }

        public override void UnloadContent()
        {
            DebugView.RemoveFlags(DebugViewFlags.Shape);

            base.UnloadContent();
        }
    }
}