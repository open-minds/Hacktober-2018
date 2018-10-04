using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerBehaviour : MonoBehaviour {

	NavMeshAgent nma;
	Vector3 target;
	Collider collider;
	int gardenMask;

	// Use this for initialization
	void Start () {
		nma = GetComponent<NavMeshAgent>();	
		collider = GetComponent<Collider>();
		target = transform.position;	
		gardenMask = LayerMask.GetMask("Garden");
	}
	
	// Update is called once per frame
	void Update () {
		nma.SetDestination(target);
		if(Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(Physics.Raycast(ray,out hit)){
				if(hit.collider != collider){
					target = hit.point;
					print(target);
				}
			}
		}
	}
}
