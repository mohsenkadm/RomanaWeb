using RomanaWeb.Classes;
using RomanaWeb.Helper.Interface;
using RomanaWeb.Model;
using RomanaWeb.Model.General;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RomanaWeb.Models.Entity;

namespace RomanaWeb.Helper.Repository
{
    public class QuestionsService : IQuestionsService, IRegisterScopped
    {
        private readonly DB_Context _context;

        public QuestionsService(DB_Context context)
        {
            _context = context;
        }

        public async Task<ResObj> GetAllApp()
        {
            List<Questions> data = await _context.Questions
                .AsSplitQuery()
                .AsNoTracking()
                .Where(i => i.IsShow == true)
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.QuestionId)
                .ToListAsync();
            return Result.Return(true, data);
        }

        public async Task<ResObj> GetAll()
        {
            List<Questions> data = await _context.Questions
                .AsSplitQuery()
                .AsNoTracking()
                .OrderBy(i => i.DisplayOrder)
                .ThenBy(i => i.QuestionId)
                .ToListAsync();
            return Result.Return(true, data);
        }

        public async Task<ResObj> Post(Questions Questions)
        {
            if (Questions.QuestionText.IsEmpty())
                return Result.Return(false, "رجاءا اكتب السؤال");

            if (Questions.AnswerText.IsEmpty())
                return Result.Return(false, "رجاءا اكتب الإجابة");

            await _context.Questions.AddAsync(Questions);
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح", Questions);
        }

        public async Task<ResObj> Update(Questions Questions)
        {
            if (Questions.QuestionText.IsEmpty())
                return Result.Return(false, "رجاءا اكتب السؤال");

            if (Questions.AnswerText.IsEmpty())
                return Result.Return(false, "رجاءا اكتب الإجابة");

            Questions Questions1 = await GetQuestionsById(Questions.QuestionId);
            if (Questions1 is null)
                return Result.Return(false, "حدث خطا اثناء عملية جلب البيانات");

            Questions1.QuestionText = Questions.QuestionText;
            Questions1.AnswerText = Questions.AnswerText;
            Questions1.IsShow = Questions.IsShow;
            Questions1.DisplayOrder = Questions.DisplayOrder;

            _context.Entry(Questions1).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحفظ بنجاح", Questions1);
        }

        public async Task<ResObj> Delete(int Id)
        {
            Questions Questions1 = await GetQuestionsById(Id);
            if (Questions1 is null)
                return Result.Return(false, "السؤال غير موجود");

            _context.Entry(Questions1).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return Result.Return(true, "تم الحذف بنجاح");
        }

        public async Task<Questions> GetQuestionsById(int Id)
        {
            return await _context.Questions
                .AsSplitQuery()
                .AsNoTracking()
                .Where(i => i.QuestionId == Id)
                .FirstOrDefaultAsync();
        }

        public async Task<ResObj> GetById(int Id)
        {
            Questions Questions = await GetQuestionsById(Id);
            return Result.Return(true, Questions);
        }
    }
}