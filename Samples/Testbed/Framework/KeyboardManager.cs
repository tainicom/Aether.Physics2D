/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using Microsoft.Xna.Framework.Input;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public class KeyboardManager
    {
        internal KeyboardState _newKeyboardState;
        internal KeyboardState _oldKeyboardState;

        public bool IsNewKeyPress(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key) && _oldKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyDown(Keys key)
        {
            return _newKeyboardState.IsKeyDown(key);
        }

        internal bool IsKeyUp(Keys key)
        {
            return _newKeyboardState.IsKeyUp(key);
        }
    }
}