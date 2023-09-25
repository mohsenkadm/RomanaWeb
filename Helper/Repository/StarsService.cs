using RomanaWeb.Classes;    
using RomanaWeb.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RomanaWeb.Helper.Interface;                          
using RomanaWeb.Model;
using System;

namespace RomanaWeb.Helper.Repository
{
    public class StarsService : IStarsService  , IRegisterScopped
    {
        private DB_Context _Context;
        private readonly IDapperRepository<Stars> _Repository;
        public StarsService(DB_Context dB_Context, IDapperRepository<Stars> repository)
        {
            _Context = dB_Context;
            _Repository = repository;
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.Stars.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.StarsId == Id);
            if (item != null)
            {
                 _Context.Stars.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }    
            return Result.Return(false);
        }
             

        public async Task<ResObj> GetById(int Id)
        {
            var item= await _Context.Stars.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.StarsId == Id);
            if (item != null)
                return Result.Return(true,item);
            else 
                return Result.Return(false);
        }

        public async Task<ResObj> Post(Stars stars)
        {
                                    
            if(stars.StarsId==0)
            {
                var info = await _Context.Stars.AsSplitQuery().AsNoTracking().Where(i => i.RestaurantId == stars.UserId).FirstOrDefaultAsync();
                if (info != null)
                {
                    return Result.Return(false, "لا يمكن تقييم  لاكثر من مرة");
                }
                await _Context.Stars.AddAsync(stars);
            }
            else
            {
                var item= await _Context.Stars.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.StarsId == stars.StarsId);
                if (item != null)
                {                           
                   item.UserId = stars.UserId;
                   item.StarsCount=stars.StarsCount;    
                   item.RestaurantId = stars.RestaurantId;
                   item.Comments=stars.Comments;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true,stars);
        }
        public async Task<ResObj> GetByRestaurantId(int RestaurantId)
        {
            var item = await _Repository.GetEntityListAsync("dbo.GetStarsByRestaurantId", new { RestaurantId });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }               
        public async Task<ResObj> GetAll(string? RestaurantName,int index)
        {                                                                                                  
              var item = await _Repository.GetEntityListAsync("dbo.GetStarsAll", new { RestaurantName, index });
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
    }
}
