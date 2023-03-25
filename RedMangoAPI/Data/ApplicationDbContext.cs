using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedMangoAPI.Models;

namespace RedMangoAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<MenuItem>().HasData(
               new MenuItem
               {
                   Id = 1,
                   Name = "Spring Roll",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/spring roll.jpg",
                   Price = 240,
                   Category = "Appetizer",
                   SpecialTag = ""
               }, new MenuItem
               {
                   Id = 2,
                   Name = "Idli",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/idli.jpg",
                   Price = 270,
                   Category = "Appetizer",
                   SpecialTag = ""
               }, new MenuItem
               {
                   Id = 3,
                   Name = "Panu Puri",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/pani puri.jpg",
                   Price = 270,
                   Category = "Appetizer",
                   SpecialTag = "Best Seller"
               }, new MenuItem
               {
                   Id = 4,
                   Name = "Hakka Noodles",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/hakka noodles.jpg",
                   Price = 300,
                   Category = "Entrée",
                   SpecialTag = ""
               }, new MenuItem
               {
                   Id = 5,
                   Name = "Malai Kofta",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/malai kofta.jpg",
                   Price = 360,
                   Category = "Entrée",
                   SpecialTag = "Top Rated"
               }, new MenuItem
               {
                   Id = 6,
                   Name = "Paneer Pizza",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/paneer pizza.jpg",
                   Price = 330,
                   Category = "Entrée",
                   SpecialTag = ""
               }, new MenuItem
               {
                   Id = 7,
                   Name = "Paneer Tikka",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/paneer tikka.jpg",
                   Price = 400,
                   Category = "Entrée",
                   SpecialTag = "Chef's Special"
               }, new MenuItem
               {
                   Id = 8,
                   Name = "Carrot Love",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/carrot love.jpg",
                   Price = 150,
                   Category = "Dessert",
                   SpecialTag = ""
               }, new MenuItem
               {
                   Id = 9,
                   Name = "Rasmalai",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/rasmalai.jpg",
                   Price = 150,
                   Category = "Dessert",
                   SpecialTag = "Chef's Special"
               }, new MenuItem
               {
                   Id = 10,
                   Name = "Sweet Rolls",
                   Description = "Fusc tincidunt maximus leo, sed scelerisque massa auctor sit amet. Donec ex mauris, hendrerit quis nibh ac, efficitur fringilla enim.",
                   Image = "https://asvtredmangoimages.blob.core.windows.net/redmango/sweet rolls.jpg",
                   Price = 120,
                   Category = "Dessert",
                   SpecialTag = "Top Rated"
               }
            );
        }
    }
}
