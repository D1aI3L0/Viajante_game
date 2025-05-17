using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public abstract class Unit : MonoBehaviour
{
	public HexGrid Grid { get; set; }
	protected int maxStamina = 24;
	protected int stamina = 24;
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
	public virtual HexCell Location
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
				Grid.IncreaseVisibility(value, VisionRange);
				location.Unit = null;
			}
			if (!value)
				return;
			value.Unit = this;
			location = value;
			transform.localPosition = value.Position;
			Grid.MakeChildOfChunk(transform, value.ColumnIndex, value.LineIndex);
		}
	}

	protected float orientation;
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

	protected virtual void OnEnable()
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

	public virtual bool IsValidDestination(HexCell cell)
	{
		return cell.IsExplored && !cell.IsUnderwater;
	}

	public virtual bool IsValidMove(HexCell cell)
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

	public virtual void Die()
	{
		location.Unit = null;
		Destroy(gameObject);
	}

	public int GetMoveCost(HexCell fromCell, HexDirection direction)
	{
		HexCell toCell = fromCell.GetNeighbor(direction);
		HexEdgeType edgeType = fromCell.GetEdgeType(toCell);
		if (edgeType == HexEdgeType.Cliff)
			return -1;

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

	public virtual void ResetStamina()
	{
		Stamina = maxStamina;
	}
	//============================================================================================================
	//                                       Движение по пути и анимация
	//============================================================================================================
	protected List<HexCell> pathToTravel;
	private const float travelSpeed = 3f;
	private const float rotationSpeed = 180f;

	public virtual void Travel(List<HexCell> path)
	{
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath_v2());
	}

	protected IEnumerator TravelPath_v2()
	{
		//GameUI.Locked = true;
		bool movementAborted = false;

		for (int i = 0; i < pathToTravel.Count - 1; i++)
		{
			HexCell fromCell = pathToTravel[i];
			HexCell toCell = pathToTravel[i + 1];

			if (toCell.HasUnit && toCell.Unit is Squad squad && squad.squadType == SquadType.Enemy)
			{
				BattleConfirmationUI.Instance.Show(squad, confirmed =>
				{
					if (confirmed)
					{
						//GlobalMapGameManager.Instance.SetBattleParametres((Squad)this, squad);
						//SceneManager.LoadScene("BattleScene");
						return;
					}
					else
					{
						movementAborted = true;
					}
				});

				while (BattleConfirmationUI.Instance.enabled)
					yield return null;

				if (movementAborted) break;
			}

			//Grid.DecreaseVisibility(fromCell, VisionRange);

			Vector3 startPos = fromCell.Position;
			Vector3 endPos = toCell.Position;

			yield return LookAt(endPos);

			int nextColumn = toCell.ColumnIndex;
			int nextLine = toCell.LineIndex;
			if (fromCell.ColumnIndex != nextColumn)
			{
				if (nextColumn < fromCell.ColumnIndex - 1)
				{
					startPos.x -= HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
					endPos.x -= HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
				}
				else if (nextColumn > fromCell.ColumnIndex + 1)
				{
					startPos.x += HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
					endPos.x += HexMetrics.innerDiameter * HexMetrics.wrapSizeX;
				}
			}
			if (fromCell.LineIndex != nextLine)
			{
				if (nextLine < fromCell.LineIndex - 1)
				{
					startPos.z -= HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
					endPos.z -= HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
				}
				else if (nextLine > fromCell.LineIndex + 1)
				{
					startPos.z += HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
					endPos.z += HexMetrics.outerDiametr * HexMetrics.wrapSizeZ;
				}
			}

			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime * travelSpeed;
				transform.localPosition = Vector3.Lerp(startPos, endPos, t);
				yield return null;
			}

			transform.localPosition = endPos;
			Grid.MakeChildOfChunk(transform, toCell.ColumnIndex, toCell.LineIndex);

			if (fromCell.Unit && fromCell.Unit == this)
				fromCell.Unit = null;

			location = toCell;

			if (!toCell.HasUnit)
				toCell.Unit = this;

			Grid.IncreaseVisibility(toCell, VisionRange);
		}

		Stamina -= location.Distance;

		pathToTravel = null;
		currentTravelLocation = null;
		orientation = transform.localRotation.eulerAngles.y;
		//GameUI.Locked = false;
	}

	protected IEnumerator LookAt(Vector3 point)
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


	public virtual void Save(BinaryWriter writer)
	{
		location.coordinates.Save(writer);
		writer.Write(orientation);
		writer.Write(Stamina);
	}

	public virtual void Load(BinaryReader reader, HexGrid grid)
	{
		Grid = grid;
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		Location = grid.GetCell(coordinates);
		Orientation = reader.ReadSingle();
		Stamina = reader.ReadInt32();
	}
}