namespace WebApi.Entities
{
    public class FilesByReceipt
    {
        public int id { get; set; }
      public string Name { get; set; }
        public int Size { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public int EmbarqueId { get; set; }
    }
}
