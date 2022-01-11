# Live Streaming Codec


The described codec is a codec of specialized purpose, primarily for video sources that contain scenes with slow movement of visual elements, such as a game of chess or video surveillance. The transmission depends on the dynamics of the visual scene being transmitted, ie. how many pixels have changed between two frames, the best compression is achieved in scenes with little change, while in dynamic scenes with a lot of changes there is a degradation of compression and poor results. As the transfer is performed directly from the server to the user, without intermediaries, the latency in the transfer is minimal. By using the quantization technique, there is a minimal loss of image quality, which cannot be noticed by the human eye, so the image is obtained in the original format. The transmission uses a variable bitrate, which allows a different amount of data to be required for each frame, and thus in scenes where there is little movement of the elements, it is possible to significantly save throughput.

The transfer server was created using the .NET development environment using the C# language and windows forms to create the user interface. The role of the server is to select the display segment, package and compress data, and send packets to clients. The main elements of compression used are the redundancy between adjacent frames and the coding of repetitive sequences.

The client side uses a web browser as the image display interface, so the web application is built using standard web technologies such as JavaScript language, canvas element for pixel processing and web socket for network communication with the transmission server. The client's role is to decode packets, synchronize with the server, and display the image.

| System | Transport | Source selection | Codec | Clients |
| --- | --- | --- | --- | --- |
| Youtube live (standard use) | RTMP (Real Time Messaging Protocol) | OBS Studio | H.264 | Browser, mobile application, tv |
| The system described in this work | Web socket | Developed using windows libraries | Uses quantization technique, residuals between frames, sequence coding | Browser |


## Code

### Server packages
Package 1 - Synchronization - Base for further compression and synchronization mechanism. It contains an image in PNG format without losing information. 

Package 2 - Compression - Contains the compressed difference of two adjacent frames added to the base package. There is minimal loss of information.  

Package 3 - Errors - Indicates that an error has occurred on the server, for example, the server did not finish processing the previous frame, but started with the new one.  