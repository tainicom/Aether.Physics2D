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

using System.Collections.Generic;
using System.Diagnostics;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using tainicom.Aether.Physics2D.Dynamics.Contacts;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class CollisionTest : Test
    {
        private Body wall, ball;
        Vector2 position = Vector2.One;
        Vector2 speed = Vector2.One * -10.6f;
        float dT = 0.0016666f;
        int collisions = 0;
        int separations = 0;
        
        private CollisionTest()
        {
            World.Gravity = Vector2.Zero;

            wall = World.CreateRectangle(2F, 20f, 1f, new Vector2(-1F, 10f));
            wall.Tag = "wall";
            wall.SetRestitution(1);
            wall.SetFriction(0);
            wall.FixedRotation = true;
            wall.IsBullet = true;
            
            ball = World.CreateCircle(0.51f, 1f, position, BodyType.Dynamic);
            ball.Tag = "ball";
            ball.LinearVelocity = speed;
            ball.SetRestitution(1);
            ball.SetFriction(0);
            ball.FixedRotation = true;
            ball.IsBullet = true;

            ball.OnCollision += ball_OnCollision;
            ball.OnSeparation += ball_OnSeparation;

            Distance.GJKCalls = 0; Distance.GJKIters = 0; Distance.GJKMaxIters = 0;
            TimeOfImpact.TOICalls = 0; TimeOfImpact.TOIIters = 0;
            TimeOfImpact.TOIRootIters = 0; TimeOfImpact.TOIMaxRootIters = 0;
        }
        
        bool ball_OnCollision(Fixture sender, Fixture other, Contact contact)
        {
            collisions++;
            return true;
        }

        void ball_OnSeparation(Fixture sender, Fixture other, Contact contact)
        {
            separations++;
        }

        private void Launch()
        {
            wall.Position = new Vector2(-1F, 10f);
            wall.LinearVelocity = Vector2.Zero;
            wall.Rotation = 0;
            wall.AngularVelocity = 0;

            ball.Position = position;
            ball.LinearVelocity = speed;
            ball.Rotation = 0;
            ball.AngularVelocity = 0;

            collisions = 0; 
            separations = 0;

            CheckCollision();
        }

        private void CheckCollision()
        {
            ContactEdge ballContacts1 = ball.ContactList;
            Debug.Assert(ballContacts1 == null);

            // move ball to create first contact
            World.Step(dT * 28);

            ContactEdge ballContacts2 = ball.ContactList;
            Debug.Assert(ballContacts2 != null);
            Debug.Assert(ballContacts2.Other == wall);
            Contact contact = ballContacts2.Contact;

            return;
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);

            DrawString("(C) CCD: " + Settings.ContinuousPhysics);
            DrawString("(W) wall.IsBullet: " + wall.IsBullet);
            DrawString("(B) ball.IsBullet: " + ball.IsBullet);
            DrawString("(E) wall.IgnoreCCD: " + wall.IgnoreCCD);
            DrawString("(N) ball.IgnoreCCD: " + ball.IgnoreCCD);
            DrawString("");

            DrawString("collisions: " + collisions);
            DrawString("separations: " + separations);

            if (Distance.GJKCalls > 0)
                DrawString(string.Format("GJK calls = {0:n}, Ave GJK iters = {1:n}, Max GJK iters = {2:n}", Distance.GJKCalls, Distance.GJKIters / (float)Distance.GJKCalls, Distance.GJKMaxIters));

            if (TimeOfImpact.TOICalls > 0)
            {
                DrawString(string.Format("TOI calls = {0:n}, Ave TOI iters = {1:n}, Max TOI iters = {2:n}", TimeOfImpact.TOICalls, TimeOfImpact.TOIIters / (float)TimeOfImpact.TOICalls, TimeOfImpact.TOIMaxRootIters));
                DrawString(string.Format("Ave TOI root iters = {0:n}, Max TOI root iters = {1:n}", TimeOfImpact.TOIRootIters / (float)TimeOfImpact.TOICalls, TimeOfImpact.TOIMaxRootIters));
            }

            if (StepCount % 120 == 0)
                Launch();
        }

        public override void Keyboard(KeyboardManager keyboardManager)
        {
            base.Keyboard(keyboardManager);

            if (keyboardManager.IsNewKeyPress(Keys.C))
                Settings.ContinuousPhysics = !Settings.ContinuousPhysics;
            
            if (keyboardManager.IsNewKeyPress(Keys.W))
                wall.IsBullet = !wall.IsBullet;
            if (keyboardManager.IsNewKeyPress(Keys.B))
                ball.IsBullet = !ball.IsBullet;

            if (keyboardManager.IsNewKeyPress(Keys.E))
                wall.IgnoreCCD = !wall.IgnoreCCD;
            if (keyboardManager.IsNewKeyPress(Keys.N))
                ball.IgnoreCCD = !ball.IgnoreCCD;
        }

        internal static Test Create()
        {
            return new CollisionTest();
        }
    }
}