using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour {

     public Transform target;
    public Vector3 distance;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = target.position + distance;
    }
}
