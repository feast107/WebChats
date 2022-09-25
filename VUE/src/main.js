import { createApp } from 'vue'
import App from './App.vue'
import ElementPlus  from 'element-plus'
//国际化
import zhCn from 'element-plus/es/locale/lang/zh-cn'
//储存
import Storage from "@/utils/storage"

import { ElMessage ,ElMessageBox } from "element-plus";

import router from "@/routers/router";

import * as ElementPlusIcons from '@element-plus/icons-vue'
import 'element-plus/dist/index.css'

const global =  createApp(App)
for (const [key, component] of Object.entries(ElementPlusIcons)) {
    global.component(key, component)
}
global.config.globalProperties.$message = ElMessage
window.$message = ElMessage
global.config.globalProperties.$storage = Storage
global.config.globalProperties.$confirm = ElMessageBox
global.use(ElementPlus ,{locale: zhCn,})
global.use(router)
global.mount('#app')
