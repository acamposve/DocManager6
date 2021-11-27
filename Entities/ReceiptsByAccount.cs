using System;

namespace WebApi.Entities
{
    public class ReceiptsByAccount
    {
        public int id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Referencia { get; set; }
        public DateTime FechaArribo { get; set; } 
        public string Origen { get; set; }
        public string Destino { get; set; }
    }
}
