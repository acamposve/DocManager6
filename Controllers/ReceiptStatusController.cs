using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApi.Interfaces;
using WebApi.Models.ReceiptsStatus;

namespace WebApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ReceiptStatusController : ControllerBase
    {

        private IReceiptStatusService _service;
        private IMapper _mapper;

        public ReceiptStatusController(IReceiptStatusService service,
            IMapper mapper)
        {
            _service = service;
            _mapper = mapper;

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



        [HttpPost]
        public IActionResult Create(CreateRequest model)
        {
            _service.Create(model);
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
