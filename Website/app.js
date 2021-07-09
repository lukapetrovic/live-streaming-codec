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
    console.log(`[message] Image recieved from the server`);

    if (msgNum === 1) {
        phaseOne(event.data);
    } else {
        phaseTwo(event.data);
        console.log(`[message] Frame difference recieved`);
        console.log(event.data);
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

function phaseOne(image) {
    img.src = "placeholder.png";
    let urlCreator = window.URL || window.webkitURL;
    let imageUrl = urlCreator.createObjectURL(image);
    document.querySelector("#image").src = imageUrl;

    msgNum++;
}

function phaseTwo(difference) {

    let response = new Response(difference);
    response.text().then((text) => {
        let matrix = JSON.parse(text);
        console.log(matrix);
    })

    let imageData = canvas.getImageData(0, 0, 10, 10);

    for (let i = 0; i < imageData.data.length; i += 4) {
        imageData.data[i] = 233;
        imageData.data[i + 1] = 144;
        imageData.data[i + 2] = 144;
        imageData.data[i + 3] = 0.5;
    }

    canvas.putImageData(imageData, 0, 0);

    console.log(imageData);
}