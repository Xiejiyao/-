using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DiscountControl : MonoBehaviour
{
    [SerializeField]
    private InfiniteLayoutGroup goodsInfiniteLayoutGroup;
    private List<GoodsItemControl.ItemData> goodsItemDatasList = new List<GoodsItemControl.ItemData>();
    [SerializeField]
    private InputField goodsNameInputField;
    [SerializeField]
    private InputField goodsPriceInputField;
    [SerializeField]
    private Button addGoodsButton;
    [SerializeField]
    private InputField fullPriceInputField;
    [SerializeField]
    private InputField discountPriceInputField;
    [SerializeField]
    private Button calcDiscountButton;
    private class Goods
    {
        public GoodsItemControl.ItemData Data;
        public int Count;
        public int MaxCount = -1;
    }
    [SerializeField]
    private InfiniteLayoutGroup planInfiniteLayoutGroup;
    private List<PlanItemControl.ItemData> planItemDatasList = new List<PlanItemControl.ItemData>();
    [SerializeField]
    private Text detailsText;
    [SerializeField]
    private Button clearButton;
    [SerializeField]
    private Button quitButton;
    // Start is called before the first frame update
    void Start()
    {
        goodsInfiniteLayoutGroup.Init(0, (cellG, dataIndex) =>//初始化物品列表
        {
            GoodsItemControl goodsItemControl = cellG.GetComponent<GoodsItemControl>();
            goodsItemControl.Init(goodsItemDatasList[dataIndex], () =>
            {
                goodsItemDatasList.RemoveAt(dataIndex);
                goodsInfiniteLayoutGroup.RemoveCellAt(dataIndex);
            });
        });

        addGoodsButton.onClick.AddListener(() =>//添加物品
        {
            if (goodsNameInputField.text == string.Empty)
            {
                return;
            }
            if (float.TryParse(goodsPriceInputField.text, out float price))
            {
                if (price <= 0f)
                {
                    return;
                }
            }
            else
            {
                return;
            }
            goodsItemDatasList.Add(new GoodsItemControl.ItemData { Name = goodsNameInputField.text, Price = price });
            goodsInfiniteLayoutGroup.AddCell();
        });

        calcDiscountButton.onClick.AddListener(() =>//计算折扣
        {
            //判断输入是否正确，不正确则无效
            if (!float.TryParse(fullPriceInputField.text, out float fullPrice))
            {
                return;
            }
            if (!float.TryParse(discountPriceInputField.text, out float discountPrice))
            {
                return;
            }
            if (fullPrice < 0f || discountPrice < 0f || discountPrice > fullPrice)
            {
                return;
            }
            //清空方案数据
            planItemDatasList.Clear();
            //生成并排序物品
            Goods[] goods = goodsItemDatasList.Select(goodsItemData => new Goods { Data = goodsItemData }).ToArray();
            Array.Sort(goods, (goodsA, goodsB) => -goodsA.Data.Price.CompareTo(goodsB.Data.Price));
            float sumPrice = 0f;
            for (int i = 0; i < goods.Length; i++)
            {
                while (true)
                {
                    if ((goods[i].MaxCount == -1 || goods[i].Count < goods[i].MaxCount) && sumPrice < fullPrice && sumPrice + goods[i].Data.Price <= fullPrice + 10f)//判断物品最大数量和价格
                    {
                        goods[i].Count++;
                        sumPrice += goods[i].Data.Price;
                    }
                    else
                    {
                        break;
                    }
                }
                if (sumPrice >= fullPrice)//总价符合满减
                {
                    //添加物品和数量到方案列表中
                    PlanItemControl.ItemData planItemData = new PlanItemControl.ItemData { SumPrice = sumPrice, Discount = discountPrice / sumPrice };
                    foreach (var item in goods)
                    {
                        if (item.Count > 0)
                        {
                            planItemData.GoodsItems.Add(new PlanItemControl.ItemData.GoodsItem { Data = item.Data, Count = item.Count });
                        }
                    }
                    planItemDatasList.Add(planItemData);
                    i = -1;
                    for (int n = goods.Length - 1; n >= 0; n--)//从后向前对物品最大数做约束并清空物品数量
                    {
                        if (i == -1 && goods[n].Count > 0 && n != goods.Length - 1)
                        {
                            i = n;
                            goods[n].MaxCount = goods[n].Count - 1;
                        }
                        else if (i == -1)
                        {
                            goods[n].MaxCount = -1;
                        }
                        else
                        {
                            goods[n].MaxCount = goods[n].Count;
                        }
                        goods[n].Count = 0;
                    }
                    if (i == -1)
                    {
                        break;
                    }
                    i = -1;
                    sumPrice = 0f;
                }
            }
            planItemDatasList.Sort((planItemDataA, planItemDataB) => -planItemDataA.Discount.CompareTo(planItemDataB.Discount));//按折扣由大到小排序方案条目
            planInfiniteLayoutGroup.Refresh(planItemDatasList.Count);//刷新方案列表
            detailsText.text = string.Empty;
        });

        planInfiniteLayoutGroup.Init(0, (cellG, dataIndex) =>//初始化方案列表
        {
            PlanItemControl planItemControl = cellG.GetComponent<PlanItemControl>();
            planItemControl.Init(planItemDatasList[dataIndex], () =>
            {
                string details = string.Empty;
                foreach (var item in planItemDatasList[dataIndex].GoodsItems)
                {
                    details += $"商品:{item.Data.Name}({item.Data.Price}￥)  数量:{item.Count}  +{item.Data.Price * item.Count}￥\n";
                }
                details += $"\n总价:{planItemDatasList[dataIndex].SumPrice}￥  折扣:-{(planItemDatasList[dataIndex].Discount * 100f).ToString("f2")}%";
                detailsText.text = details;
            });
        });

        clearButton.onClick.AddListener(() =>//清除
        {
            goodsItemDatasList.Clear();
            goodsInfiniteLayoutGroup.Refresh(0);
            foreach (var item in this.GetComponentsInChildren<InputField>())
            {
                item.text = string.Empty;
            }
            planItemDatasList.Clear();
            planInfiniteLayoutGroup.Refresh(0);
            detailsText.text = string.Empty;
        });

        quitButton.onClick.AddListener(() =>//退出
        {
            Application.Quit();
        });
    }
}
