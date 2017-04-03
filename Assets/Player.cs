using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using DarkRift;

public class Player : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    const ushort MOVE_SUBJECT = 0;

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
            DarkRiftWriter writer = new DarkRiftWriter();
            writer.Write(transform.position.x);
            writer.Write(transform.position.y);

            TagSubjectMessage message = new TagSubjectMessage(MOVEMENT_TAG, MOVE_SUBJECT, writer);

            Client.SendMessage(message, SendMode.Unreliable);

            lastPosition = transform.position;
        }
    }
}
