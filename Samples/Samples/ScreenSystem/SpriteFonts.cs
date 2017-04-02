/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace tainicom.Aether.Physics2D.Samples.ScreenSystem
{
    public class SpriteFonts
    {
        public SpriteFont DetailsFont;
        public SpriteFont FrameRateCounterFont;
        public SpriteFont MenuSpriteFont;

        public SpriteFonts(ContentManager contentManager)
        {
            MenuSpriteFont = contentManager.Load<SpriteFont>("Fonts/menuFont");
            FrameRateCounterFont = contentManager.Load<SpriteFont>("Fonts/frameRateCounterFont");
            DetailsFont = contentManager.Load<SpriteFont>("Fonts/detailsFont");
        }
    }
}