// Copyright (c) 2017 Kastellanos Nikolaos

using System;

namespace tainicom.Aether.Physics2D.Common
{
    public struct Vector2 : IEquatable<Vector2>
    {
        private static readonly Vector2 _zero = new Vector2(0, 0);
        private static readonly Vector2 _one  = new Vector2(1, 1);


        public float X;
        public float Y;

        public static Vector2 Zero { get { return _zero; } }
        public static Vector2 One { get { return _one; } }


        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public Vector2(float xy)
        {
            this.X = xy;
            this.Y = xy;
        }
        
        internal static float Dot(Vector2 left, Vector2 right)
        {
            return left.X * right.X + left.Y * right.Y;
        }

        internal static float Distance(Vector2 v1, Vector2 v2)
        {
            v1.X = v1.X - v2.X;
            v1.Y = v1.Y - v2.Y;
            return (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y);
        }

        internal static float DistanceSquared(Vector2 v1, Vector2 v2)
        {
            v1.X = v1.X - v2.X;
            v1.Y = v1.Y - v2.Y;
            return (v1.X * v1.X + v1.Y * v1.Y);
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared()
        {
            return (X * X + Y * Y);
        }

        public void Normalize()
        {
            var length = (float)Math.Sqrt((X * X) + (Y * Y));
            var invLength = 1.0f / length;
            X *= invLength;
            Y *= invLength;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector2) ? Equals((Vector2)obj) : false;
        }

        #region Implement IEquatable<Vector2>
        public bool Equals(Vector2 other)
        {
            return (X == other.X && Y == other.Y);
        }
        #endregion Implement IEquatable<Vector2>

        public static Vector2 operator +(Vector2 left, Vector2 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            return left;
        }
        
        public static Vector2 operator -(Vector2 left, Vector2 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            return left;
        }

        public static Vector2 operator -(Vector2 right)
        {
            right.X = -right.X;
            right.Y = -right.Y;
            return right;
        }

        public static Vector2 operator *(Vector2 left, Vector2 right)
        {
            left.X *= right.X;
            left.Y *= right.Y;
            return left;
        }

        public static Vector2 operator *(Vector2 left, float right)
        {
            left.X *= right;
            left.Y *= right;
            return left;
        }

        public static Vector2 operator *(float left, Vector2 right)
        {
            right.X *= left;
            right.Y *= left;
            return right;
        }
        
        public static Vector2 operator /(Vector2 left, float right)
        {
            float invRight = 1f / right;
            left.X *= invRight;
            left.Y *= invRight;
            return left;
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.X == right.X && left.Y == right.Y;
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return left.X != right.X || left.Y != right.Y;
        }

        public override string ToString()
        {
            return String.Format("{{X: {0} Y: {1}}}", X, Y);
        }

        #region Fast ref methods
        public static void Dot(ref Vector2 left, ref Vector2 right, out float result)
        {
            result = left.X * right.X + left.Y * right.Y;
        }

        public static void Min(ref Vector2 v1, ref Vector2 v2, out Vector2 result)
        {
            result.X = (v1.X < v2.X) ? v1.X : v2.X;
            result.Y = (v1.Y < v2.Y) ? v1.Y : v2.Y;
        }

        public static void Max(ref Vector2 v1, ref Vector2 v2, out Vector2 result)
        {
            result.X = (v1.X > v2.X) ? v1.X : v2.X;
            result.Y = (v1.Y > v2.Y) ? v1.Y : v2.Y;
        }

        public static void Distance(ref Vector2 v1, ref Vector2 v2, out float result)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            result = (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static void DistanceSquared(ref Vector2 v1, ref Vector2 v2, out float result)
        {
            float dx = v1.X - v2.X;
            float dy = v1.Y - v2.Y;
            result = (dx * dx) + (dy * dy);
        }

        public static void Add(ref Vector2 left, ref Vector2 right, out Vector2 result)
        {
            result.X = left.X + right.X;
            result.Y = left.Y + right.Y;
        }
        public static void Subtract(ref Vector2 left, ref Vector2 right, out Vector2 result)
        {
            result.X = left.X - right.X;
            result.Y = left.Y - right.Y;
        }

        public static void Multiply(ref Vector2 left, ref Vector2 right, out Vector2 result)
        {
            result.X = left.X * right.X;
            result.Y = left.Y * right.Y;
        }

        public static void Multiply(ref Vector2 left, float right, out Vector2 result)
        {
            result.X = left.X * right;
            result.Y = left.Y * right;
        }

        public static void Divide(ref Vector2 left, float right, out Vector2 result)
        {
            float invRight = 1 / right;
            result.X = left.X * invRight;
            result.Y = left.Y * invRight;
        }

        #endregion Fast ref methods

    }
}
