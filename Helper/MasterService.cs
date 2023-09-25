using AutoMapper;
using RomanaWeb.Model;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.EntityFrameworkCore;

namespace RomanaWeb.Helper
{
    public class MasterService
    {
        public readonly IMapper _mapper;
                                         
        public readonly DB_Context _Context;

        public MasterService(IMapper mapper,  DB_Context context)
        {
            _mapper = mapper;  _Context = context;
        }
    }
}
