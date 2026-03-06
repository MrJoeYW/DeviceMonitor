<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import DeviceCardFlowMeter from '@/components/DeviceCardFlowMeter.vue'

type DeviceStatus = 'online' | 'offline' | 'warning' | 'unknown'
type DeviceType = 'flowmeter'

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
  { id: 'FM-001', title: '流量表 #1 (冷却回路)', description: '车间 A - 1号设备冷却液监测', status: 'online', deviceType: 'flowmeter', temperature: 32.4, flow: 68.2 },
  { id: 'FM-002', title: '流量表 #2 (清洗回路)', description: '车间 A - 超声波清洗机供液', status: 'online', deviceType: 'flowmeter', temperature: 28.9, flow: 74.6 },
  { id: 'FM-003', title: '流量表 #3 (备用回路)', description: '储备供液回路', status: 'unknown', deviceType: 'flowmeter', temperature: 22.0, flow: 0.0 },
  { id: 'FM-004', title: '流量表 #4 (冷却回路 B)', description: '车间 B - 2号设备冷却液监测', status: 'online', deviceType: 'flowmeter', temperature: 35.1, flow: 81.3 },
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
      <DeviceCardFlowMeter
        v-for="device in devices"
        :key="device.id"
        :title="device.title"
        :description="device.description"
        :status="device.status"
        :device-id="device.id"
        :temperature="device.temperature"
        :flow="device.flow"
      />
    </div>
  </div>
</template>
