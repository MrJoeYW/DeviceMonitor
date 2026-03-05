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
      'shrink-0 border-r border-border bg-card flex flex-col h-full select-none transition-all duration-200',
      isCollapsed ? 'w-[72px]' : 'w-[220px]',
    ]"
  >
    <div class="h-14 border-b border-border flex items-center gap-3 px-4">
      <div class="w-8 h-8 rounded-lg bg-primary flex items-center justify-center">
        <Cpu class="w-4 h-4 text-primary-foreground" />
      </div>
      <div v-if="!isCollapsed" class="flex flex-col leading-tight">
        <span class="text-sm font-semibold tracking-tight">DeviceMonitor</span>
        <span class="text-[10px] text-muted-foreground">工业设备监控平台</span>
      </div>
      <div class="ml-auto">
        <Button
          variant="ghost"
          size="icon-sm"
          class="text-muted-foreground hover:text-foreground"
          @click="isCollapsed = !isCollapsed"
        >
          <ChevronLeft v-if="!isCollapsed" class="size-4" />
          <ChevronRight v-else class="size-4" />
        </Button>
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

    <div class="p-3 border-t border-border">
      <div class="flex items-center gap-2 px-2 py-1">
        <div class="w-6 h-6 rounded-full bg-muted flex items-center justify-center text-[10px] font-medium text-muted-foreground">
          管
        </div>
        <div v-if="!isCollapsed" class="flex flex-col leading-tight">
          <span class="text-xs font-medium">管理员</span>
          <span class="text-[10px] text-muted-foreground">admin@system</span>
        </div>
      </div>
    </div>
  </aside>
</template>
