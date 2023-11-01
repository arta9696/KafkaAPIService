namespace KafkaAPIService.Models
{
    public class Comments
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? Updated { get; set; }
        public int PostId { get; set; }
        public int Status { get; set; }
        public string StatusMessage { get; set; }
    }
}
