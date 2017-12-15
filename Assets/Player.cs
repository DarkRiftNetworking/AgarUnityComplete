using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using DarkRift;

public class Player : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }

    Vector3 lastPosition;

    void Awake()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);

                using (Message message = Message.Create(Tags.MovePlayerTag, writer))
                    Client.SendMessage(message, SendMode.Unreliable);
            }

            lastPosition = transform.position;
        }
    }
}
