using System.Text;
using System.Text.Json;

namespace Server.Messaging
{
    public class DisconnectionMessage : Message
    {
        public int UserID { get; set; }

        public string Username { get; set; }


        public DisconnectionMessage(int userID, string username)
            : base(MessageTypes.Disconnect)
        {
            UserID = userID;
            Username = username;
        }


        public override byte[] ToByteArray() =>
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this));
    }
}
