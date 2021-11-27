using AutoMapper;
using Dapper;
using Microsoft.Extensions.Options;
using System.Data;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Interfaces;
using WebApi.Models.ReceiptsAccounts;

namespace WebApi.Services
{
    

    public class ReceiptsAccountsService : IReceiptsAccountsService
    {
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IDapper _dapper;
        public ReceiptsAccountsService(
            IOptions<AppSettings> appSettings,
            IDapper dapper,
            IMapper mapper)
        {
            _appSettings = appSettings.Value;
            _dapper = dapper;
            _mapper = mapper;
        }

       





        public async Task<int> Create(CreateRequest model)
        {
            for (int i = 0; i < model.userid.Length; i++) {
                var dbparams = new DynamicParameters();
                dbparams.Add("EmbarquesId", model.embarqueid, DbType.Int32);
                dbparams.Add("AccountId", model.userid[i], DbType.Int32);


                var result = await Task.FromResult(_dapper.Insert<int>("[dbo].[pa_insert_embarquesaccounts]", dbparams, commandType: CommandType.StoredProcedure));

            }



            return 0;

        }

    }
}
