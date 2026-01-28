using System;

namespace UI
{
    [Serializable]
    public class MaterialInventoryData
    {
        public string id;
        public string name;
        public string vendor;
        public string lotId;
        public string type;
        public string stock;
        public string status;
        public bool isExpired;

        public MaterialInventoryData(string id, string name, string vendor, string lotId,
                                     string type, string stock, string status, bool isExpired = false)
        {
            this.id = id;
            this.name = name;
            this.vendor = vendor;
            this.lotId = lotId;
            this.type = type;
            this.stock = stock;
            this.status = status;
            this.isExpired = isExpired;
        }
    }

    public static class MaterialInventoryDataProvider
    {
        public static MaterialInventoryData[] GetSampleData()
        {
            return new MaterialInventoryData[]
            {
                new MaterialInventoryData("MAT-001", "AZ 5214E", "Merck KGaA", "L892-22A",
                    "Photoresist", "50 L", "Warehouse"),
                new MaterialInventoryData("MAT-002", "OK 73 Thinner", "Tokyo Ohka", "T22-09B",
                    "Thinner", "200 L", "Warehouse"),
                new MaterialInventoryData("MAT-003", "NMD-3", "Tokyo Ohka", "D45-12C",
                    "Developer", "120 L", "Warehouse"),
                new MaterialInventoryData("MAT-004", "SU-8 2000", "Kayaku", "K99-01X",
                    "Photoresist", "10 L", "Expired", true),
                new MaterialInventoryData("MAT-005", "Buffered HF", "BASF", "E11-205",
                    "Etchant", "45 L", "Warehouse"),
                new MaterialInventoryData("MAT-006", "IPA Solvent", "LG Chem", "S78-552",
                    "Solvent", "500 L", "Warehouse")
            };
        }
    }
}
