using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    [SerializeField] private float delay = 1.2f; // アニメーションの長さに合わせる
    void Start()
    {
        Destroy(gameObject, delay);
    }
}