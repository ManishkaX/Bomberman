using System.Text;
using System.Text.Json;

namespace Server.Messaging;
public class PlaceBombMessage : Message
{
    public int PositionX { get; set; }

    public int PositionY { get; set; }

    public PlaceBombMessage(int positionX, int positionY): base(MessageTypes.PlaceBomb)
    {
        PositionX = positionX;
        PositionY = positionY;
    }

     public override byte[] ToByteArray() =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
}
