namespace Library81.Models
{
    public class LocalStorage
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Data { get; set; } = null!;
        public DateTime LastModified { get; set; }
    }
}
