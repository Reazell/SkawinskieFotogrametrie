using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class InteractableInventoryItemScript : InteractableBaseScript {

	[SerializeField]
	InventoryManagerScript.eInventoryItems inventoryItemType;

	[SerializeField]
	int inventoryQuantity=1;

	[SerializeField]
	bool enableSounds = true;
	[SerializeField]
	AudioClip inventoryGetSound;

	bool hasBeenConsumed;

	public override void Awake(){

		base.Awake();
		interactionType = eInteractionType.INVENTORY;

		if(enableSounds){

			if(enableSounds && !gameObject.GetComponent<AudioSource>()){
			}

			gameObject.GetComponent<AudioSource>().loop = false;
			gameObject.GetComponent<AudioSource>().playOnAwake = false;
			
			if(!inventoryGetSound){
				inventoryGetSound = Resources.Load("genericInventoryGet") as AudioClip;
			}

		}

	}

	void Update(){

		if(hasBeenConsumed){

			if(enableSounds){

				if(!gameObject.GetComponent<AudioSource>().isPlaying){
					Destroy(gameObject);
				}

			}else{
				Destroy(gameObject);
			}

		}

	}

	public InventoryManagerScript.eInventoryItems getInventoryItemType(){
		return inventoryItemType;
	}

	public int getInventoryQuantity(){
		return inventoryQuantity;
	}

	public void consumeInventoryItem(){

		if(!hasBeenConsumed){
			MeshRenderer[] childRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			foreach(MeshRenderer mr in childRenderers){
				mr.enabled = false;
			}

			if(enableSounds){
				gameObject.GetComponent<AudioSource> ().clip = inventoryGetSound;
				gameObject.GetComponent<AudioSource> ().Play ();
			}

			hasBeenConsumed = true;

		}

	}

}
