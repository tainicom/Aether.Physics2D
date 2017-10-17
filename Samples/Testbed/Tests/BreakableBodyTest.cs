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

using System;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using tainicom.Aether.Physics2D.Common.PhysicsLogic;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class BreakableBodyTest : Test
    {
        private float _angularVelocity;
        BreakableBody _breakableBody;
        private Vector2 _velocity;

        private BreakableBodyTest()
        {
            // Ground body
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));
        }

        public override void Initialize()
        {
            //load texture that will represent the physics body
            Texture2D polygonTexture = GameInstance.Content.Load<Texture2D>("Rock");

            //Create an array to hold the data from the texture
            uint[] data = new uint[polygonTexture.Width * polygonTexture.Height];

            //Transfer the texture data to the array
            polygonTexture.GetData(data);

            Vertices verts = PolygonTools.CreatePolygon(data, polygonTexture.Width);
            Vector2 scale = new Vector2(0.07f, 0.07f);
            verts.Scale(ref scale);

            _breakableBody = new BreakableBody(World, verts, 50, new Vector2(-10, 25));

            base.Initialize();
        }
        
        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            _breakableBody.Update();
        }

        internal static Test Create()
        {
            return new BreakableBodyTest();
        }
    }
}