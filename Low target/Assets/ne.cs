using UnityEngine;
using System.Collections;

public class ne : MonoBehaviour {
    Animator anime;

	// Use this for initialization
	void Start () {
        anime = GetComponent<Animator>();
	
	}
	
	// Update is called once per frame
	void Update () {
        
        if (anime)
        {
            Vector3 newPosition = transform.position;
            newPosition.z += anime.GetFloat("Speed") * Time.deltaTime;
            transform.position = newPosition;
        }



    }
}
