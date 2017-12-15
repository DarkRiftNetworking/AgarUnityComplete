using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;

public class NetworkPlayerManager : MonoBehaviour
{
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
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.MovePlayerTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint id = reader.ReadUInt32();
                    
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
                    networkPlayers[id].SetMovePosition(newPosition);
                }
            }
            else if (message.Tag == Tags.SetRadiusTag)
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    uint id = reader.ReadUInt32();

                    networkPlayers[id].SetRadius(reader.ReadSingle());
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
