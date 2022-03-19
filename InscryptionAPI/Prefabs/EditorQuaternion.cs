using System;
using System.Runtime.InteropServices;

namespace InscryptionAPI.Prefabs
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct EditorQuaternion : IEquatable<EditorQuaternion>
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public EditorQuaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    case 3: return W;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid EditorQuaternion index!");
                }
            }

            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    case 3: W = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid EditorQuaternion index!");
                }
            }
        }

        public override string ToString()
        {
            return $"(X:{X},Y:{Y},Z:{Z},W:{W})";
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
        }

        public override bool Equals(object other)
        {
            if (!(other is EditorQuaternion))
                return false;
            return Equals((EditorQuaternion)other);
        }

        public bool Equals(EditorQuaternion other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        public static float Dot(EditorQuaternion a, EditorQuaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        private static bool IsEqualUsingDot(float dot)
        {
            return dot > 1.0f - kEpsilon;
        }

        public static bool operator ==(EditorQuaternion lhs, EditorQuaternion rhs)
        {
            return IsEqualUsingDot(Dot(lhs, rhs));
        }

        public static bool operator !=(EditorQuaternion lhs, EditorQuaternion rhs)
        {
            return !(lhs == rhs);
        }

        private const float kEpsilon = 0.000001F;
    }
}
