using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using PurpleTreeSoftware.Network.Common;

namespace PurpleTreeSoftware.Network
{
    public class Client
    {
    
        StreamSocket _socket;
        CancellationTokenSource _cts;
        String _password;
        
        // Constructor
        public Client()
        {
           
        }


        /// <summary>
        /// Creates a new client connection
        /// </summary>
        /// <param name="p_destIP">Destination IP</param>
        /// <param name="p_destPort">Destination Port</param>
        /// <param name="p_passphrase">Passphrase to use for encryption</param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(string p_destIP, string p_destPort, string p_passphrase)
        {
            
            // Create a new cancellation token if one does not exist
            if (_cts == null)
            {
                _cts = new CancellationTokenSource();
            }

            if (!_cts.Token.IsCancellationRequested)
            {
                _password = p_passphrase;

                // Create a new stream socket
                var hostName = new HostName(p_destIP);
                _socket = new StreamSocket();

                //Indicates whether keep-alive packets are sent to the remote destination
                _socket.Control.KeepAlive = true;

                //Connect              
                await _socket.ConnectAsync(hostName, p_destPort);                    
                   
                return true;
            }
            else
            {
                return false;
            }
            
        }


      
        /// <summary>
        /// Sends a messgage
        /// </summary>
        /// <param name="pMessage">The message to send</param>
        /// <returns></returns>
        public async Task<bool> SendAsync(Message pMessage)
        {
            
            // Only send if cancellation is not requested
            if (!_cts.Token.IsCancellationRequested) {

                // Serialize the message
                String text = helper.Serialize<Message>(pMessage);

                // DataWriter
                var writer = new DataWriter(_socket.OutputStream);

                //Encrypt message            
                byte[] data = helper.Encrypt(text, _password);
                    
                //Write length of message in buffer
                writer.WriteInt32(data.Length);

                //Write message in buffer
                writer.WriteBytes(data);

                //Send buffer
                await writer.StoreAsync();

                //Clear buffer
                await writer.FlushAsync();

                //Detach the Stream
                writer.DetachStream();
                writer.Dispose();
                return true;
            }
            else {
                return false;
            }
            
        }

        /// <summary>
        /// Disconnects the client
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectAsync()
        {  
            _cts.Cancel();
            await _socket.CancelIOAsync();
            _cts = null;           
        }


   


    }
}
