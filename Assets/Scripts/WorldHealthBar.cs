using UnityEngine;
using UnityEngine.UI;

public class WorldHealthBar : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Damageable damageable;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Text healthText;
    [SerializeField] private bool hideWhenFull = true;

    private Camera mainCamera;

    private void Awake()
    {
        if (damageable == null)
        {
            damageable = GetComponentInParent<Damageable>();
        }

        if (target == null && damageable != null)
        {
            target = damageable.transform;
        }

        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (damageable == null)
        {
            return;
        }

        damageable.EnsureInitialized();
        damageable.onHealthChanged.AddListener(UpdateHealth);
        UpdateHealth(damageable.CurrentHealth, damageable.MaxHealth);
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }

    private void OnDisable()
    {
        if (damageable != null)
        {
            damageable.onHealthChanged.RemoveListener(UpdateHealth);
        }
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        float percent = maxHealth <= 0f ? 0f : currentHealth / maxHealth;

        if (healthSlider != null)
        {
            healthSlider.value = percent;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
        }

        if (hideWhenFull)
        {
            bool shouldShow = currentHealth < maxHealth && currentHealth > 0f;
            SetChildrenActive(shouldShow);
        }
    }

    private void SetChildrenActive(bool active)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
    }
}
