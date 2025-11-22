using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerStats : NetworkBehaviour
{
    public readonly SyncVar<float> Health = new SyncVar<float>(100f);
    public readonly SyncVar<float> Stamina = new SyncVar<float>(100f);

    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _maxStamina = 100f;
    [SerializeField] private float _staminaRegen = 5f;

    public float MaxHealth => _maxHealth;
    public float MaxStamina => _maxStamina;

    private void Update()
    {
        if (!IsOwner) return;

        if (Stamina.Value < _maxStamina)
            CmdRegenerateStamina(Time.deltaTime * _staminaRegen);
    }

    [ServerRpc]
    private void CmdRegenerateStamina(float amount)
    {
        Stamina.Value = Mathf.Clamp(Stamina.Value + amount, 0, _maxStamina);
    }

    public void TakeDamage(float amount)
    {
        Health.Value = Mathf.Clamp(Health.Value - amount, 0, _maxHealth);
    }

    [ServerRpc]
    public void CmdUseStamina(float amount)
    {
        Stamina.Value = Mathf.Clamp(Stamina.Value - amount, 0, _maxStamina);
    }
}
