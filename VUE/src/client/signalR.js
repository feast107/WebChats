const signalR = require("@microsoft/signalr")
let builder = new signalR.HubConnectionBuilder()
//连接集线器
let hubConnection = builder
    //请求一次后端接口,保证是可通信的状态,即状态码为101
    .withUrl("/Monitor", {
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets,
        accessTokenFactory() {
            return hubConnection.Token;
        }
    })
    .configureLogging(signalR.LogLevel.Error) //日志输出级别,可自行调整
    .build();
hubConnection.Token=null;
//监听到关闭后,每过5秒请求重新连接一次
hubConnection.onclose((error)=>{
    window.$message.warning("连接断开")
   /* let reTry=setInterval(()=>{
        hubConnection.start().then((res)=>{
            clearInterval(reTry);
        })
    },5000)*/
})
let HttpClient = hubConnection.connection._httpClient;
HttpClient.post = ( url ,form) => {
    let request = {...form, url,method:"POST"};
    return new Promise( (resolve, reject) => {
        const xhr = new XMLHttpRequest();
        xhr.open(request.method, request.url, true);
        xhr.withCredentials = true;
        // Explicitly setting the Content-Type header for React Native on Android platform.
        if (request.headers) {
            Object.keys(request.headers)
                .forEach((header) => xhr.setRequestHeader(header, request.headers[header]));
        }
        if (request.responseType) {
            xhr.responseType = request.responseType;
        }
        if (request.abortSignal) {
            request.abortSignal.onabort = () => {
                xhr.abort();
            };
        }
        if (request.timeout) {
            xhr.timeout = request.timeout;
        }
        xhr.onload = () => {
            if (request.abortSignal) {
                request.abortSignal.onabort = null;
            }
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(new signalR.HttpResponse(xhr.status, xhr.statusText, xhr.response || xhr.responseText));
            } else {
                reject(new signalR.HttpError(xhr.statusText, xhr.status));
            }
        };
        xhr.onerror = () => {
            this.logger.log(signalR.LogLevel.Warning, `Error from HTTP request. ${xhr.status}: ${xhr.statusText}`);
            reject(new signalR.HttpError(xhr.statusText, xhr.status));
        };
        xhr.ontimeout = () => {
            this.logger.log(signalR.LogLevel.Warning, `Timeout from HTTP request.`);
            reject(new signalR.TimeoutError());
        };
        xhr.send(request.content || "");
    } );
}

let connStatus = false;


export const SignalR = {
    setToken(Token){builder.Token =  hubConnection.Token = Token},
    removeToken(){ builder.Token = hubConnection.Token = null},
    start(Token){
        if(Token){
            hubConnection.Token = Token;
        }
        const conn = hubConnection.start();
        conn
            .then(()=>{connStatus=true;window.$message.success("连接成功")})
            .catch(res=>{window.$message.error("连接失败")});
        return conn;
    },
    close(){
        if (connStatus) {
            hubConnection.connection.stop()
                .then(() => {
                })
                .catch(res => window.$message.error("暂时无法断开连接"));
        }
    },
    listen(method,callback) {
        hubConnection.on(method, (...args) => {
            callback(...args);
        });
    },
    invoke(method,...args){
        let promise = hubConnection.invoke(method,...args);
        promise.catch(reject => {
            if(reject.toString().includes("unauthorized")){
                console.log("没有授权")
                window.$message.error("没有授权")
            }else{
                window.$message.error(reject.toString())
            }
        });
        return promise;
    },
    send(method,...args){
        return hubConnection.send(method,args);
    },
    stream(method,...args){
        return hubConnection.stream(method,...args);
    },
    get(url){
        if(hubConnection.Token){
            return HttpClient.get(url,{headers:{Authorization:"Bearer  "+hubConnection.Token}})
        }else {
            return HttpClient.get(url)
        }
    },
    post(url,request){
        return HttpClient.post(url,request)
    },
    delete(url) {
        return HttpClient.delete(url);
    },
    JsonRequest() {
        return {
            setContent(args) {
                this.content = JSON.stringify(args)
            },
            headers:  this.getHeaders("Json")
        }
    },
    FormRequest() {
        return {
            setContent(args) {
                this.content = JSON.stringify(args)
            },
            headers: this.getHeaders("Form")
        }
    },
    getHeaders(Type){
      let headers = {};
      if(hubConnection.Token){
          headers.Authorization = "Bearer  "+hubConnection.Token;
      }
      headers.Accept = 'application/json, text/plain, */*';
      switch (Type){
          case 'Json':
              headers['Content-Type'] = 'application/json';
              break;
          case 'Form':
              headers['Content-Type'] = 'multipart/form-data';
      }
      return headers;
    },
    methods: {
        //广播
        Broadcast: 'Broadcast',
        //广播其他连接
        BroadcastOthers: 'BroadcastOthers',
        //组播
        Multicast:'Multicast',
        //其他组播
        MulticastGroup:'MulticastGroup',
        //组内其他连接
        MulticastOthers:'MulticastOthers',
        //单播
        Unicast:'Unicast',
    },
}
