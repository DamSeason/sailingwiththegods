using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Yarn.Unity;
public class DummyCrewmate : MonoBehaviour, IInteractionPromptProvider
{
	private UISystem UI => Globals.UI;

	public string crewmateName = "Crewmate";
	public CrewType crewmateType = CrewType.Sailor;
	[TextArea(1,10)]
	public string nameplateFormat = "{0}\n-{1}-";
	public TextMeshProUGUI nameplate;

	public bool speakThroughDebug = false;

	private void Start() {
		UpdateNameplate();
	}

	public void Say(string message) {
		if (speakThroughDebug) {
			Debug.Log(FormatDialogue(message));
		}
		else {
			DialogueManager.DisplayText(FormatDialogue(message));
		}
	}

	private string FormatDialogue(string message) {
		return crewmateName + " says, \"" + message + "\"";
	}

	private void UpdateNameplate() {
		nameplate.text = string.Format(nameplateFormat, crewmateName, crewmateType);
	}

	public string GetInteractionPrompt() {
		return "speak with " + crewmateName;
	}

	public void SetNameAndType(string name, CrewType type) {
		crewmateName = name;
		crewmateType = type;
		UpdateNameplate();
	}

	public void StartDialogue() {
		Debug.Log(crewmateName + " talks to you.");
		if (UI == null) {
			Debug.LogWarning("The global UISystem is null. Please load this minigame through the main game using the playtesting hotkeys.");
			return;
		}
		StormDeckManager.instance.DisableControls();
		DialogScreen dialogScreen = UI.Show<DialogScreen>();
		SetDialogVariables(dialogScreen);
		dialogScreen.StartDialog("StartStormDeck", "darken"); // FIXME: if this errors, the player's controls will not re-enable
		StartCoroutine(WaitForDialogEnd(dialogScreen));
	}

	private void SetDialogVariables(DialogScreen dialogScreen) {
		YarnVariableList variableList = new YarnVariableList();
		variableList.Add("crewName", crewmateName);
		variableList.Add("crewType", crewmateType.ToString());
		dialogScreen.Storage.defaultVariables = variableList.ToArray();
		dialogScreen.Storage.ResetToDefaults();
	}

	IEnumerator WaitForDialogEnd(DialogScreen dialogScreen) {
		while (dialogScreen.gameObject.activeInHierarchy)
		{
			yield return new WaitForEndOfFrame();
		}
		OnDialogEnd();
	}

	private void OnDialogEnd() {
		StormDeckManager.instance.EnableControls();
	}
}