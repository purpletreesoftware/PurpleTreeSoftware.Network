using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using PurpleTreeSoftware.Network.Common;

namespace PurpleTreeSoftware.Network
{
    public class Server
    {

        private CancellationTokenSource _cts;
        private StreamSocketListener _listener;
        private String _password;
        readonly Guid _token = Guid.NewGuid(); 
        private readonly Client _serverClient = new Client();        
        private Host _activeHost;
        private DateTime _ServerHeartbeat;
        private DispatcherTimer _dispatcherServerHeartbeatTimer;
       
        public int HeartbeatInterval { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pHeartBeatSeconds">Number of seconds to send heartbeat. Set to zero to disable heartbeat.</param>
        public Server(int pHeartBeatSeconds)
        {
            // Initialise server heartbeat timer
            if (pHeartBeatSeconds > 0) { 
                _dispatcherServerHeartbeatTimer = new DispatcherTimer();
                _dispatcherServerHeartbeatTimer.Tick += dispatcherTimer_HeartBeat_Tick;
                _dispatcherServerHeartbeatTimer.Interval = new TimeSpan(0, 0, pHeartBeatSeconds);
                HeartbeatInterval = pHeartBeatSeconds;
            }

        }
        

        /// <summary>
        /// Start listener
        /// </summary>
        /// <param name="p_host"></param>
        /// <param name="p_port"></param>
        /// <param name="p_password"></param>
        /// <returns></returns>
        public async Task<StreamSocketListener> StartListener(Host p_host, String p_port, String p_password) {
           
           
                // Set encryption password
                if (p_password.Length < 6)
                {
                    throw new System.ArgumentException("Passphrase must be at least 6 characters");
                }
                _password = p_password;

                
                // Bind a new end point if a different host is selected or if the local port is not bound or is changed
                if (_activeHost == null || _activeHost.Name != p_host.Name || _listener == null || _listener.Information == null || _listener.Information.LocalPort != p_port) {
                    // Cancel current connection if it exists
                    await CloseConnection();
                                  
                    //Create a new Cancel Token and socket listener     
                    _cts = new CancellationTokenSource();
                    _listener = new StreamSocketListener();

                    //Assigned event when have a new connection
                    _listener.ConnectionReceived += Listener_ConnectionReceived;
                               
                    //Bind port                              
                    
                    await _listener.BindEndpointAsync(p_host.Name, p_port);
                    _activeHost = p_host;

                       
                    // Initiate a client connection to the server for testing the connection is still open
                    var connIinit = await _serverClient.ConnectAsync(p_host.DisplayString, _listener.Information.LocalPort, p_password);

                    // Start timer, send heartbeat periodically  
                    if (_dispatcherServerHeartbeatTimer != null) { 
                        _dispatcherServerHeartbeatTimer.Start();
                    }
            }
                else
                {                    
                    throw new ListenerAlreadyRunningException("Listener already started on " + p_host.DisplayString + ", port " + p_port);
                }


                return _listener;           
            
        }

        /// <summary>
        /// Sends a heartbeat to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void dispatcherTimer_HeartBeat_Tick(object sender, object e)
        {
            Message msg = new Message();           
            msg.Content = DateTime.UtcNow.ToString();
            msg.Token = _token.ToString();
            var msgsuccess = await _serverClient.SendAsync(msg);
        }

        /// <summary> 
        /// Invoked once a connection is accepted by StreamSocketListener. 
        /// </summary> 
        /// <param name="sender">The listener that accepted the connection.</param> 
        /// <param name="args">Parameters associated with the accepted connection.</param> 
        private async void Listener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {

           
           StreamSocket socket = args.Socket;
            var reader = new DataReader(args.Socket.InputStream);
            try
            {
                           
                while (_cts != null && !_cts.IsCancellationRequested)
                {
                    // Receive data
                    byte[] data = await ReceiveData(reader);

                    if (data != null) { 
                        IBuffer buffer = data.AsBuffer();

                        //Decrypt message
                        string text = helper.Decrypt(buffer, _password);

                        //Invoke event when message is received 
                        Message msg = helper.Deserialize<Message>(text);

                        if (msg.Token == Convert.ToString(_token))
                        {
                            _ServerHeartbeat = DateTime.UtcNow;
                        }


                    }
                }
            }           
            catch (Exception ex)
            {
                int errCode = ex.HResult;

                switch (errCode)
                {
                    case -2147024873:
                        // Could not decrypt message, so just ignore it.                        
                        break;
                    default:                        
                        throw;                      
                }
                          
            }
                   
        }


        /// <summary>
        /// Read data from the buffer
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private async Task<byte[]> ReceiveData(DataReader reader)
        {
            //Read length of Message
            uint sizeFieldCount = await reader.LoadAsync(sizeof(uint));
            //if disconnect
            if (sizeFieldCount != sizeof(uint))
            {
                // The underlying socket was closed before we were able to read the whole data. 
                return null;
            }

            //Get length of Message from buffer
            uint bufferSize = reader.ReadUInt32();

            //Read Message
            uint dataReceive = await reader.LoadAsync(bufferSize);
            
            //if disconnect
            if (dataReceive != bufferSize) {
                // The underlying socket was closed before we were able to read the whole data. 
                return null;
            }

            var data = new byte[bufferSize];
            
            //Get message from buffer
            reader.ReadBytes(data);

            return data;
        }

        /// <summary>
        /// Close connection
        /// </summary>
        /// <returns></returns>
         public async Task CloseConnection()
            {
                // Stop Hearbeat
                _dispatcherServerHeartbeatTimer.Stop();

                // Request cancellation
                if (_cts != null) { 
                    _cts.Cancel();
                    _cts.Dispose();
                    _cts = null;
                }

                // Close connection.
                if (_listener != null) { 
                    await _listener.CancelIOAsync();
                    _listener.Dispose();
                    _listener = null;
                }

                _activeHost = null;

            }

      

        /// <summary>
        /// Returns a value indicating that a server heartbeat is being received
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
        {
            double secondsSinceLastBeat = (DateTime.UtcNow - _ServerHeartbeat).TotalSeconds;

            if (secondsSinceLastBeat <= HeartbeatInterval)
            {
                // Green
                return true;
            }
            else
            {
                // Red
                return false;
            }
        }
    


 }
}

