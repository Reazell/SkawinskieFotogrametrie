using UnityEngine;


[RequireComponent (typeof (Collider))]
public class PutBackScript : MonoBehaviour {

	public GameObject myPickupObject;

	int pickupObjectID;

	void Awake(){

		if(!gameObject.GetComponent<Collider>().isTrigger){
			gameObject.GetComponent<Collider>().isTrigger = true;
		}

		gameObject.layer = LayerMask.NameToLayer("PutBackObjects");

		if(myPickupObject != null) {
			pickupObjectID = myPickupObject.GetInstanceID();
		}

	}

	public int getPickupObjectID(){
		return pickupObjectID;
	}

	public void setPickupObjectID(int id){
		pickupObjectID = id;
	}

}
