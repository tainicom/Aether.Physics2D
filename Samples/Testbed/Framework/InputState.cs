//   Copyright 2021 Kastellanos Nikolaos


using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace tainicom.Aether.Physics2D.Samples.Testbed.Framework
{
    public class InputState
    {
        #region Fields
        // Touch
        private TouchLocation _mouseTouchLocation;
        private TouchLocation _prevMouseTouchLocation;
        public bool IsGestureAvailable;
        private TouchCollection _touchCollection;
        private List<GestureSample> _gestures = new List<GestureSample>(4);

        // GamePad (... and Back button on phones)
        private GamePadState _prevGamePadState;
        private GamePadState _gamePadState;

        // Keyboard
        private KeyboardState _prevKeyboardState;
        private KeyboardState _keyboardState;

        // Mouse
        private MouseState _prevMouseState;
        private MouseState _mouseState;
        #endregion

        private DateTime _pressTimestamp;

        #region Properties
        public TouchCollection TouchCollection { get { return _touchCollection; } }
        public List<GestureSample> Gestures { get { return _gestures; } }
        public GamePadState GamePadState { get { return _gamePadState; } }
        public GamePadState PrevGamePadState { get { return _prevGamePadState; } }
        public KeyboardState KeyboardState { get { return _keyboardState; } }
        public KeyboardState PrevKeyboardState { get { return _prevKeyboardState; } }
        public MouseState MouseState { get { return _mouseState; } }
        public MouseState PrevMouseState { get { return _prevMouseState; } }

        public int ScrollWheelDelta { get { return MouseState.ScrollWheelValue - PrevMouseState.ScrollWheelValue; } }
        public int HScrollWheelDelta { get { return MouseState.HorizontalScrollWheelValue - PrevMouseState.HorizontalScrollWheelValue; } }


        #endregion

        #region Initialization
        public InputState()
        {
        }
        #endregion


        /// <param name="isActive">The value of Game.IsActive</param>
        public void Update(bool isActive)
        {
            _prevGamePadState = _gamePadState;
            _gamePadState = GamePad.GetState(PlayerIndex.One);

            _prevKeyboardState = _keyboardState;
            _keyboardState = Keyboard.GetState();

            _prevMouseState = _mouseState;
            _mouseState = Mouse.GetState();

            _touchCollection = TouchPanel.GetState();

            IsGestureAvailable = false;
            if(TouchPanel.EnabledGestures != GestureType.None)
            {
                IsGestureAvailable = TouchPanel.IsGestureAvailable;
                _gestures.Clear();
                while (TouchPanel.IsGestureAvailable)
                {
                    _gestures.Add(TouchPanel.ReadGesture());
                }
            }

            return;
        }
        
        public bool IsButtonPressed(Buttons button)
        {
            return (_gamePadState.IsButtonDown(button) && _prevGamePadState.IsButtonUp(button));
        }
        public bool IsButtonReleased(Buttons button)
        {
            return (_gamePadState.IsButtonUp(button) && _prevGamePadState.IsButtonDown(button));
        }
        public bool IsButtonUp(Buttons button)
        {
            return (_gamePadState.IsButtonUp(button) && _prevGamePadState.IsButtonUp(button));
        }
        public bool IsButtonDown(Buttons button)
        {
            return (_gamePadState.IsButtonDown(button) && _prevGamePadState.IsButtonDown(button));
        }
        
        internal bool IsLeftButtonPressed()
        {
            return MouseState.LeftButton == ButtonState.Pressed && PrevMouseState.LeftButton == ButtonState.Released;
        }
        internal bool IsLeftButtonReleased()
        {
            return MouseState.LeftButton == ButtonState.Released && PrevMouseState.LeftButton == ButtonState.Pressed;
        }
        internal bool IsRightButtonPressed()
        {
            return MouseState.RightButton == ButtonState.Pressed && PrevMouseState.RightButton == ButtonState.Released;
        }
        internal bool IsRightButtonReleased()
        {
            return MouseState.RightButton == ButtonState.Released && PrevMouseState.RightButton == ButtonState.Pressed;
        }

        public bool IsKeyPressed(Keys key)
        {   
            return (_keyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyUp(key));
        }
        public bool IsKeyReleased(Keys key)
        {
            return (_keyboardState.IsKeyUp(key)   && _prevKeyboardState.IsKeyDown(key));
        }
        public bool IsKeyUp(Keys key)
        {
            return (_keyboardState.IsKeyUp(key)   && _prevKeyboardState.IsKeyUp(key));
        }
        public bool IsKeyDown(Keys key)
        {
            return (_keyboardState.IsKeyDown(key) && _prevKeyboardState.IsKeyDown(key));
        }

        
    }
}
