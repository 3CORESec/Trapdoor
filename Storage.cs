using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;

namespace Trapdoor
{
    [DynamoDBTable("TRAPDOOR_LOG")]
    public class SessionLog
    {
        [DynamoDBHashKey] public string Id { get; set; }
        [DynamoDBProperty] public byte[] Data { get; set; }
    }

    public class Storage<T> : DynamoDBContext
    {


        public Storage(IAmazonDynamoDB client) : base(client)
        {
        }

        public async Task<List<T>> GetByHash(string hash)
        {
            try
            {
                var res = await QueryAsync<T>(hash).GetRemainingAsync();
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<T> Get(string hash, string range)
        {
            try
            {
                var res = await LoadAsync<T>(hash, range);
                return res;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public async Task<T> Store(T dto)
        {
            try
            {
                await SaveAsync(dto);
                return dto;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
