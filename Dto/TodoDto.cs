namespace AspNetDemoPortalAPI.Dto
{
    public class TodoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
