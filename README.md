# Telemetry-Client
This C# Client accepts real-time data from a micro-controller board.

The micro-controller board outputs 80+ sensor readings in one group of data. In test mode, the board is capable of outputting 1,200 groups per second. In production mode, the board produces less than 50 groups of data per second.

The current hardware / sofware configuration of the C# client, can process 60 groups of data. Performance was measured on worst case scenario hardware platforms to ensure high levels of performance in production mode.

The salient features of the program are:

1. UDP communications to/from board

2. Actively or passively acquire board data

3. The program is much less performant than the Java counterpart due to the lack
   of muli-threading in the program.  The program will be upgrade in the near
   term to increase performance.

4. Since this was originally written as a 'throw away', there are various
   structural issues to be addressed for a 'near' production environment.  For instance,
   objects for each data type, splitting out code from large objects, etc.  

5. Visual Studio 2015

