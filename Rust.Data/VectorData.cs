using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProtoBuf
{
	public partial struct VectorData : IEquatable<VectorData>
	{
		public VectorData(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static implicit operator VectorData(Vector3 v)
		{
			return new VectorData(v.x, v.y, v.z);
		}

		public static implicit operator VectorData(Quaternion q)
		{
			return q.eulerAngles;
		}

		public static implicit operator Vector3(VectorData v)
		{
			return new Vector3(v.x, v.y, v.z);
		}

		public static implicit operator Quaternion(VectorData v)
		{
			return Quaternion.Euler(v);
		}

		public bool Equals(VectorData other)
		{
            return x == other.x && y == other.y && z == other.z;
        }

		public override bool Equals(object obj)
		{
            if (obj is VectorData)
			{
                return Equals((VectorData)obj);
            }
            return false;
        }

		public static bool operator ==(VectorData a, VectorData b )
		{
			return a.Equals(b);
		}

		public static bool operator !=(VectorData a, VectorData b )
		{
            return !a.Equals(b);
        }

		public override int GetHashCode()
		{
            return HashCode.Combine(x, y, z);
        }
	}
}
