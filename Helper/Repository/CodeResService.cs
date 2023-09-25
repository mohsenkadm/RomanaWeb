using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class CodeResService : MasterService, ICodeResService, IRegisterScopped
    {                                                            
        public CodeResService(DB_Context dB_Context,  IMapper mapper) : base(mapper, dB_Context)
        {                                    
        }
        public async Task<ResObj> Delete(int Id)
        {
            var item = await _Context.CodeRes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CodeResId == Id);
            if (item != null)
            {
                _Context.CodeRes.Remove(item);
                await _Context.SaveChangesAsync();
                return Result.Return(true);
            }
            return Result.Return(false);
        }
        public async Task<ResObj> GetAll()
        {

            var item = await _Context.CodeRes.AsSplitQuery().AsNoTracking().ToListAsync();
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> GetById(int Id)
        {
            var item = await _Context.CodeRes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CodeResId == Id);
            if (item != null)
                return Result.Return(true, item);
            else
                return Result.Return(false);
        }
        public async Task<ResObj> Post(CodeRes CodeRes)
        {
            if (CodeRes.CodeResId == 0)
            {
                var check = await _Context.CodeRes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.Code == CodeRes.Code);
                if (check != null)
                {
                    return Result.Return(false, "تم الحفظ سابقا");
                }

                await _Context.CodeRes.AddAsync(CodeRes);
            }
            else
            {
                var item = await _Context.CodeRes.AsSplitQuery().AsNoTracking().FirstOrDefaultAsync(i => i.CodeResId == CodeRes.CodeResId);
                if (item != null)
                {
                    item.IsFree = CodeRes.IsFree;
                    item.Code = CodeRes.Code;
                    _Context.Entry(item).State = EntityState.Modified;
                }
            }
            await _Context.SaveChangesAsync();
            return Result.Return(true, "تم الحفظ بنجاح", CodeRes);
        }
    }
}