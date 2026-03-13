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
import FuzzyText from '@/components/ui/FuzzyText.vue'

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
  <div class="relative group h-full">
    <!-- Card 本文 -->
    <Card 
      class="relative h-full flex flex-col transition-all duration-300 bg-background/50 backdrop-blur-sm min-h-[16rem] z-10 border shadow-sm border-purple-500/20"
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

      <CardFooter class="relative z-10 pt-1 pb-3">
        <div class="flex items-center justify-between w-full text-[10px] text-muted-foreground border-t border-purple-500/20 pt-3 mt-1">
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
            <span class="opacity-80">连接中</span>
          </div>
        </div>
      </CardFooter>
    </Card>
  </div>
</template>
