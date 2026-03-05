import { createRouter, createWebHashHistory } from 'vue-router'
import RealtimeMonitor from '@/views/RealtimeMonitor.vue'
import DeviceManagement from '@/views/DeviceManagement.vue'
import Logs from '@/views/Logs.vue'
import SystemSettings from '@/views/SystemSettings.vue'

const routes = [
    { path: '/', redirect: '/monitor' },
    { path: '/monitor', component: RealtimeMonitor, meta: { title: '实时监控' } },
    { path: '/devices', component: DeviceManagement, meta: { title: '设备管理' } },
    { path: '/logs', component: Logs, meta: { title: '日志' } },
    { path: '/settings', component: SystemSettings, meta: { title: '系统设置' } },
]

const router = createRouter({
    history: createWebHashHistory(),
    routes,
})

export default router
