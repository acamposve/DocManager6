using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ReceiptsAccounts
{
    public class CreateRequest
    {
        
        [Required]
        public int embarqueid { get; set; }
        
        [Required]
        public int[] userid { get; set; }
    }
}
