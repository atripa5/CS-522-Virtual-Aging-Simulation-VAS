using UnityEngine;
using System.Collections;

public class changescene : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        StartCoroutine("Countdown");
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(20);
        Application.LoadLevel(9);
    }
    // Update is called once per frame
    //	void Update () {

    //}
}

