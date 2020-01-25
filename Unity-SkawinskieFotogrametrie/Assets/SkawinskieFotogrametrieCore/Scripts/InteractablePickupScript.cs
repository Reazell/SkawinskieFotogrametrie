using UnityEngine;

[RequireComponent(typeof (Rigidbody))]
[RequireComponent(typeof (Collider))]
[RequireComponent(typeof (AudioSource))]
public class InteractablePickupScript : InteractableBaseScript {

	public string putBackString = "<DEFAULT PUT BACK STRING>";
	public string postExaminationInteractionString = "";
	public string itemDescription = "";

	public bool autoGeneratePutBackObject;
	public float examinationOffsetUp;
	public float examinationOffsetForward;
	public Vector3 pickupRotationOffset = Vector3.zero;
	public float tossStrength = 1.0f;
	public float tossOffsetUp = 0.1f;
	public float tossOffsetForward = 0.1f;
	public enum eRotationType {FREE,HORIZONTAL,VERTICAL,NONE};
	public eRotationType rotationLockType = eRotationType.FREE;

	public bool enableSounds = true;
	public AudioClip[] pickupSounds;
	public AudioClip[] putBackSounds;
	public AudioClip[] impactSounds;

	float impactSoundCountdown = 1.0f;
	float minImpactSoundVolume = 0.5f;
	bool playImpactSounds;
	protected bool beingPutBack;
	protected bool pickedUp;

	public override void Awake(){

		base.Awake();
		interactionType = eInteractionType.PICKUP;
		canInteractWithWhileHoldingObject = false;

		gameObject.layer = LayerMask.NameToLayer("PickupObjects");

		if(enableSounds){

			if(enableSounds && !gameObject.GetComponent<AudioSource>()){
			}

			gameObject.GetComponent<AudioSource>().loop = false;
			gameObject.GetComponent<AudioSource>().playOnAwake = false;

			if(impactSounds.Length == 0){
				impactSounds = new AudioClip[1];
				impactSounds[0] = Resources.Load("genericPhysicsImpact") as AudioClip;
			}

			if(pickupSounds.Length == 0){
				pickupSounds = new AudioClip[1];
				pickupSounds[0] = Resources.Load("genericPickup") as AudioClip;
			}

			if(putBackSounds.Length == 0){
				putBackSounds = new AudioClip[1];
				putBackSounds[0] = Resources.Load("genericPutBack") as AudioClip;
			}

		}

		if(autoGeneratePutBackObject){
			generatePutBackPlace();
		}

	}

	public virtual void Update(){

		if(enableSounds){

			if(!playImpactSounds){
				impactSoundCountdown -= Time.deltaTime;
				if(impactSoundCountdown <= 0.0f){
					playImpactSounds = true;
				}
			}

			if(beingPutBack){
				if(!gameObject.GetComponent<AudioSource>().isPlaying){
					beingPutBack = false;
				}
			}

		}

	}

	void OnCollisionEnter(){

		if(!gameObject.GetComponent<Rigidbody>().isKinematic && playImpactSounds && !beingPutBack && enableSounds){

			float impactVolume = Mathf.Max(minImpactSoundVolume, Mathf.Min(1.0f, (gameObject.GetComponent<Rigidbody>().velocity.magnitude / 5.0f)));

			if(gameObject.GetComponent<AudioSource>().isPlaying){
				gameObject.GetComponent<AudioSource>().Stop();
			}
			gameObject.GetComponent<AudioSource>().volume = impactVolume;
			gameObject.GetComponent<AudioSource>().clip = impactSounds[Random.Range(0,impactSounds.Length)];
			gameObject.GetComponent<AudioSource>().Play();

		}

	}

	public virtual void doPickupPutdown(bool putback){

		if(pickedUp){

			pickedUp = false;

			if(putback){

				if(enableSounds){

					beingPutBack = true;

					if(gameObject.GetComponent<AudioSource>().isPlaying){
						gameObject.GetComponent<AudioSource>().Stop();
					}
					gameObject.GetComponent<AudioSource>().volume = 1.0f;
					gameObject.GetComponent<AudioSource>().clip = putBackSounds[Random.Range(0,putBackSounds.Length)];
					gameObject.GetComponent<AudioSource>().Play();

				}

			}

		}else{

			beingPutBack = false;
			pickedUp = true;

			if(enableSounds){

				if(gameObject.GetComponent<AudioSource>().isPlaying){
					gameObject.GetComponent<AudioSource>().Stop();
				}
				gameObject.GetComponent<AudioSource>().volume = 1.0f;
				gameObject.GetComponent<AudioSource>().clip = pickupSounds[Random.Range(0,pickupSounds.Length)];
				gameObject.GetComponent<AudioSource>().Play();

			}

		}

	}

	public bool isCurrentlyPickedUp(){
		return pickedUp;
	}

	void generatePutBackPlace(){
		
		GameObject putBackPlace = new GameObject(gameObject.name + "PutBackObject");
		putBackPlace.transform.position = gameObject.transform.position;
		putBackPlace.transform.rotation = gameObject.transform.rotation;
		putBackPlace.transform.localScale = gameObject.transform.localScale;
		
		if(gameObject.GetComponent<MeshCollider>()){
			
			putBackPlace.AddComponent<MeshCollider>();
			putBackPlace.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshCollider>().sharedMesh;
			putBackPlace.GetComponent<MeshCollider>().convex = true;
			putBackPlace.GetComponent<MeshCollider>().isTrigger = true;
			
		}else if(gameObject.GetComponent<SphereCollider>()){
			
			putBackPlace.AddComponent<SphereCollider>();
			putBackPlace.GetComponent<SphereCollider>().radius = gameObject.GetComponent<SphereCollider>().radius;
			putBackPlace.GetComponent<SphereCollider>().center = gameObject.GetComponent<SphereCollider>().center;
			putBackPlace.GetComponent<SphereCollider>().isTrigger = true;
			
		}else if(gameObject.GetComponent<CapsuleCollider>()){
			
			putBackPlace.AddComponent<CapsuleCollider>();
			putBackPlace.GetComponent<CapsuleCollider>().height = gameObject.GetComponent<CapsuleCollider>().height;
			putBackPlace.GetComponent<CapsuleCollider>().radius = gameObject.GetComponent<CapsuleCollider>().radius;
			putBackPlace.GetComponent<CapsuleCollider>().center = gameObject.GetComponent<CapsuleCollider>().center;
			putBackPlace.GetComponent<CapsuleCollider>().direction = gameObject.GetComponent<CapsuleCollider>().direction;
			putBackPlace.GetComponent<CapsuleCollider>().isTrigger = true;
			
		}else if(gameObject.GetComponent<BoxCollider>()){
			
			putBackPlace.AddComponent<BoxCollider>();
			putBackPlace.GetComponent<BoxCollider>().size = gameObject.GetComponent<BoxCollider>().size;
			putBackPlace.GetComponent<BoxCollider>().center = gameObject.GetComponent<BoxCollider>().center;
			putBackPlace.GetComponent<BoxCollider>().isTrigger = true;
			
		}else{
		}
		
		putBackPlace.AddComponent<PutBackScript>();
		putBackPlace.GetComponent<PutBackScript>().setPickupObjectID(gameObject.GetInstanceID());
		
	}

}
