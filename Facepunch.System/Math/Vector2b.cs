using UnityEngine;
using System;

[Serializable]
public struct Vector2b : IEquatable<Vector2b>
{
	public static readonly Vector2b alltrue  = new Vector2b(true, true);
	public static readonly Vector2b allfalse = new Vector2b(false, false);

	public bool x;
	public bool y;

	public Vector2b(bool x, bool y)
	{
		this.x = x;
		this.y = y;
	}

	public static bool operator ==(Vector2b a, Vector2b b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(Vector2b a, Vector2b b)
	{
		return !a.Equals(b);
	}

	public bool Equals(Vector2b o)
	{
		return x == o.x && y == o.y;
	}

	public override int GetHashCode()
	{
		return x.GetHashCode() ^ y.GetHashCode();
	}

	public override bool Equals(object o)
	{
		return (o != null) && (o is Vector2b) && Equals((Vector2b)o);
	}

	public override string ToString()
	{
		return string.Format("[{0},{1}]", x, y);
	}
}
