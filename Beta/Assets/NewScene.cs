using UnityEngine;
using System.Collections;

public class NewScene : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Application.LoadLevel("Information2");
        }

    }

}
