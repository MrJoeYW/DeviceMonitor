<script setup lang="ts">
import { Settings2 } from 'lucide-vue-next'
</script>

<template>
  <div class="p-6 space-y-6">
    <div>
      <h1 class="text-xl font-semibold tracking-tight">系统设置</h1>
      <p class="text-sm text-muted-foreground mt-1">配置系统参数与连接信息</p>
    </div>

    <div class="flex flex-col items-center justify-center h-64 rounded-lg border border-dashed border-border text-muted-foreground gap-3">
      <Settings2 class="w-10 h-10 opacity-30" />
      <p class="text-sm">系统设置功能开发中</p>
    </div>
  </div>
</template>
