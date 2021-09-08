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
    if (msgNum == 1) {
        console.log(`[message] Image recieved from the server`);
        phaseOne(event.data);
        msgNum++;
    } else {
        console.log(`[message] Frame difference recieved`);
        phaseTwo(event.data);
        msgNum++;
    }
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

function phaseOne(blob) {
    let urlCreator = window.URL || window.webkitURL;
    let imageUrl = urlCreator.createObjectURL(blob);
    img.src = imageUrl;
}

function phaseTwo(difference) {

    let imageData = canvas.getImageData(0, 0, img.width, img.height);
    let response = new Response(difference);

    response.arrayBuffer().then((buffer) => {
        try {
            let view = new Int32Array(buffer);
            for (let i = 0; i < view.length; i = i + 5) {
                let pixelNum = (view[i + 1] * 4) + (view[i] * img.width * 4);
                imageData.data[pixelNum] = view[i + 2];
                imageData.data[pixelNum + 1] = view[i + 3];
                imageData.data[pixelNum + 2] = view[i + 4];
            }
            canvas.putImageData(imageData, 0, 0);

        } catch (error) {
            console.log(error);
        }
    })
}