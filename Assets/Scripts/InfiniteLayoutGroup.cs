using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class InfiniteLayoutGroup : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform parentRectT;
    private RectTransform thisRectT;
    public RectOffset padding;
    public Vector2 cellSize;
    public Vector2 spacing;
    private Vector2Int dimension;
    public GameObject cellPrefab;
    private List<RectTransform> cacheCellRectTsList = new List<RectTransform>();
    private float lastVerticalNormalizedPosition;
    private int startCacheCellDataIndex;
    private int lastCacheCellDataIndex;
    private int cellCount;
    private UnityAction<GameObject, int> onCellViewed;
    private bool isInited;
    private bool isRelayout;
    void Awake()
    {
        scrollRect = this.GetComponentInParent<ScrollRect>();
        parentRectT = this.transform.parent.GetComponent<RectTransform>();
        thisRectT = this.GetComponent<RectTransform>();
        lastVerticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
    }
    void Update()
    {
        if (cacheCellRectTsList.Count > 0)
        {
            if (isRelayout || cacheCellRectTsList[cacheCellRectTsList.Count - 1].position.y + cacheCellRectTsList[cacheCellRectTsList.Count - 1].rect.yMax <= thisRectT.position.y - thisRectT.rect.height)
            {
                int startPrevRow = Mathf.FloorToInt((thisRectT.localPosition.y + spacing.y - padding.top) / (cellSize.y + spacing.y));
                if (startPrevRow < 0)
                {
                    startPrevRow = 0;
                }
                float startPos = -padding.top + cellSize.y * (cacheCellRectTsList[cacheCellRectTsList.Count - 1].pivot.y - 1f) - startPrevRow * (cellSize.y + spacing.y);
                if (startPrevRow > 0 && Mathf.CeilToInt((startPos + thisRectT.rect.height) / (cellSize.y + spacing.y)) < Mathf.CeilToInt(cacheCellRectTsList.Count * 1f / dimension.x))
                {
                    startPrevRow--;
                    startPos += cellSize.y + spacing.y;
                }
                startCacheCellDataIndex = startPrevRow * dimension.x;
                lastCacheCellDataIndex = startCacheCellDataIndex - 1;
                for (int i = 0; i < cacheCellRectTsList.Count; i++)
                {
                    float posY = startPos - (i / dimension.x) * (cellSize.y + spacing.y);
                    cacheCellRectTsList[i].localPosition = new Vector3(cacheCellRectTsList[i].localPosition.x, posY);
                    if (lastCacheCellDataIndex == cellCount - 1)
                    {
                        cacheCellRectTsList[i].gameObject.SetActive(false);
                    }
                    else
                    {
                        cacheCellRectTsList[i].gameObject.SetActive(true);
                        lastCacheCellDataIndex++;
                        onCellViewed?.Invoke(cacheCellRectTsList[i].gameObject, lastCacheCellDataIndex);
                    }
                }
                isRelayout = false;
            }
            if (scrollRect.verticalNormalizedPosition < lastVerticalNormalizedPosition && lastCacheCellDataIndex != cellCount - 1)
            {
                if (cacheCellRectTsList[cacheCellRectTsList.Count - 1].position.y + cacheCellRectTsList[cacheCellRectTsList.Count - 1].rect.yMin >= parentRectT.position.y)
                {
                    isRelayout = true;
                }
                else
                {
                    List<RectTransform> updateRectTsList = new List<RectTransform>();
                    foreach (var item in cacheCellRectTsList)
                    {
                        if (item.position.y + item.rect.yMin > parentRectT.position.y)
                        {
                            updateRectTsList.Add(item);
                        }
                    }
                    float startPos = cacheCellRectTsList[cacheCellRectTsList.Count - 1].localPosition.y;
                    for (int i = 0; i < updateRectTsList.Count; i++)
                    {
                        cacheCellRectTsList.Remove(updateRectTsList[i]);
                        cacheCellRectTsList.Add(updateRectTsList[i]);
                        float posY = startPos - (i / dimension.x + 1) * (cellSize.y + spacing.y);
                        updateRectTsList[i].localPosition = new Vector3(updateRectTsList[i].localPosition.x, posY);
                        startCacheCellDataIndex++;
                        if (lastCacheCellDataIndex == cellCount - 1)
                        {
                            updateRectTsList[i].gameObject.SetActive(false);
                        }
                        else
                        {
                            lastCacheCellDataIndex++;
                            onCellViewed?.Invoke(updateRectTsList[i].gameObject, lastCacheCellDataIndex);
                        }
                    }
                }
            }
            else if (scrollRect.verticalNormalizedPosition > lastVerticalNormalizedPosition && startCacheCellDataIndex != 0)
            {
                if (cacheCellRectTsList[0].position.y + cacheCellRectTsList[0].rect.yMax <= parentRectT.position.y + parentRectT.rect.yMin)
                {
                    isRelayout = true;
                }
                else
                {
                    List<RectTransform> updateRectTsList = new List<RectTransform>();
                    for (int i = cacheCellRectTsList.Count - 1; i >= 0; i--)
                    {
                        RectTransform cacheCellRectT = cacheCellRectTsList[i];
                        if (cacheCellRectT.position.y + cacheCellRectT.rect.yMax < parentRectT.position.y + parentRectT.rect.yMin)
                        {
                            updateRectTsList.Add(cacheCellRectT);
                            if (updateRectTsList.Count == startCacheCellDataIndex)
                            {
                                break;
                            }
                        }
                    }
                    float startPos = cacheCellRectTsList[0].localPosition.y;
                    for (int i = 0; i < updateRectTsList.Count; i++)
                    {
                        cacheCellRectTsList.Remove(updateRectTsList[i]);
                        cacheCellRectTsList.Insert(0, updateRectTsList[i]);
                        float posY = startPos + (i / dimension.x + 1) * (cellSize.y + spacing.y);
                        updateRectTsList[i].localPosition = new Vector3(updateRectTsList[i].localPosition.x, posY);
                        startCacheCellDataIndex--;
                        onCellViewed?.Invoke(updateRectTsList[i].gameObject, startCacheCellDataIndex);
                        if (updateRectTsList[i].gameObject.activeSelf)
                        {
                            lastCacheCellDataIndex--;
                        }
                        else
                        {
                            updateRectTsList[i].gameObject.SetActive(true);
                        }
                    }
                }
            }
            lastVerticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }
    }
    public void Refresh(int cellCount)
    {
        if (!isInited)
        {
            return;
        }
        scrollRect.verticalNormalizedPosition = 1f;
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.GetComponent<RectTransform>());
        foreach (var item in cacheCellRectTsList)
        {
            DestroyImmediate(item.gameObject);
        }
        cacheCellRectTsList.Clear();
        this.cellCount = cellCount;
        int column = Mathf.FloorToInt((thisRectT.rect.width + spacing.x - padding.horizontal) / (cellSize.x + spacing.x));
        if (column < 0)
        {
            column = 0;
        }
        else if (column == 0)
        {
            column = 1;
        }
        int row = Mathf.CeilToInt(cellCount * 1f / column);
        dimension = new Vector2Int(column, row);
        float height = padding.vertical + row * cellSize.y + (row - 1) * spacing.y;
        thisRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        int cacheRow = Mathf.CeilToInt((parentRectT.rect.height - padding.top) / (cellSize.y + spacing.y)) + 1;
        startCacheCellDataIndex = cellCount == 0 ? -1 : 0;
        lastCacheCellDataIndex = -1;
        for (int i = 0; i < cacheRow; i++)
        {
            if (cellCount <= 0)
            {
                break;
            }
            for (int n = 0; n < column; n++)
            {
                GameObject cellG = Instantiate<GameObject>(cellPrefab, this.transform);
                RectTransform cellRectT = cellG.GetComponent<RectTransform>();
                cacheCellRectTsList.Add(cellRectT);
                cellRectT.anchorMin = new Vector2(0f, 1f);
                cellRectT.anchorMax = new Vector2(0f, 1f);
                cellRectT.sizeDelta = cellSize;
                float posX = padding.left + cellSize.x * cellRectT.pivot.x + n * (cellSize.x + spacing.x);
                float posY = -padding.top + cellSize.y * (cellRectT.pivot.y - 1f) - i * (cellSize.y + spacing.y);
                cellRectT.localPosition = new Vector3(posX, posY);
                if (cellCount > 0)
                {
                    cellG.SetActive(true);
                    lastCacheCellDataIndex++;
                    onCellViewed?.Invoke(cellG, lastCacheCellDataIndex);
                }
                cellCount--;
            }
        }
    }
    private IEnumerator InitCoroutine(int cellCount)
    {
        yield return null;
        Refresh(cellCount);
    }
    public void Init(int cellCount, UnityAction<GameObject, int> onCellViewed)
    {
        this.onCellViewed = onCellViewed;
        isInited = true;
        StartCoroutine(InitCoroutine(cellCount));
    }
    public void AddCell()
    {
        if (!isInited)
        {
            return;
        }
        cellCount++;
        int row = Mathf.CeilToInt(cellCount * 1f / dimension.x);
        if (row > dimension.y)
        {
            dimension.y = row;
            float height = padding.vertical + row * cellSize.y + (row - 1) * spacing.y;
            thisRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        int cacheRow = Mathf.CeilToInt((parentRectT.rect.height - padding.top) / (cellSize.y + spacing.y)) + 1;
        if (cellCount <= cacheCellRectTsList.Count)
        {
            lastCacheCellDataIndex++;
        }
        else if (cellCount < dimension.x * cacheRow)
        {
            for (int i = 0; i < dimension.x; i++)
            {
                GameObject cellG = Instantiate<GameObject>(cellPrefab, this.transform);
                RectTransform cellRectT = cellG.GetComponent<RectTransform>();
                cacheCellRectTsList.Add(cellRectT);
                cellRectT.anchorMin = new Vector2(0f, 1f);
                cellRectT.anchorMax = new Vector2(0f, 1f);
                cellRectT.sizeDelta = cellSize;
                float posX = padding.left + cellSize.x * cellRectT.pivot.x + i % dimension.x * (cellSize.x + spacing.x);
                float posY = -padding.top + cellSize.y * (cellRectT.pivot.y - 1f) - cacheRow * (cellSize.y + spacing.y);
                cellRectT.localPosition = new Vector3(posX, posY);
            }
            lastCacheCellDataIndex += dimension.x;
        }
        scrollRect.verticalNormalizedPosition = 0f;
        isRelayout = true;
    }
    public void RemoveCellAt(int index)
    {
        if (!isInited || index < 0 || index >= cellCount)
        {
            return;
        }
        cellCount--;
        int row = Mathf.CeilToInt(cellCount * 1f / dimension.x);
        if (row < dimension.y)
        {
            dimension.y = row;
            float height = padding.vertical + row * cellSize.y + (row - 1) * spacing.y;
            thisRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
        if (row < Mathf.CeilToInt(cacheCellRectTsList.Count * 1f / dimension.x))
        {
            for (int i = 0; i < dimension.x; i++)
            {
                Destroy(cacheCellRectTsList[cacheCellRectTsList.Count - 1].gameObject);
                cacheCellRectTsList.RemoveAt(cacheCellRectTsList.Count - 1);
            }
            lastCacheCellDataIndex -= dimension.x;
        }
        else if (cellCount < cacheCellRectTsList.Count)
        {
            lastCacheCellDataIndex--;
        }
        isRelayout = true;
    }
}
