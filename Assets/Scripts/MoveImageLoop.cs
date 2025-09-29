using UnityEngine;

public class MoveUpLoop : MonoBehaviour
{
    public float speed = 2f;
    public float targetY = 10f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += Vector3.up * speed * Time.deltaTime;

        if (transform.position.y >= targetY)
        {
            transform.position = startPos;
        }
    }
}
