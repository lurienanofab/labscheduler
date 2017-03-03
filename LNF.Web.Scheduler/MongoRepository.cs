using LNF.Models.Scheduler;
using LNF.Repository.Scheduler;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using MongoDB.Driver.Linq;

namespace LNF.Web.Scheduler
{
    public class MongoRepository
    {
        private static MongoRepository _Current;

        static MongoRepository()
        {
            _Current = new MongoRepository();
        }

        public static MongoRepository Current
        {
            get { return _Current; }
        }

        private MongoClient _client;

        private MongoRepository()
        {
            string connstr = ConfigurationManager.AppSettings["MongoConnectionString"];
            _client = new MongoClient(connstr);
        }

        public SchedulerProperties GetProperties()
        {
            var db = _client.GetDatabase("scheduler");
            var col = db.GetCollection<SchedulerProperties>("properties");
            var props = col.Find(Builders<SchedulerProperties>.Filter.Empty).FirstOrDefault();
            if (props == null)
            {
                props = new SchedulerProperties()
                {
                    Granularities = new Dictionary<int, int[]> { { 0, new[] { 5, 10, 15, 30, 60, 120, 180, 240 } } }
                };

                col.InsertOne(props);
            }
            return props;
        }

        public UserState GetUserState(int clientId)
        {
            var db = _client.GetDatabase("scheduler");
            var col = db.GetCollection<UserState>("userstate");

            // setup the ttl index (hopefully nothing happens if it already exists)
            var options = new CreateIndexOptions<UserState>();
            var builder = new IndexKeysDefinitionBuilder<UserState>();
            var keys = builder.Ascending(x => x.AccessedAt);
            options.Name = "accessedAt_ttl_index";
            options.ExpireAfter = TimeSpan.FromMinutes(30);
           
            col.Indexes.CreateOne(keys, options);

            var result = col.Find(x => x.ClientID == clientId).FirstOrDefault();

            if (result == null)
                result = CreateUserState(clientId);

            result.AccessedAt = DateTime.Now;

            col.ReplaceOne(x => x.ClientID == clientId, result, new UpdateOptions() { IsUpsert = true });

            return result;
        }

        public bool UpdateUserState(UserState model)
        {
            var db = _client.GetDatabase("scheduler");
            var col = db.GetCollection<UserState>("userstate");
            model.ModifiedAt = DateTime.Now;
            var updateResult = col.ReplaceOne(x => x.ClientID == model.ClientID, model);
            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        private UserState CreateUserState(int clientId)
        {
            var cs = ClientSetting.GetClientSettingOrDefault(clientId);

            var result = new UserState()
            {
                ClientID = clientId,
                //Date = DateTime.Now.Date,
                View = cs.GetDefaultViewOrDefault(),
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                AccessedAt = DateTime.Now,
                Actions = new List<UserAction>()
            };

            return result;
        }
    }

    public class SchedulerProperties
    {
        public ObjectId Id { get; set; }

        public Dictionary<int, int[]> Granularities { get; set; }

        public int[] GetGranularityValues(int resourceId)
        {
            if (Granularities.ContainsKey(resourceId))
                return Granularities[resourceId];
            else
                return Granularities[0];
        }
    }
}
