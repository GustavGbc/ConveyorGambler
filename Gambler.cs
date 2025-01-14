using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct Item
{
    public string name;
    public Color color;
    public UnityEvent OnWin;
}

public class Gambler : MonoBehaviour
{
    [Header("Items")]
    public Item guarenteedWinner;
    [SerializeField] private Item[] items;

    [Header("Settings")]
    [SerializeField] private AnimationCurve rollCurve;

    [SerializeField] private float ItemSpacing;

    [SerializeField] private int minFillerItems;
    [SerializeField] private int maxFillerItems;

    [SerializeField] private float minDuration;
    [SerializeField] private float maxDuration;

    [Header("References")]
    [SerializeField] private Transform ItemParent;
    [SerializeField] private SpriteRenderer ItemPrefab;
    
    private Coroutine rollCoroutine;
    private SpriteRenderer[] spawnedItems;

    // Spawn in max possible prefabs
    // Set spawned prefabs to random items
    // Set winner item to specefic item

    private void Start()
    {
        // Spawn items for later use
        spawnedItems = new SpriteRenderer[maxFillerItems];
        for (int i = 0; i < maxFillerItems; i++)
        {
            spawnedItems[i] = Instantiate(ItemPrefab, ItemParent);
        }
    }

    public void Roll()
    {
        Item winnerItem = guarenteedWinner.Equals(null) ? items[Random.Range(0, items.Length)] : guarenteedWinner;
        int winnerIndex = Random.Range(minFillerItems, maxFillerItems);
        float winnerPosition = ItemSpacing * winnerIndex;

        ScrambleItems(winnerIndex ,winnerItem);

        if (rollCoroutine != null)
        {
            StopCoroutine(rollCoroutine);

            RollDone(winnerItem);
        }
        rollCoroutine = StartCoroutine(LerpToWinner(winnerPosition, winnerItem));
    }

    private void ScrambleItems(int winnerIndex , Item winnerItem)
    {
        // Set filler items
        for (int i = 0; i < spawnedItems.Length; i++)
        {
            int randomItemIndex = Random.Range(0, items.Length);
            spawnedItems[i].color = items[randomItemIndex].color;
            spawnedItems[i].transform.localPosition = new Vector3(i * ItemSpacing, 0, 0);
        }

        // Set winner item
        spawnedItems[winnerIndex].color = winnerItem.color;
    }

    private IEnumerator LerpToWinner(float winnerPosition, Item winnerItem)
    {
        float t = 0;
        float duration = Random.Range(minDuration, maxDuration);
        float startPosition = ItemParent.localPosition.x;
        float endPosition = -winnerPosition;

        while (t < duration)
        {
            t += Time.deltaTime;
            ItemParent.localPosition = new Vector2(Mathf.Lerp(startPosition, endPosition, rollCurve.Evaluate(t)), 0);//Vector3.Lerp(startPosition, endPosition, t / duration);
            yield return null;
        }

        ItemParent.localPosition = new Vector2(endPosition, 0);

        RollDone(winnerItem);
    }

    private void RollDone(Item winnerItem)
    {
        Debug.Log("Roll Done. Winner is: " + winnerItem.name);
        winnerItem.OnWin.Invoke();
    }
}
