<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import DeviceCardFlowMeter from '@/components/DeviceCardFlowMeter.vue'
import DeviceCardPowerMeter from '@/components/DeviceCardPowerMeter.vue'
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'

type DeviceStatus = 'online' | 'offline' | 'warning' | 'unknown'
type DeviceType = 'flowmeter' | 'powermeter'

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
  { id: 'PM-001', title: '主电能表 (车间A进线)', description: '车间 A - 进线柜电能监测', status: 'online', deviceType: 'powermeter' },
  { id: 'FM-003', title: '流量表 #3 (备用回路)', description: '储备供液回路', status: 'unknown', deviceType: 'flowmeter', temperature: 22.0, flow: 0.0 },
  { id: 'FM-004', title: '流量表 #4 (冷却回路 B)', description: '车间 B - 2号设备冷却液监测', status: 'online', deviceType: 'flowmeter', temperature: 35.1, flow: 81.3 },
  { id: 'PM-002', title: '电能表 (车间B进线)', description: '车间 B - 进线柜电能监测', status: 'online', deviceType: 'powermeter' },
  { id: 'FM-005', title: '流量表 #5 (纯水回路)', description: '实验室制水', status: 'warning', deviceType: 'flowmeter', temperature: 25.0, flow: 12.5 },
  { id: 'PM-003', title: '照明电能表', description: '厂区照明', status: 'online', deviceType: 'powermeter' },
  { id: 'FM-006', title: '流量表 #6', description: '冷却液回流', status: 'offline', deviceType: 'flowmeter', temperature: 30.0, flow: 0.0 },
  { id: 'FM-007', title: '流量表 #7', description: '清洗液回流', status: 'online', deviceType: 'flowmeter', temperature: 27.5, flow: 60.1 },
  { id: 'PM-004', title: '空调电能表', description: '厂区HVAC系统', status: 'online', deviceType: 'powermeter' },
  { id: 'FM-008', title: '流量表 #8', description: '排污监测', status: 'online', deviceType: 'flowmeter', temperature: 15.0, flow: 120.5 },
  { id: 'PM-005', title: '空压机电能表', description: '动力车间', status: 'warning', deviceType: 'powermeter' },
  { id: 'FM-009', title: '流量表 #9', description: '酸洗槽', status: 'online', deviceType: 'flowmeter', temperature: 45.0, flow: 15.2 },
  { id: 'FM-010', title: '流量表 #10', description: '碱洗槽', status: 'online', deviceType: 'flowmeter', temperature: 40.0, flow: 18.7 },
  { id: 'PM-006', title: '备用电能表', description: '备用电源', status: 'offline', deviceType: 'powermeter' },
])

const selectedType = ref<string>('all')
const currentPage = ref(1)
const pageSize = 10 // 5x2 grid = 10 items per page

const filteredDevices = computed(() => {
  if (selectedType.value === 'all') return devices.value
  return devices.value.filter(d => d.deviceType === selectedType.value)
})

const totalPages = computed(() => {
  return Math.ceil(filteredDevices.value.length / pageSize) || 1
})

const paginatedDevices = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredDevices.value.slice(start, start + pageSize)
})

watch(selectedType, () => {
  currentPage.value = 1
})

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
  <div class="space-y-6 flex flex-col h-full min-h-[calc(100vh-8rem)]">
    <!-- 页面标题与筛选 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">实时监控</h1>
        <p class="text-sm text-muted-foreground mt-1">实时查看所有设备的运行状态</p>
      </div>
      
      <div class="flex flex-col items-end gap-3">
        <!-- 状态图例 -->
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

        <!-- 设备类型筛选 -->
        <Tabs v-model="selectedType" class="w-[300px]">
          <TabsList class="grid w-full grid-cols-3">
            <TabsTrigger value="all">全部设备</TabsTrigger>
            <TabsTrigger value="flowmeter">流量表</TabsTrigger>
            <TabsTrigger value="powermeter">电能表</TabsTrigger>
          </TabsList>
        </Tabs>
      </div>
    </div>

    <!-- 设备卡片网格 (最大 5 列 * 2 行 = 10 个卡片) -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4 flex-1 content-start">
      <template v-for="device in paginatedDevices" :key="device.id">
        <DeviceCardFlowMeter
          v-if="device.deviceType === 'flowmeter'"
          :title="device.title"
          :description="device.description"
          :status="device.status"
          :device-id="device.id"
          :temperature="device.temperature"
          :flow="device.flow"
        />
        <DeviceCardPowerMeter
          v-else-if="device.deviceType === 'powermeter'"
          :title="device.title"
          :description="device.description"
          :status="device.status"
          :device-id="device.id"
        />
      </template>
    </div>

    <!-- 分页控件 -->
    <div class="flex items-center justify-between pt-4 border-t mt-auto">
      <div class="text-sm text-muted-foreground">
        共 <span class="font-medium text-foreground">{{ filteredDevices.length }}</span> 个设备
        <span v-if="totalPages > 1" class="ml-2">
          (第 {{ currentPage }} / {{ totalPages }} 页)
        </span>
      </div>
      <div class="flex items-center gap-2" v-if="totalPages > 1">
        <Button 
          variant="outline" 
          size="sm" 
          :disabled="currentPage === 1"
          @click="currentPage--"
        >
          上一页
        </Button>
        <div class="flex gap-1">
          <Button
            v-for="page in totalPages"
            :key="page"
            :variant="currentPage === page ? 'default' : 'ghost'"
            size="sm"
            class="w-8 p-0"
            @click="currentPage = page"
          >
            {{ page }}
          </Button>
        </div>
        <Button 
          variant="outline" 
          size="sm" 
          :disabled="currentPage === totalPages"
          @click="currentPage++"
        >
          下一页
        </Button>
      </div>
    </div>
  </div>
</template>
