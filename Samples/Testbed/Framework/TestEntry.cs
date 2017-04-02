/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public struct TestEntry
    {
        public Func<Test> CreateTest;
        public string Name;
    }
}