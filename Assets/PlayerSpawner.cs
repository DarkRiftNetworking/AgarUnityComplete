using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using DarkRift.Client;
using System;
using DarkRift;

public class PlayerSpawner : MonoBehaviour
{
    const byte SPAWN_TAG = 0;

    const ushort SPAWN_SUBJECT = 0;
    const ushort DESPAWN_SUBJECT = 1;

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
        using (TagSubjectMessage message = e.GetMessage() as TagSubjectMessage)
        {
            if (message != null && message.Tag == SPAWN_TAG)
            {
                if (message.Subject == SPAWN_SUBJECT)
                    SpawnPlayer(sender, e);
                else
                    DespawnPlayer(sender, e);
            }
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 19 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet.");
                return;
            }

            while (reader.Position < reader.Length)
            {
                uint id = reader.ReadUInt32();
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
        using (DarkRiftReader reader = e.GetMessage().GetReader())
            networkPlayerManager.DestroyPlayer(reader.ReadUInt32());
    }
}
