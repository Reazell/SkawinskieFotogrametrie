using UnityEngine;

public class InventoryManagerScript : MonoBehaviour {

	public enum eInventoryItems { KEYCARD=0 };

	int inventoryItemCount = 1;
	int[] theInventory;

	void Awake(){
	
		theInventory = new int[inventoryItemCount];

		for(int i = 0; i < inventoryItemCount; i++){
			theInventory[i] = 0;
		}

	}

	public int hasInventoryItem(InventoryManagerScript.eInventoryItems itemToCheck){
		return theInventory[(int)itemToCheck];
	}

	public void giveInventoryItem(InventoryManagerScript.eInventoryItems itemToGive, int quantity=1){
		theInventory[(int)itemToGive] += quantity;
	}

	public bool useInventoryItem(InventoryManagerScript.eInventoryItems itemToUse, int quantity=1){

		bool success = false;

		if(theInventory[(int)itemToUse] >= quantity){
			theInventory[(int)itemToUse] -= quantity;
			success = true;
		}

		return success;

	}

}
