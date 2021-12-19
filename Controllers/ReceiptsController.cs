using AutoMapper;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApi.Helpers;
using WebApi.Interfaces;
using WebApi.Models.Receipts;
namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private static string apikey = "AIzaSyCYxi0P89e9RgtP8kGZIL8DGsceg1rAOPw";
        private static string bucket = "documentmanager-31b76.appspot.com";
        private static string authEmail = "alexcamposve@gmail.com";
        private static string authPass = "2MP42qi!a";
        private IReceiptService _service;
        private IReceiptsFilesService _fileservice;
        private IReceiptsAccountsService _accountservice;
        private readonly AppSettings _appSettings;
        private readonly ILoggerManager _logger;
        public ReceiptsController(
                                    IReceiptService service,
                                    IReceiptsFilesService fileservice,
                                    IReceiptsAccountsService accountservice,
                                    IOptions<AppSettings> appSettings,
                                    ILoggerManager logger
                                    )
        {
            _service = service;
            _appSettings = appSettings.Value;
            _fileservice = fileservice;
            _accountservice = accountservice;
            _logger = logger;
        }

        //[Authorize(Roles = Role.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetAll();
            return Ok(users);
        }
        [HttpGet("ReceiptsByAccount/{id}")]
        public async Task<IActionResult> GetAllByAccount(int id)
        {
            var users = await _service.GetAllByAccount(id);
            return Ok(users);
        }
        [HttpGet("FilesByReceipt/{id}")]
        public async Task<IActionResult> GetFilesByReceipt(int id)
        {
            var users = await _service.GetFilesByReceipt(id);
            return Ok(users);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _service.GetById(id);

            if (user == null)
                return NotFound();

            return Ok(user);
        }
        
        
        [HttpPost("addUsers")]
        public async Task<IActionResult> AddUsersToReceipt([FromBody] Models.ReceiptsAccounts.CreateRequest usuarios)
        {
            await _accountservice.Create(usuarios);
            return Ok();
        }


        [HttpPost("addSingleUser")]
        public async Task<IActionResult> AddUserToReceipt([FromBody] Models.ReceiptsAccounts.CreateSingle usuarios)
        {
            await _accountservice.CreateUnique(usuarios);
            return Ok();
        }



        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            var formCollection = await Request.ReadFormAsync();



            try
            {
                Models.Receipts.CreateRequest request = new Models.Receipts.CreateRequest();
                request.FechaArribo = DateTime.Parse(formCollection["fechaarribo"]);
                request.CantidadContainers = formCollection["cantidadcontainers"];
                request.Referencia = formCollection["referencia"];
                request.Origen = formCollection["origen"];
                request.Destino = formCollection["destino"];
                request.Mercancia = formCollection["mercancia"];
                request.StatusId = int.Parse(formCollection["statusid"]);

                var receiptcreado = await _service.Create(request);

                //foreach(var accounts in formCollection["userid"].Count())
                //Models.ReceiptsAccounts.CreateRequest requestAccounts = new();

                //var account = await _accountservice.Create();



                var files = Request.Form.Files;
                var folderName = Path.Combine("Resources", "Images");


                _logger.LogInformation("Metodo Informativo" + Path.Combine(Directory.GetCurrentDirectory()));
                var pathToSave = _appSettings.imgDestino;//Path.Combine(Directory.GetCurrentDirectory(), folderName);

                _logger.LogInformation("Metodo Informativo" + pathToSave);




                //cancellation token


                if (files.Any(f => f.Length == 0))
                {
                    return BadRequest();
                }
                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    _logger.LogInformation("ruta completa " + fullPath);

                    byte[] imageData = null;






                    var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require
                    using (var stream = new FileStream(dbPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(apikey));
                    var a = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPass);
                    Guid obj = Guid.NewGuid();
                    var streamToFb = new FileStream(dbPath, FileMode.Open);
                    var upload = new FirebaseStorage(bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true
                        }
                        ).Child(obj.ToString())
                        

                        .PutAsync(streamToFb);



                    // Await the task to wait until upload is completed and get the download url
                    var downloadUrl = await upload;
                    Models.ReceiptsFiles.CreateRequest requestfiles = new Models.ReceiptsFiles.CreateRequest();
                    requestfiles.EmbarqueId = receiptcreado;
                    requestfiles.Extension = file.ContentType;
                    requestfiles.Path = downloadUrl;
                    requestfiles.Size = int.Parse(file.Length.ToString());
                    requestfiles.Name = downloadUrl;
                    requestfiles.imageData = imageData;
                    await _fileservice.Create(requestfiles);
                }
                return Ok(new { message = "All the files are successfully uploaded." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.InnerException);

                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut]
        public async Task<IActionResult> Update()
        {
            var formCollection = await Request.ReadFormAsync();



            try
            {
                Models.Receipts.UpdateRequest request = new Models.Receipts.UpdateRequest();
                request.FechaArribo = DateTime.Parse(formCollection["fechaarribo"]);
                request.CantidadContainers = formCollection["cantidadcontainers"];
                request.Referencia = formCollection["referencia"];
                request.Origen = formCollection["origen"];
                request.Destino = formCollection["destino"];
                request.Mercancia = formCollection["mercancia"];
                request.StatusId = int.Parse(formCollection["statusid"]);
                request.id = int.Parse(formCollection["id"]);
                var receiptcreado = int.Parse(formCollection["id"]);




                _service.Update(request.id, request);
                //foreach(var accounts in formCollection["userid"].Count())
                //Models.ReceiptsAccounts.CreateRequest requestAccounts = new();

                //var account = await _accountservice.Create();



                var files = Request.Form.Files;
                var folderName = Path.Combine("Resources", "Images");


                _logger.LogInformation("Metodo Informativo" + Path.Combine(Directory.GetCurrentDirectory()));
                var pathToSave = _appSettings.imgDestino;//Path.Combine(Directory.GetCurrentDirectory(), folderName);

                _logger.LogInformation("Metodo Informativo" + pathToSave);




                //cancellation token


                if (files.Any(f => f.Length == 0))
                {
                    return BadRequest();
                }
                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);

                    _logger.LogInformation("ruta completa " + fullPath);

                    byte[] imageData = null;






                    var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require
                    using (var stream = new FileStream(dbPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(apikey));
                    var a = await auth.SignInWithEmailAndPasswordAsync(authEmail, authPass);
                    Guid obj = Guid.NewGuid();
                    var streamToFb = new FileStream(dbPath, FileMode.Open);
                    var upload = new FirebaseStorage(bucket,
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                            ThrowOnCancel = true
                        }
                        ).Child(obj.ToString())


                        .PutAsync(streamToFb);



                    // Await the task to wait until upload is completed and get the download url
                    var downloadUrl = await upload;
                    Models.ReceiptsFiles.CreateRequest requestfiles = new Models.ReceiptsFiles.CreateRequest();
                    requestfiles.EmbarqueId = receiptcreado;
                    requestfiles.Extension = file.ContentType;
                    requestfiles.Path = downloadUrl;
                    requestfiles.Size = int.Parse(file.Length.ToString());
                    requestfiles.Name = downloadUrl;
                    requestfiles.imageData = imageData;
                    await _fileservice.Create(requestfiles);
                }
                return Ok(new { message = "All the files are successfully uploaded." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " " + ex.InnerException);

                return StatusCode(500, "Internal server error");
            }


            return Ok(new { message = "User updated" });
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "User deleted" });
        }



        [HttpDelete("DeleteReceiptFile/{id}")]
        public IActionResult DeleteReceiptFile(int id)
        {
            _fileservice.Delete(id);
            return Ok(new { message = "User deleted" });
        }


        [HttpGet("files/{id:int}")]
        public async Task<ActionResult> DownloadFile(int id)
        {

            var pathToSave = _appSettings.imgDestino;
            var filePath = pathToSave + "/" + "team-1.jpg";
            // ... code for validation and get the file

            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(filePath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
            return File(bytes, contentType, Path.GetFileName(filePath));
        }

        private byte[] ReadFile(string sPath)
        {
            //Initialize byte array with a null value initially.
            byte[] data = null;

            //Use FileInfo object to get file size.
            FileInfo fInfo = new FileInfo(sPath);
            long numBytes = fInfo.Length;

            //Open FileStream to read file
            FileStream fStream = new FileStream(sPath, FileMode.Open, FileAccess.Read);

            //Use BinaryReader to read file stream into byte array.
            BinaryReader br = new BinaryReader(fStream);

            //When you use BinaryReader, you need to supply number of bytes 
            //to read from file.
            //In this case we want to read entire file. 
            //So supplying total number of bytes.
            data = br.ReadBytes((int)numBytes);

            return data;
        }



        [HttpGet("accounts/{id:int}")]
        public async Task<ActionResult> GetAccountsByReceipt(int id)
        {
            var users = await _service.GetAccountByReceipt(id);
            return Ok(users);
        }

        [HttpGet("accountsnptinreceipt/{id:int}")]
        public async Task<ActionResult> GetAccountsNotReceipt(int id)
        {
            var users = await _service.GetAccountNotInReceipt(id);
            return Ok(users);
        }

    }
}
