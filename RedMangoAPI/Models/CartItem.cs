﻿using System.ComponentModel.DataAnnotations.Schema;

namespace RedMangoAPI.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int MenuItemId { get; set; }
        [ForeignKey("MenuItemId")]
        public MenuItem MenuItem { get; set; } = new MenuItem();
        public int Quantity { get; set; }
        public int ShoppingCartId { get; set; }
    }
}
