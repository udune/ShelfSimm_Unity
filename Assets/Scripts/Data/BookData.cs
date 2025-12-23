using System;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class BookData
    {
        [Header("기본 정보")]
        [SerializeField] private string id;
        [SerializeField] public string title;
        [SerializeField] private string author;
        [SerializeField] private string isbn;

        [Header("물리적 특성 (mm 단위)")]
        [SerializeField] private int thickness;
        [SerializeField] private int height;
        [SerializeField] private int width;
        
        [Header("재고")]
        [SerializeField] private int stockQuantity;

        [Header("기타")]
        [SerializeField] private string category;
        [SerializeField] private bool isAvailable;

        public BookData(string id, string title, string author, int thickness, int height, int width, int stock, string category = "일반", string isbn = "")
        {
            this.id = id;
            this.title = title;
            this.author = author;
            this.thickness = thickness;
            this.height = height;
            this.width = width;
            this.stockQuantity = stock;
            this.category = category;
            this.isbn = isbn;
            isAvailable = true;
        }

        public BookData()
        {
            isAvailable = true;
        }

        public string Id => id;
        public string Title => title;
        public int Thickness => thickness;
        public int Height => height;
        public int Width => width;

        public string DisplayText => $"{title} - {author}";

        public void ChangeStock(int amount)
        {
            stockQuantity += amount;
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
