using UnityEngine;
using System.Collections;
using DarkRift.Client.Unity;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;

public class FoodManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The food item prefab.")]
    GameObject foodPrefab;

    Dictionary<ushort, AgarObject> food = new Dictionary<ushort, AgarObject>();

    void Awake()
    {
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            if (message.Tag == Tags.SpawnFoodTag)
                SpawnFood(sender, e);
            if (message.Tag == Tags.MoveFoodTag)
                MoveFood(sender, e);
        }
    }

    void SpawnFood(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 13 != 0)
            {
                Debug.LogWarning("Received malformed spawn packet.");
                return;
            }

            while (reader.Position < reader.Length)
            {
                ushort id = reader.ReadUInt16();
                Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());
                Color32 color = new Color32(
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    255
                );

                Debug.Log("Spawning food with ID = " + id + ".");

                GameObject obj = Instantiate(foodPrefab, position, Quaternion.identity) as GameObject;

                AgarObject agarObj = obj.GetComponent<AgarObject>();

                agarObj.SetRadius(0.2f);
                agarObj.SetColor(color);

                food.Add(id, agarObj);
            }
        }
    }

    void MoveFood(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            if (reader.Length % 10 != 0)
            {
                Debug.LogWarning("Received malformed move packet.");
                return;
            }

            ushort id = reader.ReadUInt16();
            Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle());

            if (food.ContainsKey(id))
                food[id].transform.position = position;
        }
    }
}
