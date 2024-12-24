namespace ClientTopicMVC.Models
{
    public class HomeViewModel
    {
        public List<RabbitMQDivision> RabbitMQDivisions { get; set; }
        public string RoutingKey { get; set; }
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
    }

}
