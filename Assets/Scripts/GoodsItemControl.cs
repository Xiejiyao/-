using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class GoodsItemControl : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text priceText;
    [SerializeField]
    private Button deleteButton;
    public class ItemData
    {
        public string Name;
        public float Price;
    }
    private ItemData data;
    public ItemData Data
    {
        get
        {
            return data;
        }
    }
    private UnityAction onDeleted;
    void Awake()
    {
        deleteButton.onClick.AddListener(() =>
        {
            onDeleted?.Invoke();
        });
    }
    public void Init(ItemData data, UnityAction onDeleted)
    {
        this.data = data;
        nameText.text = data.Name;
        priceText.text = $"{data.Price}￥";
        this.onDeleted = onDeleted;
    }
}
