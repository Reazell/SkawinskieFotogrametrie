using UnityEngine;
using UnityEngine.UI;

public class InteractionManagerScript : MonoBehaviour {

	public Text descriptionText;
	
	public float examinationDeadzone = 0.1f;
	public float examineRotationSpeed = 5.8f;
	public bool reticleEnabled = true;
	public bool interactionTextEnabled = true;
	public Sprite inactiveReticle;
	public Sprite activeReticle;
	public LayerMask putbackLayerMask;
	public LayerMask interactionLayerMask;

	float interactionRange = Mathf.Infinity;
	RectTransform interactionLabel;
	Vector3 interactionLabelTargetScale = Vector3.zero;
	Vector3 interactionLabelLargestScale = Vector3.zero;
	Vector3 interactionLabelSmallestScale = Vector3.zero;

	RectTransform reticle;
	// Journal
	RectTransform journalCloseButton;
	RectTransform journalPreviousButton;
	RectTransform journalNextButton;
	RectTransform journalBackground;
	RectTransform journalPage;
	Sprite[] currentJournalPages;

	int currentJournalPageIndex;
	//Audio diary
	RectTransform audioDiaryLabel;
	Vector3 audioDiaryLabelTargetScale = Vector3.zero;
	Vector3 audioDiaryLabelLargestScale = Vector3.zero;
	Vector3 audioDiaryLabelSmallestScale = Vector3.zero;
	bool playingAudioDiary;
	GameObject currentAudioDiary;
	bool fadingDiaryText;
	Color defaultDiaryColor;

	Color diaryFadeColor;
	// Object Interaction
	GameObject currentInteractableObject;
	GameObject currentHeldObject;
	GameObject currentPutbackObject;
	GameObject interactionObjectPickupLocation;
	GameObject interactionObjectExamineLocation;
	GameObject interactionObjectTossLocation;
	GameObject audioDiaryPlayer;
	GameObject journalSFXPlayer;

	bool examiningObject;

	public float zoomedFOV = 24.0f;

	float unZoomedFOV;
	bool cameraZoomedIn;
	float cameraZoomChangeRate = 0.1f;
	public Vector2 zoomedMouseSensitivity = new Vector2(1.5f,1.5f);
	public bool slowMouseOnInteractableObjectHighlight = true;
	public Vector2 highlightedMouseSensitivity = new Vector2(1.5f, 1.5f);

	Vector2 startingMouseSensitivity = Vector2.zero;
	Vector2 targetMouseSensitivity = Vector2.zero;
	bool smoothMouseChange;
	float smoothMouseChangeRate = 0.5f;
	GameObject thePlayer;
	bool mouseLookEnabled = true;

	Quaternion lastObjectHeldRotation = Quaternion.identity;
	float tossImpulseFactor = 2.5f;

	public AudioClip journalOpen;
	public AudioClip journalClose;
	public AudioClip journalPageTurn;
	GameObject currentJournal;

	public bool showMouseControlHints = true;
	public string mouseHintPickUpText = "Pick Up";
	public string mouseHintPutBackText = "Put Back";
	public string mouseHintExamineText = "Examine";
	public string mouseHintDropText = "Drop";
	public string mouseHintZoomText = "Zoom In";
	public string mouseHintActivateText = "Activate";
	public string mouseHintJournalText = "Read";

	RectTransform mouseLMBHelperIcon;
	RectTransform mouseLMBHelperText;
	RectTransform mouseRMBHelperIcon;
	RectTransform mouseRMBHelperText;

	public float fadeAmountPerTenthSecond = 0.1f;

	bool fadingDiaryAudio;
	float fadeCounter;

	void Start() {
		descriptionText.text = "";
		RectTransform[] childObjects = gameObject.GetComponentsInChildren<RectTransform>();

		foreach(RectTransform rt in childObjects){

			if(rt.transform.name == "Reticle"){
				reticle = rt;
			}else if(rt.transform.name == "InteractionTextLabel"){
				interactionLabel = rt;
			}else if(rt.transform.name == "CloseButton"){
				journalCloseButton = rt;
			}else if(rt.transform.name == "PreviousButton"){
				journalPreviousButton = rt;
			}else if(rt.transform.name == "NextButton"){
				journalNextButton = rt;
			}else if(rt.transform.name == "JournalBackground"){
				journalBackground = rt;
			}else if(rt.transform.name == "JournalPage"){
				journalPage = rt;
			}else if(rt.transform.name == "AudioDiaryTitleLabel"){
				audioDiaryLabel = rt;
			}else if(rt.transform.name == "MouseLMBHelperIcon"){
				mouseLMBHelperIcon = rt;
			}else if(rt.transform.name == "MouseLMBHelperText"){
				mouseLMBHelperText = rt;
			}else if(rt.transform.name == "MouseRMBHelperIcon"){
				mouseRMBHelperIcon = rt;
			}else if(rt.transform.name == "MouseRMBHelperText"){
				mouseRMBHelperText = rt;
			}

		}

		if(!reticle || ! interactionLabel || !audioDiaryLabel || !journalCloseButton || !journalPreviousButton || !journalNextButton || !journalBackground || !journalPage){
		}

		if(!reticleEnabled){
			reticle.GetComponentInChildren<Image>().enabled = false;
		}

		if(!interactionTextEnabled){
			interactionLabel.GetComponentInChildren<Text>().enabled = false;
		}

		if(!mouseLMBHelperIcon || !mouseLMBHelperText || !mouseRMBHelperIcon || !mouseRMBHelperText){
		}

		interactionObjectPickupLocation = GameObject.Find("ObjectPickupLocation");
		interactionObjectExamineLocation = GameObject.Find("ObjectExamineLocation");
		interactionObjectTossLocation = GameObject.Find("ObjectTossLocation");
		thePlayer = GameObject.FindGameObjectWithTag("Player");
		audioDiaryPlayer = GameObject.Find("AudioDiaryPlayer");
		journalSFXPlayer = GameObject.Find("JournalSFX");

		interactionObjectPickupLocation.GetComponentInChildren<MeshRenderer>().enabled = false;
		interactionObjectExamineLocation.GetComponentInChildren<MeshRenderer>().enabled = false;
		interactionObjectTossLocation.GetComponentInChildren<MeshRenderer>().enabled = false;

		if(!thePlayer || !interactionObjectPickupLocation || !interactionObjectExamineLocation || !interactionObjectTossLocation){
		}

		if(!audioDiaryPlayer || !journalSFXPlayer){
		}

		rememberStartingMouseSensitivity();

		interactionLabelLargestScale = new Vector3(1.0f,1.0f,1.0f);
		interactionLabelSmallestScale = new Vector3(0.0f,0.0f,0.0f);

		audioDiaryLabelLargestScale = new Vector3(1.1f,1.1f,1.1f);
		audioDiaryLabelSmallestScale = new Vector3(0.9f,0.9f,0.9f);

		defaultDiaryColor = audioDiaryLabel.GetComponent<Text>().color;
		diaryFadeColor = audioDiaryLabel.GetComponent<Text>().color;
		diaryFadeColor.a = 0.0f;

		unZoomedFOV = Camera.main.fieldOfView;

		closeJournal(false);
		hideAudioDiaryTitle();
		setMouseHints("","");

	}
	
	void Update(){

		Ray rayInteractable = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
		RaycastHit hitInteractable;
		
		if(Physics.Raycast(rayInteractable, out hitInteractable, interactionRange, interactionLayerMask)){
			
			if(hitInteractable.transform.gameObject.GetComponent<InteractableBaseScript>() && (hitInteractable.distance < hitInteractable.transform.gameObject.GetComponent<InteractableBaseScript>().getInteractionDistance())){
				
				if(currentInteractableObject){
					currentInteractableObject.GetComponent<InteractableBaseScript>().unHighlightObject();
					currentInteractableObject = null;
				}

				currentInteractableObject = hitInteractable.transform.gameObject;

				if((!currentHeldObject) || (currentHeldObject && currentInteractableObject.GetComponent<InteractableBaseScript>().interactionsAllowedWhenHoldingObject())){
						
					activateReticle(hitInteractable.transform.gameObject.GetComponent<InteractableBaseScript>().interactionString);

					currentInteractableObject.GetComponent<InteractableBaseScript>().highlightObject();
					
					if(slowMouseOnInteractableObjectHighlight){
						setMouseSensitivity(highlightedMouseSensitivity);
					}

					if(showMouseControlHints){
						
						InteractableBaseScript.eInteractionType tempInteractionType = hitInteractable.transform.gameObject.GetComponent<InteractableBaseScript>().getInteractionType();

						if(currentHeldObject){

							switch(tempInteractionType){
								
								case InteractableBaseScript.eInteractionType.ACTIVATE:
								case InteractableBaseScript.eInteractionType.JOURNAL:
								case InteractableBaseScript.eInteractionType.PICKUP:
								case InteractableBaseScript.eInteractionType.STATIC:
								default:
									setMouseHints(mouseHintDropText,mouseHintExamineText);
									break;
								
							}

						}else{

							switch(tempInteractionType){
								
								case InteractableBaseScript.eInteractionType.ACTIVATE:
									setMouseHints(mouseHintActivateText,mouseHintZoomText);
									break;
								case InteractableBaseScript.eInteractionType.JOURNAL:
									setMouseHints(mouseHintJournalText,mouseHintZoomText);
									break;
								case InteractableBaseScript.eInteractionType.PICKUP:
									setMouseHints(mouseHintPickUpText,mouseHintZoomText);
									break;
								case InteractableBaseScript.eInteractionType.STATIC:
								default:
									setMouseHints("",mouseHintZoomText);
									break;
								
							}

						}
						
					}

				}

			}else{
				
				if(currentInteractableObject){
					currentInteractableObject.GetComponent<InteractableBaseScript>().unHighlightObject();
					currentInteractableObject = null;
				}
				
				deactivateReticle();
				
				if(showMouseControlHints){
					if(currentJournal == null){
						setMouseHints("",mouseHintZoomText);
					}else{
						setMouseHints("","");
					}
				}
				
				if(!cameraZoomedIn){
					restorePreviousMouseSensitivity(true);
				}
				
			}
			
		}else{

			if(currentInteractableObject){
				currentInteractableObject.GetComponent<InteractableBaseScript>().unHighlightObject();
				currentInteractableObject = null;
			}

			currentPutbackObject = null;
			
			deactivateReticle();
			
			if(showMouseControlHints){
				if(currentJournal == null){
					setMouseHints("",mouseHintZoomText);
				}else{
					setMouseHints("","");
				}
			}
			
			if(!cameraZoomedIn){
				restorePreviousMouseSensitivity(true);
			}
			
		}

		if(currentHeldObject){

			if(examiningObject) {
				descriptionText.text = currentHeldObject.GetComponent<InteractablePickupScript>().itemDescription;
				float examinationOffsetUp = currentHeldObject.GetComponent<InteractablePickupScript>().examinationOffsetUp;
				float examinationOffsetForward = currentHeldObject.GetComponent<InteractablePickupScript>().examinationOffsetForward;
				currentHeldObject.transform.position = interactionObjectExamineLocation.transform.position + Vector3.up * examinationOffsetUp + interactionObjectExamineLocation.transform.forward * examinationOffsetForward;

				float rotationInputX = 0.0f;
				float rotationInputY = 0.0f;

				float examinationChangeX = Input.GetAxis("Mouse X");
				if(examinationChangeX == 0.0f){
					examinationChangeX = Input.GetAxis("Gamepad Look X");
				}

				float examinationChangeY = Input.GetAxis("Mouse Y");
				if(examinationChangeY == 0.0f){
					examinationChangeY = -Input.GetAxis("Gamepad Look Y");
				}

				if(Mathf.Abs(examinationChangeX) > examinationDeadzone){
					rotationInputX = -(examinationChangeX * examineRotationSpeed);
				}
				
				if(Mathf.Abs(examinationChangeY) > examinationDeadzone){
					rotationInputY = (examinationChangeY * examineRotationSpeed);
				}

				switch(currentHeldObject.GetComponent<InteractablePickupScript>().rotationLockType){

					case InteractablePickupScript.eRotationType.FREE:
						currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.up, rotationInputX, Space.World);
						currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.right, rotationInputY, Space.World);
						break;
					case InteractablePickupScript.eRotationType.HORIZONTAL:
						currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.up, rotationInputX, Space.World);
						break;
					case InteractablePickupScript.eRotationType.VERTICAL:
						currentHeldObject.transform.Rotate(interactionObjectExamineLocation.transform.right, rotationInputY, Space.World);
						break;
					case InteractablePickupScript.eRotationType.NONE:
					default:
						break;

				}

				if(showMouseControlHints){
					setMouseHints("","");
				}

			}else{

				currentHeldObject.transform.position = interactionObjectPickupLocation.transform.position;
				currentHeldObject.transform.rotation = Quaternion.Slerp(currentHeldObject.transform.rotation, interactionObjectPickupLocation.transform.rotation * Quaternion.Euler(lastObjectHeldRotation.eulerAngles), 0.2f);

				Ray rayPutBack = Camera.main.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
				RaycastHit hitPutBack;

				if(Physics.Raycast(rayPutBack, out hitPutBack, interactionRange, putbackLayerMask)){

					if(hitPutBack.transform.gameObject.GetComponent<PutBackScript>() && hitPutBack.transform.gameObject.GetComponent<PutBackScript>().getPickupObjectID() == currentHeldObject.GetInstanceID()){
						currentPutbackObject = hitPutBack.transform.gameObject;
						activateReticle(currentHeldObject.GetComponent<InteractablePickupScript>().putBackString);
						descriptionText.text = "";
						if(showMouseControlHints){
							setMouseHints(mouseHintPutBackText,mouseHintExamineText);
						}
					}else{
						currentPutbackObject = null;
						deactivateReticle();
						if(showMouseControlHints){
							setMouseHints(mouseHintDropText,mouseHintExamineText);
						}
					}
					
				}else{

					currentPutbackObject = null;

					if(!currentInteractableObject){
						deactivateReticle();
						if(showMouseControlHints){
							setMouseHints(mouseHintDropText,mouseHintExamineText);
						}
					}

					if(!cameraZoomedIn){
						restorePreviousMouseSensitivity(true);
					}

				}

			}

		}

		if((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0) || Input.GetButtonDown("Gamepad Interact")) && !examiningObject){

			if(currentHeldObject){

				if(currentPutbackObject){

					currentHeldObject.GetComponent<InteractablePickupScript>().doPickupPutdown(true);

					currentHeldObject.transform.position = currentPutbackObject.transform.position;
					currentHeldObject.transform.rotation = currentPutbackObject.transform.rotation;
					currentHeldObject.transform.parent = null;
					currentHeldObject.GetComponent<Collider>().isTrigger = false;
					currentHeldObject.GetComponent<Rigidbody>().isKinematic = false;
					currentHeldObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
					currentHeldObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

					Transform[] objectTransforms = currentHeldObject.GetComponentsInChildren<Transform>();
					foreach(Transform t in objectTransforms){
						t.gameObject.layer = LayerMask.NameToLayer("PickupObjects");
					}

					currentHeldObject = null;

					interactionLabel.localScale = interactionLabelSmallestScale;
						
				}else if(currentInteractableObject && currentInteractableObject.GetComponent<InteractableBaseScript>().interactionsAllowedWhenHoldingObject()){

					if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.STATIC){

						currentInteractableObject.GetComponent<InteractableBaseScript>().interact();

					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.ACTIVATE){

						currentInteractableObject.GetComponent<InteractableActivateScript>().activate();
					
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.PICKUP){
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.AUDIODIARY){
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.JOURNAL){
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.INVENTORY){
						
						gameObject.GetComponent<InventoryManagerScript>().giveInventoryItem(currentInteractableObject.GetComponent<InteractableInventoryItemScript>().getInventoryItemType(),currentInteractableObject.GetComponent<InteractableInventoryItemScript>().getInventoryQuantity());
						currentInteractableObject.GetComponent<InteractableInventoryItemScript>().consumeInventoryItem();
						
					}

					else{
					}

				}else{

					currentHeldObject.GetComponent<InteractablePickupScript>().doPickupPutdown(false);

					currentHeldObject.transform.parent = null;
					currentHeldObject.GetComponent<Collider>().isTrigger = false;
					currentHeldObject.GetComponent<Rigidbody>().isKinematic = false;
					float tossStrength = currentHeldObject.GetComponent<InteractablePickupScript>().tossStrength;
					float tossOffsetUp = currentHeldObject.GetComponent<InteractablePickupScript>().tossOffsetUp;
					float tossOffsetForward = currentHeldObject.GetComponent<InteractablePickupScript>().tossOffsetForward;
					currentHeldObject.transform.position = interactionObjectTossLocation.transform.position + Vector3.up * tossOffsetUp + Camera.main.transform.forward * tossOffsetForward;
					currentHeldObject.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * tossImpulseFactor * tossStrength, ForceMode.Impulse);

					Transform[] objectTransforms = currentHeldObject.GetComponentsInChildren<Transform>();
					foreach(Transform t in objectTransforms){
						t.gameObject.layer = LayerMask.NameToLayer("PickupObjects");
					}

					currentHeldObject = null;

				}

			}else{

				if(currentInteractableObject){

					lastObjectHeldRotation = Quaternion.identity;

					if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.PICKUP){

						currentInteractableObject.GetComponent<InteractablePickupScript>().doPickupPutdown(false);

						currentInteractableObject.GetComponent<InteractableBaseScript>().unHighlightObject();
						currentInteractableObject.GetComponent<Rigidbody>().isKinematic = true;
						currentInteractableObject.GetComponent<Collider>().isTrigger = true;

						currentInteractableObject.transform.position = interactionObjectPickupLocation.transform.position;

						currentHeldObject = currentInteractableObject;
						currentInteractableObject = null;

						interactionLabel.localScale = interactionLabelSmallestScale;

						Transform[] objectTransforms = currentHeldObject.GetComponentsInChildren<Transform>();
						foreach(Transform t in objectTransforms){
							t.gameObject.layer = LayerMask.NameToLayer("ObjectExamination");
						}

						cameraZoomedIn = false;
						restorePreviousMouseSensitivity(false);

					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.STATIC){

						currentInteractableObject.GetComponent<InteractableBaseScript>().interact();

					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.JOURNAL){

						currentInteractableObject.GetComponent<InteractableJournalScript>().activateJournal();
						currentJournal = currentInteractableObject;
						cameraZoomedIn = false;
						restorePreviousMouseSensitivity(false);
						openJournal();
						
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.ACTIVATE){

						currentInteractableObject.GetComponent<InteractableActivateScript>().activate();
						
					}else if(currentInteractableObject.GetComponent<InteractableBaseScript>().getInteractionType() == InteractableBaseScript.eInteractionType.INVENTORY){

						gameObject.GetComponent<InventoryManagerScript>().giveInventoryItem(currentInteractableObject.GetComponent<InteractableInventoryItemScript>().getInventoryItemType(),currentInteractableObject.GetComponent<InteractableInventoryItemScript>().getInventoryQuantity());
						currentInteractableObject.GetComponent<InteractableInventoryItemScript>().consumeInventoryItem();
						
					}
					else{
					}

				}

			}

		}

		if((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1) || Input.GetButtonDown("Gamepad Examine")) && currentHeldObject){

			hideReticleAndInteractionLabel();
			examiningObject = true;
			disableMouseLook();
			disableMovement();

			if(currentHeldObject.GetComponent<InteractablePickupScript>().postExaminationInteractionString != ""){
				currentHeldObject.GetComponent<InteractablePickupScript>().interactionString = currentHeldObject.GetComponent<InteractablePickupScript>().postExaminationInteractionString;
			}

			currentHeldObject.transform.position = interactionObjectExamineLocation.transform.position;

			if(currentHeldObject.GetComponent<InteractablePickupScript>().rotationLockType == InteractablePickupScript.eRotationType.FREE){

				if(lastObjectHeldRotation == Quaternion.identity){
					
					Vector3 relativePos = Camera.main.transform.position - currentHeldObject.transform.position;
					Quaternion rotation = Quaternion.LookRotation(relativePos);
					currentHeldObject.transform.rotation = rotation;
					
				}else{
					currentHeldObject.transform.rotation = lastObjectHeldRotation;
				}

			}else{

				Vector3 relativePos = Camera.main.transform.position - currentHeldObject.transform.position;
				Quaternion rotation = Quaternion.LookRotation(relativePos);
				currentHeldObject.transform.rotation = rotation;

			}

		}

		if((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetMouseButtonUp(1) || Input.GetButtonUp("Gamepad Examine")) && currentHeldObject){

			lastObjectHeldRotation = currentHeldObject.transform.rotation;
			showReticleAndInteractionLabel();
			examiningObject = false;
			enableMouseLook();
			enableMovement();

		}

		if(Input.GetKeyDown(KeyCode.X) || Input.GetButtonDown("Gamepad Close")){

			if(currentAudioDiary){

				currentAudioDiary.GetComponent<InteractableAudioDiaryScript>().stopAudioDiary();
				fadingDiaryAudio = true;
				fadingDiaryText = true;

			}

		}

		if(currentHeldObject == null && currentJournal == null){

			if(Input.GetMouseButtonDown(1) || Input.GetButtonDown("Gamepad Examine")){
				setMouseSensitivity(zoomedMouseSensitivity);
			}
			if(Input.GetMouseButtonUp(1) || Input.GetButtonUp("Gamepad Examine")){
				restorePreviousMouseSensitivity(false);
			}
			if(Input.GetMouseButton(1) || Input.GetButton("Gamepad Examine")){
				cameraZoomedIn = true;
			}else{
				cameraZoomedIn = false;
			}

		}

		if(cameraZoomedIn){
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, zoomedFOV, cameraZoomChangeRate);
		}else{
			Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, unZoomedFOV, cameraZoomChangeRate);
		}

		interactionLabel.localScale = Vector3.Lerp(interactionLabel.localScale, interactionLabelTargetScale, 0.25f);

		if(playingAudioDiary){

			audioDiaryLabel.localScale = Vector3.Lerp(audioDiaryLabel.localScale, audioDiaryLabelTargetScale, 0.01f);

			if((audioDiaryLabelTargetScale == audioDiaryLabelLargestScale) && (Vector3.Distance(audioDiaryLabel.localScale, audioDiaryLabelLargestScale) < 0.1f)){
				audioDiaryLabelTargetScale = audioDiaryLabelSmallestScale;
			}else if((audioDiaryLabelTargetScale == audioDiaryLabelSmallestScale) && (Vector3.Distance(audioDiaryLabel.localScale, audioDiaryLabelSmallestScale) < 0.1f)){
				audioDiaryLabelTargetScale = audioDiaryLabelLargestScale;
			}

		}

		if(fadingDiaryText){

			audioDiaryLabel.GetComponent<Text>().color = Color.Lerp(audioDiaryLabel.GetComponent<Text>().color, diaryFadeColor, 0.1f);

			if(audioDiaryLabel.GetComponent<Text>().color.a <= 0.1f){
				audioDiaryLabel.GetComponent<Text>().text = "";
				audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;
				fadingDiaryText = false;
			}

		}

		if(playingAudioDiary && !audioDiaryPlayer.GetComponent<AudioSource>().isPlaying){

			if(currentAudioDiary){
				currentAudioDiary.GetComponent<InteractableAudioDiaryScript>().stopAudioDiary();
			}

			hideAudioDiaryTitle();

		}
		
		if(fadingDiaryAudio){

			fadeCounter += Time.deltaTime;

			if(fadeCounter >= 0.1f){

				audioDiaryPlayer.GetComponent<AudioSource>().volume -= fadeAmountPerTenthSecond;

				if(audioDiaryPlayer.GetComponent<AudioSource>().volume <= 0.0f){
					audioDiaryPlayer.GetComponent<AudioSource>().Stop();
					fadingDiaryAudio = false;
				}

				fadeCounter = 0.0f;

			}

		}

		if(smoothMouseChange){
			thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().XSensitivity = Mathf.Lerp(thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().XSensitivity, targetMouseSensitivity.x, smoothMouseChangeRate);
			thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().YSensitivity = Mathf.Lerp(thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().YSensitivity, targetMouseSensitivity.y, smoothMouseChangeRate);
		}

	}

	void activateReticle(string interactionString){
		if(reticleEnabled){
			reticle.GetComponent<Image>().overrideSprite = activeReticle;
		}
		if(interactionTextEnabled){
			interactionLabel.GetComponent<Text>().text = interactionString;
			interactionLabelTargetScale = interactionLabelLargestScale;
		}
	}

	void deactivateReticle(){
		if(reticleEnabled){
			reticle.GetComponent<Image>().overrideSprite = inactiveReticle;
		}
		if(interactionTextEnabled){
			interactionLabel.GetComponent<Text>().text = "";
			interactionLabelTargetScale = interactionLabelSmallestScale;
		}
	}

	void hideReticleAndInteractionLabel(){
		if(reticleEnabled){
			reticle.GetComponentInChildren<Image>().enabled = false;
		}
		if(interactionTextEnabled){
			interactionLabel.GetComponentInChildren<Text>().text = "";
		}
	}

	void showReticleAndInteractionLabel(){
		if(reticleEnabled){
			reticle.GetComponentInChildren<Image>().enabled = true;
		}
		if(interactionTextEnabled){
			interactionLabel.GetComponentInChildren<Text>().text = "";
		}
	}

	public void playNewAudioDiary(GameObject diary){

		if(playingAudioDiary){
			currentAudioDiary.GetComponent<InteractableAudioDiaryScript>().stopAudioDiary();
			audioDiaryLabel.GetComponent<Text>().text = "";
			audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;
		}

		currentAudioDiary = diary.gameObject;
		audioDiaryLabel.GetComponent<Text>().color = defaultDiaryColor;
		audioDiaryLabel.GetComponent<Text>().text = "Playing '"+diary.GetComponent<InteractableAudioDiaryScript>().audioDiaryTitle+"' - Press 'X' to Skip";
		playingAudioDiary = true;
		audioDiaryLabelTargetScale = audioDiaryLabelLargestScale;
		fadingDiaryAudio = false;
		fadingDiaryText = false;
		audioDiaryPlayer.GetComponent<AudioSource>().clip = diary.GetComponent<InteractableAudioDiaryScript>().audioDiaryClip;
		audioDiaryPlayer.GetComponent<AudioSource>().volume = 1.0f;
		audioDiaryPlayer.GetComponent<AudioSource>().Play();

	}

	public void hideAudioDiaryTitle(){

		fadingDiaryText = true;
		currentAudioDiary = null;
		playingAudioDiary = false;

	}

	public void openJournal(){

		disableMovement();
		disableMouseLook();
		setCursorVisibility(true);

		journalSFXPlayer.GetComponent<AudioSource>().clip = journalOpen;
		journalSFXPlayer.GetComponent<AudioSource>().Play();

		journalCloseButton.transform.gameObject.GetComponentInChildren<Image>().enabled = true;
		journalPreviousButton.transform.gameObject.GetComponentInChildren<Image>().enabled = true;
		journalNextButton.transform.gameObject.GetComponentInChildren<Image>().enabled = true;
		journalBackground.transform.gameObject.GetComponentInChildren<Image>().enabled = true;
		journalPage.transform.gameObject.GetComponentInChildren<Image>().enabled = true;

		currentJournalPages = currentJournal.GetComponent<InteractableJournalScript>().journalPages;
		if(currentJournalPages.Length > 0){
			journalPage.transform.gameObject.GetComponentInChildren<Image>().overrideSprite = currentJournalPages[currentJournalPageIndex];
		}else{
		}
	
	}

	public void nextJournalPage(){

		currentJournalPageIndex++;
		if(currentJournalPageIndex > currentJournalPages.Length - 1){
			currentJournalPageIndex = currentJournalPages.Length - 1;
		}else{
			journalSFXPlayer.GetComponent<AudioSource>().clip = journalPageTurn;
			journalSFXPlayer.GetComponent<AudioSource>().Play();
		}

		journalPage.transform.gameObject.GetComponentInChildren<Image>().overrideSprite = currentJournalPages[currentJournalPageIndex];

	}

	public void previousJournalPage(){

		currentJournalPageIndex--;
		if(currentJournalPageIndex < 0){
			currentJournalPageIndex = 0;
		}else{
			journalSFXPlayer.GetComponent<AudioSource>().clip = journalPageTurn;
			journalSFXPlayer.GetComponent<AudioSource>().Play();
		}

		journalPage.transform.gameObject.GetComponentInChildren<Image>().overrideSprite = currentJournalPages[currentJournalPageIndex];

	}

	public void closeJournal(bool playSound=true){

		if(playSound){
			journalSFXPlayer.GetComponent<AudioSource>().clip = journalClose;
			journalSFXPlayer.GetComponent<AudioSource>().Play();
		}

		journalCloseButton.transform.gameObject.GetComponentInChildren<Image>().enabled = false;
		journalPreviousButton.transform.gameObject.GetComponentInChildren<Image>().enabled = false;
		journalNextButton.transform.gameObject.GetComponentInChildren<Image>().enabled = false;
		journalBackground.transform.gameObject.GetComponentInChildren<Image>().enabled = false;
		journalPage.transform.gameObject.GetComponentInChildren<Image>().enabled = false;

		if(currentJournal){
			currentJournal.GetComponent<InteractableJournalScript>().deactivateJournal();
		}

		currentJournal = null;
		currentJournalPageIndex = 0;
		currentJournalPages = null;

		setCursorVisibility(false);
		enableMouseLook();
		enableMovement();

	}

	void setMouseHints(string LMBHintText, string RMBHintText){

		if(LMBHintText == ""){
			mouseLMBHelperIcon.GetComponent<Image>().enabled = false;
			mouseLMBHelperText.GetComponent<Text>().text = "";
			mouseLMBHelperText.GetComponent<Text>().enabled = false;
		}else{
			mouseLMBHelperIcon.GetComponent<Image>().enabled = true;
			mouseLMBHelperText.GetComponent<Text>().text = LMBHintText;
			mouseLMBHelperText.GetComponent<Text>().enabled = true;
		}

		if(RMBHintText == ""){
			mouseRMBHelperIcon.GetComponent<Image>().enabled = false;
			mouseRMBHelperText.GetComponent<Text>().text = "";
			mouseRMBHelperText.GetComponent<Text>().enabled = false;
		}else{
			mouseRMBHelperIcon.GetComponent<Image>().enabled = true;
			mouseRMBHelperText.GetComponent<Text>().text = RMBHintText;
			mouseRMBHelperText.GetComponent<Text>().enabled = true;
		}

	}

	void setCursorVisibility(bool visible){
		Cursor.visible = visible;
	}

	void rememberStartingMouseSensitivity(){
		startingMouseSensitivity.x = thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().XSensitivity;
		startingMouseSensitivity.y = thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().YSensitivity;
	}
	void setMouseSensitivity(Vector2 sensitivity){
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().XSensitivity = sensitivity.x;
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().YSensitivity = sensitivity.y;
		smoothMouseChange = false;
	}

	void restorePreviousMouseSensitivity(bool smoothTransition){
		if(smoothTransition){
			targetMouseSensitivity.x = startingMouseSensitivity.x;
			targetMouseSensitivity.y = startingMouseSensitivity.y;
			smoothMouseChange = true;
		}else{
			thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().XSensitivity = startingMouseSensitivity.x;
			thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().YSensitivity = startingMouseSensitivity.y;
			smoothMouseChange = false;
		}
	}
	void disableMouseLook(){
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().enableMouseLook = false;
		mouseLookEnabled = false;
	}
	void enableMouseLook(){
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.MouseLook>().enableMouseLook = true;
		mouseLookEnabled = true;
	}
	void disableMovement(){
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enableMovement = false;
	}
	void enableMovement(){
		thePlayer.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enableMovement = true;
	}

	public bool isMouseLookEnabled(){
		return mouseLookEnabled;
	}


}
