using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ConveyorGambler : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Item[] items;

    [Header("Settings")]
    [SerializeField] private AnimationCurve rollCurve;
    [SerializeField][Min(3)] private int visibleItemAmount;

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


    private void Start()
    {
        SpawnItems();
    }

    private void SpawnItems()
    {
        // Spawn items for later use
        spawnedItems = new SpriteRenderer[visibleItemAmount];
        float startX = -(visibleItemAmount - 1) * ItemSpacing / 2; // Calculate starting x position

        for (int i = 0; i < visibleItemAmount; i++)
        {
            spawnedItems[i] = Instantiate(ItemPrefab, ItemParent);
            spawnedItems[i].transform.localPosition = new Vector3(startX + i * ItemSpacing, 0, 0); // Set position
        }
    }

    public void Roll()
    {
        ItemParent.localPosition = Vector2.zero;

        Item winnerItem = items[Random.Range(0, items.Length)];
        int winnerIndex = Random.Range(minSkippedItems, maxSkippedItems);
        float winnerPosition = ItemSpacing * winnerIndex;

        ScrambleItems();

        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);

            RollDone(winnerItem);
        }
        rollCoroutine = StartCoroutine(LerpToWinner(winnerPosition, winnerItem, winnerIndex));
    }

    private void ScrambleItems()
    {
        // Set filler items
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            int randomItemIndex = Random.Range(0, items.Length);
            spawnedItems[i].color = items[randomItemIndex].color;
        }
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

                // Move left most item to right most position
                SpriteRenderer leftMostItem = spawnedItems[0];
                for (int i = 0; i < spawnedItems.Length - 1; i++)
                {
                    spawnedItems[i] = spawnedItems[i + 1];
                }
                spawnedItems[spawnedItems.Length - 1] = leftMostItem;
                leftMostItem.transform.localPosition = new Vector3(spawnedItems[spawnedItems.Length - 2].transform.localPosition.x + ItemSpacing, 0, 0);
                leftMostItem.color = items[Random.Range(0, items.Length)].color;

                if (skipped -1 == winnerIndex)
                {
                    leftMostItem.color = winnerItem.color;
                }
            }

            yield return null;
        }

        ItemParent.localPosition = new Vector2(endPosition, 0);

        RollDone(winnerItem);

        rollCoroutine = null;
    }

    private void RollDone(Item winnerItem)
    {
        Debug.Log("Roll Done. Winner is: " + winnerItem.name);
        winnerItem.OnWin.Invoke();
    }
}
