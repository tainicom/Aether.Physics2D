/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
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

using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class ShapeEditingTest : Test
    {
        private Body _body;
        private Fixture _fixture2;

        public ShapeEditingTest()
        {
            //Ground
            World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            _body = World.CreateBody();
            _body.BodyType = BodyType.Dynamic;
            _body.Position = new Vector2(0.0f, 10.0f);

            Vertices box = PolygonTools.CreateRectangle(4.0f, 4.0f);
            PolygonShape shape2 = new PolygonShape(box, 10);
            _body.CreateFixture(shape2);

            _fixture2 = null;
        }

        public override void Keyboard(InputState input)
        {
            if (input.IsKeyPressed(Keys.C) && _fixture2 == null)
            {
                CircleShape shape = new CircleShape(3.0f, 10);
                shape.Position = new Vector2(0.5f, -4.0f);
                _fixture2 = _body.CreateFixture(shape);
                _body.Awake = true;
            }

            if (input.IsKeyPressed(Keys.D) && _fixture2 != null)
            {
                _body.Remove(_fixture2);
                _fixture2 = null;
                _body.Awake = true;
            }

            base.Keyboard(input);
        }

        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);
            DrawString("Press: (c) create a shape, (d) destroy a shape.");
            
        }

    }
}