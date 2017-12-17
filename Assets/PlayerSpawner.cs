using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using DarkRift.Client;
using System;
using DarkRift;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network controllable player prefab.")]
    GameObject networkPrefab;

    [SerializeField]
    [Tooltip("The network player manager.")]
    NetworkPlayerManager networkPlayerManager;

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.SpawnPlayerTag)
                SpawnPlayer(sender, e);
            else if (message.Tag == Tags.DespawnPlayerTag)
                DespawnPlayer(sender, e);
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 17 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet.");
                return;
            }

            while (reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                float radius = reader.ReadSingle();
                Color32 color = new Color32(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    255
                );

                Debug.Log("Spawning client for ID = " + id + ".");

                GameObject obj;
                if (id == client.ID)
                {
                    obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;

                    Player player = obj.GetComponent<Player>();
                    player.Client = client;

                    Camera.main.GetComponent<CameraFollow>().Target = obj.transform;
                }
                else
                {
                    obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                }

                AgarObject agarObj = obj.GetComponent<AgarObject>();

                agarObj.SetRadius(radius);
                agarObj.SetColor(color);

                networkPlayerManager.Add(id, agarObj);
            }
        }
    }

    void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
            networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
    }
}
