using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Gradient healthbarGradient;

    [SerializeField] GameObject crosshair;
    [SerializeField] GameObject healthbar;
    [SerializeField] GameObject energyBar;
    [SerializeField] Text scoreText;

    Slider healthSlider;
    [SerializeField] Image healthbarImage;
    Slider energySlider;

    private void Start()
    {
        healthSlider = healthbar.GetComponent<Slider>();
        energySlider = energyBar.GetComponent<Slider>();
    }

    public void HideComponents(bool isDead)
    {
        crosshair.SetActive(!isDead);
        healthbar.SetActive(!isDead);
        energyBar.SetActive(!isDead);
    }

    public void SetHealthbarFill(float health)
    {
        health = Mathf.Clamp(health, 0f, 1f);
        healthSlider.value = health;
        healthbarImage.color = healthbarGradient.Evaluate(healthSlider.normalizedValue);
    }

    public void SetEnergyBarFill(float energy, float maxEnergy)
    {
        Debug.Log("Current Energy: " + energy);
        energy /= maxEnergy;
        energySlider.value = energy;
    }

    public void SetScore(string score)
    {
        scoreText.text = score;
    }
}
