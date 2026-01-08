using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class MaterialData
    {
        [Header("기본 정보")]
        [SerializeField] private string id;
        [SerializeField] public string name;
        [SerializeField] private string vendor;
        [SerializeField] private string lotId;

        [Header("재고")]
        [SerializeField] private int stockQty;

        [Header("기타")]
        [SerializeField] private string type;
        [SerializeField] private string expiryDate;
        [SerializeField] private string category;
        [SerializeField] private bool isAvailable;

        public MaterialData(string id, string name, string vendor, string lotId, int stockQty, string type = "", string expiryDate = "", string category = "일반")
        {
            this.id = id;
            this.name = name;
            this.vendor = vendor;
            this.lotId = lotId;
            this.stockQty = stockQty;
            this.type = type;
            this.expiryDate = expiryDate;
            this.category = category;
            isAvailable = true;
        }

        public MaterialData()
        {
            isAvailable = true;
        }

        public string Id => id;
        public string Name => name;
        public string LotId => lotId;
        public int StockQty => stockQty;
        public string ExpiryDate => expiryDate;
        public string Type => type;

        public string DisplayText => $"{name} - {vendor}";

        public void ChangeStock(int amount)
        {
            stockQty += amount;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
