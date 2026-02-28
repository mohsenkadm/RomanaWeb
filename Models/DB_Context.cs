using RomanaWeb.Model.EntityMap;  
using Microsoft.EntityFrameworkCore;  
using RomanaWeb.Models.Entity;
using RomanaWeb.Models.EntityMap;

namespace RomanaWeb.Model
{
    public class DB_Context : DbContext
    {
        public DB_Context(DbContextOptions<DB_Context> options) : base(options)
        {

        }

        protected DB_Context(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {                                                      
            modelBuilder.ApplyConfiguration(new UsersMap());       
            modelBuilder.ApplyConfiguration(new NotificationMap());     
            modelBuilder.ApplyConfiguration(new CarouselMap());        
            modelBuilder.ApplyConfiguration(new AdminMap());         
            modelBuilder.ApplyConfiguration(new RestaurantMap());     
            modelBuilder.ApplyConfiguration(new CategoriesMap());         
            modelBuilder.ApplyConfiguration(new StarsMap());        
            modelBuilder.ApplyConfiguration(new RestaurantSubCategoriesMap());        
            modelBuilder.ApplyConfiguration(new PromoCodeMap());        
            modelBuilder.ApplyConfiguration(new CodeResMap());        
            modelBuilder.ApplyConfiguration(new SubCategoriesMap());        
            modelBuilder.ApplyConfiguration(new ProductsMap());        
            modelBuilder.ApplyConfiguration(new OrderDetailMap());        
            modelBuilder.ApplyConfiguration(new OrdersMap());        
            modelBuilder.ApplyConfiguration(new RestaurantCityMap());        
            modelBuilder.ApplyConfiguration(new CityMap());        
            modelBuilder.ApplyConfiguration(new SaleManMap());        
            modelBuilder.ApplyConfiguration(new RestaurantSaleManMap());        
            modelBuilder.ApplyConfiguration(new CountriesMap());        
            modelBuilder.ApplyConfiguration(new ImagesMap());        
            modelBuilder.ApplyConfiguration(new DeliveryMap());        
            modelBuilder.ApplyConfiguration(new QuestionsMap());        
        }
                                      
        public DbSet<Users> Users { get; set; }         
        public DbSet<Notification> Notification { get; set; }   
        public DbSet<Carousel> Carousel { get; set; }      
        public DbSet<Admin> Admin { get; set; }       
        public DbSet<Permission> Permission { get; set; }          
        public DbSet<Restaurant> Restaurant { get; set; }    
        public DbSet<Categories> Categories { get; set; }           
        public DbSet<CodeRes> CodeRes { get; set; }           
        public DbSet<Stars> Stars { get; set; }           
        public DbSet<RestaurantSubCategories> RestaurantSubCategories { get; set; }           
        public DbSet<PromoCode> PromoCodes { get; set; }           
        public DbSet<SubCategories> SubCategories { get; set; }           
        public DbSet<Products> Products { get; set; }           
        public DbSet<OrderDetail> OrderDetail { get; set; }           
        public DbSet<Orders> Orders { get; set; }           
        public DbSet<RestaurantCity> RestaurantCity { get; set; }           
        public DbSet<City> City { get; set; }           
        public DbSet<SaleMan> SaleMan { get; set; }           
        public DbSet<RestaurantSaleMan> RestaurantSaleMan { get; set; }           
        public DbSet<Countries> Countries { get; set; }           
        public DbSet<Images> Images { get; set; }           
        public DbSet<Delivery> Delivery { get; set; }           
        public DbSet<Questions> Questions { get; set; }           
    }
}
