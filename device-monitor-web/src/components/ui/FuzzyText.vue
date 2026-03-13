<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, nextTick } from 'vue'

interface Props {
  text: string
  fontSize?: string | number
  fontWeight?: string | number
  fontFamily?: string
  color?: string
  enableHover?: boolean
  baseIntensity?: number
  hoverIntensity?: number
  fuzzRange?: number
  fps?: number
  class?: string
}

const props = withDefaults(defineProps<Props>(), {
  fontSize: 'inherit',
  fontWeight: 'inherit',
  fontFamily: 'inherit',
  color: 'inherit',
  enableHover: true,
  baseIntensity: 0.18,
  hoverIntensity: 0.5,
  fuzzRange: 30,
  fps: 60,
})

const canvasRef = ref<HTMLCanvasElement | null>(null)
let animationFrameId: number
let isHovering = false

onMounted(async () => {
  await nextTick()
  const canvas = canvasRef.value
  if (!canvas) return

  const ctx = canvas.getContext('2d', { willReadFrequently: true })
  if (!ctx) return

  let computedFontFamily = props.fontFamily
  if (computedFontFamily === 'inherit') {
    computedFontFamily = window.getComputedStyle(canvas).fontFamily || 'sans-serif'
  }

  let computedColor = props.color
  if (computedColor === 'inherit') {
    computedColor = window.getComputedStyle(canvas).color || '#fff'
  }

  const fontSizeStr = typeof props.fontSize === 'number' ? `${props.fontSize}px` : props.fontSize
  const fontString = `${props.fontWeight} ${fontSizeStr} ${computedFontFamily}`

  try {
    await document.fonts.load(fontString)
  } catch {
    await document.fonts.ready
  }

  // Create offscreen canvas to measure text and rasterize it
  const offscreen = document.createElement('canvas')
  const offCtx = offscreen.getContext('2d', { willReadFrequently: true })
  if (!offCtx) return

  offCtx.font = fontString
  offCtx.textBaseline = 'alphabetic'
  
  const text = props.text
  const metrics = offCtx.measureText(text)
  const actualLeft = metrics.actualBoundingBoxLeft ?? 0
  const actualRight = metrics.actualBoundingBoxRight ?? metrics.width
  let numericFontSize = 24
  
  if (typeof props.fontSize === 'number') {
    numericFontSize = props.fontSize
  } else {
    const temp = document.createElement('span')
    temp.style.fontSize = props.fontSize
    document.body.appendChild(temp)
    numericFontSize = parseFloat(window.getComputedStyle(temp).fontSize)
    document.body.removeChild(temp)
  }

  const actualAscent = metrics.actualBoundingBoxAscent ?? numericFontSize
  const actualDescent = metrics.actualBoundingBoxDescent ?? numericFontSize * 0.2

  const textBoundingWidth = Math.ceil(actualLeft + actualRight)
  const tightHeight = Math.ceil(actualAscent + actualDescent)

  const extraWidthBuffer = 10
  const offscreenWidth = textBoundingWidth + extraWidthBuffer

  offscreen.width = offscreenWidth
  offscreen.height = tightHeight

  const xOffset = extraWidthBuffer / 2
  // Re-apply font after resizing canvas!
  offCtx.font = fontString
  offCtx.textBaseline = 'alphabetic'
  offCtx.fillStyle = computedColor
  
  offCtx.fillText(text, xOffset - actualLeft, actualAscent)

  const horizontalMargin = props.fuzzRange + 20
  
  canvas.width = offscreenWidth + horizontalMargin * 2
  canvas.height = tightHeight

  // Shift to account for horizontal fuzzy overflow
  ctx.translate(horizontalMargin, 0)
  
  let currentIntensity = props.baseIntensity
  let lastFrameTime = 0
  const frameDuration = 1000 / props.fps

  const run = (timestamp: number) => {
    animationFrameId = requestAnimationFrame(run)

    if (timestamp - lastFrameTime < frameDuration) {
      return
    }
    lastFrameTime = timestamp

    ctx.clearRect(
      -horizontalMargin,
      0,
      canvas.width,
      canvas.height
    )

    const targetIntensity = isHovering ? props.hoverIntensity : props.baseIntensity
    
    // Smooth transition
    currentIntensity += (targetIntensity - currentIntensity) * 0.1

    // Horizontal only jitter logic
    for (let j = 0; j < tightHeight; j++) {
      const dx = Math.floor(currentIntensity * (Math.random() - 0.5) * props.fuzzRange)
      // Only horizontal shift! No dy
      ctx.drawImage(offscreen, 0, j, offscreenWidth, 1, dx, j, offscreenWidth, 1)
    }
  }

  animationFrameId = requestAnimationFrame(run)
})

onBeforeUnmount(() => {
  if (animationFrameId) {
    cancelAnimationFrame(animationFrameId)
  }
})

const handleMouseEnter = () => {
  if (props.enableHover) isHovering = true
}

const handleMouseLeave = () => {
  if (props.enableHover) isHovering = false
}
</script>

<template>
  <canvas 
    ref="canvasRef" 
    :class="props.class"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    style="user-select: none;"
  />
</template>
