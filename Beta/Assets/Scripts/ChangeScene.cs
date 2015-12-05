using UnityEngine;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Application.LoadLevel("Main 2");
        }
       
    }

}
