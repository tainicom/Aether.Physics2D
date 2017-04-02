/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System.Collections.Generic;
using tainicom.Aether.Physics2D.Collision.Shapes;
using tainicom.Aether.Physics2D.Dynamics;

namespace tainicom.Aether.Content.Pipeline
{
    public class FixtureTemplate
    {
        public Shape Shape;
        public float Restitution;
        public float Friction;
        public string Name;
    }

    public class BodyTemplate
    {
        public List<FixtureTemplate> Fixtures;
        public float Mass;
        public BodyType BodyType;

        public BodyTemplate()
        {
            Fixtures = new List<FixtureTemplate>();
        }
    }

    public class BodyContainer : Dictionary<string, BodyTemplate> { }
}
