using UnityEngine;

/// <summary>
/// Проста анімація об'єкта скриптом: обертання + коливання вгору-вниз.
/// </summary>
public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);
    [SerializeField] private float bobAmplitude = 0.25f;
    [SerializeField] private float bobSpeed = 2f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime, Space.Self);

        if (bobAmplitude > 0f)
        {
            float offset = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.position = startPosition + new Vector3(0f, offset, 0f);
        }
    }
}
