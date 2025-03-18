using Amazon.EC2.Model;
using Amazon.EC2;
using MySql.Data.MySqlClient;
using Renci.SshNet;
using System.Data;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using AWS_QA_Course_Test_Project.DTOs;
using Newtonsoft.Json;
using System.Net.Sockets;

namespace AWS_QA_Course_Test_Project.Utils
{
    public class DatabaseConnectionManager : IDisposable
    {
        private SshClient _sshClient;
        private ForwardedPortLocal _portForwarded;
        private MySqlConnection _connection;

        public async Task<MySqlConnection> ConnectToDatabaseAsync()
        {
            // Retrieve database secret from AWS Secrets Manager
            string secretName = await GetSecretNameAsync();
            var secretsManagerClient = new AmazonSecretsManagerClient();
            var getSecretValueRequest = new GetSecretValueRequest
            {
                SecretId = secretName
            };
            var getSecretValueResponse = await secretsManagerClient.GetSecretValueAsync(getSecretValueRequest);
            var secretString = getSecretValueResponse.SecretString;
            var secret = JsonConvert.DeserializeObject<SecretValuesDTO>(secretString);

            string sshUser = "ec2-user";
            int localPort = 3306;
            string privateKeyFilePath = "Config/ssh_key_for_db.pem";

            // Retrieve EC2 instance details for SSH
            var sshKeyName = await GetSshKeyNameAsync();
            var ec2Client = new AmazonEC2Client();
            var describeInstancesRequest = new DescribeInstancesRequest
            {
                Filters = new List<Amazon.EC2.Model.Filter>
                {
                    new Amazon.EC2.Model.Filter("key-name", new List<string> { sshKeyName })
                }
            };
            //var describeInstancesResponse = await ec2Client.DescribeInstancesAsync(describeInstancesRequest);
            //var ec2Instance = describeInstancesResponse.Reservations.SelectMany(r => r.Instances).FirstOrDefault();
            var instances = await EC2Helper.DescribeInstancesAsync(ec2Client);
            var ec2Instance = EC2Helper.GetPublicInstance(instances);

            if (ec2Instance == null)
            {
                throw new Exception("EC2 instance not found.");
            }

            var sshHost = ec2Instance.PublicIpAddress;

            // Establish SSH connection and port forwarding
            _sshClient = new SshClient(sshHost, 22, sshUser, new PrivateKeyFile(privateKeyFilePath));
            _sshClient.Connect();
            Console.WriteLine("SSH connection established.");

            _portForwarded = new ForwardedPortLocal("127.0.0.1", (uint)localPort, secret.Host, (uint)secret.Port);
            _sshClient.AddForwardedPort(_portForwarded);
            _portForwarded.Start();
            Console.WriteLine($"Port forwarding started: 127.0.0.1:{localPort} -> {secret.Host}:{secret.Port}");

            string connectionString = $"Server=127.0.0.1;Port={localPort};Database={secret.DBname};Uid={secret.Username};Pwd={secret.Password};";
            Console.WriteLine($"Connection string: {connectionString}");

            _connection = new MySqlConnection(connectionString);
            await _connection.OpenAsync();
            Console.WriteLine("Connection opened.");

            if (_connection.State != ConnectionState.Open)
            {
                throw new Exception("Failed to connect to the database.");
            }

            Console.WriteLine("Connection state is open.");
            return _connection;
        }

        private async Task<string> GetSshKeyNameAsync()
        {
            var ec2Client = new AmazonEC2Client();
            var describeKeyPairsResponse = await ec2Client.DescribeKeyPairsAsync(new DescribeKeyPairsRequest());
            var keyPair = describeKeyPairsResponse.KeyPairs.FirstOrDefault();

            if (keyPair == null)
            {
                throw new Exception("No SSH key pairs found.");
            }

            return keyPair.KeyName;
        }

        private async Task<string> GetSecretNameAsync()
        {
            var secretsManagerClient = new AmazonSecretsManagerClient();
            var listSecretsResponse = await secretsManagerClient.ListSecretsAsync(new ListSecretsRequest());
            var secret = listSecretsResponse.SecretList.FirstOrDefault(s => s.Name.Contains("Database"));

            if (secret == null)
            {
                throw new Exception("No database secrets found.");
            }

            return secret.Name;
        }

        public void Dispose()
        {
            _connection?.Dispose();
            if (_portForwarded != null)
            {
                _portForwarded.Stop();
                Console.WriteLine("Port forwarding stopped.");
            }
            if (_sshClient != null && _sshClient.IsConnected)
            {
                _sshClient.Disconnect();
                Console.WriteLine("SSH connection closed.");
            }
        }
    }
}
