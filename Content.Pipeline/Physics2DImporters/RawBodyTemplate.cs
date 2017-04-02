using System.Collections.Generic;
using tainicom.Aether.Physics2D.Dynamics;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Content.Pipeline
{
    struct RawFixtureTemplate
    {
        public string Path;
        public string Name;
        public Matrix Transformation;
        public float Density;
        public float Friction;
        public float Restitution;
    }

    struct RawBodyTemplate
    {
        public List<RawFixtureTemplate> Fixtures;
        public string Name;
        public float Mass;
        public BodyType BodyType;
    }
}
