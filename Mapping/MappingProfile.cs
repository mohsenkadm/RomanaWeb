using AutoMapper;
using RomanaWeb.Models.EntityMapper;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Mapping
{
    public class MappingProfile    : Profile
    {
        public MappingProfile()
        {                                      
            CreateMap<RestaurantModel, Restaurant>();         
            CreateMap<CarouselModel, Carousel>();         
            CreateMap<QuestionsModel, Questions>();         
            CreateMap<CategoriesModel, Categories>();         
            CreateMap<ProductsModel, Products>();         
            CreateMap<OrderDetailModel, OrderDetail>();      
            CreateMap<OrdersModel, Orders>();      
            CreateMap<SaleManModel, SaleMan>();      
        }
    }
}
