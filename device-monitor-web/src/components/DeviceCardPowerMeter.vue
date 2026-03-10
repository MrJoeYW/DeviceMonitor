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
import { onMounted, onBeforeUnmount, ref } from 'vue'

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

const canvasRef = ref<HTMLCanvasElement | null>(null)
const containerRef = ref<HTMLDivElement | null>(null)

let animationFrameId: number
let resizeObserver: ResizeObserver | null = null

const initFuzzyBorder = () => {
  const canvas = canvasRef.value
  const container = containerRef.value
  if (!canvas || !container) return
  
  const ctx = canvas.getContext('2d', { willReadFrequently: true })
  if (!ctx) return
  
  const offscreen = document.createElement('canvas')
  const offCtx = offscreen.getContext('2d', { willReadFrequently: true })
  if (!offCtx) return
  
  let width = 0
  let height = 0
  const margin = 20 // Space for horizontal jitter
  const radius = 12 // Card border radius
  
  const drawOffscreen = () => {
    width = container.clientWidth
    height = container.clientHeight
    
    canvas.width = width + margin * 2
    canvas.height = height + margin * 2
    offscreen.width = canvas.width
    offscreen.height = canvas.height
    
    offCtx.clearRect(0, 0, offscreen.width, offscreen.height)
    
    // To reliably get the theme colors in Canvas without manually parsing HSL strings,
    // we set the fill and stroke styles directly using the var() syntax, which Canvas supports if attached to DOM
    offCtx.fillStyle = '#ffffff'
    offCtx.strokeStyle = 'rgba(0, 0, 0, 0.2)'
    offCtx.lineWidth = 1
    offCtx.shadowBlur = 0
    
    const x = margin
    const y = margin
    const w = width
    const h = height
    
    offCtx.beginPath()
    offCtx.moveTo(x + radius, y)
    offCtx.lineTo(x + w - radius, y)
    offCtx.arcTo(x + w, y, x + w, y + radius, radius)
    offCtx.lineTo(x + w, y + h - radius)
    offCtx.arcTo(x + w, y + h, x + w - radius, y + h, radius)
    offCtx.lineTo(x + radius, y + h)
    offCtx.arcTo(x, y + h, x, y + h - radius, radius)
    offCtx.lineTo(x, y + radius)
    offCtx.arcTo(x, y, x + radius, y, radius)
    offCtx.closePath()
    offCtx.fill()
    offCtx.stroke()
  }
  
  drawOffscreen()
  
  resizeObserver = new ResizeObserver(() => {
    drawOffscreen()
  })
  resizeObserver.observe(container)
  
  const fuzzRange = 10 // Reduced amplitude (less horizonal spikes)
  const intensity = 0.5 // Reduced baseline chance
  const fps = 24 // Slower frame rate for less erratic boiling

  const frameDuration = 1000 / fps
  let lastFrameTime = 0
  
  const run = (timestamp: number) => {
    animationFrameId = requestAnimationFrame(run)
    if (timestamp - lastFrameTime < frameDuration) return
    lastFrameTime = timestamp
    
    ctx.clearRect(0, 0, canvas.width, canvas.height)
    
    // Strictly matching React Bits FuzzyText: Slice horizontally and apply random horizontal shift
    for (let j = 0; j < canvas.height; j++) {
      const dx = Math.floor(intensity * (Math.random() - 0.5) * fuzzRange)
      ctx.drawImage(offscreen, 0, j, canvas.width, 1, dx, j, canvas.width, 1)
    }
  }
  
  animationFrameId = requestAnimationFrame(run)
}

onMounted(() => {
  initFuzzyBorder()
})

onBeforeUnmount(() => {
  if (animationFrameId) cancelAnimationFrame(animationFrameId)
  if (resizeObserver) resizeObserver.disconnect()
})
</script>

<template>
  <div class="relative group h-full" ref="containerRef">
    <!-- 用 Canvas 完美还原 React Bits Fuzzy Text 的逐行扫描与横向抖动像素偏移 -->
    <canvas 
      ref="canvasRef" 
      class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 pointer-events-none z-0"
    ></canvas>

    <!-- Card 本体去掉自带背景和边框，完全让底部 Canvas 充当抖动背景和边框 -->
    <Card class="relative h-full flex flex-col transition-all duration-300 bg-transparent min-h-[16rem] z-10 border-transparent shadow-none">
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

      <CardContent class="relative z-10 py-4 flex-1 flex items-center justify-center">
        <!-- 空白内容区 -->
        <div class="text-sm text-foreground/70 font-mono tracking-widest border border-dashed border-purple-500/30 bg-purple-500/5 p-8 rounded-lg w-full text-center">
          WAITING FOR SIGNAL...
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
