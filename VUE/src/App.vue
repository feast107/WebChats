<template>
  <ChatHub v-if="Visible">

  </ChatHub>
</template>

<script>
import HelloWorld from './components/HelloWorld.vue'
import ChatHub from "@/components/ChatHub";

export default {
  name: 'App',
  components: {
    ChatHub,
  },
  data(){
    return {
      Visible:true,
    }
  },
  provide(){
    return {
      reload:this.reload
    }
  },
  methods:{
    reload(){
      this.Visible = false;
      this.$nextTick(() => {
        this.Visible = true
      });
    },
  }
}
</script>

<style>
html{
  height: 100%;
}
body{
  margin: 10px;
  height: 92%;
}
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  height: 100%;
}
</style>
