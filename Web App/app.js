let socket = new WebSocket(`ws://${ip_address}:3000`);

let msgNum = 1;

let firstFullFrame = false;

let canvas = document.getElementById('canvas').getContext('2d');
let canvasDOM = document.getElementById('canvas');
let img = new Image();
img.onload = () => {
    canvasDOM.width = img.width;
    canvasDOM.height = img.height;
    canvas.drawImage(img, 0, 0);
};

let msgBox = document.getElementById("message-box");

socket.onopen = (e) => {
    console.log("[open] Connection established");
    console.log("Sending to server");

    socket.send("Connection established");
};

socket.onmessage = (event) => {

    msgBox.textContent = "Server synchronization...";

    // Get server response as binary data
    let data = new Response(event.data);
    // Convert binary to array buffer data
    data.arrayBuffer().then((buffer) => {
        try {
            // 16 Bytes for metadata, first 4 bytes - package code
            // Code 0 - Server error
            // Code 1 - Sync package
            // Code 2 - Frame difference package
            let metadataBuffer = buffer.slice(0, 16);
            let metadata = new Int32Array(metadataBuffer);

            // View data as 1 byte elements 
            let byteView = new Uint8Array(buffer);

            // Sort packages
            if (metadata[0] == 1) {
                insertFullFrame(byteView.slice(16));
                firstFullFrame = true;
                msgBox.style = "display: none";
                // Skip if sync package has not arrived yet
            } else if (metadata[0] == 2 && firstFullFrame == true) {
                insertCompressedFrame(byteView.slice(16), metadata[1], metadata[2], metadata[3]);
            } else {
                // If error package, ignore
                return;
            }
        } catch (error) {
            console.log(`[error] ${error}`);
        }
    })
};

socket.onclose = (event) => {
    if (event.wasClean) {
        console.log(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);
        msgBox.textContent = `Connection closed cleanly`;
        msgBox.style = "display: block";
    } else {
        // e.g. server process killed or network down
        // event.code is usually 1006 in this case
        console.log('[close] Connection died');
        msgBox.textContent = "Connection died";
        msgBox.style = "display: block";
    }
};

socket.onerror = (error) => {
    console.log(`[error] ${error.message}`);
};

function insertFullFrame(byteArray) {
    let blob = new Blob([byteArray]);
    let urlCreator = window.URL || window.webkitURL;
    let imageUrl = urlCreator.createObjectURL(blob);
    img.src = imageUrl;
}

function insertCompressedFrame(dataBuffer, numRed, numGreen) {

    const LIMITER = 127;
    const PIXEL_SIZE = 4;
    const EVEN = 2;
    let imageData = canvas.getImageData(0, 0, img.width, img.height);
    let pixelNum = img.width * img.height;

    // Memory alocation
    let redPixelsBuffer = new Array(pixelNum);
    let greenPixelsBuffer = new Array(pixelNum);
    let bluePixelsBuffer = new Array(pixelNum);

    // View data as an array of 1 byte elements
    let dataView = new Uint8Array(dataBuffer);

    // Decode pixels
    let redCounter = 0;
    let greenCounter = 0;
    let blueCounter = 0;

    // Unpack data using reverse run length encoding
    dataView.map((value, index) => {
        if (index % EVEN != 0) {
            return;
        }
        if (index < numRed) {
            for (let i = 0; i < value; i++) {
                redPixelsBuffer[redCounter++] = (dataView[index + 1] - LIMITER) * 2;
            }
        } else if (index < numRed + numGreen) {
            for (let i = 0; i < value; i++) {
                greenPixelsBuffer[greenCounter++] = (dataView[index + 1] - LIMITER) * 2;
            }
        } else {
            for (let i = 0; i < value; i++) {
                bluePixelsBuffer[blueCounter++] = (dataView[index + 1] - LIMITER) * 2;
            }
        }
    })

    // Substract base pixel values from frame residuals
    for (let i = 0; i < pixelNum; i++) {
        let red = imageData.data[(i * PIXEL_SIZE) + 0] - redPixelsBuffer[i];
        let green = imageData.data[(i * PIXEL_SIZE) + 1] - greenPixelsBuffer[i];
        let blue = imageData.data[(i * PIXEL_SIZE) + 2] - bluePixelsBuffer[i];

        imageData.data[(i * PIXEL_SIZE) + 0] = red;
        imageData.data[(i * PIXEL_SIZE) + 1] = green;
        imageData.data[(i * PIXEL_SIZE) + 2] = blue;
    }
    // Put new values on the canvas
    canvas.putImageData(imageData, 0, 0);
}