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
        private readonly ILoggerManager _logger;
        public ReceiptService(
            IOptions<AppSettings> appSettings,
            IDapper dapper,
            IMapper mapper, ILoggerManager logger)
        {
            _appSettings = appSettings.Value;
            _dapper = dapper;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<IEnumerable<Receipt>> GetAll()
        {
            var sql = $"SELECT dbo.Receipts.id, dbo.Receipts.Referencia, dbo.Receipts.FechaArribo, dbo.Receipts.Origen, dbo.Receipts.Destino, dbo.ReceiptStatus.status FROM dbo.Receipts INNER JOIN dbo.ReceiptStatus ON dbo.Receipts.statusid = dbo.ReceiptStatus.id";
            return await Task.FromResult(_dapper.GetAll<Receipt>(sql, null, commandType: CommandType.Text));

        }
        public async Task<IEnumerable<ReceiptsByAccount>> GetAllByAccount(int id)
        {
            string sql = $"SELECT        dbo.Receipts.Referencia, dbo.Receipts.FechaArribo, dbo.Receipts.Origen, dbo.Receipts.Destino, dbo.Receipts.id, dbo.ReceiptStatus.status FROM dbo.Receipts INNER JOIN dbo.EmbarquesAccounts ON dbo.Receipts.id = dbo.EmbarquesAccounts.EmbarquesId INNER JOIN dbo.ReceiptStatus ON dbo.Receipts.StatusId = dbo.ReceiptStatus.id WHERE (dbo.EmbarquesAccounts.AccountId = { id })";
            return await Task.FromResult(_dapper.GetAll<ReceiptsByAccount>(sql, null, commandType: CommandType.Text));
        }
        public async Task<Receipt> GetById(int id)
        {
            var sql = $"SELECT dbo.Receipts.id, dbo.ReceiptStatus.status, dbo.Receipts.Referencia, dbo.Receipts.FechaArribo, dbo.Receipts.Origen, dbo.Receipts.Destino, dbo.Receipts.CantidadContainers, dbo.Receipts.Mercancia FROM dbo.Receipts INNER JOIN dbo.ReceiptStatus ON dbo.Receipts.StatusId = dbo.ReceiptStatus.id WHERE (dbo.Receipts.id = { id})";
            var result = await Task.FromResult(_dapper.Get<Receipt>(sql, null, commandType: CommandType.Text));
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

            try
            {
                _logger.LogInformation("id es " + id);

                _logger.LogInformation("modelo es " + model);

                var user = await GetById(id);

                var userBD = await Task.FromResult(_dapper.Get<Receipt>($"Select * from [Receipts] where Referencia = '{model.Referencia}'", null, commandType: CommandType.Text));

                // validate
                if (model.Referencia != user.Referencia && userBD != null)
                    throw new AppException("User with the email '" + model.Referencia + "' already exists");



                var dbparams = new DynamicParameters();
                dbparams.Add("id", user.id);
                dbparams.Add("Referencia", model.Referencia, DbType.String);
                dbparams.Add("FechaArribo", model.FechaArribo, DbType.DateTime);
                dbparams.Add("Origen", model.Origen, DbType.String);
                dbparams.Add("Destino", model.Destino, DbType.String);
                dbparams.Add("StatusId", model.StatusId, DbType.Int32);
                dbparams.Add("CantidadContainers", model.CantidadContainers, DbType.String);
                dbparams.Add("Mercancia", model.Mercancia, DbType.String);
                var updateArticle = Task.FromResult(_dapper.Update<int>("[dbo].[pa_update_receipts]", dbparams, commandType: CommandType.StoredProcedure));
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.InnerException);

                throw;
            }

        }
        public async void Delete(int id)
        {
            var dbPara = new DynamicParameters();
            dbPara.Add("id", id);
            var updateArticle = await Task.FromResult(_dapper.Update<int>("[dbo].[pa_delete_receipts]", dbPara, commandType: CommandType.StoredProcedure));
        }



        public async Task<IEnumerable<FilesByReceipt>> GetFilesByReceipt(int id)
        {
            string sql = $"SELECT [id],[Name],[Size],[Extension],[Path],[EmbarqueId] FROM [dbo].[ReceiptFiles] where embarqueid = { id }";
            return await Task.FromResult(_dapper.GetAll<FilesByReceipt>(sql, null, commandType: CommandType.Text));
        }

        public async Task<IEnumerable<AccountByReceipt>> GetAccountByReceipt(int id)
        {
            string sql = $"SELECT dbo.Users.id, dbo.Users.FirstName, dbo.Users.LastName FROM dbo.EmbarquesAccounts INNER JOIN dbo.Users ON dbo.EmbarquesAccounts.AccountId = dbo.Users.id WHERE (dbo.EmbarquesAccounts.EmbarquesId = { id })";
            return await Task.FromResult(_dapper.GetAll<AccountByReceipt>(sql, null, commandType: CommandType.Text));
        }


        public async Task<IEnumerable<AccountByReceipt>> GetAccountNotInReceipt(int id)
        {
            string sql = $"select u.id, u.FirstName, u.lastname from users u where u.id not in (SELECT EmbarquesAccounts.AccountId FROM dbo.EmbarquesAccounts INNER JOIN dbo.Users ON dbo.EmbarquesAccounts.AccountId = dbo.Users.id WHERE (dbo.EmbarquesAccounts.EmbarquesId = { id }))";
            return await Task.FromResult(_dapper.GetAll<AccountByReceipt>(sql, null, commandType: CommandType.Text));
        }

        
    }
}
