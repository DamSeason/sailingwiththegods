using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TavernCityViewModel : CityViewModel
{
	private DialogScreen ds;
	public int CostForHint {
		get {
			float initialCost = CoordinateUtil.GetDistanceBetweenTwoLatLongCoordinates(GameVars.currentSettlement.location_longXlatY, City.location_longXlatY) / 10000f;
			return Mathf.RoundToInt(initialCost - (initialCost * GameVars.GetOverallCloutModifier(City.settlementID)));
		}
	}

	public int CostToHire {
		get {
			float initialCost = CoordinateUtil.GetDistanceBetweenTwoLatLongCoordinates(GameVars.currentSettlement.location_longXlatY, City.location_longXlatY) / 1000f;
			return Mathf.RoundToInt(initialCost - (initialCost * GameVars.GetOverallCloutModifier(City.settlementID)));
		}
	}
	
	public DialogScreen GetDS {
		get { return ds; }
	}

	public TavernCityViewModel(Settlement city, DialogScreen d) : base(city, null) 
	{
		ds = d;
	}

	public string GetInfoOnNetworkedSettlementResource(Resource resource) {
		if (resource.amount_kg < 100)
			return "I hear they are running inredibly low on " + resource.name;
		else if (resource.amount_kg < 300)
			return "Someone mentioned that they have modest stores of " + resource.name;
		else
			return "A sailor just came from there and said he just unloaded an enormous quantity of " + resource.name;

	}

	// User for Trading Goods. This is getting resource from city.
	public void GUI_BuyHint() {

		if (GameVars.playerShipVariables.ship.currency < CostForHint) {
			GameVars.ShowANotificationMessage("Not enough money to buy this information!");
		}
		else {
			GameVars.playerShipVariables.ship.currency -= CostForHint;
			GameVars.ShowANotificationMessage(GetInfoOnNetworkedSettlementResource(City.cargo[UnityEngine.Random.Range(0, City.cargo.Length)]));
		}

	}
	// NOT USED. Navigator currently set in YarnTavern.cs
	public void GUI_HireANavigator() {
		//Do this if button pressed
		//Check to see if player has enough money to hire
		if (GameVars.playerShipVariables.ship.currency >= CostToHire) {
			//subtract the cost from the players currency
			GameVars.playerShipVariables.ship.currency -= (int)CostToHire;
			//change location of beacon
			Vector3 location = Vector3.zero;
			for (int x = 0; x < GameVars.settlement_masterList_parent.transform.childCount; x++)
				if (GameVars.settlement_masterList_parent.transform.GetChild(x).GetComponent<script_settlement_functions>().thisSettlement.settlementID == City.settlementID)
					location = GameVars.settlement_masterList_parent.transform.GetChild(x).position;
			GameVars.ActivateNavigatorBeacon(GameVars.navigatorBeacon, location);
			GameVars.playerShipVariables.ship.currentNavigatorTarget = City.settlementID;
			GameVars.ShowANotificationMessage("You hired a navigator to " + City.name + " for " + CostToHire + " drachma.");
			//If not enough money, then let the player know
		}
		else {
			GameVars.ShowANotificationMessage("You can't afford to hire a navigator to " + City.name + ".");
		}
	}
}

public class TavernViewModel : Model
{
	GameVars GameVars => Globals.GameVars;

	public ICollectionModel<CityViewModel> Cities { get; private set; }

	public TavernViewModel(DialogScreen d) {
		Cities = ValueModel.Wrap(GameVars.playerShipVariables.ship.playerJournal.knownSettlements)
			//.Where(id => id != GameVars.currentSettlement.settlementID)
			.Select(id => new TavernCityViewModel(GameVars.GetSettlementFromID(id), d) as CityViewModel);
	}
}

public class TavernView : ViewBehaviour<TavernViewModel>
{
	[SerializeField] CityListView CityList = null;

	public override void Bind(TavernViewModel model) {
		base.Bind(model);

		CityList.Bind(model.Cities);
	}
}
