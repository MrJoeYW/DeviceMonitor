<script setup lang="ts">
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import FuzzyText from '@/components/ui/FuzzyText.vue'
import { FlipCard } from '@/components/ui/flip-card'

interface Props {
  title?: string
  description?: string
  status?: 'online' | 'offline' | 'warning' | 'unknown'
  deviceId?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: '电能表',
  description: '设备描述',
  status: 'unknown',
  deviceId: '--',
})

const statusConfig = {
  online: { label: '在线', class: 'bg-emerald-500/15 text-emerald-500 border-emerald-500/30' },
  offline: { label: '离线', class: 'bg-muted text-muted-foreground border-border' },
  warning: { label: '告警', class: 'bg-amber-500/15 text-amber-500 border-amber-500/30' },
  unknown: { label: '未知', class: 'bg-muted text-muted-foreground border-border' },
}

const currentStatus = () => statusConfig[props.status]
</script>

<template>
  <FlipCard class="w-full min-h-[19rem]">
    <!-- 正面 -->
    <Card 
      class="relative h-full flex flex-col transition-all duration-300 bg-background/50 backdrop-blur-sm z-10 border-0 shadow-none"
    >
      <CardHeader class="relative z-10 pb-2">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <CardTitle class="text-[15px] font-semibold truncate text-foreground">{{ title }}</CardTitle>
            <CardDescription class="text-xs mt-1 truncate">{{ description }}</CardDescription>
            <div class="mt-2.5 flex items-center gap-2">
              <span class="text-[10px] px-2 py-0.5 rounded-full border bg-purple-500/10 text-purple-500 border-purple-500/20 font-medium">
                电能监测设备
              </span>
            </div>
          </div>
          <Badge
            variant="outline"
            :class="['text-[10px] px-2 py-0.5 h-6 shrink-0 font-medium border bg-card/80', currentStatus().class]"
          >
            {{ currentStatus().label }}
          </Badge>
        </div>
      </CardHeader>

      <CardContent class="relative z-10 py-4 flex-1 flex flex-col items-center justify-center gap-2">
        <div class="border border-dashed border-purple-500/30 bg-purple-500/5 p-6 rounded-lg w-full text-center flex flex-col items-center justify-center h-full">
          <FuzzyText
            text="NO SIGNAL"
            :fontSize="32"
            :fontWeight="900"
            color="rgba(147, 112, 219, 0.7)"
            :baseIntensity="0.1"
            :hoverIntensity="0.4"
            :fuzzRange="15"
            :fps="24"
            class="font-mono tracking-widest"
          />
          <FuzzyText
            text="SCANNING GRID..."
            :fontSize="10"
            color="rgba(168, 85, 247, 0.6)"
            :baseIntensity="0.05"
            :hoverIntensity="0.3"
            :fuzzRange="5"
            :fps="24"
            class="uppercase font-mono tracking-widest mt-2"
          />
        </div>
      </CardContent>
    </Card>

    <!-- 背面 -->
    <template #back>
      <div class="h-full flex flex-col p-4 bg-background/95 backdrop-blur-xl border border-border/50">
        <div class="flex items-center gap-2 mb-4">
          <div class="w-1 h-5 bg-purple-500 rounded-full"></div>
          <h3 class="font-bold text-sm tracking-tight">电能参数配置</h3>
        </div>
        
        <div class="flex-1 space-y-3">
          <div class="p-2.5 rounded-lg border border-border bg-muted/20">
            <div class="text-[10px] text-muted-foreground mb-0.5">设备序列号 (ID)</div>
            <div class="font-mono text-xs font-semibold">{{ deviceId }}</div>
          </div>
          
          <div class="grid grid-cols-1 gap-1">
            <div class="flex items-center justify-between text-[11px] py-1.5 border-b border-border/30">
              <span class="text-muted-foreground">电压系数</span>
              <span class="font-medium">220.0 (Custom)</span>
            </div>
            <div class="flex items-center justify-between text-[11px] py-1.5 border-b border-border/30">
              <span class="text-muted-foreground">电流阈值</span>
              <span class="font-medium">15.0A</span>
            </div>
            <div class="flex items-center justify-between text-[11px] py-1.5">
              <span class="text-muted-foreground">通信节点</span>
              <span class="font-medium">Gateway #01</span>
            </div>
          </div>
        </div>

        <div class="mt-auto pt-3 border-t border-border/50 flex justify-between items-center">
          <span class="text-[9px] text-muted-foreground/60 tracking-wider">CONFIG V1.2</span>
          <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
        </div>
      </div>
    </template>
  </FlipCard>
</template>
