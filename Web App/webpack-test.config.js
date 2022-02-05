const path = require('path');

module.exports = {
    entry: './src/app.spec.js',
    output: {
        filename: 'app.spec.js',
        path: path.resolve(__dirname, 'dist'),
    },
    mode: "development"
};
