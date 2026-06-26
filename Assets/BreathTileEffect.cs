using UnityEngine;

public class BreathTileEffect : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.35f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
