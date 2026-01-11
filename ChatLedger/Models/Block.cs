using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ChatLedger.Models
{
    public class Block
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public long Index { get; set; }
        public DateTime Timestamp { get; set; }
        public ChatLog Data { get; set; }
        public string PreviousHash { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string ValidatorSignature { get; set; } = string.Empty;
    }
}
