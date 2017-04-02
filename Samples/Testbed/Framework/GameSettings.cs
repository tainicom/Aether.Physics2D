/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public class GameSettings
    {
        public float Hz;
        public bool Pause;
        public bool SingleStep;

        public GameSettings()
        {
#if WINDOWS_PHONE
			Hz = 30.0f;
#else
            Hz = 60.0f;
#endif
        }
    }
}