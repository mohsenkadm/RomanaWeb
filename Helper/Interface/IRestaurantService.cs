using RomanaWeb.Models.Entity;
using RomanaWeb.Classes;
using System.Runtime.InteropServices;

namespace RomanaWeb.Helper.Interface
{
    public interface IRestaurantService
    {
        Task<ResObj> Login(string UserName, string password);
        Task<ResObj> GetAllForApp(string? Name, int CategoriesId, double Long, double Lat,int? CityId);
        Task<ResObj> GetAll(string? Name);
        Task<ResObj> Post(Restaurant Restaurant);
        Task<ResObj> Update(Restaurant Restaurant);
        Task<ResObj> Delete(int Id);
        Task<Restaurant> GetRestaurantById(int Id);
        Task<ResObj> GetById(int Id);       
        Task<ResObj> SetIsColsed(int id,bool stars);
        Task<ResObj> SetIsStars(int id,bool closed);
        Task<ResObj> SetIsApproved(int id);
        Task<ResObj> GetResNotApproveAll();
        Task<ResObj> GetTopAllForApp(double Long, double Lat, int? CityId);
        Task<ResObj> GetCountForRes(int Id, DateTime datefrom, DateTime dateto);
        Task<ResObj> CheckCode(string? code);
        Task<ResObj> UpdateLocationInfo(int id, double Long, double Lat);
        Task UpdateCode(CodeRes code);
        Task<ResObj> GetReportRes(string restaurantName, DateTime datefrom, DateTime dateto);
        Task<ResObj> SetInsta(int id, string url);
        Task<ResObj> RefreshToken(int id);
    }
}
