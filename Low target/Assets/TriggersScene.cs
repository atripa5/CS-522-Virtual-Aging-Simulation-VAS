using UnityEngine;
using System.Collections;

public class TriggersScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        print("Still colliding with trigger object " + other.name);
        //If a gameObject with the tag "Player" enters this trigger, load a scene.
        if (other.gameObject.tag == "Player")
        {
            Application.LoadLevel("hospital_entrance");
        }
    }
}
