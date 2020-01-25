using UnityEngine;

public class InteractableActivateScript : InteractableBaseScript {

	public override void Awake() {
		base.Awake ();
		interactionType = eInteractionType.ACTIVATE;
	}

	public virtual void activate() {
		Debug.LogWarning("Brak override'a w child class.");
	}

}
