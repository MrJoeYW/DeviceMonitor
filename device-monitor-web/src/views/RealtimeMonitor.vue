<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import DeviceCard from '@/components/DeviceCard.vue'

type DeviceStatus = 'online' | 'offline' | 'warning' | 'unknown'
type DeviceType = 'flowmeter' | 'robot' | 'camera' | 'conveyor' | 'plc' | 'environment' | 'default'

type DeviceItem = {
  id: string
  title: string
  description: string
  status: DeviceStatus
  deviceType: DeviceType
  temperature?: number
  flow?: number
}

const devices = ref<DeviceItem[]>([
  { id: 'DEV-001', title: '焊接机器人 #1', description: '焊接工位 A - 机械臂', status: 'online', deviceType: 'robot' },
  { id: 'DEV-002', title: '焊接机器人 #2', description: '焊接工位 B - 机械臂', status: 'online', deviceType: 'robot' },
  { id: 'DEV-003', title: '视觉检测仪 #1', description: '质检工位 - 相机阵列', status: 'warning', deviceType: 'camera' },
  { id: 'DEV-004', title: '传送带 A', description: '上料段 - 主传送线', status: 'online', deviceType: 'conveyor' },
  { id: 'DEV-005', title: '传送带 B', description: '下料段 - 出货传送线', status: 'offline', deviceType: 'conveyor' },
  { id: 'DEV-006', title: 'PLC 控制器 #1', description: '主控 PLC - S7-1500', status: 'online', deviceType: 'plc' },
  { id: 'DEV-007', title: 'PLC 控制器 #2', description: '辅控 PLC - S7-300', status: 'online', deviceType: 'plc' },
  { id: 'DEV-008', title: '环境传感器', description: '车间温湿度监测', status: 'unknown', deviceType: 'environment' },
  { id: 'DEV-009', title: '流量表 #1', description: '回路 A - 冷却液监测', status: 'online', deviceType: 'flowmeter', temperature: 32.4, flow: 68.2 },
  { id: 'DEV-010', title: '流量表 #2', description: '回路 B - 生产线供液', status: 'online', deviceType: 'flowmeter', temperature: 28.9, flow: 74.6 },
])

const statusSummary = computed(() => {
  return devices.value.reduce(
    (acc, device) => {
      acc[device.status] += 1
      return acc
    },
    { online: 0, warning: 0, offline: 0, unknown: 0 },
  )
})

const randomInRange = (min: number, max: number) => Math.random() * (max - min) + min
const clamp = (value: number, min: number, max: number) => Math.min(Math.max(value, min), max)

let timer: number | undefined

onMounted(() => {
  timer = window.setInterval(() => {
    devices.value = devices.value.map((device) => {
      if (device.deviceType !== 'flowmeter') {
        return device
      }
      const temperature = clamp((device.temperature ?? 30) + randomInRange(-1.5, 1.5), 18, 85)
      const flow = clamp((device.flow ?? 60) + randomInRange(-6, 6), 20, 140)
      return { ...device, temperature, flow }
    })
  }, 1200)
})

onBeforeUnmount(() => {
  if (timer) {
    window.clearInterval(timer)
  }
})
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">实时监控</h1>
        <p class="text-sm text-muted-foreground mt-1">实时查看所有设备的运行状态</p>
      </div>
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
          <span>在线 {{ statusSummary.online }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-amber-500"></div>
          <span>告警 {{ statusSummary.warning }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-zinc-400"></div>
          <span>离线 {{ statusSummary.offline }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-zinc-600"></div>
          <span>未知 {{ statusSummary.unknown }}</span>
        </div>
      </div>
    </div>

    <!-- 设备卡片网格 -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <DeviceCard
        v-for="device in devices"
        :key="device.id"
        :title="device.title"
        :description="device.description"
        :status="device.status"
        :device-id="device.id"
        :device-type="device.deviceType"
        :temperature="device.temperature"
        :flow="device.flow"
      />
    </div>
  </div>
</template>
