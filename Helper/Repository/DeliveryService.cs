using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Polly;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;
using static NuGet.Packaging.PackagingConstants;

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
            , DateTime datefrom, DateTime dateto, int? No)
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
            var item = GetDeliveryAsync(Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        private async Task<Delivery> GetDeliveryAsync(int Id)
        {
            return await _Context.Delivery.FirstOrDefaultAsync(i => i.DeliveryId == Id);
        }

        public async Task<ResObj> GetDeliveryByNoAndRestaurantId(int? No, int? RestaurantId)
        {
            List<Delivery> data = await _DeliveryService.GetEntityListAsync("dbo.GetDeliveryByNoAndRestaurantId", new { No, RestaurantId });

            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Delivery Delivery)
        {
            if (Delivery.DeliveryId == 0)
            {
                var lastorder = await _Context.Delivery.AsSplitQuery().AsNoTracking().OrderBy(i => i.DeliveryId).LastOrDefaultAsync();
                if (lastorder != null)
                {
                    Delivery.No = lastorder.No + 1;
                }
                else Delivery.No = 1;
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

        public async Task<ResObj> SetIsNotDelivered(int id, string Reason)
        {
            var orders = await GetDeliveryAsync(id);

            orders.IsNotDelivered = true;
            orders.IsDelivered = false;
            orders.IsWaiting = false;
            orders.Reason = Reason;

            _Context.Entry(orders).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تمت الغاء توصيل بنجاح", orders);
        }
        public async Task<ResObj> SetIsWaiting(int id, string Reason2)
        {
            var orders = await GetDeliveryAsync(id);

            orders.IsNotDelivered = false;
            orders.IsDelivered = false;
            orders.IsWaiting = true;
            orders.Reason2 = Reason2;

            _Context.Entry(orders).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تمت تأجيل توصيل بنجاح", orders);
        }
        public async Task<ResObj> SetIsDelivered(int id)
        {
            var orders = await GetDeliveryAsync(id);

            orders.IsNotDelivered = false;
            orders.IsDelivered = true;

            orders.IsWaiting = false;
            _Context.Entry(orders).State = EntityState.Modified;
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تمت  توصيل  بنجاح", orders);
        }
    }
}