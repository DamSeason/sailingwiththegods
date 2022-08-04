using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CityBuildingSpawner : MonoBehaviour
{
	[SerializeField] int NumHouses = 30;
	[SerializeField] float CityRadius = 1f;
	[SerializeField] float BuildingRadius = 0.05f;
	[SerializeField] float DockOffset = -0.3f;
	[SerializeField] float Scale = 0.75f;

	[Serializable]
	class BuildingOption
	{
		public GameObject Prefab = null;
		public float Weight = 0;
	}

	[SerializeField] BuildingOption[] HousePrefabs = null;
	[SerializeField] GameObject DockPrefab = null;

	// fallback to slower FindObjectByType for editor usage
	World World => Globals.World
		?? GameObject.FindObjectOfType<World>();

	// Start is called before the first frame update
	void Start() {
		Regenerate();
		PlaceDock();
	}

	[Button]
	void Regenerate() {
		Clear();

		for (var i = 0; i < NumHouses; i++) {
			var house = GameObject.Instantiate(
				HousePrefabs.WeightedRandomElement(HousePrefabs.Select(h => h.Weight)).Prefab,
				GetBestRandomPosition(),
				Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0),
				transform
			);

			house.transform.localScale = Vector3.one * Scale;
		}

		// kill buildings that are in the water. we don't try to replace them since it'd probably make things too dense
		foreach (var building in transform.GetChildren().ToList()) {
			if (World.IsBelowWaterLevel(building.position)) {
				SafeDestroy(building.gameObject);
			}
		}
	}

	(Vector3 pos, Vector3 dir) GetDockPos(Vector3 cityPos) {
		var pos = World.GetNearestPosInWater(cityPos);
		var diffToWater = pos - cityPos;
		var dirToWater = diffToWater.normalized;
		var dir = Vector3.ProjectOnPlane(-dirToWater, Vector3.up);
		var adjustedPos = pos + dir * -DockOffset;

		return (adjustedPos, dir);
	}

	[Button]
	void PlaceDock() {
		const int maxTries = 100;
		var buildingMask = LayerMask.GetMask("cityBuilding");
		var (basePos, dir) = GetDockPos(transform.position);

		Vector3 bestPos = basePos;
		for (var i = 0; i < maxTries; i++) {
			var randomPos = Utils.RandomPositionIn(new Bounds(basePos, new Vector3(1, 0, 1)));

			if (!Physics.OverlapSphere(randomPos, BuildingRadius, buildingMask).Any()) {
				bestPos = randomPos;
				break;
			}
		}

		GameObject.Instantiate(
			DockPrefab,
			bestPos,
			Quaternion.LookRotation(dir, Vector3.up),
			transform
		);
	}

	Vector3 GetRandomPositionOption() {
		var pos = Utils.RandomPositionIn(new Bounds(transform.position, Vector3.one * CityRadius));
		return World.GetPosOnLand(pos);
	}

	Vector3 GetBestRandomPosition() {
		const int maxTries = 100;
		var buildingMask = LayerMask.GetMask("cityBuilding");

		Vector3 pos = GetRandomPositionOption();
		for (var i = 0; i < maxTries; i++) {
			if (!Physics.OverlapSphere(pos, BuildingRadius, buildingMask).Any()) {
				return pos;
			}

			pos = GetRandomPositionOption();
		}

		// use the last pick if we couldn't find a non-overlapping option
		return pos;
	}

	[Button]
	void Clear() {
		foreach (var child in transform.GetChildren().ToList()) {
			SafeDestroy(child.gameObject);
		}
	}

	void SafeDestroy(GameObject obj) {
		if (Application.isPlaying) {
			Destroy(obj);
		}
		else {
			DestroyImmediate(obj);
		}
	}
}
