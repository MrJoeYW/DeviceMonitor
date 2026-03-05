<script setup lang="ts">
import DeviceCard from '@/components/DeviceCard.vue'

// 示例占位数据，后续替换为真实接口数据
const demoDevices = [
  { id: 'DEV-001', title: '焊接机器人 #1', description: '焊接工位 A - 机械臂', status: 'online' as const },
  { id: 'DEV-002', title: '焊接机器人 #2', description: '焊接工位 B - 机械臂', status: 'online' as const },
  { id: 'DEV-003', title: '视觉检测仪 #1', description: '质检工位 - 相机阵列', status: 'warning' as const },
  { id: 'DEV-004', title: '传送带 A', description: '上料段 - 主传送线', status: 'online' as const },
  { id: 'DEV-005', title: '传送带 B', description: '下料段 - 出货传送线', status: 'offline' as const },
  { id: 'DEV-006', title: 'PLC 控制器 #1', description: '主控 PLC - S7-1500', status: 'online' as const },
  { id: 'DEV-007', title: 'PLC 控制器 #2', description: '辅控 PLC - S7-300', status: 'online' as const },
  { id: 'DEV-008', title: '环境传感器', description: '车间温湿度监测', status: 'unknown' as const },
]
</script>

<template>
  <div class="space-y-6">
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">实时监控</h1>
        <p class="text-sm text-muted-foreground mt-1">实时查看所有设备的运行状态</p>
      </div>
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-emerald-500"></div>
          <span>在线 5</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-amber-500"></div>
          <span>告警 1</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-zinc-400"></div>
          <span>离线 1</span>
        </div>
        <div class="flex items-center gap-1.5">
          <div class="w-2 h-2 rounded-full bg-zinc-600"></div>
          <span>未知 1</span>
        </div>
      </div>
    </div>

    <!-- 设备卡片网格 -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
      <DeviceCard
        v-for="device in demoDevices"
        :key="device.id"
        :title="device.title"
        :description="device.description"
        :status="device.status"
        :device-id="device.id"
      />
    </div>
  </div>
</template>
