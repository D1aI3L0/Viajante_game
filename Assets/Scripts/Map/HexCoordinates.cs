using UnityEngine;
using System.IO;

[System.Serializable]
public struct HexCoordinates
{

	[SerializeField]
	private int x, z;

	public int X
	{
		get { return x; }
	}

	public int Z
	{
		get { return z; }
	}

	public int Y
	{
		get { return -X - Z; }
	}

	public HexCoordinates(int x, int z)
	{
		if (HexMetrics.WrappingX)
		{
			int oX = x + z / 2;
			if (oX < 0)
			{
				x += HexMetrics.wrapSizeX;
			}
			else if (oX >= HexMetrics.wrapSizeX)
			{
				x -= HexMetrics.wrapSizeX;
			}
		}
		this.x = x;
		this.z = z;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		return new HexCoordinates(x - z / 2, z);
	}

	public override string ToString()
	{
		return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
	}

	public string ToStringOnSeparateLines()
	{
		return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
	}

	public static HexCoordinates FromPosition(Vector3 position)
	{
		float x = position.x / HexMetrics.innerDiameter;
		float y = -x;
		float offset = position.z / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x - y);

		if (iX + iY + iZ != 0)
		{
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x - y - iZ);

			if (dX > dY && dX > dZ)
			{
				iX = -iY - iZ;
			}
			else if (dZ > dY)
			{
				iZ = -iX - iY;
			}
		}

		return new HexCoordinates(iX, iZ);
	}

	public int DistanceTo(HexCoordinates other)
	{
		int xy =
			(x < other.x ? other.x - x : x - other.x) +
			(Y < other.Y ? other.Y - Y : Y - other.Y);

		if (HexMetrics.WrappingX)
		{
			other.x += HexMetrics.wrapSizeX;
			int xyWrapped =
				(x < other.x ? other.x - x : x - other.x) +
				(Y < other.Y ? other.Y - Y : Y - other.Y);
			if (xyWrapped < xy)
			{
				xy = xyWrapped;
			}
			else
			{
				other.x -= 2 * HexMetrics.wrapSizeX;
				xyWrapped =
					(x < other.x ? other.x - x : x - other.x) +
					(Y < other.Y ? other.Y - Y : Y - other.Y);
				if (xyWrapped < xy)
				{
					xy = xyWrapped;
				}
			}
		}

		int xyz = xy + (z < other.z ? other.z - z : z - other.z);

		if (HexMetrics.WrappingZ)
		{
			other.z += HexMetrics.wrapSizeZ;
			int xyzWrapped = xy + (z < other.z ? other.z - z : z - other.z);
			if (xyzWrapped < xyz)
			{
				xyz = xyzWrapped;
			}
			else
			{
				other.z -= 2 * HexMetrics.wrapSizeZ;
				xyzWrapped = xy + (z < other.z ? other.z - z : z - other.z);

				if (xyzWrapped < xyz)
				{
					xyz = xyzWrapped;
				}
			}
		}

		return xyz / 2;
	}

	public void Save(BinaryWriter writer)
	{
		writer.Write(x);
		writer.Write(z);
	}

	public static HexCoordinates Load(BinaryReader reader)
	{
		HexCoordinates c;
		c.x = reader.ReadInt32();
		c.z = reader.ReadInt32();
		return c;
	}
}