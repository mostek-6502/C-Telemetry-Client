# Telemetry-Client
This Visual Studio 2015 C# /.Net Telemetry client is used for receiving controller board output 
via UDP.  This also sends commands to the controller board in order to dynamically adjust board 
settings.

This is paired on GitHub with the small section of the Controller-C-Code listing.  
This is only useful for the specific controller output; however, there may be meaningful sections 
of code in this.

If the reader decides to download / compile / build / run the code, the following is a small
section of logging to help you understand what is happening.  Please create a "C\Logs\" directory on 
your local machine.  The client can be run from anywhere on the machine.  To my knowledge, this does 
not install anything and uninstalling is a simple delete.

The following is a small section of telemetry logging.
  
  SolarData() - Ready!
  
	Initialize_Socket Initializing Socket()
  
	UDP.Initialize()  Get Local Machine IP
  
	UDP.Initialize()  Local IP: 192.168.1.7
  
	UDP.Initialize()  UDP.Set_Socket()
  
	UDP.Initialize()  UDP.Set_IPs()
  
	UDP.Initialize()  UDP.Bind()
  
	UDP.Initialize()  UDP Initialization Complete!
  
	Initialize_Socket Socket() Initialized!
  
	Initialize_Socket - UDP.ReceiveFrom() Setup
  
	Send_SYNCH_1_To_Subnet - Sending [SYNCH_1 ] To: 192.168.1.76
  
	PIM - Data ->SYNCH_2 <-  From: 192.168.1.9:55056 <---  This is the IP / Port of the controller board.
  
	Connect_With_Tiva() - Waiting for Tiva Synch [SYNCH_2 ] Data.
  
	Button_Retrieve_Data_Click - Connected!
  
	PIM Received [SYNCH_2 ] From Tiva.  Sending [SYNCH_3 ]
  
	Send_Data_To_EndPoint() Data Successfully Sent: SYNCH_3
    
	PIM - Data ->EEPROM  ,20170808,2,120000000,9,0,0,25,0,1,1,0,55056,192.168.1.50    ,1000,300,40<-  From: 192.168.1.9:55056
	
  PIM - Data >TEMPS,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0<-  From: 192.168.1.9:55056

PIM - Data ->ROMCODES,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00,0,00-00-00-00-00-00-00-00<-  From: 192.168.1.9:55056

PIM - Data ->PUMPS   ,0,0,6,0,6,0,6,0,6<-  From: 192.168.1.9:55056

PIM - Data ->DISH    ,0,0,0,0,0,9,0,0,0,0,0,10<-  From: 192.168.1.9:55056

PIM - Data ->SUPPORT ,0,0,1,0,0,0,0<-  From: 192.168.1.9:55056

PIM - Data ->RUNTIME ,86,0,1065690,0,1647715,2,0,0,1,1<-  From: 192.168.1.9:55056

PIM - Data ->COMREPLY,?  (chat window must be open)
             COMREPLY,Something1,Something2,Something3,,,Something4

The above data was created without full board functionality due to a 'magic smoke' release.
Also, all of the data is ASCII / UTF-8.   If the packet if formatted correctly, the client 
will show the data.

Running the client.

	1. Bring up this telemetry client.  
  
	2. Note the default Port that the client is using.
  
	3. Open up a generic UDP Client and configure it to send data to the telemetry client IP / port
  
	4. Cut and Paste a command from above such as:
  
  	[DISH    ,0,0,0,0,0,9,0,0,0,0,0,10] and see where it populates data on the screen.
    
    [COMREPLY,Something1,Something2,Something3,,,Something4]  Requires the 'update settings' to be open.
    
		Do not type the [ ] brackes, just everything between them.
