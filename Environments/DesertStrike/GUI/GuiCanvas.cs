using UnityEngine;
using UnityEngine.UI;

public class GuiCanvas : MonoBehaviour {

    int m_Score = 0;

    [SerializeField]
    Text m_ScoreText;
    [SerializeField]
    Text m_NumberleftText;
    public UIInput m_Input;
    // Use this for initialization
    void Awake ()
    {
        MESSAGE_CANVAS_ADD_SCORE(0);
    }
	
    void MESSAGE_CANVAS_ADD_SCORE(int lAmount)
    {
        m_Score += lAmount;
        m_ScoreText.text = "Score:" + m_Score;
    }
	// Update is called once per frame
	void Update ()
    {
        

    }
}
