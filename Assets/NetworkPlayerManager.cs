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

    public void Start()
    {
        client.Subscribe(MOVEMENT_TAG, MovementMessageReceived);
    }

    void MovementMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        DarkRiftReader reader = e.Message.GetReader();

        uint id = reader.ReadUInt32();

        switch (((TagSubjectMessage)e.Message).Subject)
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
