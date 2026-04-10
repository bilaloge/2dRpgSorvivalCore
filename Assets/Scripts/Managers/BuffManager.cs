using System.Collections.Generic;
using UnityEngine;
using Zenject;

[System.Serializable]
public class ActiveBuff
{
    public StatusEffects effectData;
    public float remainingDuration;
    public float nextTickTime; // Bir sonraki hasar/iyileþtirme aný
}

public class BuffManager : MonoBehaviour
{
    [Inject] private GameManager _gameManager;
    public static BuffManager Instance { get; private set; }

    [Header("Aktif Durumlar")]
    [SerializeField] private List<ActiveBuff> _activeBuffs = new List<ActiveBuff>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        // Oyun duraklatýlmýþsa efektleri iþletme
        if (_gameManager.CurrentState != GameState.Playing) return;

        ProcessBuffs();
    }
    public void ApplyStatusEffect(StatusEffects newEffect)
    {
        //Ayný isimde bir efekt zaten var mý?
        ActiveBuff existingBuff = _activeBuffs.Find(b => b.effectData.effectName == newEffect.effectName);

        if (existingBuff != null)
        {
            //Efekt zaten varsa süresini yeniliyoruz (Stacklenmez, resetlenir)
            existingBuff.remainingDuration = newEffect.durationInSeconds;
            Debug.Log($"{newEffect.effectName} etkisi yenilendi.");
        }
        else
        {
            //Yeni efekt listeye eklenir
            _activeBuffs.Add(new ActiveBuff
            {
                effectData = newEffect,
                remainingDuration = newEffect.durationInSeconds,
                nextTickTime = Time.time + 1f // Ýlk hasar 1 saniye sonra
            });
            Debug.Log($"{newEffect.effectName} uygulanmaya baþlandý.");
        }
    }

    private void ProcessBuffs()
    {
        if (_activeBuffs.Count == 0) return;

        //Listeden eleman silerken hata almamak için 
        for (int i = _activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = _activeBuffs[i];

            //TICK MANTIÐI (Saniyede bir hasar ver)
            if (Time.time >= buff.nextTickTime)
            {
                ApplyEffectLogic(buff.effectData);
                buff.nextTickTime = Time.time + 1f;
            }

            //SÜRE YÖNETÝMÝ
            if (!buff.effectData.isPermanent)
            {
                buff.remainingDuration -= Time.deltaTime;

                if (buff.remainingDuration <= 0)
                {
                    Debug.Log($"{buff.effectData.effectName} süresi doldu.");
                    _activeBuffs.RemoveAt(i);
                }
            }
        }
    }

    private void ApplyEffectLogic(StatusEffects effect)
    {
        // Deðiþti: HealthSystem'daki yeni 'TakeEffectDamage' metodunu çaðýrýyoruz
        if (effect.healthChange < 0)
        {
            HealthSystem.Instance.TakeEffectDamage(Mathf.Abs(effect.healthChange));
        }
        else if (effect.healthChange > 0)
        {
            HealthSystem.Instance.Heal(effect.healthChange);
        }

        // Ýleride eklenecek: SpeedModifier, ManaRegen vb.
    }
    //Gün sonu (uyku) geldiðinde geçici etkileri temizler
    public void ClearAllBuffs()
    {
        _activeBuffs.Clear();
        Debug.Log("Günün tüm etkileri temizlendi. Karakter yeni güne hazýr.");
    }
}