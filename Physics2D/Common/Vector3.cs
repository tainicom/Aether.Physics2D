// Copyright (c) 2017 Kastellanos Nikolaos

using System;

namespace tainicom.Aether.Physics2D.Common
{
    public struct Vector3 : IEquatable<Vector3>
    {
        private static readonly Vector3 _zero = new Vector3(0, 0, 0);
        private static readonly Vector3 _one  = new Vector3(1, 1, 1);


        public float X;
        public float Y;
        public float Z;

        public static Vector3 Zero { get { return _zero; } }
        public static Vector3 One { get { return _one; } }


        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public Vector3(float xyz)
        {
            this.X = xyz;
            this.Y = xyz;
            this.Z = xyz;
        }


        internal static Vector3 Cross(Vector3 left, Vector3 right)
        {
            Vector3 result;
            result.X = left.Y * right.Z - left.Z * right.Y;
            result.Y = left.Z * right.X - left.X * right.Z;
            result.Z = left.X * right.Y - left.Y * right.X;
            return result;

        }

        internal static float Dot(Vector3 left, Vector3 right)
        {
            return left.X * right.X + left.Y * right.Y + left.Z * right.Z;
        }

        public override int GetHashCode()
        {
            return (X.GetHashCode() ^ Y.GetHashCode()^ Z.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector3) ? Equals((Vector3)obj) : false;
        }

        #region Implement IEquatable<Vector3>
        public bool Equals(Vector3 other)
        {
            return (X == other.X && Y == other.Y && Z == other.Z);
        }
        #endregion Implement IEquatable<Vector3>

        public static Vector3 operator +(Vector3 left, Vector3 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            return left;
        }
        
        public static Vector3 operator -(Vector3 left, Vector3 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            return left;
        }

        public static Vector3 operator -(Vector3 right)
        {
            right.X = -right.X;
            right.Y = -right.Y;
            right.Z = -right.Z;
            return right;
        }

        public static Vector3 operator *(Vector3 left, Vector3 right)
        {
            left.X *= right.X;
            left.Y *= right.Y;
            left.Z *= right.Z;
            return left;
        }

        public static Vector3 operator *(Vector3 left, float right)
        {
            left.X *= right;
            left.Y *= right;
            left.Z *= right;
            return left;
        }

        public static Vector3 operator *(float left, Vector3 right)
        {
            right.X *= left;
            right.Y *= left;
            right.Z *= left;
            return right;
        }
        
        public static Vector3 operator /(Vector3 left, float right)
        {
            float invRight = 1f / right;
            left.X *= invRight;
            left.Y *= invRight;
            left.Z *= invRight;
            return left;
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return left.X != right.X || left.Y != right.Y || left.Z != right.Z;
        }

        public override string ToString()
        {
            return String.Format("{{X: {0} Y: {1} Z: {2}}}", X, Y, Z);
        }
    }
}
