<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'
import { ref } from 'vue'
import {
  Activity,
  MonitorDot,
  ScrollText,
  Settings2,
  Cpu,
  ChevronLeft,
  ChevronRight,
} from 'lucide-vue-next'
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { Button } from '@/components/ui/button'

const route = useRoute()

const navItems = [
  { path: '/monitor', label: '实时监控', icon: Activity },
  { path: '/devices', label: '设备管理', icon: MonitorDot },
  { path: '/logs', label: '日志', icon: ScrollText },
  { path: '/settings', label: '系统设置', icon: Settings2 },
]

const isActive = (path: string) => route.path === path
const isCollapsed = ref(false)
</script>

<template>
  <aside
    :class="[
      'relative shrink-0 border-r border-border bg-card flex flex-col h-full select-none transition-all duration-300 z-50 overflow-visible',
      isCollapsed ? 'w-[72px]' : 'w-[220px]',
    ]"
  >
    <!-- Toggle Button (Floating) -->
    <Button
      variant="ghost"
      size="icon-sm"
      class="absolute -right-3 top-7 -translate-y-1/2 h-6 w-6 rounded-full border bg-card shadow-md z-[60] hover:bg-accent hover:text-primary transition-all duration-300 flex items-center justify-center group"
      @click="isCollapsed = !isCollapsed"
    >
      <ChevronLeft v-if="!isCollapsed" class="size-3.5 transition-transform group-hover:-translate-x-0.5" />
      <ChevronRight v-else class="size-3.5 transition-transform group-hover:translate-x-0.5" />
    </Button>

    <div :class="['h-14 border-b border-border flex items-center transition-all duration-200', isCollapsed ? 'px-0 justify-center' : 'px-4 gap-3']">
      <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center shrink-0 shadow-sm shadow-primary/20">
        <Cpu class="w-4 h-4 text-primary-foreground" />
      </div>
      
      <div v-if="!isCollapsed" class="flex flex-col leading-tight min-w-0 flex-1 animate-in fade-in slide-in-from-left-2 duration-300">
        <span class="text-sm font-bold tracking-tight truncate">DeviceMonitor</span>
        <span class="text-[10px] text-muted-foreground truncate font-medium">工业设备监控平台</span>
      </div>
    </div>

    <nav class="flex-1 py-3 px-2 space-y-1">
      <TooltipProvider :delay-duration="0">
        <template v-for="item in navItems" :key="item.path">
          <Tooltip v-if="isCollapsed">
            <TooltipTrigger as-child>
              <RouterLink
                :to="item.path"
                class="flex items-center justify-center px-3 py-2.5 rounded-md text-sm font-medium transition-colors duration-150 group"
                :class="
                  isActive(item.path)
                    ? 'bg-primary text-primary-foreground shadow-sm'
                    : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                "
              >
                <component
                  :is="item.icon"
                  class="w-4 h-4 shrink-0"
                  :class="isActive(item.path) ? 'text-primary-foreground' : 'text-muted-foreground group-hover:text-accent-foreground'"
                />
              </RouterLink>
            </TooltipTrigger>
            <TooltipContent side="right">{{ item.label }}</TooltipContent>
          </Tooltip>
          <RouterLink
            v-else
            :to="item.path"
            class="flex items-center gap-3 px-3 py-2.5 rounded-md text-sm font-medium transition-colors duration-150 group"
            :class="
              isActive(item.path)
                ? 'bg-primary text-primary-foreground shadow-sm'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
            "
          >
            <component
              :is="item.icon"
              class="w-4 h-4 shrink-0"
              :class="isActive(item.path) ? 'text-primary-foreground' : 'text-muted-foreground group-hover:text-accent-foreground'"
            />
            <span>{{ item.label }}</span>
            <span
              v-if="item.path === '/monitor'"
              class="ml-auto flex h-1.5 w-1.5 rounded-full"
              :class="isActive(item.path) ? 'bg-primary-foreground/60' : 'bg-emerald-500'"
            ></span>
          </RouterLink>
        </template>
      </TooltipProvider>
    </nav>

    <div class="p-3 border-t border-border shrink-0">
      <div :class="['flex items-center transition-all duration-200', isCollapsed ? 'justify-center' : 'gap-2 px-2 py-1']">
        <div class="w-6 h-6 rounded-full bg-muted flex items-center justify-center text-[10px] font-medium text-muted-foreground shrink-0 border border-border shadow-inner">
          管
        </div>
        <div v-if="!isCollapsed" class="flex flex-col leading-tight min-w-0 animate-in fade-in slide-in-from-left-1 duration-300">
          <span class="text-xs font-bold truncate">管理员</span>
          <span class="text-[10px] text-muted-foreground truncate opacity-70">admin@system</span>
        </div>
      </div>
    </div>
  </aside>
</template>
