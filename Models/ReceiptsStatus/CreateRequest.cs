using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ReceiptsStatus
{
    public class CreateRequest
    {
        [Required]
        public string status { get; set; }
    }
}
