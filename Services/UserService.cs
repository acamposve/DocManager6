using AutoMapper;
using Dapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Interfaces;
using WebApi.Models.Users;

namespace WebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;
        private readonly IDapper _dapper;
        private readonly ILoggerManager _logger;

        public UserService(IOptions<AppSettings> appSettings, IDapper dapper,
            IMapper mapper, ILoggerManager logger)
        {
            _appSettings = appSettings.Value;
            _dapper = dapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            _logger.LogInformation("Metodo Informativo");


            var user = await Task.FromResult(_dapper.Get<User>($"Select * from [Users] where username = '{username}'", null, commandType: CommandType.Text));
            //return result;

            //_users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);

            return user.WithoutPassword();
        }
        public async Task<IEnumerable<User>> GetAll()
        {

            return await Task.FromResult(_dapper.GetAll<User>($"Select * from [Users]", null, commandType: CommandType.Text));

        }
        public async Task<User> GetById(int id)
        {
            var result = await Task.FromResult(_dapper.Get<User>($"Select * from [Users] where Id = {id}", null, commandType: CommandType.Text));
            return result;
        }
        public async void Create(CreateRequest model)
        {

            var userBD = await Task.FromResult(_dapper.Get<User>($"Select * from [Users] where Username = '{model.Username}'", null, commandType: CommandType.Text));




            // validate
            if (userBD != null)
                throw new AppException("User with the email '" + model.Username + "' already exists");

            // map model to new user object
            var user = _mapper.Map<User>(model);

            var dbparams = new DynamicParameters();
            dbparams.Add("FirstName", model.FirstName, DbType.String);
            dbparams.Add("LastName", model.LastName, DbType.String);
            dbparams.Add("Username", model.Username, DbType.String);
            dbparams.Add("Role", model.Role, DbType.String);
            dbparams.Add("Password", model.Password, DbType.String);


            var result = await Task.FromResult(_dapper.Insert<int>("[dbo].[pa_insert_users]"
                    , dbparams,
                    commandType: CommandType.StoredProcedure));

        }
        public async void Update(int id, UpdateRequest model)
        {
            var user = await GetById(id);

            var userBD = await Task.FromResult(_dapper.Get<User>($"Select * from [Users] where Username = '{model.Username}'", null, commandType: CommandType.Text));

                // validate
                if (model.Username != user.Username && userBD != null )
                throw new AppException("User with the email '" + model.Username + "' already exists");


            // copy model to user and save
            _mapper.Map(model, user);

            var dbPara = new DynamicParameters();
            dbPara.Add("id", user.Id);
            dbPara.Add("FirstName", user.FirstName, DbType.String);
            dbPara.Add("LastName", user.LastName, DbType.String);
            dbPara.Add("Username", user.Username, DbType.String);
            dbPara.Add("Role", user.Role, DbType.String);
            dbPara.Add("Password", user.Password, DbType.String);

            var updateArticle = Task.FromResult(_dapper.Update<int>("[dbo].[pa_update_users]",
                            dbPara,
                            commandType: CommandType.StoredProcedure));
            


        }
        public async void Delete(int id)
        {

            var dbPara = new DynamicParameters();
            dbPara.Add("id", id);

            var updateArticle = Task.FromResult(_dapper.Update<int>("[dbo].[pa_delete_users]",
                            dbPara,
                            commandType: CommandType.StoredProcedure));


        }
    }
}