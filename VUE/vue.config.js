const { defineConfig } = require('@vue/cli-service')
const serviceUrl = "https://127.0.0.1:7014/"

module.exports = defineConfig({
  transpileDependencies: true,
  lintOnSave: false,
  publicPath: "./",
  devServer:{
    port:8080,
    open:false,
    proxy: {
      '/Monitor': {
        target: serviceUrl,
        changeOrigin: true,
        ws: true,
      },
      '/Service': {
        target: serviceUrl,
        changeOrigin: true,
        ws: true,
      }
    }
  }



})