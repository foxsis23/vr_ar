using UnityEngine;

/// <summary>
/// Проста ігрова логіка: рахунок, здоров'я, повідомлення на екрані.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float messageDuration = 2f;

    private int score;
    private int health;
    private string message = "";
    private float messageTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        health = maxHealth;
    }

    private void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f) message = "";
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        ShowMessage($"Зібрано монету! Рахунок: {score}");
    }

    public void TakeDamage(int amount)
    {
        health = Mathf.Max(0, health - amount);
        ShowMessage($"Удар об перешкоду! Здоров'я: {health}");
    }

    public void Finish()
    {
        ShowMessage($"Фініш! Підсумковий рахунок: {score}");
    }

    public void ShowMessage(string text)
    {
        message = text;
        messageTimer = messageDuration;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20f, 20f, 400f, 24f), $"Рахунок: {score}");
        GUI.Label(new Rect(20f, 44f, 400f, 24f), $"Здоров'я: {health}");
        GUI.Label(new Rect(20f, 68f, 600f, 24f), "WASD / стрілки - рух, Space - стрибок, Shift - прискорення");

        if (!string.IsNullOrEmpty(message))
        {
            GUI.Label(new Rect(20f, 100f, 600f, 24f), message);
        }
    }
}
