const path = require('path');
const webpack = require('webpack');

module.exports = {
  entry: './src/index.js',
  output: {
    filename: 'near-wallet.js',
    path: path.resolve(__dirname, '../wwwroot/js'),
    library: 'nearWallet',
    libraryTarget: 'var'
  },
  mode: 'production',
  resolve: {
    fallback: {
      "buffer": require.resolve("buffer/"),
      "https": require.resolve("https-browserify"),
      "http": require.resolve("stream-http"),
      "crypto": require.resolve("crypto-browserify"),
      "stream": require.resolve("stream-browserify"),
      "process": require.resolve("process/browser"),
      "util": require.resolve("util/"),
      "url": require.resolve("url/"),
      "vm": require.resolve("vm-browserify")
    },
    alias: {
      'near-api-js/lib/providers': path.resolve(__dirname, 'node_modules/near-api-js/lib/providers/index.js')
    }
  },
  plugins: [
    new webpack.ProvidePlugin({
      process: 'process/browser',
      Buffer: ['buffer', 'Buffer'],
    }),
  ],
};
