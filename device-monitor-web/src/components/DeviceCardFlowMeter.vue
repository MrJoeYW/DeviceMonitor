<script setup lang="ts">

import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { StarsBackground } from '@/components/ui/bg-stars'

interface Props {
  title?: string
  description?: string
  status?: 'online' | 'offline' | 'warning' | 'unknown'
  deviceId?: string
  temperature?: number
  flow?: number
}

const props = withDefaults(defineProps<Props>(), {
  title: '流量表',
  description: '设备描述',
  status: 'unknown',
  deviceId: '--',
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

const formatValue = (value: number, digits = 1) => Number.isFinite(value) ? value.toFixed(digits) : '0.0'
</script>

<template>
  <Card class="group relative overflow-hidden transition-all duration-300 hover:shadow-lg hover:border-sky-500/40 cursor-pointer bg-card/60 backdrop-blur-md min-h-[16rem]">
    <!-- 星空动画背景 -->
    <StarsBackground class="absolute inset-0 z-0 pointer-events-none opacity-60" />
    
    <CardHeader class="relative z-10 pb-2">
      <div class="flex items-start justify-between gap-2">
        <div class="flex-1 min-w-0">
          <CardTitle class="text-[15px] font-semibold truncate text-foreground">{{ title }}</CardTitle>
          <CardDescription class="text-xs mt-1 truncate">{{ description }}</CardDescription>
          <div class="mt-2.5 flex items-center gap-2">
            <span class="text-[10px] px-2 py-0.5 rounded-full border bg-sky-500/10 text-sky-500 border-sky-500/20 font-medium">
              流量监测设备
            </span>
          </div>
        </div>
        <Badge
          variant="outline"
          :class="['text-[10px] px-2 py-0.5 h-6 shrink-0 font-medium border', currentStatus().class]"
        >
          {{ currentStatus().label }}
        </Badge>
      </div>
    </CardHeader>

    <CardContent class="relative z-10 py-4">
      <div class="grid grid-cols-2 gap-3">
        <!-- 温度数值 -->
        <div class="flex flex-col items-center justify-center py-4 px-2 rounded-xl bg-background/60 shadow-inner border border-border/40 backdrop-blur-md">
          <span class="text-[11px] text-muted-foreground font-medium mb-1">当前温度</span>
          <div class="flex items-baseline gap-1">
            <span class="text-3xl font-bold tracking-tight text-foreground font-mono transition-all duration-200">
              {{ formatValue(temperature, 1) }}
            </span>
            <span class="text-xs text-muted-foreground font-medium">℃</span>
          </div>
        </div>
        <!-- 流量数值 -->
        <div class="flex flex-col items-center justify-center py-4 px-2 rounded-xl bg-sky-500/5 shadow-inner border border-sky-500/20 backdrop-blur-md">
          <span class="text-[11px] text-sky-600 dark:text-sky-400 font-medium mb-1">瞬时流量</span>
          <div class="flex items-baseline gap-1">
            <span class="text-3xl font-bold tracking-tight text-sky-600 dark:text-sky-400 font-mono transition-all duration-200">
              {{ formatValue(flow, 1) }}
            </span>
            <span class="text-xs text-sky-600/70 dark:text-sky-400/70 font-medium">m³/h</span>
          </div>
        </div>
      </div>
    </CardContent>

    <CardFooter class="relative z-10 pt-1 pb-3">
      <div class="flex items-center justify-between w-full text-[10px] text-muted-foreground border-t border-border/40 pt-3 mt-1">
        <span class="font-mono opacity-60">ID: {{ deviceId }}</span>
        <div class="flex items-center gap-1.5">
          <div
            class="w-1.5 h-1.5 rounded-full"
            :class="{
              'bg-emerald-500 animate-[pulse_2s_ease-in-out_infinite]': status === 'online',
              'bg-zinc-400': status === 'offline',
              'bg-amber-500 animate-[pulse_1s_ease-in-out_infinite]': status === 'warning',
              'bg-zinc-500': status === 'unknown',
            }"
          ></div>
          <span class="opacity-80">实时采集</span>
        </div>
      </div>
    </CardFooter>
  </Card>
</template>
