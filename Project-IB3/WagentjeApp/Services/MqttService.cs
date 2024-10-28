﻿using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using WagentjeApp.Models;  // Gebruik enkel de Models namespace voor Traject en TrajectCommand
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace WagentjeApp.Services
{
    public class MqttService
    {
        // Singleton instance
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
        private TaskCompletionSource<List<Traject>> _loadTrajectsCompletionSource;
        private TaskCompletionSource<bool> _executeTrajectCompletionSource;
        private User _currentUser; // Voor het opslaan van de huidige gebruiker
        private string _mqttServerIp = "192.168.0.143"; // Standaard IP-adres
                                   //private string _mqttServerIp = "172.18.230.3"; // Standaard IP-adres


        // Private constructor to prevent instantiation
        private MqttService()
        {
            var factory = new MqttFactory();
            _client = factory.CreateMqttClient();
            InitializeMqttOptions();
            _client.UseApplicationMessageReceivedHandler(OnMessageReceived);
        }
        private void InitializeMqttOptions()
        {
            _options = new MqttClientOptionsBuilder()
                .WithClientId("WagentjeAppClient")
                .WithTcpServer(_mqttServerIp, 1883) // Gebruik het opgeslagen IP-adres
                .Build();
        }
        public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                await _client.ConnectAsync(_options);
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/login/response").Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/register/response").Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/load_trajects/response").Build());
                await _client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic("raspberrypi/execute_traject/response").Build());
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
            string payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                UserId = userId,
                Commands = commands
            });

            string topic = "raspberrypi/execute_traject";
            await PublishMessageAsync(topic, payload);
        }

        // Method for loading saved trajectories
        public async Task<List<Traject>> LoadTrajectsAsync(int userId)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { UserId = userId });
            _loadTrajectsCompletionSource = new TaskCompletionSource<List<Traject>>();

            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/load_trajects", payload);

            var trajectsList = await Task.WhenAny(_loadTrajectsCompletionSource.Task, Task.Delay(10000)) == _loadTrajectsCompletionSource.Task
                ? _loadTrajectsCompletionSource.Task.Result
                : new List<Traject>();

            await DisconnectAsync();
            return trajectsList;
        }

        // Method to save a new trajectory
        public async Task SaveTrajectAsync(Traject traject, int userId)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { UserId = userId, Traject = traject });
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

        // Method to delete a trajectory
        public async Task DeleteTrajectAsync(int trajectId, int userId)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { TrajectId = trajectId, UserId = userId });
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/delete_traject", payload);
            await DisconnectAsync();
        }

        // Method to execute a trajectory
        public async Task<bool> ExecuteTrajectAsync(int trajectId, int userId)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new { TrajectId = trajectId, UserId = userId });
            _executeTrajectCompletionSource = new TaskCompletionSource<bool>();

            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/execute_traject", payload);

            var isSuccess = await Task.WhenAny(_executeTrajectCompletionSource.Task, Task.Delay(10000)) == _executeTrajectCompletionSource.Task
                ? _executeTrajectCompletionSource.Task.Result
                : false;

            await DisconnectAsync();
            return isSuccess;
        }

        // Method for user login
        public async Task<bool> LoginAsync(string username, string password)
        {
            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password
            });

            _loginCompletionSource = new TaskCompletionSource<bool>();
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/login", payload);

            var isLoginSuccessful = await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(10000)) == _loginCompletionSource.Task
                                    ? _loginCompletionSource.Task.Result
                                    : false;
            await DisconnectAsync();
            return isLoginSuccessful;
        }

        public User GetCurrentUser()
        {
            return _currentUser;
        }

        public async Task LogoutAsync()
        {
            // Verbreek de verbinding
            await DisconnectAsync();
            Console.WriteLine("Gebruiker is uitgelogd en MQTT-verbinding is verbroken.");
        }

        // Method for user registration
        public async Task<bool> RegisterAsync(string username, string password, string confirmPassword, string email)
        {
            // Validate passwords and email
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

            var payload = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Username = username,
                Password = password,
                Email = email
            });

            _registerCompletionSource = new TaskCompletionSource<bool>();
            await ConnectAsync();
            await PublishMessageAsync("raspberrypi/register", payload);

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
            if (topic == "raspberrypi/register/response")
            {
                bool isSuccess = message.Equals("Registration successful", StringComparison.OrdinalIgnoreCase);
                _registerCompletionSource?.SetResult(isSuccess);
            }
            if (topic == "raspberrypi/load_trajects/response")
            {
                var trajects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Traject>>(message);
                _loadTrajectsCompletionSource?.SetResult(trajects);
            }
            if (topic == "raspberrypi/execute_traject/response")
            {
                bool isSuccess = message.Equals("true", StringComparison.OrdinalIgnoreCase);
                _executeTrajectCompletionSource?.SetResult(isSuccess);
            }
        }

        // Validate email format
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

        // Method to get the current IP address
        public string GetIpAddress()
        {
            return _mqttServerIp;
        }

        // Method to set the new IP address
        public void SetIpAddress(string ipAddress)
        {
            _mqttServerIp = ipAddress;
            InitializeMqttOptions(); // Update the MQTT options with the new IP address
        }

        // Example method to get the latest measurement
        public async Task<Measurement> GetLatestMeasurementAsync()
        {
            return await Task.FromResult(new Measurement { Value = 42 });
        }

        // Dummy Measurement class (replace with actual implementation)
        public class Measurement
        {
            public int Value { get; set; }
        }
    }



}
