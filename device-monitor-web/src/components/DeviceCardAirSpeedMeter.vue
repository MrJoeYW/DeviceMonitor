<script setup lang="ts">
import { computed } from 'vue'
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { FlipCard } from '@/components/ui/flip-card'

interface Props {
  title?: string
  description?: string
  status?: 'online' | 'offline' | 'warning' | 'unknown' | 'disabled'
  deviceId?: string
  speed?: number
  direction?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '风速仪',
  description: '设备描述',
  status: 'unknown',
  deviceId: '--',
  speed: 0,
  direction: 0,
})

const defaultStatus = { label: '未知', class: 'bg-muted text-muted-foreground border-border' }

const statusConfig: Record<string, { label: string; class: string }> = {
  online: { label: '在线', class: 'bg-emerald-500/15 text-emerald-500 border-emerald-500/30' },
  offline: { label: '离线', class: 'bg-muted text-muted-foreground border-border' },
  warning: { label: '告警', class: 'bg-amber-500/15 text-amber-500 border-amber-500/30' },
  unknown: defaultStatus,
  disabled: { label: '禁用', class: 'bg-muted text-muted-foreground border-border' },
}

const currentStatus = computed(() => statusConfig[props.status] ?? defaultStatus)

const formatValue = (value: number | undefined, digits = 1) => {
  if (value === undefined || value === null || !Number.isFinite(value)) return '0.0'
  return value.toFixed(digits)
}

const getWindDirectionLabel = (deg: number | undefined) => {
  if (deg === undefined || deg === null) return '--'
  const directions = ['北', '东北', '东', '东南', '南', '西南', '西', '西北']
  const index = Math.round(deg / 45) % 8
  return directions[index]
}
</script>

<template>
  <FlipCard class="w-full min-h-[19rem]">
    <!-- 正面 -->
    <Card class="h-full relative overflow-hidden transition-all duration-300 bg-card/60 backdrop-blur-md border-0 shadow-none">
      <CardHeader class="relative z-10 pb-2">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <CardTitle class="text-[15px] font-semibold truncate text-foreground">{{ title }}</CardTitle>
            <CardDescription class="text-xs mt-1 truncate">{{ description }}</CardDescription>
            <div class="mt-2.5 flex items-center gap-2">
              <span class="text-[10px] px-2 py-0.5 rounded-full border bg-teal-500/10 text-teal-500 border-teal-500/20 font-medium">
                风速监测设备
              </span>
            </div>
          </div>
          <Badge
            variant="outline"
            :class="['text-[10px] px-2 py-0.5 h-6 shrink-0 font-medium border bg-card/80', currentStatus.class]"
          >
            {{ currentStatus.label }}
          </Badge>
        </div>
      </CardHeader>

      <CardContent class="relative z-10 py-4 flex-1 flex flex-col justify-center">
        <div class="grid grid-cols-2 gap-3">
          <!-- 风速数值 -->
          <div class="flex flex-col items-center justify-center py-4 px-2 rounded-xl bg-teal-500/5 shadow-inner border border-teal-500/20 backdrop-blur-md">
            <span class="text-[11px] text-teal-600 dark:text-teal-400 font-medium mb-1">瞬时风速</span>
            <div class="flex items-baseline gap-1">
              <span class="text-3xl font-bold tracking-tight text-teal-600 dark:text-teal-400 font-mono transition-all duration-200">
                {{ formatValue(speed, 1) }}
              </span>
              <span class="text-xs text-teal-600/70 dark:text-teal-400/70 font-medium">m/s</span>
            </div>
          </div>
          <!-- 风向数值 -->
          <div class="flex flex-col items-center justify-center py-4 px-2 rounded-xl bg-background/60 shadow-inner border border-border/40 backdrop-blur-md">
            <span class="text-[11px] text-muted-foreground font-medium mb-1">风向</span>
            <div class="flex items-baseline gap-1">
              <span class="text-3xl font-bold tracking-tight text-foreground font-mono transition-all duration-200">
                {{ formatValue(direction, 0) }}
              </span>
              <span class="text-xs text-muted-foreground font-medium">°</span>
            </div>
            <span class="text-[10px] text-muted-foreground/70 mt-1">{{ getWindDirectionLabel(direction) }}</span>
          </div>
        </div>
      </CardContent>
    </Card>

    <!-- 背面 -->
    <template #back>
      <div class="h-full flex flex-col p-4 bg-background/95 backdrop-blur-xl border border-border/50">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-1 h-5 bg-teal-500 rounded-full"></div>
          <h3 class="font-bold text-sm tracking-tight">设备配置详情</h3>
        </div>
        
        <div class="flex-1 space-y-3">
          <div class="p-2.5 rounded-lg border border-border bg-muted/20">
            <div class="text-[10px] text-muted-foreground mb-0.5">设备序列号 (ID)</div>
            <div class="font-mono text-xs font-semibold">{{ deviceId }}</div>
          </div>
          
          <div class="grid grid-cols-1 gap-1">
            <div class="flex items-center justify-between text-[11px] py-1.5 border-b border-border/30">
              <span class="text-muted-foreground">采样频率</span>
              <span class="font-medium">1000ms</span>
            </div>
            <div class="flex items-center justify-between text-[11px] py-1.5 border-b border-border/30">
              <span class="text-muted-foreground">通讯协议</span>
              <span class="font-medium">Modbus RTU</span>
            </div>
            <div class="flex items-center justify-between text-[11px] py-1.5">
              <span class="text-muted-foreground">量程范围</span>
              <span class="font-medium">0~60 m/s</span>
            </div>
          </div>
        </div>

        <div class="mt-auto pt-3 border-t border-border/50 flex justify-between items-center">
          <span class="text-[9px] text-muted-foreground/60 tracking-wider">REV. 2024.03</span>
          <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
        </div>
      </div>
    </template>
  </FlipCard>
</template>
