namespace BlockSenseAPI.Models
{
    public class TokenCache
    {
        public Guid TokenId { get; set; }
        public int UserId {  get; set; }
        public byte[] Data { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public byte[] Hmac { get; set; }
    }
}
