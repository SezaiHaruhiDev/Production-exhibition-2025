using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float delay = 1.2f;
    void Start()
    {
        Destroy(gameObject, delay);
    }
}