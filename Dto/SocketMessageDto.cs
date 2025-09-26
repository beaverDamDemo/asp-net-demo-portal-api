namespace AspNetDemoPortalAPI.Dto
{
    public class SocketMessageDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
