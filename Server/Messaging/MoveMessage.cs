using System.Text;
using System.Text.Json;

namespace Server.Messaging;
public class MoveMessage : Message
{
    public int UserID { get; set; }

    public string Username { get; set; }

    public int PositionX { get; set; }

    public int PositionY { get; set; }


    public MoveMessage(int userID, string username, int positionX, int positionY) : base(MessageTypes.Move)
    {
        UserID = userID;
        Username = username;
        PositionX = positionX;
        PositionY = positionY;
    }


    public override byte[] ToByteArray() =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
}