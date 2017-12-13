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
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Joints;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class Multithread2Test : Test
    {
        private const int Count = 600;
        private int _count;

        Multithread2Test()
        {
            Body ground = World.CreateBody();

            Body tumblerBody = World.CreateBody(new Vector2(0, 10));
            tumblerBody.SleepingAllowed = false;
            tumblerBody.BodyType = BodyType.Dynamic;

            tumblerBody.CreateRectangle(1, 20, 5, new Vector2(10, 0));
            tumblerBody.CreateRectangle(1, 20, 5, new Vector2(-10, 0));
            tumblerBody.CreateRectangle(20, 1, 5, new Vector2(0, 10));
            tumblerBody.CreateRectangle(20, 1, 5, new Vector2(0, -10));

            RevoluteJoint joint = JointFactory.CreateRevoluteJoint(World, ground, tumblerBody, new Vector2(0, 10), Vector2.Zero);
            joint.ReferenceAngle = 0.0f;
            joint.MotorSpeed = 0.05f * MathHelper.Pi;
            joint.MaxMotorTorque = 1e8f;
            joint.MotorEnabled = true;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            if (_count < Count)
            {
                Body box = World.CreateRectangle(0.125f * 2, 0.125f * 2, 1, new Vector2(0, 10));
                box.BodyType = BodyType.Dynamic;
                ++_count;
            }

            DrawString("Press 1-4 to set VelocityConstraintsMultithreadThreshold. (1-(0 - Always ON), 2-(128), 3-(256), 4-(int.MaxValue - Always OFF))");
            DrawString("Press 5-8 to set PositionConstraintsMultithreadThreshold. (5-(0 - Always ON), 6-(128), 7-(256), 8-(int.MaxValue - Always OFF))");
            var threshold = Settings.VelocityConstraintsMultithreadThreshold;
            if (threshold == 0) DrawString("VelocityConstraintsMultithreadThreshold is Currently: 0");
            else if (threshold == 128) DrawString("VelocityConstraintsMultithreadThreshold is Currently: 128");
            else if (threshold == 256) DrawString("VelocityConstraintsMultithreadThreshold is Currently: 256");
            else if (threshold == int.MaxValue) DrawString("VelocityConstraintsMultithreadThreshold is Currently: int.MaxValue");
            else DrawString("VelocityConstraintsMultithreadThreshold is Currently: " + threshold);
            threshold = Settings.PositionConstraintsMultithreadThreshold;
            if (threshold == 0) DrawString("PositionConstraintsMultithreadThreshold is Currently: 0");
            else if (threshold == 128) DrawString("PositionConstraintsMultithreadThreshold is Currently: 128");
            else if (threshold == 256) DrawString("PositionConstraintsMultithreadThreshold is Currently: 256");
            else if (threshold == int.MaxValue) DrawString("PositionConstraintsMultithreadThreshold is Currently: int.MaxValue");
            else DrawString("PositionConstraintsMultithreadThreshold is Currently: " + threshold);

            if (gameTime.IsRunningSlowly)
                DrawString("[IsRunningSlowly]");
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            base.Keyboard(keyboardManager);

            if (keyboardManager.IsNewKeyPress(Keys.D1))
                Settings.VelocityConstraintsMultithreadThreshold = 0;
            if (keyboardManager.IsNewKeyPress(Keys.D2))
                Settings.VelocityConstraintsMultithreadThreshold = 128;
            if (keyboardManager.IsNewKeyPress(Keys.D3))
                Settings.VelocityConstraintsMultithreadThreshold = 256;
            if (keyboardManager.IsNewKeyPress(Keys.D4))
                Settings.VelocityConstraintsMultithreadThreshold = int.MaxValue;

            if (keyboardManager.IsNewKeyPress(Keys.D5))
                Settings.PositionConstraintsMultithreadThreshold = 0;
            if (keyboardManager.IsNewKeyPress(Keys.D6))
                Settings.PositionConstraintsMultithreadThreshold = 128;
            if (keyboardManager.IsNewKeyPress(Keys.D7))
                Settings.PositionConstraintsMultithreadThreshold = 256;
            if (keyboardManager.IsNewKeyPress(Keys.D8))
                Settings.PositionConstraintsMultithreadThreshold = int.MaxValue;
        }

        public static Test Create()
        {
            return new Multithread2Test();
        }
    }
}