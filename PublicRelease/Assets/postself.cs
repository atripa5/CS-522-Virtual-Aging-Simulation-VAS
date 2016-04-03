using UnityEngine;
using System.Collections;

public class postself : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine("Countdown");
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(30);
        Application.LoadLevel(28);
    }
    // Update is called once per frame
    //	void Update () {

    //}
}


