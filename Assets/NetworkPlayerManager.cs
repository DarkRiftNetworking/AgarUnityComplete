using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;

public class NetworkPlayerManager : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;

    const ushort MOVE_SUBJECT = 0;
    const ushort RADIUS_SUBJECT = 1;

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    Dictionary<uint, AgarObject> networkPlayers = new Dictionary<uint, AgarObject>();

    public void Awake()
    {
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (TagSubjectMessage message = e.GetMessage() as TagSubjectMessage)
        {
            if (message != null && message.Tag == MOVEMENT_TAG)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint id = reader.ReadUInt32();

                    switch (message.Subject)
                    {
                        case MOVE_SUBJECT:
                            Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
                            networkPlayers[id].SetMovePosition(newPosition);
                            break;

                        case RADIUS_SUBJECT:
                            networkPlayers[id].SetRadius(reader.ReadSingle());
                            break;
                    }
                }
            }
        }
    }

    public void Add(uint id, AgarObject player)
    {
        networkPlayers.Add(id, player);
    }

    public void DestroyPlayer(uint id)
    {
        AgarObject o = networkPlayers[id];

        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    } 
}
