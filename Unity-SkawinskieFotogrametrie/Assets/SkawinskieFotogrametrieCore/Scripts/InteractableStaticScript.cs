public class InteractableStaticScript : InteractableBaseScript {

	public override void Awake() {
		base.Awake();
		interactionType = eInteractionType.STATIC;
	}
}
