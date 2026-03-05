<script setup lang="ts">
import AppSidebar from '@/components/AppSidebar.vue'
import { RouterView, useRoute } from 'vue-router'
import { computed } from 'vue'

const route = useRoute()
const pageTitle = computed(() => (route.meta.title as string) ?? '')
</script>

<template>
  <div class="flex h-screen w-screen overflow-hidden bg-background text-foreground font-inter">
    <!-- Sidebar -->
    <AppSidebar />

    <!-- Main Area -->
    <div class="flex flex-col flex-1 overflow-hidden">
      <!-- Top Header -->
      <header class="h-14 border-b border-border flex items-center px-6 gap-3 shrink-0 bg-background/80 backdrop-blur-sm">
        <div class="flex items-center gap-2">
          <div class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></div>
          <span class="text-sm font-semibold tracking-tight">{{ pageTitle }}</span>
        </div>
        <div class="ml-auto flex items-center gap-2 text-xs text-muted-foreground">
          <span>设备监控系统</span>
          <span class="text-border">|</span>
          <span>v1.0.0</span>
        </div>
      </header>

      <!-- Page Content -->
      <main class="flex-1 overflow-y-auto p-6">
        <RouterView />
      </main>
    </div>
  </div>
</template>
