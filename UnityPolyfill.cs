using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEngine
{

    
    
    public struct Vector2 : IEquatable<Vector2>
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Vector2 other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }

        public static bool operator ==(Vector2 left, Vector2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector2 left, Vector2 right)
        {
            return !left.Equals(right);
        }
    }

    public struct Vector3 : IEquatable<Vector3>
    {
        public float x;
        public float y;
        public float z;
        
        private static readonly Vector3 zeroVector = new Vector3(0.0f, 0.0f, 0.0f);
        private static readonly Vector3 oneVector = new Vector3(1f, 1f, 1f);
        private static readonly Vector3 upVector = new Vector3(0.0f, 1f, 0.0f);
        private static readonly Vector3 downVector = new Vector3(0.0f, -1f, 0.0f);
        private static readonly Vector3 leftVector = new Vector3(-1f, 0.0f, 0.0f);
        private static readonly Vector3 rightVector = new Vector3(1f, 0.0f, 0.0f);
        private static readonly Vector3 forwardVector = new Vector3(0.0f, 0.0f, 1f);
        private static readonly Vector3 backVector = new Vector3(0.0f, 0.0f, -1f);
        private static readonly Vector3 positiveInfinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        private static readonly Vector3 negativeInfinityVector = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float newX, float newY, float newZ)
        {
        this.x = newX;
        this.y = newY;
        this.z = newZ;
        }

        /// <summary>
        ///   <para>Multiplies two vectors component-wise.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Scale(Vector3 a, Vector3 b) => new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);

        /// <summary>
        ///   <para>Multiplies every component of this vector by the same component of scale.</para>
        /// </summary>
        /// <param name="scale"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(Vector3 scale)
        {
        this.x *= scale.x;
        this.y *= scale.y;
        this.z *= scale.z;
        }

        /// <summary>
        ///   <para>Cross Product of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
        return new Vector3((float) ((double) lhs.y * (double) rhs.z - (double) lhs.z * (double) rhs.y), (float) ((double) lhs.z * (double) rhs.x - (double) lhs.x * (double) rhs.z), (float) ((double) lhs.x * (double) rhs.y - (double) lhs.y * (double) rhs.x));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
        return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2;
        }

        /// <summary>
        ///   <para>Returns true if the given vector is exactly equal to this vector.</para>
        /// </summary>
        /// <param name="other"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Vector3 other1 && this.Equals(other1);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3 other)
        {
        return (double) this.x == (double) other.x && (double) this.y == (double) other.y && (double) this.z == (double) other.z;
        }

        /// <summary>
        ///   <para>Reflects a vector off the plane defined by a normal.</para>
        /// </summary>
        /// <param name="inDirection">The direction vector towards the plane.</param>
        /// <param name="inNormal">The normal vector that defines the plane.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
        float num = -2f * Vector3.Dot(inNormal, inDirection);
        return new Vector3(num * inNormal.x + inDirection.x, num * inNormal.y + inDirection.y, num * inNormal.z + inDirection.z);
        }

        /// <summary>
        ///   <para>Returns a normalized vector based on the given vector. The normalized vector has a magnitude of 1 and is in the same direction as the given vector. Returns a zero vector If the given vector is too small to be normalized.</para>
        /// </summary>
        /// <param name="value">The vector to be normalized.</param>
        /// <returns>
        ///   <para>A new vector with the same direction as the original vector but with a magnitude of 1.0.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Normalize(Vector3 value)
        {
        float num = Vector3.Magnitude(value);
        return (double) num > 9.999999747378752E-06 ? value / num : Vector3.zero;
        }

        /// <summary>
        ///   <para>Makes this vector have a magnitude of 1.</para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
        float num = Vector3.Magnitude(this);
        if ((double) num > 9.999999747378752E-06)
          this = this / num;
        else
          this = Vector3.zero;
        }

        /// <summary>
        ///   <para>Returns a normalized vector based on the current vector. The normalized vector has a magnitude of 1 and is in the same direction as the current vector. Returns a zero vector If the current vector is too small to be normalized.</para>
        /// </summary>
        public Vector3 normalized
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.Normalize(this);
        }

        /// <summary>
        ///   <para>Dot Product of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Vector3 lhs, Vector3 rhs)
        {
        return (float) ((double) lhs.x * (double) rhs.x + (double) lhs.y * (double) rhs.y + (double) lhs.z * (double) rhs.z);
        }

        /// <summary>
        ///   <para>Projects a vector onto another vector.</para>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="onNormal"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        {
        float num1 = Vector3.Dot(onNormal, onNormal);
        if ((double) num1 < (double) Mathf.Epsilon)
          return Vector3.zero;
        float num2 = Vector3.Dot(vector, onNormal);
        return new Vector3(onNormal.x * num2 / num1, onNormal.y * num2 / num1, onNormal.z * num2 / num1);
        }

        /// <summary>
        ///   <para>Projects a vector onto a plane.</para>
        /// </summary>
        /// <param name="vector">The vector to project on the plane.</param>
        /// <param name="planeNormal">The normal which defines the plane to project on.</param>
        /// <returns>
        ///   <para>The orthogonal projection of vector on the plane.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        {
        float num1 = Vector3.Dot(planeNormal, planeNormal);
        if ((double) num1 < (double) Mathf.Epsilon)
          return vector;
        float num2 = Vector3.Dot(vector, planeNormal);
        return new Vector3(vector.x - planeNormal.x * num2 / num1, vector.y - planeNormal.y * num2 / num1, vector.z - planeNormal.z * num2 / num1);
        }

        /// <summary>
        ///   <para>Calculates the angle between two vectors.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        /// <returns>
        ///   <para>The angle in degrees between the two vectors.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Vector3 from, Vector3 to)
        {
        float num = (float) Math.Sqrt((double) from.sqrMagnitude * (double) to.sqrMagnitude);
        return (double) num < 1.0000000036274937E-15 ? 0.0f : (float) Math.Acos((double) Mathf.Clamp(Vector3.Dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }

        /// <summary>
        ///   <para>Calculates the signed angle between vectors from and to in relation to axis.</para>
        /// </summary>
        /// <param name="from">The vector from which the angular difference is measured.</param>
        /// <param name="to">The vector to which the angular difference is measured.</param>
        /// <param name="axis">A vector around which the other vectors are rotated.</param>
        /// <returns>
        ///   <para>Returns the signed angle between from and to in degrees.</para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
        {
        float num1 = Vector3.Angle(from, to);
        float num2 = (float) ((double) from.y * (double) to.z - (double) from.z * (double) to.y);
        float num3 = (float) ((double) from.z * (double) to.x - (double) from.x * (double) to.z);
        float num4 = (float) ((double) from.x * (double) to.y - (double) from.y * (double) to.x);
        float num5 = Mathf.Sign((float) ((double) axis.x * (double) num2 + (double) axis.y * (double) num3 + (double) axis.z * (double) num4));
        return num1 * num5;
        }

        /// <summary>
        ///   <para>Returns the distance between a and b.</para>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(Vector3 a, Vector3 b)
        {
        float num1 = a.x - b.x;
        float num2 = a.y - b.y;
        float num3 = a.z - b.z;
        return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2 + (double) num3 * (double) num3);
        }

        /// <summary>
        ///   <para>Returns a copy of vector with its magnitude clamped to maxLength.</para>
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="maxLength"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        {
        float sqrMagnitude = vector.sqrMagnitude;
        if ((double) sqrMagnitude <= (double) maxLength * (double) maxLength)
          return vector;
        float num1 = (float) Math.Sqrt((double) sqrMagnitude);
        float num2 = vector.x / num1;
        float num3 = vector.y / num1;
        float num4 = vector.z / num1;
        return new Vector3(num2 * maxLength, num3 * maxLength, num4 * maxLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Magnitude(Vector3 vector)
        {
        return (float) Math.Sqrt((double) vector.x * (double) vector.x + (double) vector.y * (double) vector.y + (double) vector.z * (double) vector.z);
        }

        /// <summary>
        ///   <para>Returns the length of this vector (Read Only).</para>
        /// </summary>
        public float magnitude
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        {
          return (float) Math.Sqrt((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z);
        }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float SqrMagnitude(Vector3 vector)
        {
        return (float) ((double) vector.x * (double) vector.x + (double) vector.y * (double) vector.y + (double) vector.z * (double) vector.z);
        }

        /// <summary>
        ///   <para>Returns the squared length of this vector (Read Only).</para>
        /// </summary>
        public float sqrMagnitude
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get
        {
          return (float) ((double) this.x * (double) this.x + (double) this.y * (double) this.y + (double) this.z * (double) this.z);
        }
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the smallest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        {
        return new Vector3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
        }

        /// <summary>
        ///   <para>Returns a vector that is made from the largest components of two vectors.</para>
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        {
        return new Vector3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, 0).</para>
        /// </summary>
        public static Vector3 zero
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.zeroVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(1, 1, 1).</para>
        /// </summary>
        public static Vector3 one
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.oneVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, 1).</para>
        /// </summary>
        public static Vector3 forward
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.forwardVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 0, -1).</para>
        /// </summary>
        public static Vector3 back
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.backVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, 1, 0).</para>
        /// </summary>
        public static Vector3 up
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.upVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(0, -1, 0).</para>
        /// </summary>
        public static Vector3 down
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.downVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(-1, 0, 0).</para>
        /// </summary>
        public static Vector3 left
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.leftVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(1, 0, 0).</para>
        /// </summary>
        public static Vector3 right
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.rightVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity).</para>
        /// </summary>
        public static Vector3 positiveInfinity
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.positiveInfinityVector;
        }

        /// <summary>
        ///   <para>Shorthand for writing Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity).</para>
        /// </summary>
        public static Vector3 negativeInfinity
        {
        [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector3.negativeInfinityVector;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
        return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
        return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);




    }

    public struct Vector4 : IEquatable<Vector4>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public bool Equals(Vector4 other)
        {
            return x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, w);
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !left.Equals(right);
        }
    }

    public struct Color : IEquatable<Color>
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public bool Equals(Color other)
        {
            return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
        }

        public override bool Equals(object obj)
        {
            return obj is Color other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(r, g, b, a);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }
    }

    public struct Ray : IEquatable<Ray>
    {
        public Vector3 origin;
        public Vector3 direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public bool Equals(Ray other)
        {
            return origin.Equals(other.origin) && direction.Equals(other.direction);
        }

        public override bool Equals(object obj)
        {
            return obj is Ray other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(origin, direction);
        }

        public static bool operator ==(Ray left, Ray right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Ray left, Ray right)
        {
            return !left.Equals(right);
        }
    }

    public static class Mathf
    {
        public static readonly float Epsilon = MathfInternal.IsFlushToZeroEnabled ? MathfInternal.FloatMinNormal : MathfInternal.FloatMinDenormal;

        public static float Min(float a, float b) => (double) a < (double) b ? a : b;
        public static float Max(float a, float b) => (double) a > (double) b ? a : b;
        public static float Sign(float f) => (double) f >= 0.0 ? 1f : -1f;
        public static float Acos(float f) => (float) Math.Acos((double) f);
        public static float Sqrt(float f) => (float) Math.Sqrt((double) f);
        public static int RoundToInt(float f) => (int) Math.Round((double) f);
        public static float Abs(float f) => Math.Abs(f);
        public static int Abs(int value) => Math.Abs(value);
        public static float Round(float f) => (float) Math.Round((double) f);
        public static float Clamp01(float value)
        {
            if ((double) value < 0.0)
                return 0.0f;
            return (double) value > 1.0 ? 1f : value;
        }
        public static float Clamp(float value, float min, float max)
        {
            if ((double) value < (double) min)
                value = min;
            else if ((double) value > (double) max)
                value = max;
            return value;
        }
        public static int NextPowerOfTwo(int n)
        {
            if (n == 0) return 1;
            n--;
            n |= n >> 1;
            n |= n >> 2;
            n |= n >> 4;
            n |= n >> 8;
            n |= n >> 16;
            return n + 1;
        }
        
        
    }
    
    public struct MathfInternal
    {
        public static volatile float FloatMinNormal = 1.1754944E-38f;
        public static volatile float FloatMinDenormal = float.Epsilon;
        public static bool IsFlushToZeroEnabled = (double) MathfInternal.FloatMinDenormal == 0.0;
    }
    
    public struct Quaternion(float x, float y, float z, float w) : IEquatable<Quaternion>, IFormattable
{
  /// <summary>
  ///   <para>X component of the Quaternion. Don't modify this directly unless you know quaternions inside out.</para>
  /// </summary>
  public float x = x;
  /// <summary>
  ///   <para>Y component of the Quaternion. Don't modify this directly unless you know quaternions inside out.</para>
  /// </summary>
  public float y = y;
  /// <summary>
  ///   <para>Z component of the Quaternion. Don't modify this directly unless you know quaternions inside out.</para>
  /// </summary>
  public float z = z;
  /// <summary>
  ///   <para>W component of the Quaternion. Do not directly modify quaternions.</para>
  /// </summary>
  public float w = w;
  private static readonly Quaternion identityQuaternion = new Quaternion(0.0f, 0.0f, 0.0f, 1f);
  public const float kEpsilon = 1E-06f;

  /// <summary>
  ///   <para>Creates a rotation from fromDirection to toDirection.</para>
  /// </summary>
  /// <param name="fromDirection">A non-unit or unit vector representing a direction axis to rotate.</param>
  /// <param name="toDirection">A non-unit or unit vector representing the target direction axis.</param>
  /// <returns>
  ///   <para>A unit quaternion which rotates from fromDirection to toDirection.</para>
  /// </returns>
  public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
  {
    Quaternion ret;
    Quaternion.FromToRotation_Injected(ref fromDirection, ref toDirection, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Returns the Inverse of rotation.</para>
  /// </summary>
  /// <param name="rotation"></param>
  public static Quaternion Inverse(Quaternion rotation)
  {
    Quaternion ret;
    Quaternion.Inverse_Injected(ref rotation, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Spherically linear interpolates between unit quaternions a and b by a ratio of t.</para>
  /// </summary>
  /// <param name="a">Start unit quaternion value, returned when t = 0.</param>
  /// <param name="b">End unit quaternion value, returned when t = 1.</param>
  /// <param name="t">Interpolation ratio. Value is clamped to the range [0, 1].</param>
  /// <returns>
  ///   <para>A unit quaternion spherically interpolated between quaternions a and b.</para>
  /// </returns>
  public static Quaternion Slerp(Quaternion a, Quaternion b, float t)
  {
    Quaternion ret;
    Quaternion.Slerp_Injected(ref a, ref b, t, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Spherically linear interpolates between unit quaternions a and b by t.</para>
  /// </summary>
  /// <param name="a">Start unit quaternion value, returned when t = 0.</param>
  /// <param name="b">End unit quaternion value, returned when t = 1.</param>
  /// <param name="t">Interpolation ratio. Value is unclamped.</param>
  /// <returns>
  ///   <para>A unit quaternion spherically interpolated between unit quaternions a and b.</para>
  /// </returns>
  public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float t)
  {
    Quaternion ret;
    Quaternion.SlerpUnclamped_Injected(ref a, ref b, t, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Interpolates between a and b by t and normalizes the result afterwards.</para>
  /// </summary>
  /// <param name="a">Start unit quaternion value, returned when t = 0.</param>
  /// <param name="b">End unit quaternion value, returned when t = 1.</param>
  /// <param name="t">Interpolation ratio. The value is clamped to the range [0, 1].</param>
  /// <returns>
  ///   <para>A unit quaternion interpolated between quaternions a and b.</para>
  /// </returns>
  public static Quaternion Lerp(Quaternion a, Quaternion b, float t)
  {
    Quaternion ret;
    Quaternion.Lerp_Injected(ref a, ref b, t, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Interpolates between a and b by t and normalizes the result afterwards. The parameter t is not clamped.</para>
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <param name="t"></param>
  public static Quaternion LerpUnclamped(Quaternion a, Quaternion b, float t)
  {
    Quaternion ret;
    Quaternion.LerpUnclamped_Injected(ref a, ref b, t, out ret);
    return ret;
  }

  private static Quaternion Internal_FromEulerRad(Vector3 euler)
  {
    Quaternion ret;
    Quaternion.Internal_FromEulerRad_Injected(ref euler, out ret);
    return ret;
  }

  private static Vector3 Internal_ToEulerRad(Quaternion rotation)
  {
    Vector3 ret;
    Quaternion.Internal_ToEulerRad_Injected(ref rotation, out ret);
    return ret;
  }

  private static void Internal_ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
  {
    Quaternion.Internal_ToAxisAngleRad_Injected(ref q, out axis, out angle);
  }

  /// <summary>
  ///   <para>Creates a rotation which rotates angle degrees around axis.</para>
  /// </summary>
  /// <param name="angle"></param>
  /// <param name="axis"></param>
  public static Quaternion AngleAxis(float angle, Vector3 axis)
  {
    Quaternion ret;
    Quaternion.AngleAxis_Injected(angle, ref axis, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Creates a rotation with the specified forward and upwards directions.</para>
  /// </summary>
  /// <param name="forward">The direction to look in.</param>
  /// <param name="upwards">The vector that defines in which direction up is.</param>
  public static Quaternion LookRotation(Vector3 forward, [DefaultValue("Vector3.up")] Vector3 upwards)
  {
    Quaternion ret;
    Quaternion.LookRotation_Injected(ref forward, ref upwards, out ret);
    return ret;
  }

  /// <summary>
  ///   <para>Creates a rotation with the specified forward and upwards directions.</para>
  /// </summary>
  /// <param name="forward">The direction to look in.</param>
  /// <param name="upwards">The vector that defines in which direction up is.</param>
  public static Quaternion LookRotation(Vector3 forward)
  {
    return Quaternion.LookRotation(forward, Vector3.up);
  }

  public float this[int index]
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get
    {
      switch (index)
      {
        case 0:
          return this.x;
        case 1:
          return this.y;
        case 2:
          return this.z;
        case 3:
          return this.w;
        default:
          throw new IndexOutOfRangeException("Invalid Quaternion index!");
      }
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] set
    {
      switch (index)
      {
        case 0:
          this.x = value;
          break;
        case 1:
          this.y = value;
          break;
        case 2:
          this.z = value;
          break;
        case 3:
          this.w = value;
          break;
        default:
          throw new IndexOutOfRangeException("Invalid Quaternion index!");
      }
    }
  }

  /// <summary>
  ///   <para>Set x, y, z and w components of an existing Quaternion.</para>
  /// </summary>
  /// <param name="newX"></param>
  /// <param name="newY"></param>
  /// <param name="newZ"></param>
  /// <param name="newW"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Set(float newX, float newY, float newZ, float newW)
  {
    this.x = newX;
    this.y = newY;
    this.z = newZ;
    this.w = newW;
  }

  /// <summary>
  ///   <para>The identity rotation (Read Only).</para>
  /// </summary>
  public static Quaternion identity
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Quaternion.identityQuaternion;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
  {
    return new Quaternion((float) ((double) lhs.w * (double) rhs.x + (double) lhs.x * (double) rhs.w + (double) lhs.y * (double) rhs.z - (double) lhs.z * (double) rhs.y), (float) ((double) lhs.w * (double) rhs.y + (double) lhs.y * (double) rhs.w + (double) lhs.z * (double) rhs.x - (double) lhs.x * (double) rhs.z), (float) ((double) lhs.w * (double) rhs.z + (double) lhs.z * (double) rhs.w + (double) lhs.x * (double) rhs.y - (double) lhs.y * (double) rhs.x), (float) ((double) lhs.w * (double) rhs.w - (double) lhs.x * (double) rhs.x - (double) lhs.y * (double) rhs.y - (double) lhs.z * (double) rhs.z));
  }

  public static Vector3 operator *(Quaternion rotation, Vector3 point)
  {
    float num1 = rotation.x * 2f;
    float num2 = rotation.y * 2f;
    float num3 = rotation.z * 2f;
    float num4 = rotation.x * num1;
    float num5 = rotation.y * num2;
    float num6 = rotation.z * num3;
    float num7 = rotation.x * num2;
    float num8 = rotation.x * num3;
    float num9 = rotation.y * num3;
    float num10 = rotation.w * num1;
    float num11 = rotation.w * num2;
    float num12 = rotation.w * num3;
    Vector3 vector3;
    vector3.x = (float) ((1.0 - ((double) num5 + (double) num6)) * (double) point.x + ((double) num7 - (double) num12) * (double) point.y + ((double) num8 + (double) num11) * (double) point.z);
    vector3.y = (float) (((double) num7 + (double) num12) * (double) point.x + (1.0 - ((double) num4 + (double) num6)) * (double) point.y + ((double) num9 - (double) num10) * (double) point.z);
    vector3.z = (float) (((double) num8 - (double) num11) * (double) point.x + ((double) num9 + (double) num10) * (double) point.y + (1.0 - ((double) num4 + (double) num5)) * (double) point.z);
    return vector3;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsEqualUsingDot(float dot) => (double) dot > 0.9999989867210388;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator ==(Quaternion lhs, Quaternion rhs)
  {
    return Quaternion.IsEqualUsingDot(Quaternion.Dot(lhs, rhs));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static bool operator !=(Quaternion lhs, Quaternion rhs) => !(lhs == rhs);

  /// <summary>
  ///   <para>The dot product between two rotations.</para>
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Dot(Quaternion a, Quaternion b)
  {
    return (float) ((double) a.x * (double) b.x + (double) a.y * (double) b.y + (double) a.z * (double) b.z + (double) a.w * (double) b.w);
  }

  /// <summary>
  ///   <para>Creates a rotation with the specified forward and upwards directions.</para>
  /// </summary>
  /// <param name="view">The direction to look in.</param>
  /// <param name="up">The vector that defines in which direction up is.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetLookRotation(Vector3 view)
  {
    Vector3 up = Vector3.up;
    this.SetLookRotation(view, up);
  }

  /// <summary>
  ///   <para>Creates a rotation with the specified forward and upwards directions.</para>
  /// </summary>
  /// <param name="view">The direction to look in.</param>
  /// <param name="up">The vector that defines in which direction up is.</param>
  public void SetLookRotation(Vector3 view, [DefaultValue("Vector3.up")] Vector3 up)
  {
    this = Quaternion.LookRotation(view, up);
  }

  /// <summary>
  ///   <para>Returns the angle in degrees between two rotations a and b. The resulting angle ranges from 0 to 180.</para>
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static float Angle(Quaternion a, Quaternion b)
  {
    float num = Mathf.Min(Mathf.Abs(Quaternion.Dot(a, b)), 1f);
    return Quaternion.IsEqualUsingDot(num) ? 0.0f : (float) ((double) Mathf.Acos(num) * 2.0 * 57.295780181884766);
  }

  private static Vector3 Internal_MakePositive(Vector3 euler)
  {
    float num1 = -9f / (500f * (float)Math.PI);
    float num2 = 360f + num1;
    if ((double) euler.x < (double) num1)
      euler.x += 360f;
    else if ((double) euler.x > (double) num2)
      euler.x -= 360f;
    if ((double) euler.y < (double) num1)
      euler.y += 360f;
    else if ((double) euler.y > (double) num2)
      euler.y -= 360f;
    if ((double) euler.z < (double) num1)
      euler.z += 360f;
    else if ((double) euler.z > (double) num2)
      euler.z -= 360f;
    return euler;
  }

  /// <summary>
  ///   <para>Returns or sets the euler angle representation of the rotation in degrees.</para>
  /// </summary>
  public Vector3 eulerAngles
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get
    {
      return Quaternion.Internal_MakePositive(Quaternion.Internal_ToEulerRad(this) * 57.29578f);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)] set
    {
      this = Quaternion.Internal_FromEulerRad(value * ((float) Math.PI / 180f));
    }
  }

  /// <summary>
  ///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis; applied in that order.</para>
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  /// <param name="z"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Euler(float x, float y, float z)
  {
    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z) * ((float) Math.PI / 180f));
  }

  /// <summary>
  ///   <para>Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis.</para>
  /// </summary>
  /// <param name="euler"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Euler(Vector3 euler)
  {
    return Quaternion.Internal_FromEulerRad(euler * ((float) Math.PI / 180f));
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ToAngleAxis(out float angle, out Vector3 axis)
  {
    Quaternion.Internal_ToAxisAngleRad(this, out axis, out angle);
    angle *= 57.29578f;
  }

  /// <summary>
  ///   <para>Creates a rotation which rotates from fromDirection to toDirection.</para>
  /// </summary>
  /// <param name="fromDirection"></param>
  /// <param name="toDirection"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection)
  {
    this = Quaternion.FromToRotation(fromDirection, toDirection);
  }

  /// <summary>
  ///   <para>Rotates a rotation from towards to.</para>
  /// </summary>
  /// <param name="from">The unit quaternion to be aligned with to.</param>
  /// <param name="to">The target unit quaternion.</param>
  /// <param name="maxDegreesDelta">The maximum angle in degrees allowed for this rotation.</param>
  /// <returns>
  ///   <para>A unit quaternion rotated towards to by an angular step of maxDegreesDelta.</para>
  /// </returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
  {
    float num = Quaternion.Angle(from, to);
    return (double) num == 0.0 ? to : Quaternion.SlerpUnclamped(from, to, Mathf.Min(1f, maxDegreesDelta / num));
  }

  /// <summary>
  ///   <para>Converts this quaternion to a quaternion with the same orientation but with a magnitude of 1.0.</para>
  /// </summary>
  /// <param name="q"></param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion Normalize(Quaternion q)
  {
    float num = Mathf.Sqrt(Quaternion.Dot(q, q));
    return (double) num < (double) Mathf.Epsilon ? Quaternion.identity : new Quaternion(q.x / num, q.y / num, q.z / num, q.w / num);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Normalize() => this = Quaternion.Normalize(this);

  /// <summary>
  ///   <para>Returns this quaternion with a magnitude of 1 (Read Only).</para>
  /// </summary>
  public Quaternion normalized
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Quaternion.Normalize(this);
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override int GetHashCode()
  {
    return this.x.GetHashCode() ^ this.y.GetHashCode() << 2 ^ this.z.GetHashCode() >> 2 ^ this.w.GetHashCode() >> 1;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override bool Equals(object other) => other is Quaternion other1 && this.Equals(other1);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public bool Equals(Quaternion other)
  {
    return this.x.Equals(other.x) && this.y.Equals(other.y) && this.z.Equals(other.z) && this.w.Equals(other.w);
  }

  /// <summary>
  ///   <para>Returns a formatted string for this quaternion.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => this.ToString((string) null, (IFormatProvider) null);

  /// <summary>
  ///   <para>Returns a formatted string for this quaternion.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToString(string format) => this.ToString(format, (IFormatProvider) null);

  /// <summary>
  ///   <para>Returns a formatted string for this quaternion.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToString(string format, IFormatProvider formatProvider)
  {
    if (string.IsNullOrEmpty(format))
      format = "F5";
    if (formatProvider == null)
      formatProvider = (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat;
    return
        $"({(object)this.x.ToString(format, formatProvider)}, {(object)this.y.ToString(format, formatProvider)}, {(object)this.z.ToString(format, formatProvider)}, {(object)this.w.ToString(format, formatProvider)})";
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion EulerRotation(float x, float y, float z)
  {
    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion EulerRotation(Vector3 euler) => Quaternion.Internal_FromEulerRad(euler);

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetEulerRotation(float x, float y, float z)
  {
    this = Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetEulerRotation(Vector3 euler) => this = Quaternion.Internal_FromEulerRad(euler);

  [Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3 ToEuler() => Quaternion.Internal_ToEulerRad(this);

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion EulerAngles(float x, float y, float z)
  {
    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion EulerAngles(Vector3 euler) => Quaternion.Internal_FromEulerRad(euler);

  [Obsolete("Use Quaternion.ToAngleAxis instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ToAxisAngle(out Vector3 axis, out float angle)
  {
    Quaternion.Internal_ToAxisAngleRad(this, out axis, out angle);
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetEulerAngles(float x, float y, float z)
  {
    this.SetEulerRotation(new Vector3(x, y, z));
  }

  [Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetEulerAngles(Vector3 euler) => this = Quaternion.EulerRotation(euler);

  [Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Vector3 ToEulerAngles(Quaternion rotation)
  {
    return Quaternion.Internal_ToEulerRad(rotation);
  }

  [Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public Vector3 ToEulerAngles() => Quaternion.Internal_ToEulerRad(this);

  [Obsolete("Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees.")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void SetAxisAngle(Vector3 axis, float angle) => this = Quaternion.AxisAngle(axis, angle);

  [Obsolete("Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public static Quaternion AxisAngle(Vector3 axis, float angle)
  {
    return Quaternion.AngleAxis(57.29578f * angle, axis);
  }

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void FromToRotation_Injected(
    ref Vector3 fromDirection,
    ref Vector3 toDirection,
    out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Inverse_Injected(ref Quaternion rotation, out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Slerp_Injected(
    ref Quaternion a,
    ref Quaternion b,
    float t,
    out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void SlerpUnclamped_Injected(
    ref Quaternion a,
    ref Quaternion b,
    float t,
    out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Lerp_Injected(
    ref Quaternion a,
    ref Quaternion b,
    float t,
    out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void LerpUnclamped_Injected(
    ref Quaternion a,
    ref Quaternion b,
    float t,
    out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Internal_FromEulerRad_Injected(ref Vector3 euler, out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Internal_ToEulerRad_Injected(ref Quaternion rotation, out Vector3 ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void Internal_ToAxisAngleRad_Injected(
    ref Quaternion q,
    out Vector3 axis,
    out float angle);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void AngleAxis_Injected(float angle, ref Vector3 axis, out Quaternion ret);

  [MethodImpl(MethodImplOptions.InternalCall)]
  private static extern void LookRotation_Injected(
    ref Vector3 forward,
    [DefaultValue("Vector3.up")] ref Vector3 upwards,
    out Quaternion ret);
}
    
    
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32(byte r, byte g, byte b, byte a) : IFormattable
{
  [FieldOffset(0)]
  private int rgba = 0;
  /// <summary>
  ///   <para>Red component of the color.</para>
  /// </summary>
  [FieldOffset(0)]
  public byte r = r;
  /// <summary>
  ///   <para>Green component of the color.</para>
  /// </summary>
  [FieldOffset(1)]
  public byte g = g;
  /// <summary>
  ///   <para>Blue component of the color.</para>
  /// </summary>
  [FieldOffset(2)]
  public byte b = b;
  /// <summary>
  ///   <para>Alpha component of the color.</para>
  /// </summary>
  [FieldOffset(3)]
  public byte a = a;

  public static implicit operator Color32(Color c)
  {
    return new Color32((byte) Mathf.Round(Mathf.Clamp01(c.r) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.g) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.b) * (float) byte.MaxValue), (byte) Mathf.Round(Mathf.Clamp01(c.a) * (float) byte.MaxValue));
  }

  public static implicit operator Color(Color32 c)
  {
    return new Color((float) c.r / (float) byte.MaxValue, (float) c.g / (float) byte.MaxValue, (float) c.b / (float) byte.MaxValue, (float) c.a / (float) byte.MaxValue);
  }

  /// <summary>
  ///   <para>Linearly interpolates between colors a and b by t.</para>
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <param name="t"></param>
  public static Color32 Lerp(Color32 a, Color32 b, float t)
  {
    t = Mathf.Clamp01(t);
    return new Color32((byte) ((double) a.r + (double) ((int) b.r - (int) a.r) * (double) t), (byte) ((double) a.g + (double) ((int) b.g - (int) a.g) * (double) t), (byte) ((double) a.b + (double) ((int) b.b - (int) a.b) * (double) t), (byte) ((double) a.a + (double) ((int) b.a - (int) a.a) * (double) t));
  }

  /// <summary>
  ///   <para>Linearly interpolates between colors a and b by t.</para>
  /// </summary>
  /// <param name="a"></param>
  /// <param name="b"></param>
  /// <param name="t"></param>
  public static Color32 LerpUnclamped(Color32 a, Color32 b, float t)
  {
    return new Color32((byte) ((double) a.r + (double) ((int) b.r - (int) a.r) * (double) t), (byte) ((double) a.g + (double) ((int) b.g - (int) a.g) * (double) t), (byte) ((double) a.b + (double) ((int) b.b - (int) a.b) * (double) t), (byte) ((double) a.a + (double) ((int) b.a - (int) a.a) * (double) t));
  }

  public byte this[int index]
  {
    get
    {
      switch (index)
      {
        case 0:
          return this.r;
        case 1:
          return this.g;
        case 2:
          return this.b;
        case 3:
          return this.a;
        default:
          throw new IndexOutOfRangeException($"Invalid Color32 index({index.ToString()})!");
      }
    }
    set
    {
      switch (index)
      {
        case 0:
          this.r = value;
          break;
        case 1:
          this.g = value;
          break;
        case 2:
          this.b = value;
          break;
        case 3:
          this.a = value;
          break;
        default:
          throw new IndexOutOfRangeException($"Invalid Color32 index({index.ToString()})!");
      }
    }
  }

  internal bool InternalEquals(Color32 other) => this.rgba == other.rgba;

  /// <summary>
  ///   <para>Returns a formatted string for this color.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public override string ToString() => this.ToString((string) null, (IFormatProvider) null);

  /// <summary>
  ///   <para>Returns a formatted string for this color.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToString(string format) => this.ToString(format, (IFormatProvider) null);

  /// <summary>
  ///   <para>Returns a formatted string for this color.</para>
  /// </summary>
  /// <param name="format">A numeric format string.</param>
  /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToString(string format, IFormatProvider formatProvider)
  {
    if (formatProvider == null)
      formatProvider = (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat;
    return
        $"RGBA({(object)this.r.ToString(format, formatProvider)}, {(object)this.g.ToString(format, formatProvider)}, {(object)this.b.ToString(format, formatProvider)}, {(object)this.a.ToString(format, formatProvider)})";
  }
}
}

namespace UnityEngine.Assertions
{
    public static class Assert
    {
        public static void AreEqual(object a, object b)
        {
            
        }

        public static void IsTrue(bool _)
        {
        }
    }    
}

public struct TimeUntil
{
    float time;

    public static implicit operator float( TimeUntil ts )
    {
        return ts.time;
    }

    public static implicit operator TimeUntil( float ts )
    {
        return new TimeUntil { time = ts };
    }
}



namespace ProtoBuf
{
    public partial struct Half3 : IEquatable<Half3>
    {
        public bool Equals( Half3 other )
        {
            return x == other.x && y == other.y && z == other.z;
        }

        public override bool Equals( object obj )
        {
            if (obj is Half3)
            {
                return Equals( (Half3)obj );
            }
            return false;
        }

        public static bool operator ==( Half3 a, Half3 b )
        {
            return a.Equals( b );
        }

        public static bool operator !=( Half3 a, Half3 b )
        {
            return !a.Equals( b );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( x, y, z );
        }
    }
}

namespace Facepunch.Nexus
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Uuid : IEquatable<Uuid>
    {
        public static readonly Uuid Empty = default;

        public int NodeId { get; set; }
        public int Sequence { get; set; }
        public ulong Timestamp { get; set; }

        public Uuid(int nodeId, int sequence, ulong timestamp)
        {
            NodeId = nodeId;
            Sequence = sequence;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            // 01234567-01234567-0123DCBA0123456789ABCDEF
            return $"{NodeId:X8}{Sequence:X8}{Timestamp:X16}";
        }

        public static implicit operator Uuid(Guid guid)
        {
            return Unsafe.As<Guid, Uuid>(ref guid);
        }

        public static implicit operator Guid(Uuid uuid)
        {
            return Unsafe.As<Uuid, Guid>(ref uuid);
        }

        #region IEquatable

        public bool Equals(Uuid other)
        {
            return NodeId == other.NodeId && Sequence == other.Sequence && Timestamp == other.Timestamp;
        }

        public override bool Equals(object obj)
        {
            return obj is Uuid other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = NodeId;
                hashCode = (hashCode * 397) ^ Sequence;
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Uuid left, Uuid right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Uuid left, Uuid right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Generator

        private static readonly object _syncRoot;
        private static readonly int _nodeId;
        private static int _sequence;
        private static ulong _previousTimestamp;

        static Uuid()
        {
            if (Marshal.SizeOf<Uuid>() != Marshal.SizeOf<Guid>())
            {
                throw new Exception("sizeof(Uuid) != sizeof(Guid)");
            }

            _syncRoot = new object();
            _nodeId = Environment.MachineName.GetHashCode();
            _sequence = Environment.TickCount;
        }

        public static Uuid Generate()
        {
            lock (_syncRoot)
            {
                var timestamp = (ulong)DateTime.UtcNow.Ticks;

                // if the timestamp went back or hasn't changed since last time we'll break the tie by incrementing the sequence
                if (timestamp <= _previousTimestamp)
                {
                    _sequence++;
                }

                _previousTimestamp = timestamp;

                return new Uuid(_nodeId, _sequence, timestamp);
            }
        }

        #endregion
    }
}

namespace UnityEngine.Profiling
{
    public class Profiler
    {
        public static void BeginSample(string name)
        {
            
        }

        public static void EndSample()
        {
            
        }
    }
}


