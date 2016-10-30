using System;
using Windows.Networking;

namespace PurpleTreeSoftware.Network.Common
{
        /// <summary> 
     /// Helper class describing a NetworkAdapter and its associated IP address 
     /// </summary> 
     public class Host
     { 
         public string DisplayString 
         { 
             get; 
             private set; 
         } 
 
 
         public HostName Name
         { 
             get; 
             private set; 
         } 
 
        
         // Constructor
         public Host(HostName pName)
         { 
            if (pName == null) 
             { 
                 throw new ArgumentNullException("Host name is null"); 
             } 
 
            Name = pName;
            DisplayString = pName.DisplayName;
         }
         
     } 
 } 

