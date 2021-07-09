let socket = new WebSocket("ws://localhost:3000");
let msgNum = 1;

let canvas = document.getElementById('canvas').getContext('2d');
let img = new Image();
img.onload = function () {
    canvas.drawImage(img, 0, 0);
};

socket.onopen = (e) => {
    console.log("[open] Connection established");
    console.log("Sending to server");
    socket.send("My name is John");
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

    response.text().then((text) => {
        let pixelsToChange = JSON.parse(text);
        for (let i = 0; i < pixelsToChange.length; i++) {
            let pixelNum = (pixelsToChange[i].col * 4) + (pixelsToChange[i].row * img.width * 4);
            imageData.data[pixelNum] = pixelsToChange[i].red;
            imageData.data[pixelNum + 1] = pixelsToChange[i].green;
            imageData.data[pixelNum + 2] = pixelsToChange[i].blue;
        }
        console.log(imageData.data);
        canvas.putImageData(imageData, 0, 0);
    })
}