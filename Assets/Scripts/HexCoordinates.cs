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
		// if (HexMetrics.WrappingX)
		// {
		// 	if (HexMetrics.WrappingZ)
		// 	{
		// 		int xyz =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(z < other.z ? other.z - z : z - other.z) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		other.x += HexMetrics.wrapSizeX;
		// 		other.z += HexMetrics.wrapSizeZ;

		// 		int xyzWrapped =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(z < other.z ? other.z - z : z - other.z) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		if (xyzWrapped < xyz)
		// 		{
		// 			return xyzWrapped / 2;
		// 		}

		// 		other.x -= 2 * HexMetrics.wrapSizeX;

		// 		xyzWrapped =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(z < other.z ? other.z - z : z - other.z) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		if (xyzWrapped < xyz)
		// 		{
		// 			return xyzWrapped / 2;
		// 		}

		// 		other.x += 2 * HexMetrics.wrapSizeX;
		// 		other.z -= 2 * HexMetrics.wrapSizeZ;

		// 		xyzWrapped =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(z < other.z ? other.z - z : z - other.z) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		if (xyzWrapped < xyz)
		// 		{
		// 			return xyzWrapped / 2;
		// 		}

		// 		return xyz / 2;
		// 	}
		// 	else
		// 	{
		// 		int xy =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		other.x += HexMetrics.wrapSizeX;

		// 		int xyWrapped =
		// 			(x < other.x ? other.x - x : x - other.x) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		if (xyWrapped < xy)
		// 		{
		// 			xy = xyWrapped;
		// 		}
		// 		else
		// 		{
		// 			other.x -= 2 * HexMetrics.wrapSizeX;

		// 			xyWrapped =
		// 				(x < other.x ? other.x - x : x - other.x) +
		// 				(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 			if (xyWrapped < xy)
		// 			{
		// 				xy = xyWrapped;
		// 			}
		// 		}

		// 		return (xy + (z < other.z ? other.z - z : z - other.z)) / 2;
		// 	}
		// }

		// if (HexMetrics.WrappingZ)
		// {
		// 	int yz =
		// 		(z < other.z ? other.z - z : z - other.z) +
		// 		(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 	other.z += HexMetrics.wrapSizeZ;

		// 	int yzWrapped =
		// 		(z < other.z ? other.z - z : z - other.z) +
		// 		(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 	if (yzWrapped < yz)
		// 	{
		// 		yz = yzWrapped;
		// 	}
		// 	else
		// 	{
		// 		other.z -= 2 * HexMetrics.wrapSizeZ;
		// 		yzWrapped =
		// 			(z < other.z ? other.z - z : z - other.z) +
		// 			(Y < other.Y ? other.Y - Y : Y - other.Y);

		// 		if (yzWrapped < yz)
		// 		{
		// 			yz = yzWrapped;
		// 		}
		// 	}

		// 	return (yz + (x < other.x ? other.x - x : x - other.x)) / 2;
		// }

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
				return (xyWrapped + (z < other.z ? other.z - z : z - other.z)) / 2;
			}
			else
			{
				other.x -= 2 * HexMetrics.wrapSizeX;
				xyWrapped =
					(x < other.x ? other.x - x : x - other.x) +
					(Y < other.Y ? other.Y - Y : Y - other.Y);
				if (xyWrapped < xy)
				{
					return (xyWrapped + (z < other.z ? other.z - z : z - other.z)) / 2;
				}
			}
		}
		return (xy + (z < other.z ? other.z - z : z - other.z)) / 2;
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