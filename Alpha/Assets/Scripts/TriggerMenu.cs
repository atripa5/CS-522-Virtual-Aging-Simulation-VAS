using UnityEngine;
using System.Collections;

public class TriggerMenu : MonoBehaviour
{

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Application.LoadLevel("VisMenu");
        }

    }

}
