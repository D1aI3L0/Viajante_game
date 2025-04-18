using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public abstract class Unit : MonoBehaviour
{
	public HexGrid Grid { get; set; }
	public int maxStamina = 24;
	public int stamina = 24;
	public int VisionRange
	{
		get
		{
			return 3;
		}
	}

	public int Stamina
	{
		get
		{
			return stamina;
		}
		set
		{
			stamina = value;
		}
	}

	protected HexCell location, currentTravelLocation;
	virtual public HexCell Location
	{
		get
		{
			return location;
		}
		set
		{
			if (location)
			{
				Grid.DecreaseVisibility(location, VisionRange);
				location.Unit = null;
			}
			value.Unit = this;
			location = value;
			Grid.IncreaseVisibility(value, VisionRange);
			transform.localPosition = value.Position;
			Grid.MakeChildOfChunk(transform, value.ColumnIndex, value.LineIndex);
		}
	}

	float orientation;
	public float Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	void OnEnable()
	{
		if (location)
		{
			transform.localPosition = location.Position;
			if (currentTravelLocation)
			{
				Grid.IncreaseVisibility(location, VisionRange);
				Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
				currentTravelLocation = null;
			}
		}
	}

	virtual public bool IsValidDestination(HexCell cell)
	{
		return cell.IsExplored && !cell.IsUnderwater;
	}

	virtual public bool IsValidMove(HexCell cell)
	{
		return cell.IsExplored && !cell.IsUnderwater;
	}

	public static bool UnitValidDestination(HexCell cell)
	{
		return cell.IsExplored && !cell.IsUnderwater;
	}

	public void ValidateLocation()
	{
		transform.localPosition = location.Position;
	}

	virtual public void Die()
	{
		if (location)
		{
			Grid.DecreaseVisibility(location, VisionRange);
		}
		location.Unit = null;
		Destroy(gameObject);
	}

	public void Save(BinaryWriter writer)
	{
		location.coordinates.Save(writer);
		writer.Write(orientation);
	}

	public static void Load(BinaryReader reader, HexGrid grid)
	{
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
	}

	public int GetMoveCost(HexCell fromCell, HexCell toCell, HexDirection direction)
	{
		HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
		if (edgeType == HexEdgeType.Cliff)
		{
			return -1;
		}
		int moveCost;
		if (fromCell.HasRoadThroughEdge(direction))
		{
			moveCost = 1;
		}
		else if (fromCell.Walled != toCell.Walled)
		{
			return -1;
		}
		else
		{
			moveCost = edgeType == HexEdgeType.Flat ? 3 : 6;
			moveCost += toCell.UrbanLevel + toCell.FarmLevel + toCell.PlantLevel;
		}
		return moveCost;
	}

	virtual public void ResetStamina()
	{
		Stamina = maxStamina;
	}
	//============================================================================================================
	//                                       Движение по пути и анимация
	//============================================================================================================
	protected List<HexCell> pathToTravel;
	const float travelSpeed = 3f;
	const float rotationSpeed = 180f;

	virtual public void Travel(List<HexCell> path)
	{
		location.Unit = null;
		location = path[^1];
		location.Unit = this;
		pathToTravel = path;
		Stamina -= path[^1].Distance;
		StopAllCoroutines();
		StartCoroutine(TravelPath());
	}

	protected IEnumerator TravelPath()
	{
		Vector3 a, b, c = pathToTravel[0].Position;
		yield return LookAt(pathToTravel[1].Position);

		if (!currentTravelLocation)
		{
			currentTravelLocation = pathToTravel[0];
		}
		Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
		int currentColumn = currentTravelLocation.ColumnIndex;
		int currentLine = currentTravelLocation.LineIndex;

		float t = Time.deltaTime * travelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++)
		{
			currentTravelLocation = pathToTravel[i];
			a = c;
			b = pathToTravel[i - 1].Position;

			int nextColumn = currentTravelLocation.ColumnIndex;
			int nextLine = currentTravelLocation.LineIndex;
			if (currentColumn != nextColumn)
			{
				if (nextColumn < currentColumn - 1)
				{
					a.x -= HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
					b.x -= HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
				}
				else if (nextColumn > currentColumn + 1)
				{
					a.x += HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
					b.x += HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
				}
				currentColumn = nextColumn;
			}
			if (currentLine != nextLine)
			{
				if (nextLine < currentLine - 1)
				{
					a.z -= HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
					b.z -= HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
				}
				else if (nextLine > currentLine + 1)
				{
					a.z += HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
					b.z += HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
				}
				currentLine = nextLine;
			}

			if (currentColumn == nextColumn || currentLine == nextLine)
				Grid.MakeChildOfChunk(transform, currentColumn, currentLine);

			c = (b + currentTravelLocation.Position) * 0.5f;
			Grid.IncreaseVisibility(pathToTravel[i], VisionRange);

			for (; t < 1f; t += Time.deltaTime * travelSpeed)
			{
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			Grid.DecreaseVisibility(pathToTravel[i], VisionRange);
			t -= 1f;
		}
		currentTravelLocation = null;

		a = c;
		b = location.Position;
		c = b;
		Grid.IncreaseVisibility(location, VisionRange);
		for (; t < 1f; t += Time.deltaTime * travelSpeed)
		{
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}
		transform.localPosition = location.Position;
		orientation = transform.localRotation.eulerAngles.y;
		pathToTravel = null;
	}

	IEnumerator LookAt(Vector3 point)
	{
		if (HexMetrics.WrappingX)
		{
			float xDistance = point.x - transform.localPosition.x;
			if (xDistance < -HexMetrics.innerRadius * HexMetrics.wrapSizeX)
			{
				point.x += HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
			}
			else if (xDistance > HexMetrics.innerRadius * HexMetrics.wrapSizeX)
			{
				point.x -= HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
			}
		}

		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
		float angle = Quaternion.Angle(fromRotation, toRotation);

		if (angle > 0f)
		{
			float speed = rotationSpeed / angle;

			for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
			{
				transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
				yield return null;
			}
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}
	//============================================================================================================
}
