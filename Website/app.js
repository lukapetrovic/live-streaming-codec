let socket = new WebSocket("ws://172.94.18.86:3000");
let msgNum = 1;

let canvas = document.getElementById('canvas').getContext('2d');
let canvasDOM = document.getElementById('canvas');
let img = new Image();
img.onload = () => {
    canvasDOM.width = img.width;
    canvasDOM.height = img.height;
    canvas.drawImage(img, 0, 0);
};

socket.onopen = (e) => {
    console.log("[open] Connection established");
    console.log("Sending to server");
    socket.send("Connection established");
};

socket.onmessage = (event) => {

    let data = new Response(event.data);

    data.arrayBuffer().then((buffer) => {
        try {

            // 16 Bytes for metadata
            let metadataBuffer = buffer.slice(0, 16);
            let metadataView = new Int32Array(metadataBuffer);

            let byteView = new Uint8Array(buffer);
            if (metadataView[0] == 1) {
                insertFullFrame(byteView.slice(16));
            } else if (metadataView[0] == 2) {
                insertCompressedFrame(byteView.slice(16), metadataView[1], metadataView[2], metadataView[3]);
            } else {
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
    } else {
        // e.g. server process killed or network down
        // event.code is usually 1006 in this case
        console.log('[close] Connection died');
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

function insertCompressedFrame(dataBuffer, numRed, numGreen, numBlue) {

    let imageData = canvas.getImageData(0, 0, img.width, img.height);
    let pixelNum = img.width * img.height;

    // Memory alocation
    let redPixelsBuffer = new Array(pixelNum);
    let greenPixelsBuffer = new Array(pixelNum);
    let bluePixelsBuffer = new Array(pixelNum);


    // Response data memory view
    let dataView = new Uint8Array(dataBuffer);


    // Decode pixels
    let redCounter = 0;
    let greenCounter = 0;
    let blueCounter = 0;

    dataView.map((value, index) => {
        if (index % 2 == 0) {
            if (index < numRed) {
                for (let i = 0; i < value; i++) {
                    redPixelsBuffer[redCounter++] = (dataView[index + 1] - 127) * 2;
                }
            } else if (index < numRed + numGreen) {
                for (let i = 0; i < value; i++) {
                    greenPixelsBuffer[greenCounter++] = (dataView[index + 1] - 127) * 2;
                }
            } else {
                for (let i = 0; i < value; i++) {
                    bluePixelsBuffer[blueCounter++] = (dataView[index + 1] - 127) * 2;
                }
            }
        }
    })

    // Image Data - RGBA
    for (let i = 0; i < pixelNum; i++) {
        let red = imageData.data[(i * 4) + 0] - redPixelsBuffer[i];
        let green = imageData.data[(i * 4) + 1] - greenPixelsBuffer[i];
        let blue = imageData.data[(i * 4) + 2] - bluePixelsBuffer[i];
        imageData.data[(i * 4) + 0] = red;
        imageData.data[(i * 4) + 1] = green;
        imageData.data[(i * 4) + 2] = blue;
    }
    canvas.putImageData(imageData, 0, 0);

}