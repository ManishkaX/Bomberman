using System.Text;
using System.Text.Json;

namespace Server.Messaging;

public class NotifyMessage : Message
{
    public List<User> Players { get; set; }

    public NotifyMessage(List<User> players) : base(MessageTypes.NotifyMessage)
    {
        Players = new List<User>(players);
    }

    public override byte[] ToByteArray() =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
}