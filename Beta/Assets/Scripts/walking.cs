using UnityEngine;
using System.Collections;

public class walking : MonoBehaviour {

    private Animator animator;
    private float vertical;
    private float horizontal;

	// Use this for initialization
	void Start () {

        animator = GetComponent<Animator> ();

	}
	
	// Update is called once per frame
	void Update () {

        vertical = Input.GetAxis ("Vertical");
        horizontal = Input.GetAxis ("Horizontal");
        animator.SetFloat ("walk", vertical);
        animator.SetFloat ("turn", horizontal);
	
	}
}
