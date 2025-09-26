using System.Net.WebSockets;
using System.Text;

namespace AspNetDemoPortalAPI.MySockets;

public class PilotTowerWebSocketHandler
{
    private static readonly string[] sampleMessages = new[]
    {
        "Request pushback approved",
        "Taxi to runway 27 via taxiway Bravo",
        "Hold short of runway 27",
        "Cleared for takeoff, runway 27",
        "Contact departure on 120.5",
        "Climb and maintain 5,000 feet",
        "Turn left heading 180",
        "Maintain present heading",
        "Flight level 350 confirmed",
        "Request frequency change approved",
        "Descend and maintain 3,000 feet",
        "Expect vectors for ILS runway 09",
        "Contact approach on 124.3",
        "Report established on the localizer",
        "Cleared to land, runway 09",
        "Taxi to parking via taxiway Alpha"
    };

    private static readonly string[] statusOptions = new[] { "OK", "INFO", "WARNING", "UPDATE" };

    public async Task HandleAsync(WebSocket socket)
    {
        var rand = new Random();

        while (socket.State == WebSocketState.Open)
        {
            var messagePayload = new
            {
                message = sampleMessages[rand.Next(sampleMessages.Length)],
                status = statusOptions[rand.Next(statusOptions.Length)],
                timestamp = DateTime.UtcNow.ToString("o")
            };

            var json = System.Text.Json.JsonSerializer.Serialize(messagePayload);
            var bytes = Encoding.UTF8.GetBytes(json);

            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            await Task.Delay(3000);
        }
    }
}
