# PurpleTreeSoftware.Network
Network Client and Server for sending messages in UWP applications


At a reference to your project
```
using PurpleTreeSoftware.Network;
```

Then instantiate the client and server objects in your code
```
public static class Provider
    {
        private static readonly Server _server = new Server(12);
        public static Server ServerObject { get { return _server; } }

        private static readonly Client _client = new Client();
        public static Client ClientObject { get { return _client; } }      

    }
```

Then you can start a server listener
```
   Host host = new Host(new HostName("192.168.0.1"));
   String port = 43243
   String passphrase = "abcdefg";
   StreamSocketListener listener = await Provider.ServerObject.StartListener(host, port, passphrase);
```   

Then you can send a message to the server
```
   Message msg = new Message();
   msg.Type = 0;  
   msg.Content = "Test";
   var msgsuccess = await Provider.ClientObject.SendAsync(msg);
```


