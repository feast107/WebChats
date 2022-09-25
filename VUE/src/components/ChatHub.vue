<template>
  <el-container id="Main">
    <el-header id="Top" style="padding: 10px">
      <el-card style="padding: 0;margin: 10px;height: 100%">
        <el-row>
          <div v-if="My" style="width: 100%">
            <el-row >
              <el-col :span="2">
                <el-button @click="ShowGroup">创建组</el-button>
              </el-col>
              <el-col :span="20">
                欢迎：{{My.userName}}
              </el-col>
              <el-col :span="2">
                <el-button @click="Logout">登出</el-button>
              </el-col>
            </el-row>
          </div>
          <div v-if="!My">
            <el-button @click="Form.Visible=true">登录</el-button>
          </div>
        </el-row>
      </el-card  >
    </el-header>
    <el-container  v-if="My" id="Bottom" style="padding: 10px">
      <el-aside id="Left" style="padding: 10px">
        <el-menu>
          <el-sub-menu index="1">
            <template #title>
              <el-badge :hidden="GroupUnReads===0" :is-dot="true">
                组
              </el-badge>
            </template>
            <el-menu-item-group>
              <el-menu-item @click="this.Chat(G)" :index="G.groupId" :key="G" v-for="G in Groups">
                <template #title>
                  <el-badge :hidden="G.unReads===0" :value="G.unReads">
                    <span>{{G.groupName}}</span>
                  </el-badge>
                </template>
              </el-menu-item>
            </el-menu-item-group>
          </el-sub-menu>

          <el-sub-menu index="2">
            <template #title>
              <el-badge :hidden="AllUnReads===0" :value="AllUnReads">
                <span>成员</span>
              </el-badge>
            </template>
            <el-menu-item-group>
              <el-menu-item @click="this.Chat(C)" :index="C.userId" :key="C" v-for="C in Clients">
                <template #title>
                  <el-badge :hidden="C.unReads===0" :value="C.unReads">
                    <span>{{C.userName}}</span>
                  </el-badge>
                </template>
              </el-menu-item>
            </el-menu-item-group>
          </el-sub-menu>
        </el-menu>
      </el-aside>
      <el-main id="Right">
        <el-card
            v-loading="this.CurrentClient.status===0"
            element-loading-text="对方已离线"
            v-if="this.CurrentClient"
            class="box-card"
            style="height:97%">
          <el-row style="height:82%">
            <!--消息框-->
            <el-container style="height: 100%;width: 100%">
              <el-scrollbar  ref="ScrollRef" style="height: 100%;width: 100%" updated>
                <div id="ElScroll" ref="ScrollView" style="height: 100%;width: 100%" >
                  <el-row
                      style="width: 100%;margin-top: 10px"
                      :justify="this.Justify(M.client.userId)"
                      :key="M"
                      v-for="M in this.CurrentClient.Messages">
                    <div style="width: min-content">
                      <el-row :justify="this.Justify(M.client.userId)">
                        {{M.client.userName}}
                      </el-row>
                      <el-row>
                        <el-card
                            style="width:min-content; height: min-content;">
                              <img
                                  @click="this.Picture.MainSrc=M.message.content;this.Picture.MainPreview=true"
                                  v-if="M.message.type==='picture'"
                                  :src="M.message.content"
                                  style="max-width: 300px"
                              />
                          <el-popover
                              style="width: min-content"
                              trigger="contextmenu">
                            <el-button style="width: 100%">复制</el-button>
                            <template #reference>
                              <span
                                  v-if="M.message.type==='chars'"
                                  style="max-width: 80% ;white-space:nowrap">
                                  {{M.message.content}}
                                </span>
                            </template>
                          </el-popover>

                        </el-card>
                      </el-row>
                    </div>
                  </el-row>
                </div>
              </el-scrollbar>
            </el-container>
          </el-row>
          <el-row  style="margin-top: 10px; height: 5%">
            <el-col :span="22">
              <el-input
                  @keyup.enter="Send"
                  clearable
                  v-model="this.CurrentClient.SendingMessage"></el-input>
            </el-col>
            <el-col :span="2">
              <el-popover
                  placement="top-start"
                  trigger="contextmenu"
              >
                <el-upload
                    action="#"
                    list-type="picture-card"
                    :limit="1"
                    :file-list="this.CurrentClient.SendingPicture"
                    :on-exceed="ExceedFile"
                    :onchange="UpLoadFilter"
                    :auto-upload="false">
                  <el-icon><Plus /></el-icon>
                  <template #file="{ file }">
                    <div>
                      <img class="el-upload-list__item-thumbnail"
                           :src="file.url"
                           alt="" />
                      <span class="el-upload-list__item-actions">
                        <span
                            class="el-upload-list__item-preview"
                            @click="()=>{
                              Picture.Preview=true
                            }"
                        >
                          <el-icon><zoom-in /></el-icon>
                        </span>
                        <span
                            @click="this.CurrentClient.SendingPicture.pop()"
                        >
                          <el-icon><Delete /></el-icon>
                        </span>
                      </span>
                    </div>
                  </template>
                  <img v-if="this.CurrentClient.SendingPicture.length>0" style="display:none" id="SendingPicture" w-full :src="this.CurrentClient.SendingPicture[0].url" alt="Preview Image" />
                </el-upload>
                <template #reference>
                  <el-button @click="Send">
                    发送
                  </el-button>
                </template>

              </el-popover>
            </el-col>
          </el-row>
        </el-card>
      </el-main>
    </el-container  >
    <el-skeleton  v-if="!My" style="margin: 10px" :rows="10" animated />
  </el-container>
  <div id="Outer">
    <el-dialog v-model="Form.Visible">
      <el-form v-model="Form.LoginForm">
        <el-form-item label="用户名：">
          <el-input v-model="Form.LoginForm.UserName"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="this.Login">登录</el-button>
          <el-button @click="Form.Visible=false">取消</el-button>
        </el-form-item>
      </el-form>
    </el-dialog>

    <el-dialog v-model="Form.G_Visible">
      <el-form v-model="Form.GroupForm">
        <el-form-item label="组名：">
          <el-input v-model="Form.GroupForm.GroupName"></el-input>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="this.CreateGroup">创建</el-button>
          <el-button @click="Form.G_Visible=false">取消</el-button>
        </el-form-item>
      </el-form>
    </el-dialog>

    <el-dialog v-model="Picture.Preview">
      <img w-full :src="this.CurrentClient.SendingPicture[0]?.url" alt="Preview Image" />
    </el-dialog>
    <el-dialog v-model="Picture.MainPreview">
      <img w-full :src="Picture.MainSrc" alt="Preview Image" />
    </el-dialog>
  </div>
</template>

<script>
import {SignalR} from "@/client/signalR";
export default {
  inject:['reload'],
  name: "ChatHub",
  updated() {
    console.log("update")
  },
  beforeCreate() {
    let Token = this.$storage.get("Token");
    if(Token){
      SignalR.setToken(Token);
      SignalR.get('/Service/Access/Current').then(res=> {
        console.log(res)
        if (res.statusCode === 200) {
          this.InitSignalR();
          SignalR.start(Token)
              .then(() => {
                SignalR.invoke("Init")
              });
        } else {
          this.$message.warning("请先登录")
          SignalR.removeToken();
          this.$storage.remove("Token")
        }
      })
    }else{
      this.$message.warning("请先登录");
    }


  },
  data(){
    return {
      Groups:[],
      Clients:[],
      My:null,
      CurrentClient:null,
      AllUnReads: 0,
      GroupUnReads:0,
      Form:{
        LoginForm:{UserName:'',Password:'123456',Role:'?'},
        Visible:false,
        GroupForm:{GroupName:''},
        G_Visible:false,
      },
      Picture:{
        Preview:false,
        file:[],
        MainPreview:false,
        MainSrc:null,
      },
      Event:{o:{},p:{},s:{},v:''}
    }
  },
  mounted() {
  },
  methods:{
    DownToLatest(){
      this.$nextTick(
          ()=>setTimeout(
              ()=>{
                this.$nextTick(function(){
                  this.$refs.ScrollRef.setScrollTop(document.getElementById("ElScroll").parentNode.getBoundingClientRect().height);
                });
              },50
          )
      );
    },
    Justify(userId){
      return userId!==this.My.userId?'start':'end'
    },
    Login() {
      let form = SignalR.JsonRequest();
      form.setContent(this.Form.LoginForm);
      SignalR.post('/Service/Access/SignIn', form).then(res =>
      {
        if(res.statusCode === 200) {
          this.$storage.set("Token", res.content)
        }
        this.reload();
      }).catch(reason => {
        console.log(reason)
      });

    },
    Logout() {
      this.$storage.remove("Token")
      SignalR.removeToken();
      SignalR.close();
      this.reload();
    },
    CreateGroup(){
      SignalR.invoke("CreateGroup",this.Form.GroupForm.GroupName);
      this.Form.G_Visible=false;
    },
    ShowGroup() {
      if(this.Groups.length>0) {
        debugger
        let s = this.Groups.find(x=>(x.creator && x.creator.userId === this.My.userId));
        if(s){
          this.$message.warning('您正管理着一个组');
          return ;
        }
      }
      this.Form.G_Visible = true;
    },
    InitSignalR(){
      SignalR.listen("System",(handler,args)=>{
        args = JSON.parse(args);
        console.log(args)
        switch (handler) {
          case "Init":
            this.My = args;
            break;
          case "Groups":
            this.Groups = args;
            this.Groups.forEach(x=>{this.InitNewClient(x);})
            break;
          case "Clients":
            this.Clients = args;
            this.Clients.forEach(x=>{this.InitNewClient(x);})
            break;
          case "Alert":
            this.$message.warning(args);
            break;
        }
      });
      SignalR.listen("Entire",(handler,args)=>{
        args = JSON.parse(args);
        console.log(args)
        switch (handler) {
          case "OnLine":
            this.InitNewClient(args);
            this.ClientOnLine(args);
            break;
          case "OffLine":
          {
            /*this.Clients.forEach(function(item, index, arr) {
              if(item.userId === args.userId ) {
                arr.splice(index, 1);
              }
            });*/
            let client = this.Clients.find(x => x.userId === args.userId);
            client.status = 0;
            break;
          }
          case "Clients":
            this.Clients = args;
            break;
          case "GroupCreate":
            this.InitNewClient(args);
            this.Groups.push(args);
            break;
        }
      });
      SignalR.listen("Group",(handler,args)=>{
        args = JSON.parse(args);
        console.log(args)
        switch (handler) {
          case "GroupMessage": {
            let group = this.Groups.find(x => x.groupId === args.p.groupId)
            let client = this.Clients.find(x=>x.userId===args.s.userId);
            let message = args.o;
            let M = {message:message,client:client }
            group.Messages.push(M)
            if(!this.CurrentClient || (this.CurrentClient && this.CurrentClient.groupId!==group.groupId)){
              group.unReads++;
              this.GroupUnReads++;
            }else{
              this.DownToLatest();
            }
            break;
          }

        }
      });
      SignalR.listen("Client",(handler,args)=>{
        args = JSON.parse(args);
        console.log(args)
        switch (handler) {
          case "ClientMessage": {
            let client = this.Clients.find(x => x.userId === args.s.userId);
            let M = {message:args.o,client:client }
            client.Messages.push(M);
            if(!this.CurrentClient || (this.CurrentClient && this.CurrentClient.userId!==client.userId)){
              client.unReads++;
              this.AllUnReads++;
            }else{
              this.DownToLatest();
            }
            console.log(client)
            break;
          }
        }
      });
    },
    ClientOnLine(Client){
      let client = this.Clients.find(x => x.userId === Client.userId);
      if(client){
        client.connectionId=Client.connectionId;
        client.status=1;
      }else{
        this.Clients.push(Client);
      }
    },
    InitNewClient(Client){
      Client.unReads=0;
      Client.Messages=[];
      Client.SendingMessage='';
      Client.status = 1;
      Client.SendingPicture = [];
    },
    Chat(Client){
      this.CurrentClient = Client;
      if(Client.userId) {
        this.AllUnReads -= this.CurrentClient.unReads;
        this.CurrentClient.unReads = 0;
      }
      if(Client.groupId){
        this.GroupUnReads -= this.CurrentClient.unReads;
        this.CurrentClient.unReads = 0;
      }
      this.DownToLatest();
    },
    Send(){
      let msg = this.CurrentClient.SendingMessage
      let pic = this.CurrentClient.SendingPicture
      if(msg==="" && pic.length===0){
        this.$message.info("请输入一些内容")
        return;
      }
      let Message = {message:null,type:null}
      let handler;
      let client;
      if(this.CurrentClient.userId) {
        handler = 'ClientMessage';
        client = this.GetJsonClient(this.CurrentClient)
      }
      if(this.CurrentClient.groupId) {
        handler = 'GroupMessage';
        client = this.GetJsonGroup(this.CurrentClient);
      }
      if(msg){
        Message.content=msg;
        Message.type = "chars"
      }
      if(pic.length>0){
        Message.content=this.GetPicture();
          if (Message.content.length / 1024 / 1024 > 2) {
          this.$message.error('图片过大')
          return
        }
        Message.type = "picture"
      }
      let data = {
        s: this.My,
        p: client,
        o: Message,
        v: handler
      }
      debugger
      SignalR.invoke(handler, data).then(()=>{
        this.CurrentClient.Messages.push({message:Message,client:this.My});
        this.CurrentClient.SendingMessage = "";
        this.CurrentClient.SendingPicture = [];
        this.DownToLatest();
      });

    },
    ExceedFile(file,files){
      this.CurrentClient.SendingPicture.pop();
      this.CurrentClient.SendingPicture.push(file)
    },
    UpLoadFilter(E){
      let rawFile = this.CurrentClient.SendingPicture[0].raw;
      debugger
      if (rawFile.type !== 'image/jpeg' && rawFile.type !== 'image/png' && rawFile.type !== 'image/jpg') {
        this.$message.error('请选择图片格式')
        this.CurrentClient.SendingPicture.pop()
        return false
      } else if (rawFile.size / 1024 / 1024 > 2) {
        this.$message.error('最大长度不得超过2M')
        this.CurrentClient.SendingPicture.pop()
        return false
      }
      return true
    },
    GetPicture(){
      let img = document.getElementById("SendingPicture");
      let canvas=document.createElement("canvas");
      canvas.width=img.width;
      canvas.height=img.height;
      let ctx=canvas.getContext("2d");
      ctx.drawImage(img,0,0,img.width,img.height);
      let ext=img.src.substring(img.src.lastIndexOf(".")+1).toLowerCase();
      return canvas.toDataURL("images/"+ext);
    },
    GetJsonClient(Client){
      return  {
        userId:Client.userId,
        connectionId:Client.connectionId,
        userName:Client.userName,
        groups:Client.groups
      }
    },
    GetJsonGroup(Client){
      return {
        groupId:Client.groupId,
        groupName:Client.groupName,
      }
    },
  }
}
</script>

<style lang="scss">
.MyMessage{
  text-align: right;
  float: right;
}
.OtherMessage{
}
#Main {
  height: 100%;
  #Top{
    height: 15%;
  }
  #Left{
    .el-menu-item.is-active{
      background-color: #d3eaf8;
    }
  }
  #Bottom {
    height: 60% !important;
    .el-card__body {
      height: 100%;
    }
  }
}
</style>