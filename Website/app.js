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
            } else {
                insertCompressedFrame(byteView.slice(16), metadataView[1], metadataView[2], metadataView[3]);
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

function insertFullFrame(blob) {
    let urlCreator = window.URL || window.webkitURL;
    let imageUrl = urlCreator.createObjectURL(blob);
    img.src = imageUrl;
}

function insertCompressedFrame(dataBuffer, numRed, numGreen, numBlue) {

    let imageData = canvas.getImageData(0, 0, img.width, img.height);

    // Memory alocation
    let redPixelsBuffer = new ArrayBuffer(img.width * img.height);
    let greenPixelsBuffer = new ArrayBuffer(img.width * img.height);
    let bluePixelsBuffer = new ArrayBuffer(img.width * img.height);

    // Memory modification
    let redPixelsView = new Uint8Array(redPixelsBuffer);
    let greenPixelsView = new Uint8Array(greenPixelsBuffer);
    let bluePixelsView = new Uint8Array(bluePixelsBuffer);

    // Response data memory view
    let dataView = new Uint8Array(dataBuffer);

    // Decode pixels
    for (let i = 0; i < dataView.length; i++) {
        if (i < numRed) {
            decodeArray(dataView, redPixelsView);
        } else if (i < numGreen) {
            decodeArray(dataView, greenPixelsView);
        } else {
            decodeArray(dataView, bluePixelsView);
        }
    }

    // Image Data - RGBA
    /* for (let i = 0; i < Uint8View.length; i++) {
        imageData.data[i * 4] += Uint8View[i];
    } */
    canvas.putImageData(imageData, 0, 0);

}

function decodeArray(encodedArray, decodedArray) {
    let counter = 0;
    encodedArray.map((value, index) => {
        if (index % 2 == 0) {
            for (let i = 0; i < value; i++) {
                decodedArray[counter++] = (view[index + 1] - 127) * 2;
            }
        }
    })
}