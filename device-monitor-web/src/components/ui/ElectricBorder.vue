<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import type { CSSProperties } from 'vue'

interface Props {
  color?: string
  speed?: number
  chaos?: number
  borderRadius?: number
  class?: string
  style?: CSSProperties
}

const props = withDefaults(defineProps<Props>(), {
  color: '#9370DB', // Purple hue to match the Power Meter's aesthetic
  speed: 1,
  chaos: 0.12,
  borderRadius: 24,
})

const hexToRgba = (hex: string, alpha: number = 1): string => {
  if (!hex) return `rgba(0,0,0,${alpha})`
  let h = hex.replace('#', '')
  if (h.length === 3) {
    h = h.split('').map(c => c + c).join('')
  }
  const int = parseInt(h, 16)
  const r = (int >> 16) & 255
  const g = (int >> 8) & 255
  const b = int & 255
  return `rgba(${r}, ${g}, ${b}, ${alpha})`
}

const canvasRef = ref<HTMLCanvasElement | null>(null)
const containerRef = ref<HTMLDivElement | null>(null)

let animationId: number | null = null
let resizeObserver: ResizeObserver | null = null
let time = 0
let lastFrameTime = 0

// Noise functions ported from ElectricBorder react-bits
const random = (x: number): number => {
  return (Math.sin(x * 12.9898) * 43758.5453) % 1
}

const noise2D = (x: number, y: number): number => {
  const i = Math.floor(x)
  const j = Math.floor(y)
  const fx = x - i
  const fy = y - j

  const a = random(i + j * 57)
  const b = random(i + 1 + j * 57)
  const c = random(i + (j + 1) * 57)
  const d = random(i + 1 + (j + 1) * 57)

  const ux = fx * fx * (3.0 - 2.0 * fx)
  const uy = fy * fy * (3.0 - 2.0 * fy)

  return a * (1 - ux) * (1 - uy) + b * ux * (1 - uy) + c * (1 - ux) * uy + d * ux * uy
}

const octavedNoise = (
  x: number,
  octaves: number,
  lacunarity: number,
  gain: number,
  baseAmplitude: number,
  baseFrequency: number,
  timer: number,
  seed: number,
  baseFlatness: number
): number => {
  let y = 0
  let amplitude = baseAmplitude
  let frequency = baseFrequency

  for (let i = 0; i < octaves; i++) {
    let octaveAmplitude = amplitude
    if (i === 0) {
      octaveAmplitude *= baseFlatness
    }
    y += octaveAmplitude * noise2D(frequency * x + seed * 100, timer * frequency * 0.3)
    frequency *= lacunarity
    amplitude *= gain
  }

  return y
}

const getCornerPoint = (
  centerX: number,
  centerY: number,
  radius: number,
  startAngle: number,
  arcLength: number,
  progress: number
): { x: number; y: number } => {
  const angle = startAngle + progress * arcLength
  return {
    x: centerX + radius * Math.cos(angle),
    y: centerY + radius * Math.sin(angle)
  }
}

const getRoundedRectPoint = (
  t: number,
  left: number,
  top: number,
  width: number,
  height: number,
  radius: number
): { x: number; y: number } => {
  const straightWidth = width - 2 * radius
  const straightHeight = height - 2 * radius
  const cornerArc = (Math.PI * radius) / 2
  const totalPerimeter = 2 * straightWidth + 2 * straightHeight + 4 * cornerArc
  const distance = t * totalPerimeter

  let accumulated = 0

  if (distance <= accumulated + straightWidth) {
    const progress = (distance - accumulated) / straightWidth
    return { x: left + radius + progress * straightWidth, y: top }
  }
  accumulated += straightWidth

  if (distance <= accumulated + cornerArc) {
    const progress = (distance - accumulated) / cornerArc
    return getCornerPoint(left + width - radius, top + radius, radius, -Math.PI / 2, Math.PI / 2, progress)
  }
  accumulated += cornerArc

  if (distance <= accumulated + straightHeight) {
    const progress = (distance - accumulated) / straightHeight
    return { x: left + width, y: top + radius + progress * straightHeight }
  }
  accumulated += straightHeight

  if (distance <= accumulated + cornerArc) {
    const progress = (distance - accumulated) / cornerArc
    return getCornerPoint(left + width - radius, top + height - radius, radius, 0, Math.PI / 2, progress)
  }
  accumulated += cornerArc

  if (distance <= accumulated + straightWidth) {
    const progress = (distance - accumulated) / straightWidth
    return { x: left + width - radius - progress * straightWidth, y: top + height }
  }
  accumulated += straightWidth

  if (distance <= accumulated + cornerArc) {
    const progress = (distance - accumulated) / cornerArc
    return getCornerPoint(left + radius, top + height - radius, radius, Math.PI / 2, Math.PI / 2, progress)
  }
  accumulated += cornerArc

  if (distance <= accumulated + straightHeight) {
    const progress = (distance - accumulated) / straightHeight
    return { x: left, y: top + height - radius - progress * straightHeight }
  }
  accumulated += straightHeight

  const progress = (distance - accumulated) / cornerArc
  return getCornerPoint(left + radius, top + radius, radius, Math.PI, Math.PI / 2, progress)
}

onMounted(() => {
  const canvas = canvasRef.value
  const container = containerRef.value
  if (!canvas || !container) return

  const ctx = canvas.getContext('2d')
  if (!ctx) return

  const octaves = 10
  const lacunarity = 1.6
  const gain = 0.7
  const amplitude = props.chaos
  const frequency = 10
  const baseFlatness = 0
  const displacement = 60
  const borderOffset = 60
  let width = 0
  let height = 0

  const updateSize = () => {
    const rect = container.getBoundingClientRect()
    const newWidth = rect.width + borderOffset * 2
    const newHeight = rect.height + borderOffset * 2

    const dpr = Math.min(window.devicePixelRatio || 1, 2)
    canvas.width = newWidth * dpr
    canvas.height = newHeight * dpr
    canvas.style.width = `${newWidth}px`
    canvas.style.height = `${newHeight}px`
    ctx.scale(dpr, dpr)

    width = newWidth
    height = newHeight
  }

  updateSize()

  const drawElectricBorder = (currentTime: number) => {
    if (!canvas || !ctx) return

    const deltaTime = (currentTime - lastFrameTime) / 1000
    time += deltaTime * props.speed
    lastFrameTime = currentTime

    const dpr = Math.min(window.devicePixelRatio || 1, 2)
    ctx.setTransform(1, 0, 0, 1, 0, 0)
    ctx.clearRect(0, 0, canvas.width, canvas.height)
    ctx.scale(dpr, dpr)

    ctx.strokeStyle = props.color
    ctx.lineWidth = 1
    ctx.lineCap = 'round'
    ctx.lineJoin = 'round'

    const scale = displacement
    const left = borderOffset
    const top = borderOffset
    const borderWidth = width - 2 * borderOffset
    const borderHeight = height - 2 * borderOffset
    const maxRadius = Math.min(borderWidth, borderHeight) / 2
    const radius = Math.min(props.borderRadius, maxRadius)

    const approximatePerimeter = 2 * (borderWidth + borderHeight) + 2 * Math.PI * radius
    const sampleCount = Math.floor(approximatePerimeter / 2)

    ctx.beginPath()

    for (let i = 0; i <= sampleCount; i++) {
      const progress = i / sampleCount

      const point = getRoundedRectPoint(progress, left, top, borderWidth, borderHeight, radius)

      const xNoise = octavedNoise(
        progress * 8,
        octaves,
        lacunarity,
        gain,
        amplitude,
        frequency,
        time,
        0,
        baseFlatness
      )
      const yNoise = octavedNoise(
        progress * 8,
        octaves,
        lacunarity,
        gain,
        amplitude,
        frequency,
        time,
        1,
        baseFlatness
      )

      const displacedX = point.x + xNoise * scale
      const displacedY = point.y + yNoise * scale

      if (i === 0) {
        ctx.moveTo(displacedX, displacedY)
      } else {
        ctx.lineTo(displacedX, displacedY)
      }
    }

    ctx.closePath()
    ctx.stroke()

    animationId = requestAnimationFrame(drawElectricBorder)
  }

  resizeObserver = new ResizeObserver(() => {
    updateSize()
  })
  resizeObserver.observe(container)

  animationId = requestAnimationFrame(drawElectricBorder)
})

onBeforeUnmount(() => {
  if (animationId) {
    cancelAnimationFrame(animationId)
  }
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})
</script>

<template>
  <div
    ref="containerRef"
    :class="['relative overflow-visible isolate', props.class]"
    :style="{ '--electric-border-color': props.color, borderRadius: props.borderRadius + 'px', ...props.style }"
  >
    <div class="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 pointer-events-none z-[12]">
      <canvas ref="canvasRef" class="block" style="pointer-events: none;" />
    </div>
    <div class="absolute inset-0 rounded-[inherit] pointer-events-none z-0">
      <div
        class="absolute inset-0 rounded-[inherit] pointer-events-none"
        :style="{ border: `2px solid ${hexToRgba(props.color, 0.6)}`, filter: 'blur(1px)' }"
      />
      <div
        class="absolute inset-0 rounded-[inherit] pointer-events-none"
        :style="{ border: `2px solid ${props.color}`, filter: 'blur(4px)' }"
      />
      <div
        class="absolute inset-0 rounded-[inherit] pointer-events-none -z-[1] scale-110 opacity-30"
        :style="{
          filter: 'blur(32px)',
          background: `linear-gradient(-30deg, ${props.color}, transparent, ${props.color})`
        }"
      />
    </div>
    <div class="relative rounded-[inherit] z-[1]">
      <slot />
    </div>
  </div>
</template>
