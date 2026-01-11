using System.Security.Cryptography;
using ChatLedger.Models;
using MongoDB.Driver;
using System.Text;
using System.Text.Json;

namespace ChatLedger.Services
{
    public class BlockchainService
    {
        private readonly IMongoCollection<Block> _collection;
        private readonly KeyService _keyService;

        public BlockchainService(IConfiguration config, KeyService keyService)
        {
            _keyService = keyService;
            var client = new MongoClient(config.GetConnectionString("MongoDb"));
            var database = client.GetDatabase("ChatLedgerDB");
            _collection = database.GetCollection<Block>("Messages");
        }

        public async Task<Block> AddMessageAsync(ChatLog chatData)
        {
            var lastBlock = await _collection.Find(_ => true)
                .SortByDescending(x => x.Index)
                .FirstOrDefaultAsync();


            if (lastBlock != null)
            {
                if (lastBlock.Hash != CalculateHash(lastBlock))
                    throw new InvalidOperationException("GÜVENLİK UYARISI: Geçmiş mesaj kayıtları değiştirilmiş! Yeni kayıt girilemez.");

                if (!_keyService.VerifySignature(lastBlock.Hash, lastBlock.ValidatorSignature))
                    throw new InvalidOperationException("GÜVENLİK UYARISI: Son bloğun imzası geçersiz.");
            }

            long newIndex = lastBlock == null ? 1 : lastBlock.Index + 1;
            string prevHash = lastBlock == null ? "0" : lastBlock.Hash;

            var newBlock = new Block
            {
                Index = newIndex,
                Timestamp = DateTime.UtcNow,
                Data = chatData,
                PreviousHash = prevHash
            };

            newBlock.Hash = CalculateHash(newBlock);
            newBlock.ValidatorSignature = _keyService.SignData(newBlock.Hash);

            await _collection.InsertOneAsync(newBlock);
            return newBlock;
        }

        public async Task<(bool IsValid, string Message)> ValidateChainAsync()
        {

            var blocks = await _collection.Find(_ => true).SortBy(x => x.Index).ToListAsync();

            for (int i = 0; i < blocks.Count; i++)
            {
                var current = blocks[i];

                if (current.Hash != CalculateHash(current))
                    return (false, $"Mesaj #{current.Index} içeriği değiştirilmiş!");

                if (!_keyService.VerifySignature(current.Hash, current.ValidatorSignature))
                    return (false, $"Mesaj #{current.Index} sahte imza taşıyor!");

                if (i > 0)
                {
                    var previous = blocks[i - 1];
                    if (current.PreviousHash != previous.Hash)
                        return (false, $"Mesaj #{current.Index} zincirden koparılmış!");
                }
            }
            return (true, "Tüm sohbet geçmişi güvenli ve doğrulanabilir.");
        }

        private string CalculateHash(Block block)
        {
            string rawData = $"{block.Index}-{block.Timestamp}-{JsonSerializer.Serialize(block.Data)}-{block.PreviousHash}";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return Convert.ToHexString(bytes);
        }
    }
}
