using UnityEngine;
using UnityEngine.UI;

public class FloatingBar : MonoBehaviour
{


    public Image m_FloatingImage;

    // Use this for initialization
    void Awake ()
    {
        Canvas lCanvas      = this.gameObject.GetComponentInChildren<Canvas>();
        lCanvas.worldCamera = Camera.main;
        m_FloatingImage.fillAmount = 1;

    }

    //---------------------------------------------------------------------------------------------------------------
    public void SetFloatingToPercentage(float lAmount)
    {
        m_FloatingImage.fillAmount = lAmount;
    }

    //---------------------------------------------------------------------------------------------------------------


    //---------------------------------------------------------------------------------------------------------------
    public void SetBarColour(Color lColor)
    {
        Transform[] lGameObjects = this.GetComponentsInChildren<Transform>();
        foreach (Transform lCurrent in lGameObjects)
        {
            if (lCurrent.gameObject.name == "FloatingFront")
            {
                lCurrent.GetComponent<Image>().color = lColor;
                break;
            }
        }
    }




}
