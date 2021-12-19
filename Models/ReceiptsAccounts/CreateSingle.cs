using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.ReceiptsAccounts
{
    public class CreateSingle
    {
        [Required]
        public string embarqueid { get; set; }

        [Required]
        public int accountid { get; set; }
    }
}
