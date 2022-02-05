/*
 * ATTENTION: The "eval" devtool has been used (maybe by default in mode: "development").
 * This devtool is neither made for production nor for readable output files.
 * It uses "eval()" calls to create a separate source file in the browser devtools.
 * If you are trying to read the output file, select a different devtool (https://webpack.js.org/configuration/devtool/)
 * or disable the default devtool with "devtool: false".
 * If you are looking for production-ready output files, see mode: "production" (https://webpack.js.org/configuration/mode/).
 */
/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./src/app.js":
/*!********************!*\
  !*** ./src/app.js ***!
  \********************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

eval("__webpack_require__.r(__webpack_exports__);\n/* harmony export */ __webpack_require__.d(__webpack_exports__, {\n/* harmony export */   \"Client\": () => (/* binding */ Client)\n/* harmony export */ });\nclass Client {\r\n\r\n    socket = new WebSocket(`ws://localhost:3000`);\r\n    firstFullFrame = false;\r\n    img = new Image();\r\n\r\n    canvas = document.getElementById('canvas').getContext('2d');\r\n    canvasDOM = document.getElementById('canvas');\r\n    msgBox = document.getElementById(\"message-box\");\r\n\r\n    constructor() {\r\n        this.img.onload = () => {\r\n            this.canvasDOM.width = this.img.width;\r\n            this.canvasDOM.height = this.img.height;\r\n            this.canvas.drawImage(this.img, 0, 0);\r\n        };\r\n\r\n        this.socket.onopen = (e) => {\r\n            console.log(\"[open] Connection established\");\r\n            console.log(\"Sending to server\");\r\n\r\n            this.socket.send(\"Connection established\");\r\n        };\r\n\r\n        this.socket.onerror = (error) => {\r\n            console.log(`[error] ${error.message}`);\r\n        };\r\n\r\n        this.socket.onmessage = (event) => {\r\n            this.handleServerResponse(event);\r\n        };\r\n\r\n        this.socket.onclose = (event) => {\r\n            if (event.wasClean) {\r\n                console.log(`[close] Connection closed cleanly, code=${event.code} reason=${event.reason}`);\r\n                this.msgBox.textContent = `Connection closed cleanly`;\r\n                this.msgBox.style = \"display: block\";\r\n            } else {\r\n                // e.g. server process killed or network down\r\n                // event.code is usually 1006 in this case\r\n                console.log('[close] Connection died');\r\n                this.msgBox.textContent = \"Connection died\";\r\n                this.msgBox.style = \"display: block\";\r\n            }\r\n        };\r\n    }\r\n\r\n    handleServerResponse(event) {\r\n        this.msgBox.textContent = \"Server synchronization...\";\r\n\r\n        // Get server response as binary data\r\n        let data = new Response(event.data);\r\n        // Convert binary to array buffer data\r\n        data.arrayBuffer().then((buffer) => {\r\n            try {\r\n                // 16 Bytes for metadata, first 4 bytes - package code\r\n                // Code 0 - Server error\r\n                // Code 1 - Sync package\r\n                // Code 2 - Frame difference package\r\n                let metadataBuffer = buffer.slice(0, 16);\r\n                let metadata = new Int32Array(metadataBuffer);\r\n\r\n                // View data as 1 byte elements \r\n                let byteView = new Uint8Array(buffer);\r\n\r\n                // Sort packages\r\n                if (metadata[0] == 1) {\r\n                    this.insertFullFrame(byteView.slice(16));\r\n                    this.firstFullFrame = true;\r\n                    this.msgBox.style = \"display: none\";\r\n                    // Skip if sync package has not arrived yet\r\n                } else if (metadata[0] == 2 && this.FullFrame == true) {\r\n                    this.insertCompressedFrame(byteView.slice(16), metadata[1], metadata[2], metadata[3]);\r\n                } else {\r\n                    // If error package, ignore\r\n                    return;\r\n                }\r\n            } catch (error) {\r\n                console.log(`[error] ${error}`);\r\n            }\r\n        })\r\n    }\r\n\r\n    insertFullFrame(byteArray) {\r\n        let blob = new Blob([byteArray]);\r\n        let urlCreator = window.URL || window.webkitURL;\r\n        let imageUrl = urlCreator.createObjectURL(blob);\r\n        this.img.src = imageUrl;\r\n    }\r\n\r\n    insertCompressedFrame(dataBuffer, numRed, numGreen) {\r\n\r\n        const LIMITER = 127;\r\n        const PIXEL_SIZE = 4;\r\n        const EVEN = 2;\r\n        let imageData = canvas.getImageData(0, 0, img.width, img.height);\r\n        let pixelNum = img.width * img.height;\r\n\r\n        // Memory alocation\r\n        let redPixelsBuffer = new Array(pixelNum);\r\n        let greenPixelsBuffer = new Array(pixelNum);\r\n        let bluePixelsBuffer = new Array(pixelNum);\r\n\r\n        // View data as an array of 1 byte elements\r\n        let dataView = new Uint8Array(dataBuffer);\r\n\r\n        // Decode pixels\r\n        let redCounter = 0;\r\n        let greenCounter = 0;\r\n        let blueCounter = 0;\r\n\r\n        // Unpack data using reverse run length encoding\r\n        dataView.map((value, index) => {\r\n            if (index % EVEN != 0) {\r\n                return;\r\n            }\r\n            if (index < numRed) {\r\n                for (let i = 0; i < value; i++) {\r\n                    redPixelsBuffer[redCounter++] = (dataView[index + 1] - LIMITER) * 2;\r\n                }\r\n            } else if (index < numRed + numGreen) {\r\n                for (let i = 0; i < value; i++) {\r\n                    greenPixelsBuffer[greenCounter++] = (dataView[index + 1] - LIMITER) * 2;\r\n                }\r\n            } else {\r\n                for (let i = 0; i < value; i++) {\r\n                    bluePixelsBuffer[blueCounter++] = (dataView[index + 1] - LIMITER) * 2;\r\n                }\r\n            }\r\n        })\r\n\r\n        // Substract base pixel values from frame residuals\r\n        for (let i = 0; i < pixelNum; i++) {\r\n            let red = imageData.data[(i * PIXEL_SIZE) + 0] - redPixelsBuffer[i];\r\n            let green = imageData.data[(i * PIXEL_SIZE) + 1] - greenPixelsBuffer[i];\r\n            let blue = imageData.data[(i * PIXEL_SIZE) + 2] - bluePixelsBuffer[i];\r\n\r\n            imageData.data[(i * PIXEL_SIZE) + 0] = red;\r\n            imageData.data[(i * PIXEL_SIZE) + 1] = green;\r\n            imageData.data[(i * PIXEL_SIZE) + 2] = blue;\r\n        }\r\n        // Put new values on the canvas\r\n        this.canvas.putImageData(imageData, 0, 0);\r\n    }\r\n}\r\n\r\n\n\n//# sourceURL=webpack://web-app/./src/app.js?");

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The require scope
/******/ 	var __webpack_require__ = {};
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	
/******/ 	// startup
/******/ 	// Load entry module and return exports
/******/ 	// This entry module can't be inlined because the eval devtool is used.
/******/ 	var __webpack_exports__ = {};
/******/ 	__webpack_modules__["./src/app.js"](0, __webpack_exports__, __webpack_require__);
/******/ 	
/******/ })()
;