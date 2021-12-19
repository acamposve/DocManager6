using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ReceiptsAccounts
{
    public class CreateSingle
    {
        [Required]
        public int embarqueid { get; set; }

        [Required]
        public int userid { get; set; }
    }
}
