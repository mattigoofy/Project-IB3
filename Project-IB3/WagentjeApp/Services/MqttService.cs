using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System.Text;
using WagentjeApp.Models;
using Newtonsoft.Json;

namespace WagentjeApp.Services
{
    public class MqttService
    {
        // Singleton instantie
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

        private IMqttClient _client; // MQTT client
        private IMqttClientOptions _options; // MQTT client opties
        private TaskCompletionSource<bool> _loginCompletionSource; // Voor login resultaat
        private TaskCompletionSource<bool> _registerCompletionSource; // Voor registratie resultaat
        private TaskCompletionSource<List<Traject>> _loadTrajectsCompletionSource; // Voor het laden van trajecten
        private TaskCompletionSource<List<Measurement>> _allMeasurementsCompletionSource; // Voor het laden van alle metingen
        private TaskCompletionSource<bool> _executeTrajectCompletionSource; // Voor het uitvoeren van trajecten
        private TaskCompletionSource<Measurement> _measurementCompletionSource; // Voor het ontvangen van metingen
        private User _currentUser; // Huidige gebruiker
        private string _mqttServerIp = "172.18.230.3"; // Standaard IP-adres

        // Privé constructor om instantiatie te voorkomen
        private MqttService()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            InitializeMqttOptions();
            _client.UseApplicationMessageReceivedHandler(OnMessageReceived); // Handler voor ontvangen berichten
        }

        // Initialiseer de MQTT opties
        private void InitializeMqttOptions()
        {
            _options = new MqttClientOptionsBuilder()
                .WithClientId("WagentjeAppClient")
                .WithTcpServer(_mqttServerIp, 1883) // Gebruik het opgeslagen IP-adres
                .Build();
        }

        // Verbind met de MQTT broker
        public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(_options);
                // Abonneer op relevante topics
                await SubscribeToTopics();
            }
        }

        // Abonneer op de benodigde topics
        private async Task SubscribeToTopics()
        {
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/login/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/register/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/load_trajects/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/execute_traject/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/execute_command/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/all_measurements/response").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/measurement").Build());
            await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/live_measurement").Build());
        }

        // Verbreek de verbinding met de MQTT broker
        public async Task DisconnectAsync()
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync();
            }
        }

        // Publiceer een bericht naar een specifiek topic
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

        // Callback voor ontvangen berichten
        private void OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            var message = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);

            // Verwerk het ontvangen bericht op basis van het topic
            switch (topic)
            {
                case "raspberrypi/login/response":
                    HandleLoginResponse(message);
                    break;
                case "raspberrypi/register/response":
                    HandleRegisterResponse(message);
                    break;
                case "raspberrypi/load_trajects/response":
                    HandleLoadTrajectsResponse(message);
                    break;
                case "raspberrypi/execute_traject/response ":
                    HandleExecuteTrajectResponse(message);
                    break;
                case "raspberrypi/measurement":
                    HandleMeasurement(message);
                    break;
                case "raspberrypi/all_measurements/response":
                    HandleAllMeasurementsResponse(message);
                    break;
            }
        }

        // Verwerk de login response
        private void HandleLoginResponse(string message)
        {
            message = AesEncryption.Decrypt(message);
            var loginResponse = JsonConvert.DeserializeObject<dynamic>(message);
            bool isSuccess = loginResponse.userId != null;
            if (isSuccess)
            {
                int userId = loginResponse.userId;
                _currentUser = new User
                {
                    Username = loginResponse.username,
                    UserId = userId
                };
            }
            _loginCompletionSource?.SetResult(isSuccess);
        }

        // Verwerk de registratie response
        private void HandleRegisterResponse(string message)
        {
            message = AesEncryption.Decrypt(message);
            bool isSuccess = message.Equals("Registration successful", StringComparison.OrdinalIgnoreCase);
            _registerCompletionSource?.SetResult(isSuccess);
        }

        // Verwerk de load trajects response
        private void HandleLoadTrajectsResponse(string message)
        {
            var trajects = JsonConvert.DeserializeObject<List<Traject>>(message);
            _loadTrajectsCompletionSource?.SetResult(trajects);
        }

        // Verwerk de execute traject response
        private void HandleExecuteTrajectResponse(string message)
        {
            bool isSuccess = message.Equals("true", StringComparison.OrdinalIgnoreCase);
            _executeTrajectCompletionSource?.SetResult(isSuccess);
        }

        // Verwerk de meetwaarde
        private void HandleMeasurement(string message)
        {
            try
            {
                var measurement = JsonConvert.DeserializeObject<Measurement>(message);
                _measurementCompletionSource?.TrySetResult(measurement);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij verwerking meetwaarde: {ex.Message}");
            }
        }

        // Verwerk de all measurements response
        private void HandleAllMeasurementsResponse(string message)
        {
            var measurements = JsonConvert.DeserializeObject<List<Measurement>>(message);
            _allMeasurementsCompletionSource?.SetResult(measurements);
        }

        // Methode voor gebruikerslogin
        public async Task<bool> LoginAsync(string username, string password)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password
            });
            payload = AesEncryption.Encrypt(payload);

            _loginCompletionSource = new TaskCompletionSource<bool>();
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/login", payload);

            var isLoginSuccessful = await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(10000)) == _loginCompletionSource.Task
                                    ? _loginCompletionSource.Task.Result
                                    : false;
            await DisconnectAsync();
            return isLoginSuccessful;
        }

        // Methode voor gebruikersregistratie
        public async Task<bool> RegisterAsync(string username, string password, string confirmPassword, string email)
        {
            // Valideer wachtwoorden en email
            if (password != confirmPassword)
            {
                Console.WriteLine("Wachtwoorden komen niet overeen.");
                return false;
            }

            if (!IsValidEmail(email))
            {
                Console.WriteLine("Ongeldig emailadres.");
                return false;
            }

            var payload = JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password,
                Email = email
            });
            payload = AesEncryption.Encrypt(payload);

            _registerCompletionSource = new TaskCompletionSource<bool>();
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/register", payload);

            var isRegistrationSuccessful = await Task.WhenAny(_registerCompletionSource.Task, Task.Delay(10000)) == _registerCompletionSource.Task
                ? _registerCompletionSource.Task.Result
                : false;

            await DisconnectAsync();
            return isRegistrationSuccessful;
        }

        // Methode om een TrajectCommand te verzenden
        public async Task ExecuteCommandAsync(TrajectCommand command, int userId)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                UserId = userId,
                Command = command
            });
            await ConnectAsync();
            string topic = "raspberrypi/execute_command";
            await PublishMessageAsync(topic, payload);
            await DisconnectAsync();
        }

        // Methode om een traject uit te voeren
        public async Task<bool> ExecuteTrajectAsync(int trajectId, int userId)
        {
            var payload = JsonConvert.SerializeObject(new { TrajectId = trajectId, UserId = userId });
            _executeTrajectCompletionSource = new TaskCompletionSource<bool>();

            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/execute_traject", payload);

            var isSuccess = await Task.WhenAny(_executeTrajectCompletionSource.Task, Task.Delay(10000)) == _executeTrajectCompletionSource.Task
                ? _executeTrajectCompletionSource.Task.Result
                : false;

            await DisconnectAsync();
            return isSuccess;
        }

        // Methode om een traject te verwijderen
        public async Task DeleteTrajectAsync(int trajectId, int userId)
        {
            var payload = JsonConvert.SerializeObject(new { TrajectId = trajectId, UserId = userId });
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/delete_traject", payload);
            await DisconnectAsync();
        }

        // Methode voor het laden van opgeslagen trajecten
        public async Task<List<Traject>> LoadTrajectsAsync(int userId)
        {
            var payload = JsonConvert.SerializeObject(new { UserId = userId });
            _loadTrajectsCompletionSource = new TaskCompletionSource<List<Traject>>();

            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/load_trajects", payload);

            var trajectsList = await Task.WhenAny(_loadTrajectsCompletionSource.Task, Task.Delay(10000)) == _loadTrajectsCompletionSource.Task
                ? _loadTrajectsCompletionSource.Task.Result
                : new List<Traject>();

            await DisconnectAsync();
            return trajectsList;
        }

        // Methode om een nieuw traject op te slaan
        public async Task SaveTrajectAsync(Traject traject, int userId, bool edit, int trajectId)
        {
            var payload = JsonConvert.SerializeObject(new { UserId = userId, Traject = traject, Edit = edit, TrajectId = trajectId });
            await ConnectAsync();

            try
            {
                await PublishMessageAsync("raspberrypi/save_traject", payload);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Publicatie mislukt: {ex.Message}");
            }
            finally
            {
                await DisconnectAsync();
            }
        }

        // Methode voor het laden van metingen
        public async Task<List<Measurement>> LoadMeasurementsAsync(DateTime startTimestamp, DateTime endTimestamp, int startValue, int endValue)
        {
            var payload = JsonConvert.SerializeObject(new { startTimestamp, endTimestamp, startValue, endValue });
            _allMeasurementsCompletionSource = new TaskCompletionSource<List<Measurement>>();

            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/all_measurements", payload);

            var measurementsList = await Task.WhenAny(_allMeasurementsCompletionSource.Task, Task.Delay(30000)) == _allMeasurementsCompletionSource.Task
                ? _allMeasurementsCompletionSource.Task.Result
                : new List<Measurement>();

            await DisconnectAsync();
            return measurementsList;
        }

        // Methode om de laatste meting te krijgen
        public async Task<Measurement> GetLatestMeasurementAsync()
        {
            _measurementCompletionSource = new TaskCompletionSource<Measurement>();

            // Wacht op het bericht of een timeout
            var measurement = await Task.WhenAny(_measurementCompletionSource.Task, Task.Delay(10000)) == _measurementCompletionSource.Task
                ? _measurementCompletionSource.Task.Result
                : null;

            if (measurement == null)
            {
                throw new TimeoutException("Geen meetwaarde ontvangen binnen 10 seconden.");
            }
            return measurement;
        }

        // Methode om de huidige gebruiker te krijgen
        public User GetCurrentUser()
        {
            return _currentUser;
        }

        // Methode voor uitloggen
        public async Task LogoutAsync()
        {
            // Verbreek de verbinding
            await DisconnectAsync();
            Console.WriteLine("Gebruiker is uitgelogd en MQTT-verbinding is verbroken.");
        }

        // Valideer het emailformaat
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

        // Methode om het huidige IP-adres te krijgen
        public string GetIpAddress()
        {
            return _mqttServerIp;
        }

        // Methode om het nieuwe IP-adres in te stellen
        public void SetIpAddress(string ipAddress)
        {
            _mqttServerIp = ipAddress;
            InitializeMqttOptions(); // Update de MQTT opties met het nieuwe IP-adres
        }
    }
}