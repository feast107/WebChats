import axios from "axios";
/*创建axios 实例*/
const requestInstance = axios.create({
    /*api的baseURL*/
    baseURL:
        process.env.NODE_ENV === "production"
            ? process.env.VUE_APP_REQUEST_URL
            : "/",
    /*请求超时时间*/
    timeout: 60000
});
/*request 拦截器*/
requestInstance.interceptors.request.use(
    config => {
        return config;
    },
    error => {
        /*处理请求出错*/
        return Promise.reject(error);
    }
);
/*response拦截器*/
requestInstance.interceptors.response.use(res =>
    {
        return res;
    },
    error => {
        /*处理response出错逻辑*/
        console.log(error.response.status)
        return Promise.reject(error);
    }
);
export default requestInstance;
