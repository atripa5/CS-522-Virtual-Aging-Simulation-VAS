using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExitScript : MonoBehaviour {

    public Canvas QuitMenu;
    public Button exitText;
	// Use this for initialization
	void Start () {
        QuitMenu = QuitMenu.GetComponent<Canvas>();
        exitText = exitText.GetComponent<Button>();
        QuitMenu.enabled = false;
	}

    public void exitPressed()
    {
        QuitMenu.enabled = true;
        exitText.enabled = false;

    }

    public void NoPress()
    {
        QuitMenu.enabled = false;
        exitText.enabled = true;
    }
	
    public void ExitGame()
    {
        Application.Quit ();
    }
	// Update is called once per frame
	void Update () {
	
	}
}
