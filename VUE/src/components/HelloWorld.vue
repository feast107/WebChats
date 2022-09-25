<template>
  <div class="hello">
    <h1>{{ msg }}</h1>
    <el-input v-model="Msg"></el-input>
    <el-button @click="Send">
      发送
    </el-button>
    <el-button @click="Authorize">
      登录
    </el-button>
    <el-button @click="SignOut">
      登出
    </el-button>
    <el-button @click="Stream">
     流
    </el-button>
    <br>
    <el-input v-model="Transport.handler"></el-input>
    <el-button @click="Invoke">
      调用
    </el-button>
  </div>
</template>

<script>
import {SignalR} from "@/client/signalR";
import {signIn,signOut,current} from "@/client/Http"
export default {
  name: 'HelloWorld',
  props: {
    msg: String,
  },
  beforeCreate() {
    let head = document.querySelector('head')
    let script = document.createElement('script')
    script.src = "/signalr.js"
    head.appendChild(script)
    let form = SignalR.JsonRequest();
    form.setContent({UserName:'user',Password:'123',Role:'?'});
    SignalR.post('/Service/Access/SignIn',form).then(res=>console.log(res)).catch(reason => {console.log(reason)});

    SignalR.start().then(()=>{this.$message.success("成功连接")});

    console.log(SignalR)
  },
  created() {
    this.InitSignalR(SignalR);
  },
  data(){
    return{
      SignalR:SignalR,
      Msg:'',
      Streams:['data','data'],
      Transport: {
        handler:'',
        object:{object:'?'}
      },
    }
  },
  methods:{
    InitSignalR(SignalR){
      SignalR.listen("Reply",(msg)=>{console.log(JSON.parse(msg)); this.$message.info(msg)});
      SignalR.listen(SignalR.methods.Broadcast,(handler,object)=>{console.log(object); this.$message.info(handler+' '+JSON.stringify(object))});
      SignalR.listen(SignalR.methods.Multicast,(handler,object)=>{console.log(object); this.$message.info(handler+' '+JSON.stringify(object))});
      SignalR.listen(SignalR.methods.Unicast,(handler,object)=>{console.log(object); this.$message.info(handler+' '+JSON.stringify(object))});
      },
    Invoke(){
      SignalR.invoke(SignalR.methods.Broadcast,this.Transport.handler,this.Transport.object);
    },
    Send(){
      if(this.Msg.length===0){
        return this.$message.info("请输入一些内容");
      }
      SignalR.invoke("Reply",this.Msg);
    },
    Authorize(){
      let form = SignalR.JsonRequest();
      form.setContent({UserName:'user',Password:'123',Role:'?'});
      SignalR.post('/Service/Access/SignIn',form).then(res=>console.log(res)).catch(reason => {console.log(reason)});
    },
    SignOut(){
      signOut().then(res=>console.log(res));
    },
    Stream(){
      this.SignalR.stream("Count",null)
          .subscribe({
            next: (item) => {
              this.$message.info(item);
            },
            complete: () => {
              this.$message.success("传输结束");
            },
            error: (err) => {
              this.$message.error(err.toString());
              console.log(err)
            },
          });
    }
  },
}
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
h3 {
  margin: 40px 0 0;
}
ul {
  list-style-type: none;
  padding: 0;
}
li {
  display: inline-block;
  margin: 0 10px;
}
a {
  color: #42b983;
}
</style>
