// Copyright (c) 2017 Kastellanos Nikolaos

using System;
#if XNAAPI
using Complex = tainicom.Aether.Physics2D.Common.Complex;
using Vector2 = Microsoft.Xna.Framework.Vector2;
#endif

namespace tainicom.Aether.Physics2D.Common
{
    public struct Complex
    {
        private static readonly Complex _one = new Complex(1, 0);
        private static readonly Complex _imaginaryOne = new Complex(0, 1);

        public float R;
        public float i;

        public static Complex One { get { return _one; } }
        public static Complex ImaginaryOne { get { return _imaginaryOne; } }

        public float Phase
        {
            get { return (float)Math.Atan2(i, R); }
            set 
            {
                if (value == 0)
                {
                    this = Complex.One;
                    return;
                }
                this.R = (float)Math.Cos(value);
                this.i = (float)Math.Sin(value);
            }
        }

        public float Magnitude
        {
            get { return (float)Math.Round(Math.Sqrt(MagnitudeSquared())); }
        }


        public Complex(float real, float imaginary)
        {
            R = real;
            i = imaginary;
        }
                
        public static Complex FromAngle(float angle)
        {
            if (angle == 0)
                return Complex.One;

            return new Complex(
                (float)Math.Cos(angle),
                (float)Math.Sin(angle));
        }        

        public void Conjugate()
        {
            i = -i;
        }
                
        public void Negate()
        {
            R = -R;
            i = -i;
        }

        public float MagnitudeSquared()
        {
            return (R * R) + (i * i);
        }

        public void Normalize()
        {
            var mag = Magnitude;
            R = R / mag;
            i = i / mag;            
        }

        public Vector2 ToVector2()
        {
            return new Vector2(R, i);
        }
        
        public static Complex Multiply(ref Complex left, ref Complex right)
        {
            return new Complex( left.R * right.R  - left.i * right.i,
                                left.i * right.R  + left.R * right.i);
        }

        public static Complex Divide(ref Complex left, ref Complex right)
        {
            return new Complex( right.R * left.R + right.i * left.i,
                                right.R * left.i - right.i * left.R);
        }
        public static void Divide(ref Complex left, ref Complex right, out Complex result)
        {
            result = new Complex(right.R * left.R + right.i * left.i,
                                 right.R * left.i - right.i * left.R);
        }

        public static Vector2 Multiply(ref Vector2 left, ref Complex right)
        {
            return new Vector2(left.X * right.R - left.Y * right.i,
                               left.Y * right.R + left.X * right.i);
        }
        public static void Multiply(ref Vector2 left, ref Complex right, out Vector2 result)
        {
            result = new Vector2(left.X * right.R - left.Y * right.i,
                                 left.Y * right.R + left.X * right.i);
        }
        public static Vector2 Multiply(Vector2 left, ref Complex right)
        {
            return new Vector2(left.X * right.R - left.Y * right.i,
                               left.Y * right.R + left.X * right.i);
        }

        public static Vector2 Divide(ref Vector2 left, ref Complex right)
        {
            return new Vector2(left.X * right.R + left.Y * right.i,
                               left.Y * right.R - left.X * right.i);
        }

        public static Vector2 Divide(Vector2 left, ref Complex right)
        {
            return new Vector2(left.X * right.R + left.Y * right.i,
                               left.Y * right.R - left.X * right.i);
        }
        public static void Divide(Vector2 left, ref Complex right, out Vector2 result)
        {
            result = new Vector2(left.X * right.R + left.Y * right.i,
                                 left.Y * right.R - left.X * right.i);
        }
        
        public static Complex Conjugate(ref Complex value)
        {
            return new Complex(value.R, -value.i);
        }

        public static Complex Negate(ref Complex value)
        {
            return new Complex(-value.R, -value.i);
        }

        public static Complex Normalize(ref Complex value)
        {
            var mag = value.Magnitude;
            return new Complex(value.R / mag, -value.i / mag);
        }
        
        public override string ToString()
        {
            return String.Format("{{R: {0} i: {1} Phase: {2} Magnitude: {3}}}", R, i, Phase, Magnitude);
        }
    }
}
