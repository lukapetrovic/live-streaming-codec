# Live Streaming Codec


The described codec is a codec of specialized purpose, primarily for video sources that contain scenes with slow movement of visual elements, such as a game of chess or video surveillance. The transmission depends on the dynamics of the visual scene being transmitted, ie. how many pixels have changed between two frames, the best compression is achieved in scenes with little change, while in dynamic scenes with a lot of changes there is a degradation of compression and poor results. As the transfer is performed directly from the server to the user, without intermediaries, the latency in the transfer is minimal. By using the quantization technique, there is a minimal loss of image quality, which cannot be noticed by the human eye, so the image is obtained in the original format. The transmission uses a variable bitrate, which allows a different amount of data to be required for each frame, and thus in scenes where there is little movement of the elements, it is possible to significantly save throughput.

The transfer server was created using the .NET development environment using the C# language and windows forms to create the user interface. The role of the server is to select the display segment, package and compress data, and send packets to clients. The main elements of compression used are the redundancy between adjacent frames and the coding of repetitive sequences.

The client side uses a web browser as the image display interface, so the web application is built using standard web technologies such as JavaScript language, canvas element for pixel processing and web socket for network communication with the transmission server. The client's role is to decode packets, synchronize with the server, and display the image.

| System | Transport | Source selection | Codec | Clients |
| --- | --- | --- | --- | --- |
| Youtube live (standard use) | RTMP (Real Time Messaging Protocol) | OBS Studio | H.264 | Browser, mobile application, tv |
| The system described in this work | Web socket | Developed using windows libraries | Uses quantization technique, residuals between frames, sequence coding | Browser |


## System

### Server packages

The server produces three types of packets that it passes on to clients. One package contains a PNG image that can be immediately displayed on the client side and another type of package that is compressed and requires decompression on the client side to reconstruct the image. Packet 1 is sent every six sending cycles and serves as a pixel base for compression and a mechanism for correcting errors caused by compression and degradation of pixel values. The second packet type uses the redundancy of two adjacent frames to conserve transmission space.

| Package name | Package code | Function |
| --- | --- | --- |
| Package 1 | Synchronization | Base for further compression and synchronization mechanism. It contains an image in PNG format without losing information. |
| Package 2 | Compression | Contains the compressed difference of two adjacent frames added to the base package. There is minimal loss of information.  |
| Package 3 | Errors | Indicates that an error has occurred on the server, for example, the server did not finish processing the previous frame, but started with the new one. |

Data compression is performed collectively, ie. compression occurs only once per cycle and all connected devices receive this package at the same time, in contrast to the compression performed for each device separately. Each packet is accompanied by metadata that gives the client information about which type of packet it is, and in the case of the second type of packet, additional information about packet decoding. In order to perform compression, it is necessary to always have the previous and current frames in memory, so that in the compression phase, only data that was not seen in the previous frame are sent, and not the entire current frame.

The compression process used is based on the observation that in a video where there are small visual changes in the scene over time, the base image can be sent, and in subsequent frames only additional information on how that image has changed, in order to save throughput. Packet type 1 sent to clients contains the original lossless image, packed in PNG format that achieves good compression, where the client gets the original pixels without changes, which is important in the following steps to reconstruct the next display frames with the next compressed packets from the server . In addition to the PNG image, the meta data is added that it is a type of packet 1, so that the client knows how to process the data.

## Compression

As the focus of optimization is on video transmission where most of the image will be static, it has been noticed that most pixels will not change the value between two adjacent frames. In order not to send each frame again, after the first base frame, only the difference between the base and the next frame is sent in order to reduce the network flow. The client will reverse this difference because it has a copy of the base frame in memory. If two pixels at the same position in adjacent frames have the same value, e.g. 200, their difference will be 0, which can be further used for compression. As the run length encoding algorithm compression reduces sequences of the same values, the more zeros it has, the better the compression will be.

![image](https://user-images.githubusercontent.com/3235618/151978149-9926ca95-7fb5-4050-ab1e-334d89f5a4ed.png)

The range of the pixel difference can be from -255 to +255, which would require 2 bytes, which would drastically increase the size. Therefore, it is necessary to limit the values in order the range to be between 0-255 and require only 1 byte. Switching from one set of values to a limited set is done according to the following formula:

Rest = ((Pixel_old - Pixel_new) / 2) + 127

The pixel difference is divided by the number 2 and added to 127 to limit the range to 1 byte. This technique degrades pixel value information over time. That is why a mechanism has been introduced where the base frame will be sent every 6 frames and thus remove all the degradation that has occurred. The client will get the original pixel values in the reverse process.

Each pixel at the same position of 2 adjacent frames is compared. The red, green, and blue components of each pixel are stored in separate arrays so that compression is performed on the basis of each component separately, because the colors are of similar values between adjacent frames.

### Run length encoding

Run length encoding is a type of compression algorithm where no information is lost. It was chosen as a compression algorithm due to limited capabilities on the part of the web client, which, using JavaScript as a language, has poor support for working with data smaller than one byte. If there is a string made up of 100 elements of value 5, we can represent it as (100, 5). One hundred values of number 5 and thus out of the original 100 bytes required for storage, only 2 bytes will be needed.

![image](https://user-images.githubusercontent.com/3235618/151978481-b208142e-5f7c-4a3a-b16f-3fba9d80e4ca.png)

## Results

The following is a comparison of the codec described in this paper and the H.264 codec most commonly used for real-time video transmission. The transmission was tested on a Windows computer with an AMD Ryzen 5 2500U processor and 8 GB of RAM.

Internet bandwidth required for each connection, where a game of chess is transmitted in which changes occur on the board with figures, with a timer where the remaining time changes and the list of previous moves, screen resolution 1280x720, averages 240 kb/s, or for 10 connections 2.3 mb/s. As compression is performed on a single core, in order to achieve a higher compression speed, the highest possible performance of a single processor core is required. To avoid frequent movements, 1 frame per second is used to save bandwidth.

The H.264 codec is used for a wide range of formats, from low-latency real-time Internet transmission to high-quality television and cinema. As the codec is of the general type, the compression will not primarily depend on the type of scene, but the results will be similar for all types of scenes, but the quality can be further improved by investing additional processing power. The default settings for this real-time codec are the use of 30 frames per second and constant bitrate.

The following table shows the results of the codec described in this work and the H.264 codec which is among the current leaders in video compression. The codec described in the paper achieves better compression than H.264 on scenes with very little movement in the image, while the advantage in favor of H.264 is increasing with the amount of changes in the scene.

| Scene type | Resolution | Codec described | H.264 |
| --- | --- | --- | --- |
| Chess game | 1280x720 | 160 - 960 kb/s | 3600 kb/s |
| Surveillance camera | 1280x720 | 160 – 8192 kb/s | 3600 kb/s |
| Music video | 1280x720 | 16384 kb/s | 3600 kb/s |

## Installation

The server requires .NET Desktop Runtime 5, a 32-bit library that allows the development of modern windows applications. Client launch is possible via any modern browser (Chrome, Firefox, Opera).

Before starting the server, it is necessary to open port 3000, where the server listens for incoming client messages. The Web client from the phone or computer will achieve Web socket communication targeting port 3000.

In order for a web application to be publicly available on the Internet, the contents of the Web App folder must be placed on a web server (eg Apache HTTP server) that is visible outside the local network. Then the ip address of the transfer server is set, by changing the address in the file "ip_address.js" so that the web application can communicate with it. For local testing, it is possible to install XAMPP local web server and put the web application files in the required folder.
