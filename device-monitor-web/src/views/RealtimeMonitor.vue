<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import DeviceCardFlowMeter from '@/components/DeviceCardFlowMeter.vue'
import DeviceCardPowerMeter from '@/components/DeviceCardPowerMeter.vue'
import DeviceCardAirSpeedMeter from '@/components/DeviceCardAirSpeedMeter.vue'
import DeviceCardAirConditioner from '@/components/DeviceCardAirConditioner.vue'
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { Loader2 } from 'lucide-vue-next'
import { sendMessage } from '@/api/bridge'

// ==================== 类型定义 ====================
type DeviceStatus = 'online' | 'offline' | 'warning' | 'unknown' | 'disabled'

interface DeviceSnapshot {
  deviceId: number
  deviceName: string
  deviceType: string
  integratorId: number
  isEnabled: boolean
  slaveAddress: number
  status: DeviceStatus
  values: Record<string, number>
}

interface Integrator {
  id: number
  name: string
  ipAddress: string
  port: number
  isEnabled: boolean
}

// ==================== 状态 ====================
const snapshots = ref<DeviceSnapshot[]>([])
const integrators = ref<Integrator[]>([])
const loading = ref(true)
const error = ref('')
const lastUpdate = ref<Date | null>(null)

const selectedType = ref<string>('all')
const currentPage = ref(1)
const pageSize = 10 // 5x2 grid = 10 items per page

// ==================== 数据加载 ====================
async function loadInitialData() {
  loading.value = true
  error.value = ''
  try {
    const [igs, snaps] = await Promise.all([
      sendMessage<Integrator[]>('integrator:getAll'),
      sendMessage<DeviceSnapshot[]>('snapshot:getAll'),
    ])
    integrators.value = igs || []
    snapshots.value = snaps || []
    lastUpdate.value = new Date()
  } catch (e: any) {
    error.value = '加载数据失败：' + e.message
  } finally {
    loading.value = false
  }
}

async function pollSnapshots() {
  try {
    const snaps = await sendMessage<DeviceSnapshot[]>('snapshot:getAll')
    if (snaps) {
      snapshots.value = snaps
      lastUpdate.value = new Date()
    }
  } catch (e: any) {
    console.error('轮询快照失败:', e)
  }
}

// ==================== 轮询 ====================
let pollTimer: number | undefined
const POLL_INTERVAL = 1000 // 1秒轮询

onMounted(async () => {
  await loadInitialData()
  pollTimer = window.setInterval(pollSnapshots, POLL_INTERVAL)
})

onBeforeUnmount(() => {
  if (pollTimer) {
    window.clearInterval(pollTimer)
  }
})

// ==================== 辅助函数 ====================
function getIntegratorName(id: number): string {
  return integrators.value.find(i => i.id === id)?.name ?? '未知'
}

// ==================== 计算属性 ====================
const filteredSnapshots = computed(() => {
  // 只显示启用的设备
  const enabled = snapshots.value.filter(s => s.isEnabled)
  if (selectedType.value === 'all') return enabled
  return enabled.filter(s => s.deviceType.toLowerCase() === selectedType.value)
})

const totalPages = computed(() => {
  return Math.ceil(filteredSnapshots.value.length / pageSize) || 1
})

const paginatedSnapshots = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredSnapshots.value.slice(start, start + pageSize)
})

watch(selectedType, () => {
  currentPage.value = 1
})

const statusSummary = computed(() => {
  const summary = { online: 0, warning: 0, offline: 0, unknown: 0, disabled: 0 }
  snapshots.value.forEach(s => {
    if (!s.isEnabled) {
      summary.disabled += 1
    } else {
      summary[s.status as keyof typeof summary] = (summary[s.status as keyof typeof summary] || 0) + 1
    }
  })
  return summary
})
</script>

<template>
  <div class="flex flex-col min-h-full bg-background relative">
    <!-- 1. 内容主体区域 (带内边距) -->
    <div class="p-6 space-y-6 flex-1">
      <!-- 页面标题与筛选 -->
      <div class="flex items-center justify-between">
        <div>
          <h1 class="text-xl font-semibold tracking-tight">实时监控</h1>
          <p class="text-sm text-muted-foreground mt-1">
            实时查看所有设备的运行状态
            <span v-if="lastUpdate" class="ml-2 text-xs">
              (更新于 {{ lastUpdate.toLocaleTimeString() }})
            </span>
          </p>
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
          <Tabs v-model="selectedType" class="w-[400px]">
            <TabsList class="grid w-full grid-cols-5">
              <TabsTrigger value="all">全部</TabsTrigger>
              <TabsTrigger value="flowmeter">流量计</TabsTrigger>
              <TabsTrigger value="powermeter">电能表</TabsTrigger>
              <TabsTrigger value="airspeedmeter">风速仪</TabsTrigger>
              <TabsTrigger value="airconditioner">空调</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
      </div>

      <!-- 加载状态 -->
      <div v-if="loading" class="flex items-center justify-center py-20">
        <Loader2 class="w-8 h-8 animate-spin text-muted-foreground" />
        <span class="ml-3 text-muted-foreground">加载中...</span>
      </div>

      <!-- 错误提示 -->
      <div v-else-if="error" class="flex items-center justify-center py-20 text-destructive">
        {{ error }}
      </div>

      <!-- 空状态 -->
      <div v-else-if="filteredSnapshots.length === 0" class="flex flex-col items-center justify-center py-20 text-muted-foreground">
        <p class="text-lg">暂无设备数据</p>
        <p class="text-sm mt-2">请先在设备管理中添加设备</p>
      </div>

      <!-- 设备卡片网格 (最大 5 列 * 2 行 = 10 个卡片) -->
      <div v-else class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 2xl:grid-cols-5 gap-4 content-start">
        <template v-for="snapshot in paginatedSnapshots" :key="snapshot.deviceId">
          <!-- 流量计卡片 -->
          <DeviceCardFlowMeter
            v-if="snapshot.deviceType.toLowerCase() === 'flowmeter' || snapshot.deviceType === '流量计'"
            :title="snapshot.deviceName"
            :description="`${getIntegratorName(snapshot.integratorId)} - 从站 ${snapshot.slaveAddress}`"
            :status="snapshot.status"
            :device-id="String(snapshot.deviceId)"
            :temperature="snapshot.values['temperature'] ?? snapshot.values['温度']"
            :flow="snapshot.values['flow'] ?? snapshot.values['流量']"
          />
          
          <!-- 电能表卡片 -->
          <DeviceCardPowerMeter
            v-else-if="snapshot.deviceType.toLowerCase() === 'powermeter' || snapshot.deviceType === '电能表'"
            :title="snapshot.deviceName"
            :description="`${getIntegratorName(snapshot.integratorId)} - 从站 ${snapshot.slaveAddress}`"
            :status="snapshot.status"
            :device-id="String(snapshot.deviceId)"
            :voltage="snapshot.values['voltage'] ?? snapshot.values['电压']"
            :current="snapshot.values['current'] ?? snapshot.values['电流']"
            :power="snapshot.values['power'] ?? snapshot.values['功率']"
            :energy="snapshot.values['reactive_power'] ?? snapshot.values['power_factor'] ?? snapshot.values['energy'] ?? snapshot.values['电能']"
          />
          
          <!-- 风速仪卡片 -->
          <DeviceCardAirSpeedMeter
            v-else-if="snapshot.deviceType.toLowerCase() === 'airspeedmeter' || snapshot.deviceType === '风速仪'"
            :title="snapshot.deviceName"
            :description="`${getIntegratorName(snapshot.integratorId)} - 从站 ${snapshot.slaveAddress}`"
            :status="snapshot.status"
            :device-id="String(snapshot.deviceId)"
            :speed="snapshot.values['speed'] ?? snapshot.values['风速']"
            :direction="snapshot.values['pressure_1'] ?? snapshot.values['direction'] ?? snapshot.values['风向']"
          />
          
          <!-- 空调卡片 -->
          <DeviceCardAirConditioner
            v-else-if="snapshot.deviceType.toLowerCase() === 'airconditioner' || snapshot.deviceType === '空调'"
            :title="snapshot.deviceName"
            :description="`${getIntegratorName(snapshot.integratorId)} - 从站 ${snapshot.slaveAddress}`"
            :status="snapshot.status"
            :device-id="String(snapshot.deviceId)"
            :temperature="snapshot.values['temperature'] ?? snapshot.values['温度']"
            :setTemperature="snapshot.values['settemperature'] ?? snapshot.values['设定温度']"
            :humidity="snapshot.values['alarm_relay'] ?? snapshot.values['humidity'] ?? snapshot.values['湿度']"
          />
        </template>
      </div>
    </div>

    <!-- 2. 吸底浮动分页控件 (贴边方案) -->
    <div v-if="!loading && filteredSnapshots.length > 0" class="sticky bottom-0 z-20 flex items-center justify-between pt-4 pb-6 border-t bg-background/90 backdrop-blur-md shadow-[0_-10px_20px_-5px_rgba(0,0,0,0.05)] px-6">
      <div class="text-sm text-muted-foreground font-medium">
        共 <span class="font-bold text-foreground">{{ filteredSnapshots.length }}</span> 个设备
        <span v-if="totalPages > 1" class="ml-2 text-xs opacity-70">
          (第 {{ currentPage }} / {{ totalPages }} 页)
        </span>
      </div>
      <div class="flex items-center gap-2" v-if="totalPages > 1">
        <Button 
          variant="outline" 
          size="sm" 
          :disabled="currentPage === 1"
          @click="currentPage--"
          class="h-8"
        >
          上一页
        </Button>
        <div class="flex gap-1">
          <Button
            v-for="page in totalPages"
            :key="page"
            :variant="currentPage === page ? 'default' : 'ghost'"
            size="sm"
            class="w-8 h-8 p-0"
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
          class="h-8"
        >
          下一页
        </Button>
      </div>
    </div>
  </div>
</template>
