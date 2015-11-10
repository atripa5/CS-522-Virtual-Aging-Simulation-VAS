using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {

	public void LoadScene(string name)
    {
        Application.LoadLevel(name);
    }
}
