using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlanItemControl : MonoBehaviour
{
    [SerializeField]
    private Text sumPriceText;
    [SerializeField]
    private Text discountText;
    [SerializeField]
    private Button detailsButton;
    public class ItemData
    {
        public class GoodsItem
        {
            public GoodsItemControl.ItemData Data;
            public int Count;
        }
        public List<GoodsItem> GoodsItems = new List<GoodsItem>();
        public float SumPrice;
        public float Discount;
    }
    private ItemData data;
    private UnityAction onViewedDetails;
    void Awake()
    {
        detailsButton.onClick.AddListener(() =>
        {
            onViewedDetails?.Invoke();
        });
    }
    public void Init(ItemData data, UnityAction onViewedDetails)
    {
        this.data = data;
        sumPriceText.text = $"{data.SumPrice.ToString()}￥";
        discountText.text = $"-{(data.Discount * 100f).ToString("f2")}%";
        this.onViewedDetails = onViewedDetails;
    }
}
