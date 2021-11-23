using AutoMapper;
using Dapper;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Interfaces;
using WebApi.Models.Receipts;

namespace WebApi.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IDapper _dapper;
        public ReceiptService(
            IOptions<AppSettings> appSettings, 
            IDapper dapper,
            IMapper mapper)
        {
            _appSettings = appSettings.Value;
            _dapper = dapper;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Receipt>> GetAll()
        {

            return await Task.FromResult(_dapper.GetAll<Receipt>($"Select * from [Receipts]", null, commandType: CommandType.Text));

        }
        public async Task<Receipt> GetById(int id)
        {
            var result = await Task.FromResult(_dapper.Get<Receipt>($"Select * from [Receipts] where Id = {id}", null, commandType: CommandType.Text));
            return result;
        }


        public async Task<Receipt> GetByReferencia(string referencia)
        {
            var result = await Task.FromResult(_dapper.Get<Receipt>($"Select * from [Receipts] where referencia = '{referencia}'", null, commandType: CommandType.Text));
            return result;
        }

        public async Task<int> Create(CreateRequest model)
        {
            var userBD = await Task.FromResult(_dapper.Get<User>($"Select * from [Receipts] where Referencia = '{model.Referencia}'", null, commandType: CommandType.Text));
            if (userBD != null)
                throw new AppException("User with the email '" + model.Referencia + "' already exists");

            var dbparams = new DynamicParameters();
            dbparams.Add("Referencia", model.Referencia, DbType.String);
            dbparams.Add("FechaArribo", model.FechaArribo, DbType.DateTime);
            dbparams.Add("Origen", model.Origen, DbType.String);
            dbparams.Add("Destino", model.Destino, DbType.String);
            dbparams.Add("StatusId", model.StatusId, DbType.Int32);
            dbparams.Add("CantidadContainers", model.CantidadContainers, DbType.String);
            dbparams.Add("Mercancia", model.Mercancia, DbType.String);

            var result = await Task.FromResult(_dapper.Insert<int>("[dbo].[pa_insert_receipts]", dbparams, commandType: CommandType.StoredProcedure));

            var receipt = await GetByReferencia(model.Referencia);

            return receipt.id;

        }
        public async void Update(int id, UpdateRequest model)
        {
            var user = await GetById(id);

            var userBD = await Task.FromResult(_dapper.Get<Receipt>($"Select * from [Receipts] where Referencia = '{model.Referencia}'", null, commandType: CommandType.Text));

            // validate
            if (model.Referencia != user.Referencia && userBD != null)
                throw new AppException("User with the email '" + model.Referencia + "' already exists");


            // copy model to user and save
            _mapper.Map(model, user);

            var dbparams = new DynamicParameters();
            dbparams.Add("id", user.id);
            dbparams.Add("Referencia", model.Referencia, DbType.String);
            dbparams.Add("FechaArribo", model.FechaArribo, DbType.DateTime);
            dbparams.Add("Origen", model.Origen, DbType.String);
            dbparams.Add("Destino", model.Destino, DbType.String);
            dbparams.Add("StatusId", model.StatusId, DbType.Int32);
            dbparams.Add("CantidadContainers", model.CantidadContainers, DbType.String);
            dbparams.Add("Mercancia", model.Mercancia, DbType.String);

            var updateArticle = Task.FromResult(_dapper.Update<int>("[dbo].[pa_update_receipts]",
                            dbparams,
                            commandType: CommandType.StoredProcedure));



        }
        public async void Delete(int id)
        {

            var dbPara = new DynamicParameters();
            dbPara.Add("id", id);

            var updateArticle = Task.FromResult(_dapper.Update<int>("[dbo].[pa_delete_receipts]",
                            dbPara,
                            commandType: CommandType.StoredProcedure));


        }

    }
}
