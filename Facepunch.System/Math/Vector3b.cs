using UnityEngine;
using System;

[Serializable]
public struct Vector3b : IEquatable<Vector3b>
{
	public static readonly Vector3b alltrue  = new Vector3b(true, true, true);
	public static readonly Vector3b allfalse = new Vector3b(false, false, false);

	public bool x;
	public bool y;
	public bool z;

	public Vector3b(bool x, bool y, bool z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public static bool operator ==(Vector3b a, Vector3b b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Vector3b a, Vector3b b)
	{
		return !a.Equals(b);
	}

	public bool Equals(Vector3b o)
	{
		return x == o.x && y == o.y && z == o.z;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
	}

	public override bool Equals(object o)
	{
		return (o != null) && (o is Vector3b) && Equals((Vector3b)o);
	}

	public override string ToString()
	{
		return string.Format("[{0},{1},{2}]", x, y, z);
	}
}
