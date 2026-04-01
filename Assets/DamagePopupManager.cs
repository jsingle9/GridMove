using UnityEngine;
using System.Collections.Generic;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private DamagePopup damagePopupPrefab;
    private Queue<DamagePopup> popupPool = new Queue<DamagePopup>();
    private int poolSize = 10;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePool();
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            DamagePopup popup = Instantiate(damagePopupPrefab, transform);
            popup.gameObject.SetActive(false);
            popupPool.Enqueue(popup);
        }
    }

    public void ShowDamage(int damageAmount, Vector3 worldPosition)
    {
        DamagePopup popup;

        if (popupPool.Count > 0)
        {
            popup = popupPool.Dequeue();
        }
        else
        {
            popup = Instantiate(damagePopupPrefab, transform);
        }

        popup.gameObject.SetActive(true);
        popup.Show(damageAmount, worldPosition);
    }

    public void ReturnToPool(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        popupPool.Enqueue(popup);
    }
}
