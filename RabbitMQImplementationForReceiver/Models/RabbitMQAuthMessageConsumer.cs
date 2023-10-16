using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.ComponentModel;
using System.Text;

namespace RabbitMQImplementationForReceiver.Models
{
	public class RabbitMQAuthMessageConsumer : BackgroundService
	{
		private readonly IConfiguration _configuration;
		private	readonly IConnection _connection;
		private readonly IModel _channel;
		public RabbitMQAuthMessageConsumer(IConfiguration configuration)
        {
				_configuration = configuration;
			var factory = new ConnectionFactory
			{
				HostName = "localhost",
				UserName = "guest",
				Password = "guest",
			};
			_connection=factory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.QueueDeclare(_configuration.GetValue<string>("QueueName:RegisterUserQueue"), false, false, false, null);

		}
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			stoppingToken.ThrowIfCancellationRequested();

			var consumer = new EventingBasicConsumer(_channel);
			consumer.Received += (ch, ea) =>
			{
				var content = Encoding.UTF8.GetString(ea.Body.ToArray());
				String email = JsonConvert.DeserializeObject<string>(content);
				HandleMessage(email).GetAwaiter().GetResult();
				_channel.BasicAck(ea.DeliveryTag, false);
								
			};
			_channel.BasicConsume(_configuration.GetValue<string>("QueueName:RegisterUserQueue"), false,consumer);
			return Task.CompletedTask;
		}

		private async  Task HandleMessage(String email)
		{
			Console.WriteLine(email);
		}
	}
}
