using System.Text;
using System.Text.Json;

namespace Server.Messaging;

public class ConnectionMessage : Message
{
    public int UserID { get; set; }
    public string Username { get; set; }


    public ConnectionMessage(int userID, string username)
        : base(MessageTypes.Connect)
    {
        UserID = userID;
        Username = username;
    }
    

    public override byte[] ToByteArray() => 
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
}