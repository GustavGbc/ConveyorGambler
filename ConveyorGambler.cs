using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Make ui version of this, with ui masking
// https://learn.unity.com/tutorial/ui-masking#

public class ConveyorGambler : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Item[] items;

    [Header("Settings")]
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField][Min(3)] private int visibleItemAmount;

    [SerializeField] private bool winnerOffset;
    [SerializeField] private float ItemSpacing;

    [SerializeField] private int minSkippedItems;
    [SerializeField] private int maxSkippedItems;

    [SerializeField] private float minDuration;
    [SerializeField] private float maxDuration;

    [Header("References")]
    [SerializeField] private Transform ItemParent;
    [SerializeField] private SpriteRenderer ItemPrefab;
    
    private Coroutine rollCoroutine;
    private SpriteRenderer[] spawnedItems;

    public void SetupItems()
    {
        // Check if items already spawned and is same size as visible item amount
        if (spawnedItems == null || spawnedItems.Length != visibleItemAmount)
        {
            if (spawnedItems != null)
            {
                foreach (var item in spawnedItems)
                {
                    Destroy(item.gameObject);
                }
            }
            
            spawnedItems = new SpriteRenderer[visibleItemAmount];

            for (int i = 0; i < visibleItemAmount; i++)
            {
                spawnedItems[i] = Instantiate(ItemPrefab, ItemParent);
            }
        }
        
        // Setup item positions in correct order
        float startX = -(visibleItemAmount - 1) * ItemSpacing / 2; // Calculate starting x position
        for (int i = 0; i < visibleItemAmount; i++)
        {
            spawnedItems[i].transform.localPosition = new Vector3(startX + i * ItemSpacing, 0, 0); // Set position
        }
    }

    public void Roll()
    {
        SetupItems();

        ItemParent.localPosition = Vector2.zero;

        float startX = -(visibleItemAmount - 1) * ItemSpacing / 2; // Calculate starting x position
        for (int i = 0; i < visibleItemAmount; i++)
        {
            spawnedItems[i].transform.localPosition = new Vector3(startX + i * ItemSpacing, 0, 0); // Set position
        }

        Item winnerItem = items[Random.Range(0, items.Length)];
        int winnerIndex = Random.Range(minSkippedItems, maxSkippedItems);
        float winnerPosition = ItemSpacing * winnerIndex;

        // Adjust winnerPosition if visibleItemAmount is even
        if (visibleItemAmount % 2 == 0)
        {
            winnerPosition -= ItemSpacing / 2;
        }

        if(winnerOffset)
        {
            winnerPosition += Random.Range(-ItemSpacing / 2, ItemSpacing / 2);
        }

        // Set filler items
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            ApplyRandomItemGraphics(i);
        }

        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);

            RollDone(winnerItem);
        }

        rollCoroutine = StartCoroutine(LerpToWinner(winnerPosition, winnerItem, winnerIndex));
    }

    private IEnumerator LerpToWinner(float winnerPosition, Item winnerItem, int winnerIndex)
    {
        float t = 0;
        float duration = Random.Range(minDuration, maxDuration);
        float startPosition = ItemParent.localPosition.x;
        float endPosition = -winnerPosition;
        float lastPosition = startPosition;
        int skipped = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            float newX = Mathf.Lerp(startPosition, endPosition, rollCurve.Evaluate(t / duration));
            ItemParent.localPosition = new Vector2(newX, 0);

            // Check if moved itemspacing distance
            while (Mathf.Abs(newX - lastPosition) >= ItemSpacing)
            {
                lastPosition += Mathf.Sign(newX - lastPosition) * ItemSpacing;
                skipped++;
                ShiftItems(skipped, winnerIndex, winnerItem);
            }

            yield return null;
        }

        // Final shift check with a small tolorance
        if(Mathf.Abs(endPosition - lastPosition) >= ItemSpacing - 0.1f)
        {
            skipped++;
            ShiftItems(skipped, winnerIndex, winnerItem);
        }

        ItemParent.localPosition = new Vector2(endPosition, 0);
        RollDone(winnerItem);
        rollCoroutine = null;
    }

    private void ShiftItems(int skipped, int winnerIndex, Item winnerItem)
    {
        // Move left most item to right most position
        SpriteRenderer leftMostItem = spawnedItems[0];
        for (int i = 0; i < spawnedItems.Length - 1; i++)
        {
            spawnedItems[i] = spawnedItems[i + 1];
        }
        spawnedItems[spawnedItems.Length - 1] = leftMostItem;
        leftMostItem.transform.localPosition = new Vector3(spawnedItems[spawnedItems.Length - 2].transform.localPosition.x + ItemSpacing, 0, 0);

        ApplyRandomItemGraphics(spawnedItems.Length - 1);

        if (skipped == winnerIndex - visibleItemAmount / 2)
        {
            Debug.Log("Spawn winner item: " + winnerItem.name);
            ApplyItemGraphics(leftMostItem, winnerItem);
        }
    }

    private void ApplyRandomItemGraphics(int index)
    {
        int randomItemIndex = Random.Range(0, items.Length);
        spawnedItems[index].color = items[randomItemIndex].color;
    }

    private void ApplyItemGraphics(SpriteRenderer item, Item itemData)
    {
        item.color = itemData.color;
    }

    private void RollDone(Item winnerItem)
    {
        Debug.Log("Roll Done. Winner is: " + winnerItem.name);
        winnerItem.OnWin.Invoke();
    }
}
