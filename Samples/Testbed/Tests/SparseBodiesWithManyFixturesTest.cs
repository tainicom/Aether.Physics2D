// Copyright (c) 2018 Kastellanos Nikolaos

using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using System;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class SparseBodiesWithManyFixturesTest : Test
    {
        private const int blockSize = 1;
        private const float blockDistanse = 100;
        private const float gridSize = 3000;

        private SparseBodiesWithManyFixturesTest()
        {
            float angularVelocity = 0;
            for (float x = -(gridSize/2f); x <= (gridSize/2f); x+=blockDistanse)
            {
                for (float y = -(gridSize/2f); y <= (gridSize/2f); y+=blockDistanse)
                {
                    var position = new Vector2(x, y);
                    var block = World.CreateRectangle(blockSize, blockSize, 2.6f, position, 0, BodyType.Dynamic);
                    block.IgnoreGravity = true;
                    block.SleepingAllowed = false;
                    angularVelocity = (angularVelocity + MathHelper.ToRadians(360f / 60f)) % 1f;
                    block.AngularVelocity = angularVelocity;

                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2(-1f, -1f));
                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2(-1f,  0f));
                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2(-1f,  1f));
                    
                    //block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2( 0f, -1f));
                    //block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2( 0f,  1f));
                    
                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2( 1f, -1f));
                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2( 1f,  0f));
                    block.CreateRectangle(blockSize, blockSize, 2.6f, new Vector2( 1f,  1f));


                }
            }
        }
        
        public override void Update(GameSettings settings, GameTime gameTime)
        {
            base.Update(settings, gameTime);


        }

        public static Test Create()
        {
            return new SparseBodiesWithManyFixturesTest();
        }
    }
}
