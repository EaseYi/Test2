using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    TextMeshProUGUI levelText;
    Image healthSlider;
    Image expSlider;

    private void Awake()
    {
        levelText=transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        healthSlider=transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider= transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }
    private void Update()
    {
        levelText.text = "Level " + GameManager.Instance.playerStats.characterData.currentLevel.ToString("00");
        HealthUpdate();
        ExpUpdate();
    }
    void HealthUpdate()
    { 
        float sliderPrecent=(float)GameManager.Instance.playerStats.CurrentHealth/GameManager.Instance.playerStats.MaxHealth;
        healthSlider.fillAmount= sliderPrecent;
    }
    void ExpUpdate()
    {
        float sliderPrecent = (float)GameManager.Instance.playerStats.characterData.currentExp / GameManager.Instance.playerStats.characterData.baseExp;
        expSlider.fillAmount = sliderPrecent;
    }
}
