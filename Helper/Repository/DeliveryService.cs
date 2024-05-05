using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class DeliveryService : MasterService, IDeliveryService, IRegisterScopped
    {

        private readonly IDapperRepository<Delivery> _DeliveryService;
        public DeliveryService(DB_Context dB_Context, IMapper mapper,IDapperRepository<Delivery> dapper) : base(mapper, dB_Context)
        {                                        
            _DeliveryService = dapper;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.Delivery.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.DeliveryId == Id);
            if (item != null)
            {
                _Context.Delivery.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll(string? Name, int? CountriesId, string? RestaurantName
            , DateTime datefrom, DateTime dateto, string? No)
        {
            var item = await _DeliveryService.GetEntityListAsync("dbo.GetDeliveryAll",new { Name, CountriesId, RestaurantName
           ,  datefrom ,
                dateto  ,
                No
            });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }                              
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.Delivery.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.DeliveryId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }

        public async Task<ResObj> GetDeliveryByNoAndRestaurantId(string? No, int? RestaurantId)
        {
            List<Delivery> data = await _DeliveryService.GetEntityListAsync("dbo.GetDeliveryByNoAndRestaurantId", new { No, RestaurantId });

            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Delivery Delivery)
        {
            if (Delivery.DeliveryId == 0)
            {
                 

                Delivery.DateInsert = Key.DateTimeIQ;
                await _Context.Delivery.AddAsync(Delivery);
            }
            else
            {
                var item = await _Context.Delivery.FirstOrDefaultAsync(i => i.DeliveryId == Delivery.DeliveryId);
                if (item != null)
                {
                    item.Name = Delivery.Name;
                    item.Phone = Delivery.Phone;
                    item.Address = Delivery.Address;
                    item.FunctionPoint = Delivery.FunctionPoint;
                    item.CostDelivery = Delivery.CostDelivery;
                    item.Notes = Delivery.Notes;
                    item.NetAmount = Delivery.NetAmount;
                    item.CityId = Delivery.CityId;
                    item.CountriesId = Delivery.CountriesId;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", Delivery);
        }
    }
}