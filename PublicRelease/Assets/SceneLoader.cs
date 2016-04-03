using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        StartCoroutine("Countdown");
	}
	
    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(5);
        Application.LoadLevel(1);
    }
	// Update is called once per frame
//	void Update () {
	
	//}
}
