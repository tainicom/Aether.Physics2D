/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

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
