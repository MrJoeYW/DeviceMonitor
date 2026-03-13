<script setup lang="ts">
import { ScrollText } from 'lucide-vue-next'
</script>

<template>
  <div class="p-6 space-y-6">
    <div>
      <h1 class="text-xl font-semibold tracking-tight">日志</h1>
      <p class="text-sm text-muted-foreground mt-1">查看系统运行日志与事件记录</p>
    </div>

    <div class="flex flex-col items-center justify-center h-64 rounded-lg border border-dashed border-border text-muted-foreground gap-3">
      <ScrollText class="w-10 h-10 opacity-30" />
      <p class="text-sm">日志功能开发中</p>
    </div>
  </div>
</template>
