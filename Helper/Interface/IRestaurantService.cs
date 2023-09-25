using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;
using System.Runtime.InteropServices;

namespace RomanaWeb.Helper.Interface
{
    public interface IRestaurantService
    {
        Task<ResObj> Login(string UserName, string password);   
        Task<ResObj> GetAllForApp(string? Name, int UserId,int CategoriesId);
        Task<ResObj> GetAll(string? Name);
        Task<ResObj> Post(Restaurant Restaurant);
        Task<ResObj> Update(Restaurant Restaurant);
        Task<ResObj> Delete(int Id);
        Task<Restaurant> GetRestaurantById(int Id);
        Task<ResObj> GetById(int Id);       
        Task<ResObj> SetIsColsed(int id,bool closed);    
        Task<ResObj> GetTopAllForApp(int? UserId,double Long, double Lat, int index);
        Task<ResObj> CheckCode(string? code);
        Task<ResObj> UpdateLocationInfo(int id, double Long, double Lat);
        Task UpdateCode(CodeRes code);
    }
}
