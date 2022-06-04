using System.Text;
using System.Text.Json;

namespace Server.Messaging;

public enum MessageTypes { Connect, Disconnect, Move, ModifyBlock, PlaceBomb, NotifyMessage };

public abstract class Message
{
    public MessageTypes MessageType { get; set; }


    protected Message(MessageTypes messageType)
    {
        MessageType = messageType;
    }


    public abstract byte[] ToByteArray();
}
