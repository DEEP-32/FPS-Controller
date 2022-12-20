using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthText = default;
    [SerializeField] private TextMeshProUGUI staminaText = default;

    private void OnEnable()
    {
        FirstPersonController.OnDamage += UpdateHealth;
        FirstPersonController.OnHeal += UpdateHealth;
        FirstPersonController.OnStaminaChange += UpdateStamina;
    }
    private void OnDisable()
    {
        FirstPersonController.OnHeal -= UpdateHealth;
        FirstPersonController.OnDamage -= UpdateHealth;
        FirstPersonController.OnStaminaChange -= UpdateStamina;
    }

    private void Start()
    {
        UpdateHealth(100);
        UpdateStamina(100);
    }
    private void UpdateHealth(float health)
    {
        healthText.text = health.ToString("00");
    }
    private void UpdateStamina(float stamina)
    {
        staminaText.text = stamina.ToString("00");
    }

}
