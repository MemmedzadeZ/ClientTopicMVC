using ClientTopicMVC.Models;
using RabbitMQTopicHWReceiverSection.Models;

namespace RabbitMQTopicHWReceiverSection.Services
{
    public interface IRedisService
    {

        void SaveRabbitMQDivisionList(string name, string parentName);
        List<RabbitMQDivision> GetRabbitMQDivisionList();
    } 
}
