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

interface Props {
  title?: string
  description?: string
  status?: 'online' | 'offline' | 'warning' | 'unknown'
  deviceId?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: '设备名称',
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
  <Card class="group transition-all duration-200 hover:shadow-md hover:border-border/80 cursor-pointer">
    <CardHeader class="pb-3">
      <div class="flex items-start justify-between gap-2">
        <div class="flex-1 min-w-0">
          <CardTitle class="text-sm font-semibold truncate">{{ title }}</CardTitle>
          <CardDescription class="text-xs mt-0.5 truncate">{{ description }}</CardDescription>
        </div>
        <Badge
          variant="outline"
          :class="['text-[10px] px-1.5 py-0 h-5 shrink-0 font-medium border', currentStatus().class]"
        >
          {{ currentStatus().label }}
        </Badge>
      </div>
    </CardHeader>

    <CardContent class="pb-3">
      <!-- 预留内容区域 - 子组件插槽 -->
      <slot>
        <div class="h-24 rounded-md bg-muted/50 flex items-center justify-center border border-dashed border-border">
          <span class="text-xs text-muted-foreground">内容区域待设计</span>
        </div>
      </slot>
    </CardContent>

    <CardFooter class="pt-0 pb-3">
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
