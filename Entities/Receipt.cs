using System;

namespace WebApi.Entities
{
    public class Receipt
    {
        public int id { get; set; }

        public string Referencia { get; set; }

        public DateTime FechaArribo { get; set; }

        public string Origen { get; set; }

        public string Destino { get; set; }

        public int StatusId { get; set; }

        public string CantidadContainers { get; set; }

        public string Mercancia { get; set; }

    }
}
