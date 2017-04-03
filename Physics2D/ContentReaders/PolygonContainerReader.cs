using tainicom.Aether.Physics2D.Common;
using Microsoft.Xna.Framework.Content;

namespace tainicom.Aether.Physics2D.Content
{
    public class PolygonContainerReader : ContentTypeReader<PolygonContainer>
    {
        protected override PolygonContainer Read(ContentReader input, PolygonContainer existingInstance)
        {
            PolygonContainer paths = existingInstance ?? new PolygonContainer();

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = input.ReadString();
                bool closed = input.ReadBoolean();
                int vertsCount = input.ReadInt32();
                Vertices verts = new Vertices();
                for (int j = 0; j < vertsCount; j++)
                {
                    verts.Add(input.ReadVector2());
                }
                paths[name] = new Polygon(verts, closed);
            }

            return paths;
        }
    }
}
