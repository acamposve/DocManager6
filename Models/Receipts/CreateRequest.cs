using System;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Models.Receipts
{
    public class CreateRequest
    {
        [Required]
        public string Referencia { get; set; }
        [Required]
        public DateTime FechaArribo { get; set; }
        [Required]
        public string Origen { get; set; }
        [Required]
        public string Destino { get; set; }
        [Required]
        public int StatusId { get; set; }
        [Required]
        public string CantidadContainers { get; set; }
        [Required]
        public string Mercancia { get; set; }
    }
}
