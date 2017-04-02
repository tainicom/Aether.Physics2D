using System;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public struct TestEntry
    {
        public Func<Test> CreateTest;
        public string Name;
    }
}