﻿using System.ComponentModel.DataAnnotations;

namespace RedMangoAPI.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string SpecialTag { get; set; }
        public string Category { get; set; }
        [Range(1, int.MaxValue)]
        public int Price { get; set; }
        [Required]
        public string Image { get; set; }
    }
}
