using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StormDeckManager : MonoBehaviour
{
	private List<GameObject> crewObjects = new List<GameObject>();
	private IList<CrewMember> crewRoster = null;
	private float successChance = 0.05f;
	private List<string> outcomeNarrative;

	[Header("Minigame Info")]
	[SerializeField] private MiniGameInfoScreen minigameInfoScreen = null;
	[SerializeField] private string eventTitle = "";
	[SerializeField] private string eventSubtitle = "";
	[SerializeField] private Sprite eventIcon = null;
	[SerializeField] private MinigameInfoSettings minigameInfo = null;
	[SerializeField] private MinigameInfoSettings outcomeInfo = null;

	[Header("Event Components")]
	[SerializeField] private GameObject crewPrefab = null;
	[SerializeField] private List<Transform> spawnLocations = null;
	[SerializeField] private FPSCamera fpsCamera = null;
	[SerializeField] private FPSMovement fpsMovement = null;
	[SerializeField] private GameObject eventUI = null;

	[Header("Event Info Dialog")]
	[TextArea(1, 3)]
	[SerializeField] private string eventStartMessage1 = "";
	[TextArea(1,3)]
	[SerializeField] private string eventStartMessage2 = "";

	[SerializeField] private List<string> malefactorFlavorText = null;
	[SerializeField] private List<string> historicalQuotes = null;

	// ============== DIVINATIONS, RITUALS, AND OTHER DIALOGUE VARIABLES =================

	// Malefactor variables
	private int malefactorNdx;
	private CrewMember malefactor => crewRoster[malefactorNdx];
	private string malefactorType;

	private int astragaliHolderNdx;

	// ===================================================================================

	public static StormDeckManager instance;

	private void Awake() {
		instance = this;
	}

	void Start()
    {
		crewRoster = GetCrewRoster();
		ChooseMalefactor();
		Debug.Log(crewRoster[malefactorNdx].name + " is the malefactor");

		SpawnCrew();
		crewObjects[malefactorNdx].GetComponentInChildren<FPSInteractable>().OnInteract.RemoveAllListeners();
		crewObjects[malefactorNdx].GetComponentInChildren<FPSInteractable>().OnInteract.AddListener(() => SacrificeCrewMember(crewRoster, crewRoster[malefactorNdx]));
		InitYarnFlagResponses();

		StartCoroutine(ShowInfoMenuCoroutine(EnableControls));
		ShowEventInfoDialog();
	}

	private IEnumerator ShowInfoMenuCoroutine(params UnityAction[] callbacks) {
		DisableControls();
		HideUI();
		minigameInfoScreen.gameObject.SetActive(true);
		while (minigameInfoScreen.gameObject.activeInHierarchy) {
			yield return new WaitForEndOfFrame();
		}
		ShowUI();
		foreach (UnityAction callback in callbacks) {
			callback.Invoke();
		}
	}

	private string GenerateEventStartDialog() {
		string result = "";
		result += eventStartMessage1;
		foreach (string text in GetMalefactorFlavorTexts(2)) {
			result += " " + text;
		}
		result += eventStartMessage2;
		result += GetHistoricalQuote();
		return result;
	}

	private void ShowEventInfoDialog() {
		string eventStartDialog = GenerateEventStartDialog();
		minigameInfoScreen.DisplayText(minigameInfo.title, minigameInfo.subtitle, eventStartDialog, minigameInfo.icon, minigameInfo.type);
	}

	private List<string> GetMalefactorFlavorTexts(int count = 1) {
		if (count > malefactorFlavorText.Count || count < 0) {
			throw new System.ArgumentOutOfRangeException("Input 'count' must be nonnegative and cannot exceed number of available malefactor flavor text entries.");
		}

		List<string> copy = new List<string>(malefactorFlavorText);
		List<string> output = new List<string>();
		for (int i = 0; i < count; i++) {
			int ndx = Random.Range(0, copy.Count);
			output.Add(copy[ndx]);
			copy.RemoveAt(ndx);
		}
		return output;
	}

	private string GetHistoricalQuote() {
		if (historicalQuotes.Count == 0) return "";
		int ndx = Random.Range(0, historicalQuotes.Count);
		return historicalQuotes[ndx];
	}

	private void SacrificeCrewMember(IList<CrewMember> crewList, CrewMember c) {
		Destroy(crewObjects[crewList.IndexOf(c)]);
		crewList.Remove(c);
		DialogueManager.instance.DisplayNotification(c.name + " has been thrown overboard.");
	}

	private IList<CrewMember> GetCrewRoster() {
		IList<CrewMember> cr = Globals.Game?.Session?.playerShipVariables?.ship?.crewRoster;
		if (cr == null) {
			Debug.LogWarning("Failed to find existing crew roster. Generating dummy roster.");
			cr = GenerateDebugCrew();
		}
		return cr;
	}

	private IList<CrewMember> GenerateDebugCrew() {
		string[] names = { "Seercules", "Harold", "Paul", "Michael", "John" };
		CrewType[] types = { CrewType.Seer, CrewType.Warrior, CrewType.Sailor, CrewType.Navigator, CrewType.Lawyer };

		IList<CrewMember> list = new Ship.CrewRosterList();
		for (int i = 0; i < names.Length; i++) {
			list.Add(new CrewMember(i, names[i], 0, 0, types[i], "", true, false, false, null));
		}
		return list;
	}

	private void SpawnCrew() {
		int ndx = 0;
		foreach (CrewMember c in crewRoster) {
			if (ndx >= spawnLocations.Count) {
				Debug.LogError("Attempted to spawn more crew than the number of spawn locations.");
				return;
			}
			GameObject crewObj = Instantiate(crewPrefab, transform); // instantiating new game objects in scene root causes bugs when minigame exits
			crewObj.transform.position = spawnLocations[ndx].position;
			crewObj.GetComponent<DummyCrewmate>().SetNameAndType(c.name, c.typeOfCrew);
			crewObjects.Add(crewObj);
			ndx++;
		}
	}

	public CrewMember CrewObjectToCrewMember(GameObject crewObject) {
		return crewRoster[crewObjects.IndexOf(crewObject)];
	}

	public GameObject CrewMemberToCrewObject(CrewMember crewMember) {
		return crewObjects[crewRoster.IndexOf(crewMember)];
	}

	private void ChooseMalefactor() {
		int repeatCount = 0;
		do {
			malefactorNdx = Random.Range(0, crewRoster.Count);
			repeatCount++;
			if (repeatCount > 100) {
				throw new System.NotImplementedException("Failed to choose a malefactor " + repeatCount + " times; crew likely consists only of Seers or unkillable crew. This case is not yet handled.");
			}
		} while (!IsValidMalefactor(crewRoster[malefactorNdx])); //FIXME: infinite loop if crew only consists of Seers or unkillable crew
	}

	private bool IsValidMalefactor(CrewMember crewMember) {
		return crewMember.typeOfCrew != CrewType.Seer && crewMember.isKillable;
	}

	private void GiveAstragaliToRandomCrew() {
		do {
			astragaliHolderNdx = Random.Range(0, crewRoster.Count);
		} while (crewRoster[astragaliHolderNdx].typeOfCrew != CrewType.Seer);
	}

	public void DisableControls() {
		fpsCamera.enabled = false;
		fpsMovement.enabled = false;
		InteractionManager.instance.DisableInteraction();
		Rigidbody rb = fpsMovement.transform.GetComponentInParent<Rigidbody>();
		if (rb != null)
			rb.velocity = Vector3.zero;
	}

	public void EnableControls() {
		fpsCamera.enabled = true;
		fpsMovement.enabled = true;
		InteractionManager.instance.EnableInteraction();
	}

	public void HideUI() {
		eventUI.SetActive(false);
	}

	public void ShowUI() {
		eventUI.SetActive(true);
	}

	// Player attempts to sail away after they have performed all of their rituals
	public void EndMinigame() {
		if (Random.Range(0f, 1f) < successChance) {
			Victory();
		}
		else {
			Failure();
		}
		if (Globals.MiniGames == null) {
			Debug.LogWarning("Player attempted to end the game, but the minigame manager (Globals.MiniGames) is null.");
			return;
		}
	}

	private void Victory() {
		Debug.Log("Victory!");
		ShowOutcomeInfoScreen();
	}

	private void Failure() {
		Debug.Log("Failure...");
		ShowOutcomeInfoScreen();
	}

	private void ShowOutcomeInfoScreen() {
		minigameInfoScreen.gameObject.SetActive(true);
		minigameInfoScreen.DisplayText(outcomeInfo.title, outcomeInfo.subtitle, outcomeInfo.content, outcomeInfo.icon, outcomeInfo.type);
		StartCoroutine(ShowInfoMenuCoroutine(Globals.MiniGames.Exit));
		
	}
	private void InitYarnFlagResponses() {
		YarnFlagResponse.RegisterResponse("$startDiceRitual", new RunRitualResponse(typeof(DiceMinigame)));
		YarnFlagResponse.RegisterResponse("$startSacrificeAnimalRitual", new FuncResponse(() => Debug.Log("Sacrifice animal")));
	}

	#region Private inner classes for collapsible inspector sections
	[System.Serializable]
	private class MinigameInfoSettings
	{
		public string title = "";
		public string subtitle = "";
		public string content = "";
		public Sprite icon = null;
		public MiniGameInfoScreen.MiniGame type = MiniGameInfoScreen.MiniGame.StormDeckStart;
	}
	#endregion

	#region Testing
	private void CrewRosterTest(IList<CrewMember> crewList) {
		Debug.Log("Crew List:");
		foreach (CrewMember c in crewList) {
			Debug.Log(c.name);
		}
		Debug.Log("End of list");
	}
	#endregion
}


