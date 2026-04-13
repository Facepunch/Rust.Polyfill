using UnityEngine;

public class BitUtility
{
	private const float float2byte = 255;
	private const float byte2float = 1f / float2byte;

	public static byte Float2Byte(float f)
	{
		Union32 u = new Union32();
		u.f = f;
		u.b1 = 0;
		return (byte)(u.f * float2byte + 0.5f);
	}

	public static float Byte2Float(int b)
	{
		return b * byte2float;
	}

	// No idea why we're subtracting one here, but changing it now would fuck everything up
	private const float float2short = short.MaxValue - 1;
	private const float short2float = 1f / float2short;

    public static short Float2Short(float f)
    {
        return (short)(f * float2short + 0.5f);
    }

    public static float Short2Float(int b)
    {
        return b * short2float;
    }

    private const float float2ushort = ushort.MaxValue - 1;
    private const float ushort2float = 1f / float2ushort;

	public static ushort Float2UShort(float f)
    {
        return (ushort)(f * float2ushort + 0.5f);
    }

    public static float UShort2Float(ushort s)
    {
        return s * ushort2float;
    }

	public static Color32 EncodeFloat(float f)
	{
		Union32 u = new Union32();
		u.f = f;
		return new Color32(u.b1, u.b2, u.b3, u.b4);
	}

	public static float DecodeFloat(Color32 c)
	{
		Union32 u = new Union32();
		u.b1 = c.r;
		u.b2 = c.g;
		u.b3 = c.b;
		u.b4 = c.a;
		return u.f;
	}

	public static Color32 EncodeInt(int i)
	{
		Union32 u = new Union32();
		u.i = i;
		return new Color32(u.b1, u.b2, u.b3, u.b4);
	}

	public static int DecodeInt(Color32 c)
	{
		Union32 u = new Union32();
		u.b1 = c.r;
		u.b2 = c.g;
		u.b3 = c.b;
		u.b4 = c.a;
		return u.i;
	}

	public static Color32 EncodeShort(short i)
	{
		Union16 u = new Union16();
		u.i = i;
		return new Color32(u.b1, 0, u.b2, 1);
	}

	public static short DecodeShort(Color32 c)
	{
		Union16 u = new Union16();
		u.b1 = c.r;
		u.b2 = c.b;
		return u.i;
	}

    public static Color32 EncodeUShort(ushort u)
    {
        Union16 ret = new Union16();
        ret.u = u;
        return new Color32(ret.b1, 0, ret.b2, 1);
    }

    public static ushort DecodeUShort(Color32 c)
    {
        Union16 u = new Union16();
        u.b1 = c.r;
        u.b2 = c.b;
        return u.u;
    }

	// Encode normal (result is in tangent space)
	public static Color EncodeNormal(Vector3 n)
	{
		n = (n + Vector3.one) * 0.5f; // [0, 1]
		return new Color(n.z, n.z, n.z, n.x);
	}

	// Decode normal (result is in world space)
	public static Vector3 DecodeNormal(Color c)
	{
		float nx = c.a * 2f - 1f;
		float nz = c.g * 2f - 1f;
		float ny = Mathf.Sqrt(1f - Mathf.Clamp01( nx * nx + nz * nz ));
		return new Vector3(nx, ny, nz);
	}

	public static Color32 EncodeVector(Vector4 v)
	{
		return new Color32(Float2Byte(v.x), Float2Byte(v.y), Float2Byte(v.z), Float2Byte(v.w));
	}

	public static Vector4 DecodeVector(Color32 c)
	{
		return new Vector4(Byte2Float(c.r), Byte2Float(c.g), Byte2Float(c.b), Byte2Float(c.a));
	}

	public static Color32 EncodeVector2i(Vector2i v)
	{
		var r = (byte)Mathf.Clamp(+v.x, 0, 255);
		var g = (byte)Mathf.Clamp(-v.x, 0, 255);
		var b = (byte)Mathf.Clamp(+v.y, 0, 255);
		var a = (byte)Mathf.Clamp(-v.y, 0, 255);

		return new Color32(r, g, b, a);
	}

	public static Vector2i DecodeVector2i(Color32 c)
	{
		return new Vector2i(c.r - c.g, c.b - c.a);
	}
}
