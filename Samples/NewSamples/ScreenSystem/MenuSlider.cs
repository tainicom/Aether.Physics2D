/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using tainicom.Aether.Physics2D.Samples.MediaSystem;
using Microsoft.Xna.Framework;

namespace tainicom.Aether.Physics2D.Samples.ScreenSystem
{
    public sealed class MenuSlider
    {
        private const float MaxTranslation = 15f;
        private const double HighlightTime = 0.3;

        private float _positionX;

        private float _targetY;
        private Vector2 _currentPosition;
        private Color _color;

        private double _hoverFade;
        private double _selectionFade;


        /// <summary>
        /// Constructs a new menu slider.
        /// </summary>
        public MenuSlider(Vector2 position)
        {
            _currentPosition = position;
            _positionX = _currentPosition.X;
            _targetY = _currentPosition.Y;
        }

        public Vector2 Position
        {
            get { return _currentPosition; }
        }

        public float Target
        {
            set { _targetY = value; }
        }

        public Color TileColor
        {
            get { return _color; }
        }

        /// <summary>
        /// Updates the menu slider.
        /// </summary>
        public void Update(bool isHovered, bool isSelected, GameTime gameTime)
        {
            if (isHovered)
            {
                _hoverFade = Math.Min(_hoverFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
            }
            else
            {
                _hoverFade = Math.Max(_hoverFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
            }

            if (isSelected)
            {
                _selectionFade = Math.Min(_selectionFade + (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 1.0);
            }
            else
            {
                _selectionFade = Math.Max(_selectionFade - (gameTime.ElapsedGameTime.TotalSeconds / HighlightTime), 0.0);
            }

            _color = Color.Lerp(ContentWrapper.Sky * 0.6f, ContentWrapper.Grey * 0.6f, (float)_hoverFade);
            _color = Color.Lerp(_color, ContentWrapper.Gold, (float)_selectionFade);

            float deltaY = _targetY - _currentPosition.Y;
            if (Math.Abs(deltaY) > MaxTranslation)
            {
                _currentPosition.Y += MaxTranslation * Math.Sign(deltaY);
            }
            else
            {
                _currentPosition.Y += deltaY;
            }
        }
    }
}