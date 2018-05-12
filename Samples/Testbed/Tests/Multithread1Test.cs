// Copyright (c) 2017 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class Multithread1Test : Test
    {
        private const int Count = 32;

        private Multithread1Test()
        {
            // enable multithreading
            World.ContactManager.VelocityConstraintsMultithreadThreshold = 256;
            World.ContactManager.PositionConstraintsMultithreadThreshold = 256;
            World.ContactManager.CollideMultithreadThreshold = 256;

            //Create ground
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            Vertices box = PolygonTools.CreateRectangle(0.5f, 0.5f);
            PolygonShape shape = new PolygonShape(box, 5);

            Vector2 x = new Vector2(-7.0f, 0.75f);
            Vector2 deltaX = new Vector2(0.5625f, 1.25f);
            Vector2 deltaY = new Vector2(1.125f, 0.0f);

            for (int i = 0; i < Count; ++i)
            {
                Vector2 y = x;

                for (int j = i; j < Count; ++j)
                {
                    Body body = World.CreateBody();
                    body.BodyType = BodyType.Dynamic;
                    body.Position = y;
                    body.CreateFixture(shape);
                    body.SleepingAllowed = false;

                    y += deltaY;
                }

                x += deltaX;
            }
        }
        
        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            var cMgr = World.ContactManager;

            DrawString("Press Space to switch between single-core / multi-core solvers.");
            DrawString("Press 1-3 to set VelocityConstraints Threshold. (1-(0 - Always ON), 2-(256), 3-(int.MaxValue - Always OFF))");
            DrawString("Press 4-6 to set PositionConstraints Threshold. (4-(0 - Always ON), 5-(256), 6-(int.MaxValue - Always OFF))");
            DrawString("Press 7-9 to set Collide Threshold.             (7-(0 - Always ON), 8-(256), 9-(int.MaxValue - Always OFF))");
            
            TextLine += 15 * 10;
            var threshold = cMgr.VelocityConstraintsMultithreadThreshold;
            if (threshold == 0) DrawString("VelocityConstraintsMultithreadThreshold: 0");
            else if (threshold == 256) DrawString("VelocityConstraintsMultithreadThreshold: 256");
            else if (threshold == int.MaxValue) DrawString("VelocityConstraintsMultithreadThreshold: int.MaxValue");
            else DrawString("VelocityConstraintsMultithreadThreshold: " + threshold);
            threshold = cMgr.PositionConstraintsMultithreadThreshold;
            if (threshold == 0) DrawString("PositionConstraintsMultithreadThreshold: 0");
            else if (threshold == 256) DrawString("PositionConstraintsMultithreadThreshold: 256");
            else if (threshold == int.MaxValue) DrawString("PositionConstraintsMultithreadThreshold: int.MaxValue");
            else DrawString("PositionConstraintsMultithreadThreshold is Currently: " + threshold);
            threshold = cMgr.CollideMultithreadThreshold;
            if (threshold == 0) DrawString("CollideMultithreadThreshold: 0");
            else if (threshold == 256) DrawString("CollideMultithreadThreshold:  256");
            else if (threshold == int.MaxValue) DrawString("CollideMultithreadThreshold: int.MaxValue");
            else DrawString("CollideMultithreadThreshold is Currently: " + threshold);
            
            if (gameTime.IsRunningSlowly)
                DrawString("[IsRunningSlowly]");
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            base.Keyboard(keyboardManager);

            var cMgr = World.ContactManager;         

            if (keyboardManager.IsNewKeyPress(Keys.D1))
                cMgr.VelocityConstraintsMultithreadThreshold = 0;
            if (keyboardManager.IsNewKeyPress(Keys.D2))
                cMgr.VelocityConstraintsMultithreadThreshold = 256;
            if (keyboardManager.IsNewKeyPress(Keys.D3))
                cMgr.VelocityConstraintsMultithreadThreshold = int.MaxValue;

            if (keyboardManager.IsNewKeyPress(Keys.D4))
                cMgr.PositionConstraintsMultithreadThreshold = 0;
            if (keyboardManager.IsNewKeyPress(Keys.D5))
                cMgr.PositionConstraintsMultithreadThreshold = 256;
            if (keyboardManager.IsNewKeyPress(Keys.D6))
                cMgr.PositionConstraintsMultithreadThreshold = int.MaxValue;
            
            if (keyboardManager.IsNewKeyPress(Keys.D7))
                cMgr.CollideMultithreadThreshold = 0;
            if (keyboardManager.IsNewKeyPress(Keys.D8))
                cMgr.CollideMultithreadThreshold = 256;
            if (keyboardManager.IsNewKeyPress(Keys.D9))
                cMgr.CollideMultithreadThreshold = int.MaxValue;

            if (keyboardManager.IsNewKeyPress(Keys.Space))
            {
                if (cMgr.VelocityConstraintsMultithreadThreshold == int.MaxValue)
                    cMgr.VelocityConstraintsMultithreadThreshold = 0;
                else
                    cMgr.VelocityConstraintsMultithreadThreshold = int.MaxValue;
                cMgr.PositionConstraintsMultithreadThreshold = cMgr.VelocityConstraintsMultithreadThreshold;
                cMgr.CollideMultithreadThreshold = cMgr.VelocityConstraintsMultithreadThreshold;

            }
        }

        public static Test Create()
        {
            return new Multithread1Test();
        }
    }
}