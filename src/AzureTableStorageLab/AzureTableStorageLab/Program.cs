using System;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;

namespace AzureTableStorageLab
{
    internal class Program
    {
        private static readonly string ConnectionString = "";

        private static async Task Main(string[] args)
        {
            //await Create();
            //await QueryByKey();
            //await QueryByFilter();
            //await Update();
            //await ReplaceUpdate();
            //await Upsert();
            await Delete();

            Console.WriteLine("Done.");
            Console.Read();
        }

        private static Task Create()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            var memberEntity = new MemberEntity("總管理處", 1) { Name = "Johnny", City = "Taipei", ModifiedCount = 1 };

            return tableClient.AddEntityAsync(memberEntity);
        }

        private static async Task QueryByKey()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            var response = await tableClient.GetEntityAsync<MemberEntity>("總管理處", "1");

            Console.WriteLine(response.Value.Name);

            // 回傳部分欄位
            response = await tableClient.GetEntityAsync<MemberEntity>("總管理處", "1", new[] { nameof(MemberEntity.City) });

            Console.WriteLine(response.Value.City);
        }

        private static async Task QueryByFilter()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            var response = tableClient.QueryAsync<MemberEntity>(x => x.PartitionKey == "總管理處" && x.City == "Taipei");
            
            await foreach (var page in response.AsPages())
            {
                foreach (var memberEntity in page.Values)
                {
                    Console.WriteLine(memberEntity.Name);
                }
            }
        }

        private static async Task Update()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            var queryResponse = await tableClient.GetEntityAsync<MemberEntity>("總管理處", "1");

            var memberEntity = queryResponse.Value;

            memberEntity.City = "New Taipei";

            _ = await tableClient.UpdateEntityAsync(memberEntity, ETag.All);

            // 更新部分欄位
            _ = await tableClient.UpdateEntityAsync(new TableEntity("總管理處", "1") { { "City", "New Taipei" } }, ETag.All);

            //memberEntity.ModifiedCount += 1;

            //_ = await tableClient.UpdateEntityAsync(memberEntity, memberEntity.ETag);

        }

        private static async Task ReplaceUpdate()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            // 取代更新
            _ = await tableClient.UpdateEntityAsync(new TableEntity("總管理處", "1") { { "Name", "Tom" } }, ETag.All, TableUpdateMode.Replace);
        }

        private static async Task Upsert()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            var memberEntity = new MemberEntity("總管理處", 1) { Name = "Johnny", City = "Taipei", ModifiedCount = 1 };

            _ = await tableClient.UpsertEntityAsync(memberEntity);

        }

        private static async Task Delete()
        {
            var tableServiceClient = new TableServiceClient(ConnectionString);

            var tableClient = tableServiceClient.GetTableClient("MyTable");

            _ = await tableClient.DeleteEntityAsync("總管理處", "1");

        }
    }

    public class MemberEntity : ITableEntity
    {
        private string partitionKey;
        private string rowKey;

        public MemberEntity()
        {
        }

        public MemberEntity(string department, int id)
        {
            this.Department = department;
            this.Id = id;
        }

        public string PartitionKey
        {
            get => this.partitionKey ??= this.Department;
            set => this.partitionKey = value;
        }

        public string RowKey
        {
            get => this.rowKey ??= this.Id.ToString();
            set => this.rowKey = value;
        }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        // ==User Defined Properities==
        public int Id { get; set; }

        public string Name { get; set; }

        public string Department { get; set; }

        public string City { get; set; }

        public long ModifiedCount { get; set; }

        // ==User Defined Properities==
    }
}