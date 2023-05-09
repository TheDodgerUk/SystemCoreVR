using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingPointsGUIManager : MonoBehaviour
{
    private Animator m_Animator;
    private PoolManagerLocalComponent<Transform> m_Animation;

    void Start()
    {
        m_Animator = this.GetComponentInChildren<Animator>();
        var amount = this.GetComponentInChildren<TextMeshProUGUI>();
        amount.transform.ClearLocals();
        m_Animation = new PoolManagerLocalComponent<Transform>(m_Animator.gameObject, this.transform);
    }

    public void PlayAmount(int amount)
    {
        var animation = m_Animation.SpawnObject();
        animation.SetActive(true);
        animation.GetComponentInChildren<TextMeshProUGUI>().text = $"+{amount}";
        this.WaitFor(4f, () =>
        {
            m_Animation.DeSpawnObject(animation);
        });
    }

    [EditorButton]
    private void DEBUG_PlayAmount20() => PlayAmount(40);
}
