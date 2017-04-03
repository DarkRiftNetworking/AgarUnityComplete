using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class AgarObject : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The speed that the player will move.")]
    float speed = 1f;

    [SerializeField]
    [Tooltip("Multiplier for the scaling of the player.")]
    float scale = 1f;

    Vector3 movePosition;

    void Awake()
    {
        movePosition = transform.position;
    }

    void Update()
    {
        if (speed != 0f)
            transform.position = Vector3.MoveTowards(transform.position, movePosition, speed * Time.deltaTime);
    }

    internal void SetColor(Color32 color)
    {
        Renderer renderer = GetComponent<Renderer>();

        renderer.material.color = color;
    }

    internal void SetRadius(float radius)
    {
        transform.localScale = new Vector3(radius * scale, radius * scale, 1);
    }

    internal void SetMovePosition(Vector3 newPosition)
    {
        movePosition = newPosition;
    }
}
