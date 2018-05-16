// Copyright (c) 2018 Kastellanos Nikolaos

using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
using tainicom.Aether.Physics2D.Samples.Testbed.Framework;
using Microsoft.Xna.Framework;
using System;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public class TheLeaningTowerofLireTest : Test
    {
        private const int blockWidth = 8;
        private const float blockHeight = 1f;
        private const float blockCount = 32;
        
        private TheLeaningTowerofLireTest()
        {   
            var ground = World.CreateEdge(new Vector2(-40.0f, 0.0f), new Vector2(40.0f, 0.0f));

            var position = new Vector2(0, blockHeight / 2f);
            for (int i = 0; i < blockCount; i++)
            {
                var block = World.CreateRectangle(blockWidth, blockHeight, 2.6f, position, 0, BodyType.Dynamic);
                position += new Vector2(blockWidth * (1f / (2f * (blockCount - i -1))), blockHeight);

                // some corrections are necessary to make the bridge stable.
                position.Y += Settings.LinearSlop * 1f;
                position.X -= 0.025f;
            }
        }
        
        public override void Update(GameSettings settings, GameTime gameTime)
        {   
            // run the simulation at smaller time intervals to improve accuracy
            var halfGameTime = new GameTime(gameTime.TotalGameTime, TimeSpan.FromTicks(gameTime.ElapsedGameTime.Ticks/8), gameTime.IsRunningSlowly);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
            base.Update(settings, halfGameTime);
        }

        public static Test Create()
        {
            return new TheLeaningTowerofLireTest();
        }
    }
}
