using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Runtime;

namespace UdpService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;      
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Config _c = Config.Instance;

                if (_c.Port == null || string.IsNullOrEmpty(_c.ConnectionString))
                {
                    throw new ArgumentException("Settings missing in appsettings.");
                }
                
                while (!stoppingToken.IsCancellationRequested)
                {               

                    _logger.LogInformation("UDP IOT service started.", DateTimeOffset.Now);

                    using var udpSocket = new Socket(SocketType.Dgram, ProtocolType.Udp);

                    var cancelToken = stoppingToken;

                    // Discard our socket when the user cancels.
                    using var cancelReg = cancelToken.Register(() => udpSocket.Dispose());

                    // Server.
                    IPEndPoint _blankEndpoint = new IPEndPoint(IPAddress.Any, (int)_c.Port);

                    udpSocket.Bind(new IPEndPoint(IPAddress.Any, (int)_c.Port));
                    
                    await DoReceiveAsync(udpSocket, cancelToken, _blankEndpoint);

                }

            } catch (Exception ex)
            {
                _logger.LogError(ex.Message, DateTimeOffset.Now);
                
            }     
           
        }

        private static async Task DoReceiveAsync(Socket udpSocket, CancellationToken cancelToken, IPEndPoint blankEndpoint)
        {
            byte[] buffer = GC.AllocateArray<byte>(length: 1024, pinned: true);

            Memory<byte> bufferMem = buffer.AsMemory();

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    var result = await udpSocket.ReceiveFromAsync(bufferMem, SocketFlags.None, blankEndpoint);

                    byte[] data = bufferMem.Slice(0, result.ReceivedBytes).ToArray();

                    ThreadPool.QueueUserWorkItem((state) => ReadAndSaveData(data));
                }
                catch (SocketException ex)
                {                         
                    
                    break;
                }

            }

        }

        static void ReadAndSaveData(Memory<byte> data)
        {
            Config _c = Config.Instance;
            string jsonString;
            string dataString = BitConverter.ToString(data.ToArray()).Replace("-", "");

            if (_c.Type == null)
            {
                jsonString = "{ \"data\":\"" + dataString + "\"}";
            }
            else
            {
                jsonString = "{ \"devicetype\":\"" + _c.Type + "\", \"data\":\"" + dataString + "\"}";
            }

            string queryString = $"INSERT INTO [Staging] (Message) VALUES ('{jsonString}')";           
                       
            using (SqlConnection connection = new SqlConnection(_c.ConnectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }

        }
    } 
    
}

