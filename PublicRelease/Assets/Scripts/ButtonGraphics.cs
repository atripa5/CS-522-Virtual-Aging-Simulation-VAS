using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class ButtonGraphics : MonoBehaviour
{
    public void SetActive(bool status)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Text[] texts = GetComponentsInChildren<Text>();
        Image[] GUIimages = GetComponentsInChildren<Image>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = status;
        }
        foreach (Text text in texts)
        {
            text.enabled = status;
            string temp = text.ToString();
        }
        foreach (Image image in GUIimages)
        {
            image.enabled = status;
        }

    }

    public void SetColor(Color color)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Text[] texts = GetComponentsInChildren<Text>();
        Image[] GUIimages = GetComponentsInChildren<Image>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
        foreach (Text text in texts)
        {
            text.color = color;
        }
        foreach (Image image in GUIimages)
        {
            image.color = color;
        }
    }
}
