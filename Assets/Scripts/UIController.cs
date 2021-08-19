/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField]
    GameObject UIPrefab;
    GameObject UIInstance;

    Slider slider;

    public void SetHealthbarFill(float health)
    {
        health = Mathf.Clamp(health, 0f, 1f);
*//*        healthbarFill.transform.localScale = new Vector3(health, 1f, 1f);*//*
        Debug.Log("from uiControl " + health);
        slider.value = health;
    }


    public void InstantiateUI()
    {
        UIInstance = Instantiate(UIPrefab);
        slider = UIInstance.GetComponent<Slider>();
    }

    public void ShowUI(bool showing)
    {
        UIInstance.SetActive(showing);
    }


*//*    public void SetPlayerManager(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }*//*
}
*/