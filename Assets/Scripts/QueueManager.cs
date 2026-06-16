using UnityEngine;
using System.Collections.Generic;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance { get; private set; }

    [Header("Queue Settings")]
    public Transform cartPoint; //titik coordinate gerobak
    public int maxQueueSize = 8; //maksimal antrean

    [Tooltip("Geser nilai X dan Y untuk mencari angle Isometrik yang tepat")]
    public Vector2 queueOffset = new Vector2(-32.5f, -16.25f);
    //list untuk mencatat siapa aja yang lagi antre
    private List<CustomerMovement> customerQueue = new List<CustomerMovement>();

    void Awake()
    {
        Instance = this;
    }

    // Request Queue
    public bool RequestQueueSpot(CustomerMovement customer)
    {
        if (customerQueue.Count >= maxQueueSize)
        {
            return false;
        }

        customerQueue.Add(customer);
        UpdateQueuePositions();
        return true;
    }

    public void LeaveQueue(CustomerMovement customer)
    {
        if (customerQueue.Contains(customer))
        {
            customerQueue.Remove(customer);
            UpdateQueuePositions();
        }
    }

    public void UpdateQueuePositions()
    {
        for (int i = 0; i < customerQueue.Count; i++)
        {
            Vector2 targetPos = (Vector2)cartPoint.position + (queueOffset * i);

            customerQueue[i].UpdateTargetPosition(targetPos, i == 0);
        }
    }
}
