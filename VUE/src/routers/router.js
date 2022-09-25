import {createRouter,createWebHistory} from "vue-router"
import ChatHub from "@/components/ChatHub";
const routes = [
    {path:'/' ,component:ChatHub}
]

const router = new createRouter({
    routes,
    history:createWebHistory()
})

export default router