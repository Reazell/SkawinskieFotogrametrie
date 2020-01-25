using UnityEngine;

public class InteractableAudioDiaryScript : InteractableBaseScript {

	public string audioDiaryTitle = "DEFAULT DIARY TITLE";
	public AudioClip audioDiaryClip;
	public string postPlaybackInteractionString = "";
	bool hasBeenPlayed;
	GameObject interactionManager;

	public override void Awake() {
		base.Awake();
		interactionType = eInteractionType.AUDIODIARY;

	}

	void Start() {
		interactionManager = GameObject.Find("InteractionManager");
	}
	
	public override void discoverObject() {
		base.discoverObject();

		if(!hasBeenPlayed) {
			hasBeenPlayed = true;
			interactionManager.GetComponent<InteractionManagerScript>().playNewAudioDiary(gameObject);
		}
	}

	public void stopAudioDiary() {
		if(postPlaybackInteractionString != "")
			interactionString = postPlaybackInteractionString;
	}
}
