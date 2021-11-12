/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public struct TestEntry
    {
        public readonly string Name;
        public readonly Type TestType;

        public TestEntry(string name, Type testType)
        {
            this.Name = name;
            this.TestType = testType;
        }

        public Test CreateTest()
        {
            return (Test)Activator.CreateInstance(TestType);
        }
    }
}