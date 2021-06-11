using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class CargoInventoryViewModel : ListenerModel
{
	private Resource Resource;

	public int AmountKg {
		get => Mathf.RoundToInt(Resource.amount_kg);
		set {
			Resource.amount_kg = value;
			Notify();
		}
	}

	public string Name => Resource.name;
	public Sprite Icon { get; private set; }

	public CargoInventoryViewModel(Resource resource) {
		Resource = resource;
		Listen(Resource);

		Icon = Resource.IconSprite();
	}
}

public class CargoInventoryView : ViewBehaviour<CargoInventoryViewModel>
{
	[SerializeField] Button InfoButton = null;
	[SerializeField] ImageView Icon = null;
	[SerializeField] StringView Name = null;
	[SerializeField] StringView Amount = null;

	private void Start() {
		if(InfoButton != null) {
			Subscribe(InfoButton.onClick, ShowInfo);
		}
	}

	public override void Bind(CargoInventoryViewModel model) {
		base.Bind(model);

		Amount?.Bind(new BoundModel<int>(Model, nameof(Model.AmountKg)).AsString());
		Name?.Bind(new BoundModel<string>(Model, nameof(Model.Name)));
		Icon?.Bind(new BoundModel<Sprite>(Model, nameof(Model.Icon)));
	}

	void ShowInfo() {
		Debug.Log("Cargo - " + Model.Name + " - " + Model.AmountKg + " kg");
	}
}
