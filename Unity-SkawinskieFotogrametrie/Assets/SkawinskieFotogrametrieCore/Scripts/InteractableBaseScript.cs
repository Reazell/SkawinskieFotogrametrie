using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableBaseScript: MonoBehaviour {

	public enum eInteractionType {STATIC, PICKUP, ACTIVATE, JOURNAL, AUDIODIARY, INVENTORY};
	protected eInteractionType interactionType = eInteractionType.STATIC;

	protected bool canInteractWithWhileHoldingObject = true;
	public bool highlightOnMouseOver = true;
	public Material objectHighlightMaterial;

	bool highlightMaterialSet;
	bool hasBeenDiscovered;
	public float interactionDistance = 2.0f;
	public string interactionString = "<DEFAULT INTERACTION STRING>";

	public virtual void Awake() {
		if(!objectHighlightMaterial){
			objectHighlightMaterial = Resources.Load("ObjectHighlightMaterial") as Material;
			if(!objectHighlightMaterial){
				Debug.LogError("Brak ObjectHighlightMaterial w folderze Resources");
			}
		}

	}
	public virtual void interact() { 
	}

	public void highlightObject(){

		if(!hasBeenDiscovered){
			discoverObject();
		}

		setHighlightMaterial();

	}
	
	public void unHighlightObject(){
		removeHighlightMaterial();
	}

	public eInteractionType getInteractionType(){
		return interactionType;
	}

	public bool interactionsAllowedWhenHoldingObject(){
		return canInteractWithWhileHoldingObject;
	}

	public float getInteractionDistance(){
		return interactionDistance;
	}

	void setHighlightMaterial(){

		if(objectHighlightMaterial && !highlightMaterialSet && highlightOnMouseOver){
			
			MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();

			foreach(MeshRenderer m in childMeshRenderers){
				
				Material[] childMaterials = m.materials;
				int materialCount = childMaterials.Length;
				Material[] newMaterials = new Material[materialCount + 1];
				
				for(int i = 0; i < materialCount; i++){
					newMaterials[i] = childMaterials[i];
				}
				
				Texture mainTexture = m.material.mainTexture;
				newMaterials[materialCount] = objectHighlightMaterial;
				
				if(mainTexture){
					objectHighlightMaterial.mainTexture = mainTexture;
				}else{
				}
				
				m.materials = newMaterials;
				
				MeshFilter[] childMeshFilters = gameObject.GetComponentsInChildren<MeshFilter>();

				foreach(MeshFilter mf in childMeshFilters){

					if(mf && mf.mesh && !gameObject.isStatic){
						mf.mesh.RecalculateBounds();
					}

				}
				
			}

			highlightMaterialSet = true;
			
		}
		
	}
	
	void removeHighlightMaterial(){

		if(objectHighlightMaterial && highlightMaterialSet){
			
			MeshRenderer[] childMeshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
			
			foreach(MeshRenderer m in childMeshRenderers){
				
				Material[] childMaterials = m.materials;
				int childMaterialsCount = childMaterials.Length - 1;
				
				if(childMaterialsCount >= 0){
					
					Material[] resetMaterials = new Material[childMaterialsCount];
					
					for(int i = 0; i < childMaterialsCount; i++){
						resetMaterials[i] = childMaterials[i];
					}
					
					m.materials = resetMaterials;
					
					if(!gameObject.isStatic){
						gameObject.GetComponentInChildren<MeshFilter>().mesh.RecalculateBounds();
					}
					
				}
				
			}

			highlightMaterialSet = false;

		}
		
	}

	public virtual void discoverObject(){
		hasBeenDiscovered = true;
	}

}
