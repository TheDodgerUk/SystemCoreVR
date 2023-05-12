using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUD : MonoBehaviour {

    public Text m_playerScoreText;
    public Text m_playerHealthText;
    private GameObject m_player;
    private Health m_health;
	// Use this for initialization
	void Start () 
    {
        IsThereAValidPlayer();
	}
	
	// Update is called once per frame
	void OnGUI() 
    {
        if (!IsThereAValidPlayer()) 
            return;

        m_playerScoreText.text = "player score";
       // m_playerHealthText.text = "player Health" + m_health.m_healthCurrent;
	}


    bool IsThereAValidPlayer()
    {
        if ( m_player)
            return true;

        m_player = GameObject.FindGameObjectWithTag("Player");
        if (m_player)
        {
            m_health = m_player.GetComponent<Health>();
            
            return true;
        }
        return false;
    }

}
