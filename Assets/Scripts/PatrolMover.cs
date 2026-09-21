using UnityEngine;

/// <summary>
/// Анімація руху об'єкта між двома точками (патрулювання).
/// </summary>
public class PatrolMover : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, 6f);
    [SerializeField] private float speed = 1.5f;

    private Vector3 pointA;
    private Vector3 pointB;

    private void Start()
    {
        pointA = transform.position;
        pointB = transform.position + offset;
    }

    private void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, 1f);
        transform.position = Vector3.Lerp(pointA, pointB, t);
    }
}
