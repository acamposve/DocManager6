using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using WebApi.Interfaces;
using WebApi.Models.Receipts;
using WebApi.Models.ReceiptsFiles;
namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiptsController : ControllerBase
    {
        private IReceiptService _service;
        private IReceiptsFilesService _fileservice;
        private IMapper _mapper;

        public ReceiptsController(IReceiptService service,
            IMapper mapper, IReceiptsFilesService fileservice)
        {
            _service = service;
            _mapper = mapper;
            _fileservice = fileservice;
        }

        //[Authorize(Roles = Role.Admin)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _service.GetAll();
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


        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            var formCollection = await Request.ReadFormAsync();



            try
            {
                Models.Receipts.CreateRequest request = new();
                request.FechaArribo = DateTime.Parse(formCollection["fechaarribo"]);
                request.CantidadContainers = formCollection["cantidadcontainers"];
                request.Referencia = formCollection["referencia"];
                request.Origen = formCollection["origen"];
                request.Destino = formCollection["destino"];
                request.Mercancia = formCollection["mercancia"];
                request.StatusId = int.Parse(formCollection["statusid"]);

                var receiptcreado = await _service.Create(request);





                var files = Request.Form.Files;
                var folderName = Path.Combine("Resources", "Images");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (files.Any(f => f.Length == 0))
                {
                    return BadRequest();
                }
                foreach (var file in files)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName); //you can add this path to a list and then return all dbPaths to the client if require
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    Models.ReceiptsFiles.CreateRequest requestfiles = new();
                    requestfiles.EmbarqueId = receiptcreado;
                    requestfiles.Extension = file.ContentType;
                    requestfiles.Path = fullPath;
                    requestfiles.Size = int.Parse(file.Length.ToString());
                    requestfiles.Name = file.FileName;
                    await _fileservice.Create(requestfiles);
                }
                return Ok("All the files are successfully uploaded.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }



            //            _service.Create(model);
            return Ok(new { message = "User created" });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateRequest model)
        {
            _service.Update(id, model);
            return Ok(new { message = "User updated" });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "User deleted" });
        }

    }
}
