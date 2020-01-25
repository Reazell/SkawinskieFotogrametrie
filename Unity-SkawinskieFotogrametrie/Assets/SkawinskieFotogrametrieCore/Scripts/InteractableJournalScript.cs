using UnityEngine;

public class InteractableJournalScript : InteractableBaseScript {

	public string postReadInteractionString = "";

	public Sprite[] journalPages;


	public override void Awake(){
		
		base.Awake();
		interactionType = eInteractionType.JOURNAL;
		canInteractWithWhileHoldingObject = false;
		
	}

	public void activateJournal(){
		gameObject.GetComponent<Collider>().enabled = false;
	}

	public void deactivateJournal(){
		if(postReadInteractionString != ""){
			interactionString = postReadInteractionString;
		}
		gameObject.GetComponent<Collider>().enabled = true;
	}

}
