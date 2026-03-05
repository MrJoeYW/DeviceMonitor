<script setup lang="ts">
import { computed } from 'vue'
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

interface Props {
  title?: string
  description?: string
  status?: 'online' | 'offline' | 'warning' | 'unknown'
  deviceId?: string
  deviceType?: 'flowmeter' | 'robot' | 'camera' | 'conveyor' | 'plc' | 'environment' | 'default'
  temperature?: number
  flow?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '设备名称',
  description: '设备描述',
  status: 'unknown',
  deviceId: '--',
  deviceType: 'default',
  temperature: 0,
  flow: 0,
})

const statusConfig = {
  online: { label: '在线', class: 'bg-emerald-500/15 text-emerald-500 border-emerald-500/30' },
  offline: { label: '离线', class: 'bg-muted text-muted-foreground border-border' },
  warning: { label: '告警', class: 'bg-amber-500/15 text-amber-500 border-amber-500/30' },
  unknown: { label: '未知', class: 'bg-muted text-muted-foreground border-border' },
}

const currentStatus = () => statusConfig[props.status]

const typeProfiles = {
  default: {
    label: '通用设备',
    subtitle: '运行参数稳定',
    badgeClass: 'bg-muted text-muted-foreground border-border',
    footer: '设备运行平稳',
    stats: [
      { label: '健康度', value: '96%' },
      { label: '告警', value: '0' },
    ],
  },
  robot: {
    label: '机器人',
    subtitle: '焊接轨迹正常',
    badgeClass: 'bg-emerald-500/15 text-emerald-500 border-emerald-500/30',
    footer: '节拍稳定',
    stats: [
      { label: '工作负载', value: '68%' },
      { label: '节拍', value: '1.2s' },
    ],
  },
  camera: {
    label: '视觉检测',
    subtitle: '成像质量优秀',
    badgeClass: 'bg-sky-500/15 text-sky-500 border-sky-500/30',
    footer: '光源正常',
    stats: [
      { label: '帧率', value: '48fps' },
      { label: '良率', value: '98.4%' },
    ],
  },
  conveyor: {
    label: '传送线',
    subtitle: '传输顺畅',
    badgeClass: 'bg-amber-500/15 text-amber-500 border-amber-500/30',
    footer: '无阻塞',
    stats: [
      { label: '速度', value: '1.6m/s' },
      { label: '载荷', value: '42%' },
    ],
  },
  plc: {
    label: 'PLC 控制',
    subtitle: '逻辑稳定',
    badgeClass: 'bg-indigo-500/15 text-indigo-500 border-indigo-500/30',
    footer: '运行正常',
    stats: [
      { label: 'CPU', value: '34%' },
      { label: '内存', value: '61%' },
    ],
  },
  environment: {
    label: '环境监测',
    subtitle: '环境舒适',
    badgeClass: 'bg-teal-500/15 text-teal-500 border-teal-500/30',
    footer: '环境稳定',
    stats: [
      { label: '温度', value: '26.4℃' },
      { label: '湿度', value: '53%' },
    ],
  },
  flowmeter: {
    label: '流量表',
    subtitle: '流体监测中',
    badgeClass: 'bg-sky-500/15 text-sky-500 border-sky-500/30',
    footer: '流速稳定',
    stats: [],
  },
} as const

const isFlowmeter = computed(() => props.deviceType === 'flowmeter')
const currentProfile = computed(() => typeProfiles[props.deviceType] ?? typeProfiles.default)

const clamp = (value: number, min: number, max: number) => Math.min(Math.max(value, min), max)
const tempPercent = computed(() => clamp(props.temperature ?? 0, 0, 100))
const flowPercent = computed(() => clamp(((props.flow ?? 0) / 150) * 100, 0, 100))
const formatValue = (value: number, digits = 1) => Number.isFinite(value) ? value.toFixed(digits) : '0.0'
</script>

<template>
  <Card class="group relative overflow-hidden transition-all duration-200 hover:shadow-md hover:border-border/80 cursor-pointer">
      <div v-if="isFlowmeter" class="flow-card-bg"></div>
      <CardHeader class="relative z-10 pb-3">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <CardTitle class="text-sm font-semibold truncate">{{ title }}</CardTitle>
            <CardDescription class="text-xs mt-0.5 truncate">{{ description }}</CardDescription>
            <div class="mt-2 flex items-center gap-2">
              <span
                class="text-[10px] px-2 py-0.5 rounded-full border"
                :class="currentProfile.badgeClass"
              >
                {{ currentProfile.label }}
              </span>
              <span class="text-[10px] text-muted-foreground truncate">{{ currentProfile.subtitle }}</span>
            </div>
          </div>
          <Badge
            variant="outline"
            :class="['text-[10px] px-1.5 py-0 h-5 shrink-0 font-medium border', currentStatus().class]"
          >
            {{ currentStatus().label }}
          </Badge>
        </div>
      </CardHeader>

      <CardContent class="relative z-10 pb-3">
        <div v-if="isFlowmeter" class="space-y-3">
          <div class="rounded-lg bg-muted/50 border border-border/60 p-3">
            <div class="flex items-center justify-between text-xs">
              <span class="text-muted-foreground">温度</span>
              <span class="font-semibold text-foreground">{{ formatValue(temperature, 1) }}℃</span>
            </div>
            <div class="mt-2 h-1.5 rounded-full bg-muted">
              <div class="h-full rounded-full bg-rose-500" :style="{ width: `${tempPercent}%` }"></div>
            </div>
          </div>
          <div class="rounded-lg bg-muted/50 border border-border/60 p-3">
            <div class="flex items-center justify-between text-xs">
              <span class="text-muted-foreground">流量</span>
              <span class="font-semibold text-foreground">{{ formatValue(flow, 1) }} m³/h</span>
            </div>
            <div class="mt-2 h-1.5 rounded-full bg-muted">
              <div class="h-full rounded-full bg-sky-500" :style="{ width: `${flowPercent}%` }"></div>
            </div>
          </div>
          <div class="flex items-center justify-between text-[10px] text-muted-foreground">
            <span>流体通道</span>
            <span class="text-emerald-500">流速稳定</span>
          </div>
        </div>
        <div v-else class="space-y-3">
          <div class="grid grid-cols-2 gap-3">
            <div
              v-for="stat in currentProfile.stats"
              :key="stat.label"
              class="rounded-lg bg-muted/50 border border-border/60 p-3"
            >
              <div class="text-[10px] text-muted-foreground">{{ stat.label }}</div>
              <div class="text-sm font-semibold mt-1">{{ stat.value }}</div>
            </div>
          </div>
          <div class="flex items-center justify-between text-[10px] text-muted-foreground">
            <span>{{ currentProfile.footer }}</span>
            <span class="text-emerald-500">稳定</span>
          </div>
        </div>
      </CardContent>

      <CardFooter class="relative z-10 pt-0 pb-3">
        <div class="flex items-center justify-between w-full text-[10px] text-muted-foreground">
          <span>ID: {{ deviceId }}</span>
          <div class="flex items-center gap-1">
            <div
              class="w-1.5 h-1.5 rounded-full"
              :class="{
                'bg-emerald-500 animate-pulse': status === 'online',
                'bg-zinc-400': status === 'offline',
                'bg-amber-500 animate-pulse': status === 'warning',
                'bg-zinc-500': status === 'unknown',
              }"
            ></div>
            <span>实时</span>
          </div>
        </div>
      </CardFooter>
    </Card>
</template>
