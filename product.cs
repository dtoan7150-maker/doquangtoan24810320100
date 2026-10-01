using System;

namespace QuanLyThietBi
{
    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public CategoryItem(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class Product
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }
}