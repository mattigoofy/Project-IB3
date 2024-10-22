using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;
using System.Text;
using WagentjeApp.Models; // Zorg ervoor dat je het juiste namespace toevoegt voor TrajectCommand
using System.Text.RegularExpressions;

namespace WagentjeApp.Services
{
    public class MqttService
    {
        // Singleton Instance
        private static MqttService _instance;
        public static MqttService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MqttService();
                }
                return _instance;
            }
        }

        private IMqttClient _client;
        private IMqttClientOptions _options;
        private TaskCompletionSource<bool> _loginCompletionSource;
        private TaskCompletionSource<bool> _registerCompletionSource;

        // Private constructor to prevent instantiation
        private MqttService()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            _options = new MqttClientOptionsBuilder()
                .WithClientId("WagentjeAppClient")
                //.WithTcpServer("192.168.0.143", 1883) //thuis
                .WithTcpServer("172.18.230.3", 1883) // lokaal 2.080
                .Build();
            _client.UseApplicationMessageReceivedHandler(OnMessageReceived);
        }

        public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(_options);
                // Abonneer op het juiste topic voor login en register responses
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/login/response").Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/register/response").Build());
            }
        }

        public async Task DisconnectAsync()
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync();
            }
        }

        public async Task PublishMessageAsync(string topic, string message)
        {
            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(message)
                .WithExactlyOnceQoS()
                .Build();

            if (_client.IsConnected)
            {
                await _client.PublishAsync(mqttMessage);
            }
        }

        // Method to send TrajectCommand
        public async Task SendTrajectAsync(TrajectCommand[] commands, int userId)
        {
            // Serialize commands to JSON
            string payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                UserId = userId,
                Commands = commands
            });

            // Topic for sending commands
            string topic = $"raspberrypi/execute_traject";

            // Publish the commands to the MQTT broker
            await PublishMessageAsync(topic, payload);
        }

        // Method for user login
        public async Task<bool> LoginAsync(string username, string password)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password
            });

            // Maak een TaskCompletionSource om op een response te wachten
            _loginCompletionSource = new TaskCompletionSource<bool>();

            // Verbind met de broker
            await ConnectAsync();

            // Publiceer de login gegevens naar de MQTT-broker
            await PublishMessageAsync("raspberrypi/login", payload);

            // Wacht op de response met een timeout van 10 seconden
            var isLoginSuccessful = await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(10000)) == _loginCompletionSource.Task
                                    ? _loginCompletionSource.Task.Result
                                    : false;
            await DisconnectAsync();

            return isLoginSuccessful;
        }

        // Method for user registration
        public async Task<bool> RegisterAsync(string username, string password, string confirmPassword, string email)
        {
            // Controleer of het wachtwoord en bevestigingswachtwoord overeenkomen
            if (password != confirmPassword)
            {
                Console.WriteLine("Wachtwoorden komen niet overeen.");
                return false;
            }

            // Controleer of het emailadres geldig is
            if (!IsValidEmail(email))
            {
                Console.WriteLine("Ongeldig emailadres.");
                return false;
            }

            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password,
                ConfirmPassword = confirmPassword,  // Bevestiging van wachtwoord
                Email = email  // Email veld
            });

            // Maak een TaskCompletionSource om op een response te wachten
            _registerCompletionSource = new TaskCompletionSource<bool>();

            // Verbind met de broker
            await ConnectAsync();

            // Publiceer de registratiegegevens naar de MQTT-broker
            await PublishMessageAsync("raspberrypi/register", payload);

            // Wacht op de response van de server met een timeout van 10 seconden
            var isRegistrationSuccessful = await Task.WhenAny(_registerCompletionSource.Task, Task.Delay(10000)) == _registerCompletionSource.Task
                                    ? _registerCompletionSource.Task.Result
                                    : false;
            await DisconnectAsync();

            return isRegistrationSuccessful;
        }

        // Callback voor ontvangen berichten
        private void OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            if (topic == "raspberrypi/login/response")
            {
                // Verwacht een true of false uit de payload
                bool isSuccess = message.Equals("true", StringComparison.OrdinalIgnoreCase);
                Console.WriteLine($"isSuccess: {isSuccess}");
                _loginCompletionSource?.SetResult(isSuccess);
            }

            if (topic == "raspberrypi/register/response")
            {
                // Verwerk response voor registratie
                Console.WriteLine($"Registratie response: {message}");
                bool isSuccess = message.Equals("Registration successful", StringComparison.OrdinalIgnoreCase);

                if (message.Equals("Username already exists", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Gebruikersnaam bestaat al.");
                }
                else if (message.Equals("Passwords do not match", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Wachtwoorden komen niet overeen.");
                }
                else if (message.Equals("Invalid email address", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Ongeldig emailadres.");
                }

                _registerCompletionSource?.SetResult(isSuccess);
            }
        }

        // Valideer e-mailadres met behulp van een regex
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Example method to get the latest measurement
        public async Task<Measurement> GetLatestMeasurementAsync()
        {
            // Dummy return for demonstration
            return await Task.FromResult(new Measurement { Value = 42 });
        }
    }

    // Dummy Measurement class (replace with actual implementation)
    public class Measurement
    {
        public int Value { get; set; }
    }
}
