using ClientTopicMVC.Models;
using RabbitMQTopicHWReceiverSection.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace RabbitMQTopicHWReceiverSection.Services
{
    public class RedisService : IRedisService
    {

        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        private readonly string nameDivision = "divisions";
        public RedisService()
        {
            _redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { { "redis-16071.c114.us-east-1-4.ec2.redns.redis-cloud.com", 16071 } },
                    User = "default",
                    Password = "FKYHW66StW-A21THXjGj_sIzy7Kn14ue",

                }
            );

            _database = _redis.GetDatabase();
        }

        public void SaveRabbitMQDivisionList(string name, string parentName)
        {
            // Retrieve the divisions data from the database
            var json = _database.StringGet(nameDivision);
            var divisions = string.IsNullOrEmpty(json)
                ? new List<RabbitMQDivision>()
                : JsonSerializer.Deserialize<List<RabbitMQDivision>>(json);

            // If no parentName is provided, treat it as the root level
            if (string.IsNullOrEmpty(parentName))
            {
                divisions.Add(new RabbitMQDivision
                {
                    Name = name,
                    RabbitMQDivisions = new List<RabbitMQDivision>()
                });
            }
            else
            {
                var names = parentName.Split('.', 2);

                // Search for the division at the root level
                var rootDivision = divisions.FirstOrDefault(d => d.Name == names[0]);

                if (rootDivision == null)
                {
                    // If no such division exists, create it at the root level
                    rootDivision = new RabbitMQDivision
                    {
                        Name = names[0],
                        RabbitMQDivisions = new List<RabbitMQDivision>()
                    };
                    divisions.Add(rootDivision);
                }

                // If there is more than one level in the parentName, find the nested division
                if (names.Length > 1)
                {
                    // Recursively find the nested division
                    AddNestedDivision(rootDivision.RabbitMQDivisions, names[1], name);
                }
                else
                {
                    // If no nested division, add the new division directly under the root division
                    rootDivision.RabbitMQDivisions.Add(new RabbitMQDivision
                    {
                        Name = name,
                        RabbitMQDivisions = new List<RabbitMQDivision>()
                    });
                }
            }

            // Save the updated divisions list to the database
            _database.StringSet(nameDivision, JsonSerializer.Serialize(divisions));
        }

        public List<RabbitMQDivision> GetRabbitMQDivisionList()
        {
            // Retrieve the divisions list from the database
            var jsonData = _database.StringGet(nameDivision);

            return string.IsNullOrEmpty(jsonData)
                ? new List<RabbitMQDivision>()
                : JsonSerializer.Deserialize<List<RabbitMQDivision>>(jsonData);
        }

        // Helper method to add a nested division
        private void AddNestedDivision(List<RabbitMQDivision> divisions, string remainingName, string name)
        {
            var names = remainingName.Split('.', 2);

            // Try to find the current division in the list
            var currentDivision = divisions.FirstOrDefault(d => d.Name == names[0]);

            // If the current division is not found, create it
            if (currentDivision == null)
            {
                currentDivision = new RabbitMQDivision
                {
                    Name = names[0],
                    RabbitMQDivisions = new List<RabbitMQDivision>()
                };
                divisions.Add(currentDivision);
            }

            // If there are more parts to the name, call the method recursively
            if (names.Length > 1)
            {
                AddNestedDivision(currentDivision.RabbitMQDivisions, names[1], name);
            }
            else
            {
                // If this is the last part, add the division
                currentDivision.RabbitMQDivisions.Add(new RabbitMQDivision
                {
                    Name = name,
                    RabbitMQDivisions = new List<RabbitMQDivision>()
                });
            }
        }

    }
}
