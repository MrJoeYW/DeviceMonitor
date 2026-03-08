<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { Plus, Pencil, Trash2, Cable, Settings2, Cpu, Network, MoreHorizontal, ChevronRight, Loader2 } from 'lucide-vue-next'
import { sendMessage } from '@/api/bridge'

// shadcn-ui components
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Dialog, DialogContent, DialogDescription, DialogFooter,
  DialogHeader, DialogTitle
} from '@/components/ui/dialog'
import {
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue
} from '@/components/ui/select'
import {
  Table, TableBody, TableCell, TableHead, TableHeader, TableRow
} from '@/components/ui/table'
import {
  DropdownMenu, DropdownMenuContent, DropdownMenuItem,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'

// ==================== 类型定义 ====================
interface Integrator {
  id: number
  name: string
  ipAddress: string
  port: number
  isEnabled: boolean
  remark: string
}

interface Device {
  id: number
  name: string
  integratorId: number
  slaveAddress: number
  baudRate: number
  dataBits: number
  stopBits: number
  parity: string
  readMode: string
  startRegister: number
  registerCount: number
  deviceType: string
  isEnabled: boolean
  remark: string
}

interface DeviceTagMapping {
  id: number
  deviceId: number
  valueName: string
  registerOffset: number
  dataType: string
  scale: number
  plcTagAddress: string
  unit: string
  isEnabled: boolean
}

// ==================== 状态 ====================
const integrators = ref<Integrator[]>([])
const devices = ref<Device[]>([])
// 按 deviceId 存储已加载的标签映射（懒加载）
const tagMappingsMap = ref<Record<number, DeviceTagMapping[]>>({})

const loading = ref(false)
const errorMsg = ref('')

function showError(msg: string) {
  errorMsg.value = msg
  setTimeout(() => { errorMsg.value = '' }, 5000)
}

// ==================== 初始化 ====================
async function loadData() {
  loading.value = true
  try {
    const [igs, devs] = await Promise.all([
      sendMessage<Integrator[]>('integrator:getAll'),
      sendMessage<Device[]>('device:getAll'),
    ])
    integrators.value = igs || []
    devices.value = devs || []
    console.log('--- Data Refresh ---')
    console.log('Integrators:', JSON.parse(JSON.stringify(integrators.value)))
    console.log('Devices:', JSON.parse(JSON.stringify(devices.value)))
  } catch (e: any) {
    showError('加载数据失败：' + e.message)
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

// ==================== Tab 状态 ====================
const activeTab = ref('integrators')

// ==================== 集成设备 Dialog ====================
const showIntDialog = ref(false)
const intDialogMode = ref<'add' | 'edit'>('add')
const intSaving = ref(false)
const intForm = ref<Integrator>({ id: 0, name: '', ipAddress: '', port: 502, isEnabled: true, remark: '' })

function openAddIntegrator() {
  intDialogMode.value = 'add'
  intForm.value = { id: 0, name: '', ipAddress: '', port: 502, isEnabled: true, remark: '' }
  showIntDialog.value = true
}

function openEditIntegrator(item: Integrator) {
  intDialogMode.value = 'edit'
  intForm.value = { ...item }
  showIntDialog.value = true
}

async function saveIntegrator() {
  intSaving.value = true
  try {
    if (intDialogMode.value === 'add') {
      const newId = await sendMessage<number>('integrator:add', intForm.value)
      integrators.value.push({ ...intForm.value, id: newId })
    } else {
      await sendMessage<boolean>('integrator:update', intForm.value)
      const idx = integrators.value.findIndex(i => i.id === intForm.value.id)
      if (idx !== -1) integrators.value[idx] = { ...intForm.value }
    }
    showIntDialog.value = false
  } catch (e: any) {
    showError('保存失败：' + e.message)
  } finally {
    intSaving.value = false
  }
}

async function deleteIntegrator(id: number) {
  try {
    await sendMessage<boolean>('integrator:delete', { id })
    // 本地级联清理
    const devIds = devices.value.filter(d => d.integratorId === id).map(d => d.id)
    devIds.forEach(did => { delete tagMappingsMap.value[did] })
    devices.value = devices.value.filter(d => d.integratorId !== id)
    integrators.value = integrators.value.filter(i => i.id !== id)
  } catch (e: any) {
    showError('删除失败：' + e.message)
  }
}

// ==================== 设备 Dialog ====================
const showDevDialog = ref(false)
const devDialogMode = ref<'add' | 'edit'>('add')
const devSaving = ref(false)
const devForm = ref<Device>({
  id: 0, name: '', integratorId: 0, slaveAddress: 1, baudRate: 9600,
  dataBits: 8, stopBits: 1, parity: 'None', readMode: 'HoldingRegisters',
  startRegister: 0, registerCount: 2, deviceType: 'FlowMeter', isEnabled: true, remark: ''
})

function openAddDevice() {
  devDialogMode.value = 'add'
  devForm.value = {
    id: 0, name: '', integratorId: integrators.value[0]?.id ?? 0, slaveAddress: 1, baudRate: 9600,
    dataBits: 8, stopBits: 1, parity: 'None', readMode: 'HoldingRegisters',
    startRegister: 0, registerCount: 2, deviceType: 'FlowMeter', isEnabled: true, remark: ''
  }
  showDevDialog.value = true
}

function openEditDevice(item: Device) {
  devDialogMode.value = 'edit'
  devForm.value = { ...item }
  showDevDialog.value = true
}

async function saveDevice() {
  devSaving.value = true
  try {
    if (devDialogMode.value === 'add') {
      const newId = await sendMessage<number>('device:add', devForm.value)
      devices.value.push({ ...devForm.value, id: newId })
    } else {
      await sendMessage<boolean>('device:update', devForm.value)
      const idx = devices.value.findIndex(d => d.id === devForm.value.id)
      if (idx !== -1) devices.value[idx] = { ...devForm.value }
    }
    showDevDialog.value = false
  } catch (e: any) {
    showError('保存失败：' + e.message)
  } finally {
    devSaving.value = false
  }
}

async function deleteDevice(id: number) {
  try {
    await sendMessage<boolean>('device:delete', { id })
    delete tagMappingsMap.value[id]
    devices.value = devices.value.filter(d => d.id !== id)
  } catch (e: any) {
    showError('删除失败：' + e.message)
  }
}

// --- 开关确认控制 ---
const showConfirmDialog = ref(false)
const pendingDevice = ref<Device | null>(null)
const targetEnabled = ref(false)

function onToggleSwitch(item: Device, value: boolean) {
  console.log('Switch toggled for device:', item.id, 'New value:', value)
  pendingDevice.value = item
  targetEnabled.value = value
  showConfirmDialog.value = true
}

async function confirmToggle() {
  if (!pendingDevice.value) return
  
  const item = pendingDevice.value
  const newVal = targetEnabled.value
  const originalVal = item.isEnabled
  
  console.log('Confirming toggle for device:', item.id, 'Target status:', newVal)
  
  // 先乐观更新 UI
  item.isEnabled = newVal
  showConfirmDialog.value = false
  
  try {
    const res = await sendMessage<boolean>('device:update', { ...item })
    console.log('Update response:', res)
    if (!res) throw new Error('后端返回更新失败')
  } catch (e: any) {
    // 失败则回退
    item.isEnabled = originalVal
    showError('更新状态失败：' + e.message)
    console.error('Update failed:', e)
  } finally {
    pendingDevice.value = null
  }
}

function cancelToggle() {
  showConfirmDialog.value = false
  pendingDevice.value = null
}

function getIntegratorName(id: number) {
  return integrators.value.find(i => i.id === id)?.name ?? '未知'
}

// ==================== 标签映射（懒加载） ====================
const showTagDialog = ref(false)
const tagDialogMode = ref<'add' | 'edit'>('add')
const tagSaving = ref(false)
const tagForm = ref<DeviceTagMapping>({
  id: 0, deviceId: 0, valueName: '', registerOffset: 0,
  dataType: 'Float', scale: 1.0, plcTagAddress: '', unit: '', isEnabled: true
})

// 展开的设备 ID
const expandedDeviceId = ref<number | null>(null)
const loadingTagsFor = ref<number | null>(null)

async function toggleExpandDevice(deviceId: number) {
  if (expandedDeviceId.value === deviceId) {
    expandedDeviceId.value = null
    return
  }
  expandedDeviceId.value = deviceId
  // 若尚未加载该设备的标签，则拉取
  if (!tagMappingsMap.value[deviceId]) {
    loadingTagsFor.value = deviceId
    try {
      const tags = await sendMessage<DeviceTagMapping[]>('tag:getByDeviceId', { deviceId })
      tagMappingsMap.value[deviceId] = tags || []
    } catch (e: any) {
      showError('加载标签映射失败：' + e.message)
    } finally {
      loadingTagsFor.value = null
    }
  }
}

function getDeviceTagMappings(deviceId: number): DeviceTagMapping[] {
  return tagMappingsMap.value[deviceId] ?? []
}

function openAddTag(deviceId: number) {
  tagDialogMode.value = 'add'
  tagForm.value = {
    id: 0, deviceId, valueName: '', registerOffset: 0,
    dataType: 'Float', scale: 1.0, plcTagAddress: '', unit: '', isEnabled: true
  }
  showTagDialog.value = true
}

function openEditTag(tag: DeviceTagMapping) {
  tagDialogMode.value = 'edit'
  tagForm.value = { ...tag }
  showTagDialog.value = true
}

async function saveTag() {
  tagSaving.value = true
  try {
    if (tagDialogMode.value === 'add') {
      const newId = await sendMessage<number>('tag:add', tagForm.value)
      const { deviceId } = tagForm.value
      if (!tagMappingsMap.value[deviceId]) tagMappingsMap.value[deviceId] = []
      tagMappingsMap.value[deviceId].push({ ...tagForm.value, id: newId })
    } else {
      await sendMessage<boolean>('tag:update', tagForm.value)
      const list = tagMappingsMap.value[tagForm.value.deviceId]
      if (list) {
        const idx = list.findIndex(t => t.id === tagForm.value.id)
        if (idx !== -1) list[idx] = { ...tagForm.value }
      }
    }
    showTagDialog.value = false
  } catch (e: any) {
    showError('保存失败：' + e.message)
  } finally {
    tagSaving.value = false
  }
}

async function deleteTag(tag: DeviceTagMapping) {
  try {
    await sendMessage<boolean>('tag:delete', { id: tag.id })
    const list = tagMappingsMap.value[tag.deviceId]
    if (list) {
      const idx = list.findIndex(t => t.id === tag.id)
      if (idx !== -1) list.splice(idx, 1)
    }
  } catch (e: any) {
    showError('删除失败：' + e.message)
  }
}

// ==================== 统计 ====================
const stats = computed(() => ({
  intTotal: integrators.value.length,
  devTotal: devices.value.length,
  devEnabled: devices.value.filter(d => d.isEnabled).length,
  tagTotal: Object.values(tagMappingsMap.value).reduce((s, arr) => s + arr.length, 0),
}))

// ==================== 选项常量 ====================
const baudRateOptions = [9600, 19200, 38400, 57600, 115200]
const parityOptions = ['None', 'Odd', 'Even']
const dataBitsOptions = [7, 8]
const stopBitsOptions = [1, 2]
const readModeOptions = ['HoldingRegisters', 'InputRegisters']
const dataTypeOptions = ['Float', 'Int16', 'UInt16', 'Int32', 'UInt32']
const deviceTypeOptions = ['FlowMeter']
</script>

<template>
  <div class="space-y-6">
    <!-- 错误提示条 -->
    <div
      v-if="errorMsg"
      class="flex items-center gap-2 rounded-md border border-destructive/50 bg-destructive/10 px-4 py-2 text-sm text-destructive fixed top-4 right-4 z-50 shadow-lg animate-in fade-in slide-in-from-top-2"
    >
      <span>{{ errorMsg }}</span>
    </div>

    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">设备管理</h1>
        <p class="text-sm text-muted-foreground mt-1">管理集成设备、Modbus 子设备和 PLC 标签映射</p>
      </div>
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <div v-if="loading" class="flex items-center gap-1.5 text-muted-foreground">
          <Loader2 class="w-3.5 h-3.5 animate-spin" />
          <span>加载中…</span>
        </div>
        <template v-else>
          <div class="flex items-center gap-1.5">
            <Network class="w-3.5 h-3.5" />
            <span>集成设备 {{ stats.intTotal }}</span>
          </div>
          <div class="flex items-center gap-1.5">
            <Cpu class="w-3.5 h-3.5" />
            <span>子设备 {{ stats.devEnabled }}/{{ stats.devTotal }}</span>
          </div>
          <div class="flex items-center gap-1.5">
            <Cable class="w-3.5 h-3.5" />
            <span>标签映射 {{ stats.tagTotal }}</span>
          </div>
        </template>
      </div>
    </div>

    <!-- 选项卡 -->
    <Tabs v-model="activeTab" class="w-full">
      <TabsList class="grid w-fit grid-cols-2">
        <TabsTrigger value="integrators" class="gap-1.5">
          <Network class="w-4 h-4" />
          集成设备
        </TabsTrigger>
        <TabsTrigger value="devices" class="gap-1.5">
          <Cpu class="w-4 h-4" />
          子设备 &amp; 标签
        </TabsTrigger>
      </TabsList>

      <!-- ==================== Tab 1: 集成设备 ==================== -->
      <TabsContent value="integrators">
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-4">
            <div>
              <CardTitle class="text-base">集成设备列表</CardTitle>
              <CardDescription>通过 TCP 连接的 485 网关设备</CardDescription>
            </div>
            <Button size="sm" @click="openAddIntegrator" class="gap-1">
              <Plus class="w-4 h-4" /> 新建
            </Button>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead class="w-[60px]">ID</TableHead>
                  <TableHead class="min-w-[150px]">名称</TableHead>
                  <TableHead class="w-[140px]">IP 地址</TableHead>
                  <TableHead class="w-[100px]">端口</TableHead>
                  <TableHead class="w-[100px]">子设备数</TableHead>
                  <TableHead class="min-w-[150px]">备注</TableHead>
                  <TableHead class="w-[70px] text-right">操作</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <TableRow v-for="item in integrators" :key="item.id" class="group">
                  <TableCell class="font-mono text-xs text-muted-foreground">{{ item.id }}</TableCell>
                  <TableCell class="font-medium">{{ item.name }}</TableCell>
                  <TableCell class="font-mono text-sm">{{ item.ipAddress }}</TableCell>
                  <TableCell class="font-mono text-sm">{{ item.port }}</TableCell>
                  <TableCell>
                    <Badge variant="secondary" class="font-mono">
                      {{ devices.filter(d => d.integratorId === item.id).length }}
                    </Badge>
                  </TableCell>
                  <TableCell class="text-sm text-muted-foreground max-w-[200px] truncate">{{ item.remark }}</TableCell>
                  <TableCell class="text-right">
                    <DropdownMenu>
                      <DropdownMenuTrigger as-child>
                        <Button variant="ghost" size="icon" class="h-8 w-8">
                          <MoreHorizontal class="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem @click="openEditIntegrator(item)">
                          <Pencil class="w-4 h-4 mr-2" /> 编辑
                        </DropdownMenuItem>
                        <DropdownMenuItem class="text-destructive" @click="deleteIntegrator(item.id)">
                          <Trash2 class="w-4 h-4 mr-2" /> 删除
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
                <TableRow v-if="integrators.length === 0">
                  <TableCell :colspan="7" class="h-24 text-center text-muted-foreground">
                    暂无集成设备，点击"新建"添加
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabsContent>

      <!-- ==================== Tab 2: 子设备 & 标签 ==================== -->
      <TabsContent value="devices">
        <Card>
          <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-4">
            <div>
              <CardTitle class="text-base">Modbus 子设备</CardTitle>
              <CardDescription>点击行可展开查看 PLC 标签映射</CardDescription>
            </div>
            <Button size="sm" @click="openAddDevice" class="gap-1" :disabled="integrators.length === 0">
              <Plus class="w-4 h-4" /> 新建
            </Button>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead class="w-[40px]"></TableHead>
                  <TableHead class="w-[60px]">ID</TableHead>
                  <TableHead class="min-w-[120px]">名称</TableHead>
                  <TableHead class="min-w-[140px]">所属集成设备</TableHead>
                  <TableHead class="w-[70px]">从站</TableHead>
                  <TableHead class="w-[90px]">波特率</TableHead>
                  <TableHead class="w-[100px]">读取模式</TableHead>
                  <TableHead class="w-[90px]">起始寄存器</TableHead>
                  <TableHead class="w-[70px]">数量</TableHead>
                  <TableHead class="w-[80px]">启用</TableHead>
                  <TableHead class="w-[70px] text-right">操作</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <template v-for="dev in devices" :key="dev.id">
                  <TableRow class="cursor-pointer group hover:bg-muted/50" @click="toggleExpandDevice(dev.id)">
                    <TableCell>
                      <Loader2 v-if="loadingTagsFor === dev.id" class="w-4 h-4 animate-spin text-muted-foreground" />
                      <ChevronRight v-else class="w-4 h-4 transition-transform text-muted-foreground"
                        :class="{ 'rotate-90': expandedDeviceId === dev.id }" />
                    </TableCell>
                    <TableCell class="font-mono text-xs text-muted-foreground">{{ dev.id }}</TableCell>
                    <TableCell class="font-medium">{{ dev.name }}</TableCell>
                    <TableCell>
                      <Badge variant="outline">{{ getIntegratorName(dev.integratorId) }}</Badge>
                    </TableCell>
                    <TableCell class="font-mono text-sm">{{ dev.slaveAddress }}</TableCell>
                    <TableCell class="font-mono text-sm">{{ dev.baudRate }}</TableCell>
                    <TableCell>
                      <Badge variant="secondary" class="text-xs">
                        {{ dev.readMode === 'HoldingRegisters' ? '保持' : '输入' }}
                      </Badge>
                    </TableCell>
                    <TableCell class="font-mono text-sm">{{ dev.startRegister }}</TableCell>
                    <TableCell class="font-mono text-sm">{{ dev.registerCount }}</TableCell>
                    <TableCell @click.stop>
                      <!-- 用原生 button 模拟开关，绕开 reka-ui 受控模式事件失效问题 -->
                      <button
                        type="button"
                        role="switch"
                        :aria-checked="dev.isEnabled"
                        @click.stop="onToggleSwitch(dev, !dev.isEnabled)"
                        class="inline-flex h-[1.15rem] w-8 shrink-0 items-center rounded-full border border-transparent shadow-xs transition-all outline-none focus-visible:ring-2 focus-visible:ring-ring"
                        :class="dev.isEnabled ? 'bg-primary' : 'bg-input'"
                      >
                        <span class="pointer-events-none block size-4 rounded-full bg-background transition-transform"
                          :class="dev.isEnabled ? 'translate-x-[calc(100%-2px)]' : 'translate-x-0'"
                        />
                      </button>
                    </TableCell>
                    <TableCell class="text-right" @click.stop>
                      <DropdownMenu>
                        <DropdownMenuTrigger as-child>
                          <Button variant="ghost" size="icon" class="h-8 w-8">
                            <MoreHorizontal class="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem @click="openEditDevice(dev)">
                            <Pencil class="w-4 h-4 mr-2" /> 编辑
                          </DropdownMenuItem>
                          <DropdownMenuItem @click="openAddTag(dev.id)">
                            <Cable class="w-4 h-4 mr-2" /> 添加标签映射
                          </DropdownMenuItem>
                          <DropdownMenuItem class="text-destructive" @click="deleteDevice(dev.id)">
                            <Trash2 class="w-4 h-4 mr-2" /> 删除
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>

                  <!-- 展开区域: 标签映射子表 -->
                  <TableRow v-if="expandedDeviceId === dev.id" class="bg-muted/30 hover:bg-muted/30">
                    <TableCell :colspan="11" class="p-0">
                      <div class="px-10 py-4 space-y-3">
                        <div class="flex items-center justify-between">
                          <div class="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                            <Cable class="w-4 h-4" />
                            PLC 标签映射
                            <Badge variant="secondary" class="text-xs">{{ getDeviceTagMappings(dev.id).length }}</Badge>
                          </div>
                          <Button variant="outline" size="sm" @click="openAddTag(dev.id)" class="gap-1 h-7 text-xs">
                            <Plus class="w-3 h-3" />添加
                          </Button>
                        </div>
                        <Table v-if="getDeviceTagMappings(dev.id).length > 0">
                          <TableHeader>
                            <TableRow>
                              <TableHead class="h-8 text-xs">值名称</TableHead>
                              <TableHead class="h-8 text-xs w-[100px]">偏移</TableHead>
                              <TableHead class="h-8 text-xs w-[100px]">数据类型</TableHead>
                              <TableHead class="h-8 text-xs w-[100px]">缩放</TableHead>
                              <TableHead class="h-8 text-xs">PLC 标签地址</TableHead>
                              <TableHead class="h-8 text-xs w-[100px]">单位</TableHead>
                              <TableHead class="h-8 text-xs w-[100px] text-right">操作</TableHead>
                            </TableRow>
                          </TableHeader>
                          <TableBody>
                            <TableRow v-for="tag in getDeviceTagMappings(dev.id)" :key="tag.id">
                              <TableCell class="py-1.5 font-medium text-sm">{{ tag.valueName }}</TableCell>
                              <TableCell class="py-1.5 font-mono text-xs">{{ tag.registerOffset }}</TableCell>
                              <TableCell class="py-1.5"><Badge variant="outline" class="text-xs">{{ tag.dataType }}</Badge></TableCell>
                              <TableCell class="py-1.5 font-mono text-xs">{{ tag.scale }}</TableCell>
                              <TableCell class="py-1.5 font-mono text-sm text-blue-500">{{ tag.plcTagAddress }}</TableCell>
                              <TableCell class="py-1.5 text-sm text-muted-foreground">{{ tag.unit }}</TableCell>
                              <TableCell class="py-1.5 text-right">
                                <div class="flex gap-1 justify-end">
                                  <Button variant="ghost" size="icon" class="h-6 w-6" @click="openEditTag(tag)">
                                    <Pencil class="w-3 h-3" />
                                  </Button>
                                  <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="deleteTag(tag)">
                                    <Trash2 class="w-3 h-3" />
                                  </Button>
                                </div>
                              </TableCell>
                            </TableRow>
                          </TableBody>
                        </Table>
                        <div v-else class="text-center text-sm text-muted-foreground py-4 rounded-md border border-dashed">
                          暂无标签映射，点击"添加"配置
                        </div>

                        <!-- 设备详情附加信息 -->
                        <div class="flex gap-4 text-xs text-muted-foreground pt-1">
                          <span>校验: <span class="font-mono">{{ dev.parity }}</span></span>
                          <span>数据位: <span class="font-mono">{{ dev.dataBits }}</span></span>
                          <span>停止位: <span class="font-mono">{{ dev.stopBits }}</span></span>
                          <span v-if="dev.remark">备注: {{ dev.remark }}</span>
                        </div>
                      </div>
                    </TableCell>
                  </TableRow>
                </template>

                <TableRow v-if="devices.length === 0">
                  <TableCell :colspan="11" class="h-24 text-center text-muted-foreground">
                    暂无子设备，请先创建集成设备后再添加
                  </TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </TabsContent>
    </Tabs>

    <!-- ==================== 集成设备 Dialog ==================== -->
    <Dialog v-model:open="showIntDialog">
      <DialogContent class="sm:max-w-[480px]">
        <DialogHeader>
          <DialogTitle>{{ intDialogMode === 'add' ? '新建集成设备' : '编辑集成设备' }}</DialogTitle>
          <DialogDescription>TCP 网关设备，通过 485 接口连接多个子设备</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">名称</Label>
            <Input v-model="intForm.name" class="col-span-3" placeholder="如：1号集成设备" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">IP 地址</Label>
            <Input v-model="intForm.ipAddress" class="col-span-3" placeholder="192.168.1.100" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">端口</Label>
            <Input v-model.number="intForm.port" type="number" class="col-span-3" placeholder="502" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">备注</Label>
            <Input v-model="intForm.remark" class="col-span-3" placeholder="可选" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showIntDialog = false">取消</Button>
          <Button @click="saveIntegrator" :disabled="!intForm.name || !intForm.ipAddress || intSaving">
            <Loader2 v-if="intSaving" class="w-4 h-4 mr-2 animate-spin" />
            保存
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- ==================== 设备 Dialog ==================== -->
    <Dialog v-model:open="showDevDialog">
      <DialogContent class="sm:max-w-[560px]">
        <DialogHeader>
          <DialogTitle>{{ devDialogMode === 'add' ? '新建子设备' : '编辑子设备' }}</DialogTitle>
          <DialogDescription>配置 Modbus RTU 通讯参数</DialogDescription>
        </DialogHeader>
        <div class="grid gap-6 py-4 max-h-[70vh] overflow-y-auto pr-2 px-1">
          <!-- 基本信息 -->
          <div class="space-y-4">
            <h4 class="text-sm font-medium leading-none flex items-center gap-2">
              <Cpu class="w-4 h-4 text-blue-500" /> 基本信息
            </h4>
            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-2">
                <Label for="name">设备名称</Label>
                <Input id="name" v-model="devForm.name" placeholder="如：流量计 #1" />
              </div>
              <div class="space-y-2">
                <Label for="integrator">所属集成设备</Label>
                <Select v-model="devForm.integratorId">
                  <SelectTrigger id="integrator">
                    <SelectValue placeholder="选择控制器" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="ig in integrators" :key="ig.id" :value="ig.id">
                      {{ ig.name }}
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="deviceType">设备类型</Label>
                <Select v-model="devForm.deviceType">
                  <SelectTrigger id="deviceType">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="dt in deviceTypeOptions" :key="dt" :value="dt">{{ dt }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="remark">备注</Label>
                <Input id="remark" v-model="devForm.remark" placeholder="可选" />
              </div>
            </div>
          </div>

          <!-- Modbus 参数 -->
          <div class="space-y-4">
            <h4 class="text-sm font-medium leading-none flex items-center gap-2">
              <Settings2 class="w-4 h-4 text-blue-500" /> Modbus 通讯参数
            </h4>
            <div class="grid grid-cols-2 gap-4 p-4 rounded-lg bg-muted/30 border border-muted">
              <div class="space-y-2">
                <Label for="slaveAddress">从站地址 (1-247)</Label>
                <Input id="slaveAddress" v-model.number="devForm.slaveAddress" type="number" min="1" max="247" />
              </div>
              <div class="space-y-2">
                <Label for="baudRate">波特率 (bps)</Label>
                <Select v-model="devForm.baudRate">
                  <SelectTrigger id="baudRate">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="br in baudRateOptions" :key="br" :value="br">{{ br }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="dataBits">数据位</Label>
                <Select v-model="devForm.dataBits">
                  <SelectTrigger id="dataBits">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="db in dataBitsOptions" :key="db" :value="db">{{ db }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="stopBits">停止位</Label>
                <Select v-model="devForm.stopBits">
                  <SelectTrigger id="stopBits">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="sb in stopBitsOptions" :key="sb" :value="sb">{{ sb }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="parity">校验位</Label>
                <Select v-model="devForm.parity">
                  <SelectTrigger id="parity">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="p in parityOptions" :key="p" :value="p">{{ p }}</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="readMode">读取模式</Label>
                <Select v-model="devForm.readMode">
                  <SelectTrigger id="readMode">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem v-for="rm in readModeOptions" :key="rm" :value="rm">
                      {{ rm === 'HoldingRegisters' ? '保持寄存器 (03)' : '输入寄存器 (04)' }}
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div class="space-y-2">
                <Label for="startRegister">起始寄存器地址</Label>
                <Input id="startRegister" v-model.number="devForm.startRegister" type="number" min="0" />
              </div>
              <div class="space-y-2">
                <Label for="registerCount">寄存器读取数量</Label>
                <Input id="registerCount" v-model.number="devForm.registerCount" type="number" min="1" />
              </div>
            </div>
          </div>

          <div class="flex items-center gap-2 px-1">
            <!-- 原生 checkbox，绝对可靠 -->
            <button
              type="button"
              role="switch"
              :aria-checked="devForm.isEnabled"
              @click="devForm.isEnabled = !devForm.isEnabled"
              class="inline-flex h-[1.15rem] w-8 shrink-0 items-center rounded-full border border-transparent shadow-xs transition-all outline-none focus-visible:ring-2 focus-visible:ring-ring"
              :class="devForm.isEnabled ? 'bg-primary' : 'bg-input'"
            >
              <span class="pointer-events-none block size-4 rounded-full bg-background transition-transform"
                :class="devForm.isEnabled ? 'translate-x-[calc(100%-2px)]' : 'translate-x-0'"
              />
            </button>
            <label class="cursor-pointer text-sm" @click="devForm.isEnabled = !devForm.isEnabled">启用该设备</label>
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showDevDialog = false">取消</Button>
          <Button @click="saveDevice" :disabled="!devForm.name || !devForm.integratorId || devSaving">
            <Loader2 v-if="devSaving" class="w-4 h-4 mr-2 animate-spin" />
            保存
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- ==================== 标签映射 Dialog ==================== -->
    <Dialog v-model:open="showTagDialog">
      <DialogContent class="sm:max-w-[480px]">
        <DialogHeader>
          <DialogTitle>{{ tagDialogMode === 'add' ? '添加标签映射' : '编辑标签映射' }}</DialogTitle>
          <DialogDescription>配置设备读取值到 PLC 标签的写入映射</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">值名称</Label>
            <Input v-model="tagForm.valueName" class="col-span-3" placeholder="如：Temperature" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">寄存器偏移</Label>
            <Input v-model.number="tagForm.registerOffset" type="number" class="col-span-3" min="0" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">数据类型</Label>
            <Select v-model="tagForm.dataType" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="dt in dataTypeOptions" :key="dt" :value="dt">{{ dt }}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">缩放系数</Label>
            <Input v-model.number="tagForm.scale" type="number" step="0.01" class="col-span-3" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">PLC 标签地址</Label>
            <Input v-model="tagForm.plcTagAddress" class="col-span-3" placeholder="如：DB100.DBD10" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">单位</Label>
            <Input v-model="tagForm.unit" class="col-span-3" placeholder="如：℃, L/min" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showTagDialog = false">取消</Button>
          <Button @click="saveTag" :disabled="!tagForm.valueName || !tagForm.plcTagAddress || tagSaving">
            <Loader2 v-if="tagSaving" class="w-4 h-4 mr-2 animate-spin" />
            保存
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <!-- ==================== 状态确认 Dialog ==================== -->
    <AlertDialog :open="showConfirmDialog" @update:open="showConfirmDialog = $event">
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>确认操作</AlertDialogTitle>
          <AlertDialogDescription>
            确定要 {{ targetEnabled ? '启用' : '禁用' }} 设备「{{ pendingDevice?.name }}」吗？
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel @click="cancelToggle">取消</AlertDialogCancel>
          <AlertDialogAction @click="confirmToggle">确认</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  </div>
</template>
