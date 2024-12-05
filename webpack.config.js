const path = require('path');

module.exports = {
    entry: './deploy/xmldom/lib/index.js',
    output: {
        path: path.resolve(__dirname, 'deploy', 'xmldom'),
        filename: 'xmldom.js',
    },
};