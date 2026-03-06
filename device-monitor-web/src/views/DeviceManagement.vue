<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Plus, Pencil, Trash2, Cable, Settings2, Cpu, Network, MoreHorizontal, ChevronRight } from 'lucide-vue-next'

// shadcn-ui components
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Switch } from '@/components/ui/switch'
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

// ==================== Mock 数据 ====================
const integrators = ref<Integrator[]>([
  { id: 1, name: '1号集成设备', ipAddress: '192.168.1.100', port: 502, isEnabled: true, remark: '焊接车间 A' },
  { id: 2, name: '2号集成设备', ipAddress: '192.168.1.101', port: 502, isEnabled: false, remark: '焊接车间 B' },
])

const devices = ref<Device[]>([
  { id: 1, name: '流量计 #1', integratorId: 1, slaveAddress: 1, baudRate: 9600, dataBits: 8, stopBits: 1, parity: 'None', readMode: 'HoldingRegisters', startRegister: 0, registerCount: 4, deviceType: 'FlowMeter', isEnabled: true, remark: '回路A - 冷却液监测' },
  { id: 2, name: '流量计 #2', integratorId: 1, slaveAddress: 2, baudRate: 9600, dataBits: 8, stopBits: 1, parity: 'None', readMode: 'HoldingRegisters', startRegister: 0, registerCount: 4, deviceType: 'FlowMeter', isEnabled: true, remark: '回路B - 生产线供液' },
  { id: 3, name: '流量计 #3', integratorId: 2, slaveAddress: 1, baudRate: 19200, dataBits: 8, stopBits: 1, parity: 'Even', readMode: 'InputRegisters', startRegister: 100, registerCount: 4, deviceType: 'FlowMeter', isEnabled: false, remark: '测试设备' },
])

const tagMappings = ref<DeviceTagMapping[]>([
  { id: 1, deviceId: 1, valueName: 'Temperature', registerOffset: 0, dataType: 'Float', scale: 0.1, plcTagAddress: 'DB100.DBD10', unit: '℃', isEnabled: true },
  { id: 2, deviceId: 1, valueName: 'FlowRate', registerOffset: 2, dataType: 'Float', scale: 0.01, plcTagAddress: 'DB100.DBD14', unit: 'L/min', isEnabled: true },
  { id: 3, deviceId: 2, valueName: 'Temperature', registerOffset: 0, dataType: 'Float', scale: 0.1, plcTagAddress: 'DB100.DBD20', unit: '℃', isEnabled: true },
  { id: 4, deviceId: 2, valueName: 'FlowRate', registerOffset: 2, dataType: 'Float', scale: 0.01, plcTagAddress: 'DB100.DBD24', unit: 'L/min', isEnabled: true },
])

// ==================== 自增 ID ====================
let nextIntId = 3
let nextDevId = 4
let nextTagId = 5

// ==================== Tab 状态 ====================
const activeTab = ref('integrators')

// ==================== 集成设备 Dialog ====================
const showIntDialog = ref(false)
const intDialogMode = ref<'add' | 'edit'>('add')
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

function saveIntegrator() {
  if (intDialogMode.value === 'add') {
    intForm.value.id = nextIntId++
    integrators.value.push({ ...intForm.value })
  } else {
    const idx = integrators.value.findIndex(i => i.id === intForm.value.id)
    if (idx !== -1) integrators.value[idx] = { ...intForm.value }
  }
  showIntDialog.value = false
}

function deleteIntegrator(id: number) {
  // 级联删除：子设备和标签映射
  const devIds = devices.value.filter(d => d.integratorId === id).map(d => d.id)
  tagMappings.value = tagMappings.value.filter(t => !devIds.includes(t.deviceId))
  devices.value = devices.value.filter(d => d.integratorId !== id)
  integrators.value = integrators.value.filter(i => i.id !== id)
}

function toggleIntegrator(item: Integrator) {
  item.isEnabled = !item.isEnabled
}

// ==================== 设备 Dialog ====================
const showDevDialog = ref(false)
const devDialogMode = ref<'add' | 'edit'>('add')
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

function saveDevice() {
  if (devDialogMode.value === 'add') {
    devForm.value.id = nextDevId++
    devices.value.push({ ...devForm.value })
  } else {
    const idx = devices.value.findIndex(d => d.id === devForm.value.id)
    if (idx !== -1) devices.value[idx] = { ...devForm.value }
  }
  showDevDialog.value = false
}

function deleteDevice(id: number) {
  tagMappings.value = tagMappings.value.filter(t => t.deviceId !== id)
  devices.value = devices.value.filter(d => d.id !== id)
}

function toggleDevice(item: Device) {
  item.isEnabled = !item.isEnabled
}

function getIntegratorName(id: number) {
  return integrators.value.find(i => i.id === id)?.name ?? '未知'
}

// ==================== 标签映射 Dialog ====================
const showTagDialog = ref(false)
const tagDialogMode = ref<'add' | 'edit'>('add')
const selectedDeviceIdForTag = ref<number>(0)
const tagForm = ref<DeviceTagMapping>({
  id: 0, deviceId: 0, valueName: '', registerOffset: 0,
  dataType: 'Float', scale: 1.0, plcTagAddress: '', unit: '', isEnabled: true
})

// 展开的设备 (查看标签映射)
const expandedDeviceId = ref<number | null>(null)

function toggleExpandDevice(deviceId: number) {
  expandedDeviceId.value = expandedDeviceId.value === deviceId ? null : deviceId
}

function openAddTag(deviceId: number) {
  tagDialogMode.value = 'add'
  selectedDeviceIdForTag.value = deviceId
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

function saveTag() {
  if (tagDialogMode.value === 'add') {
    tagForm.value.id = nextTagId++
    tagMappings.value.push({ ...tagForm.value })
  } else {
    const idx = tagMappings.value.findIndex(t => t.id === tagForm.value.id)
    if (idx !== -1) tagMappings.value[idx] = { ...tagForm.value }
  }
  showTagDialog.value = false
}

function deleteTag(id: number) {
  tagMappings.value = tagMappings.value.filter(t => t.id !== id)
}

function getDeviceTagMappings(deviceId: number) {
  return tagMappings.value.filter(t => t.deviceId === deviceId)
}

// ==================== 统计 ====================
const stats = computed(() => ({
  intTotal: integrators.value.length,
  intOnline: integrators.value.filter(i => i.isEnabled).length,
  devTotal: devices.value.length,
  devEnabled: devices.value.filter(d => d.isEnabled).length,
  tagTotal: tagMappings.value.length,
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
    <!-- 页面标题 -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">设备管理</h1>
        <p class="text-sm text-muted-foreground mt-1">管理集成设备、Modbus 子设备和 PLC 标签映射</p>
      </div>
      <div class="flex items-center gap-4 text-xs text-muted-foreground">
        <div class="flex items-center gap-1.5">
          <Network class="w-3.5 h-3.5" />
          <span>集成设备 {{ stats.intOnline }}/{{ stats.intTotal }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <Cpu class="w-3.5 h-3.5" />
          <span>子设备 {{ stats.devEnabled }}/{{ stats.devTotal }}</span>
        </div>
        <div class="flex items-center gap-1.5">
          <Cable class="w-3.5 h-3.5" />
          <span>标签映射 {{ stats.tagTotal }}</span>
        </div>
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
          子设备 & 标签
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
                  <TableHead class="w-[50px]">ID</TableHead>
                  <TableHead>名称</TableHead>
                  <TableHead>IP 地址</TableHead>
                  <TableHead class="w-[80px]">端口</TableHead>
                  <TableHead class="w-[100px]">子设备数</TableHead>
                  <TableHead class="w-[80px]">状态</TableHead>
                  <TableHead>备注</TableHead>
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
                  <TableCell>
                    <Switch :checked="item.isEnabled" @update:checked="toggleIntegrator(item)" />
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
                  <TableCell :colspan="8" class="h-24 text-center text-muted-foreground">
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
                  <TableHead class="w-[30px]"></TableHead>
                  <TableHead class="w-[50px]">ID</TableHead>
                  <TableHead>名称</TableHead>
                  <TableHead>所属集成设备</TableHead>
                  <TableHead class="w-[80px]">从站</TableHead>
                  <TableHead class="w-[90px]">波特率</TableHead>
                  <TableHead class="w-[100px]">读取模式</TableHead>
                  <TableHead class="w-[80px]">起始REG</TableHead>
                  <TableHead class="w-[70px]">数量</TableHead>
                  <TableHead class="w-[80px]">状态</TableHead>
                  <TableHead class="w-[70px] text-right">操作</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                <template v-for="dev in devices" :key="dev.id">
                  <TableRow class="cursor-pointer group hover:bg-muted/50" @click="toggleExpandDevice(dev.id)">
                    <TableCell>
                      <ChevronRight class="w-4 h-4 transition-transform text-muted-foreground"
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
                    <TableCell>
                      <Switch :checked="dev.isEnabled" @update:checked="toggleDevice(dev)" @click.stop />
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
                              <TableHead class="h-8 text-xs w-[80px]">偏移</TableHead>
                              <TableHead class="h-8 text-xs w-[80px]">数据类型</TableHead>
                              <TableHead class="h-8 text-xs w-[80px]">缩放</TableHead>
                              <TableHead class="h-8 text-xs">PLC 标签地址</TableHead>
                              <TableHead class="h-8 text-xs w-[60px]">单位</TableHead>
                              <TableHead class="h-8 text-xs w-[60px]">启用</TableHead>
                              <TableHead class="h-8 text-xs w-[80px] text-right">操作</TableHead>
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
                              <TableCell class="py-1.5">
                                <Switch :checked="tag.isEnabled" @update:checked="tag.isEnabled = !tag.isEnabled" class="scale-75 origin-left" />
                              </TableCell>
                              <TableCell class="py-1.5 text-right">
                                <div class="flex gap-1 justify-end">
                                  <Button variant="ghost" size="icon" class="h-6 w-6" @click="openEditTag(tag)">
                                    <Pencil class="w-3 h-3" />
                                  </Button>
                                  <Button variant="ghost" size="icon" class="h-6 w-6 text-destructive" @click="deleteTag(tag.id)">
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
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">启用</Label>
            <Switch :checked="intForm.isEnabled" @update:checked="intForm.isEnabled = $event" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showIntDialog = false">取消</Button>
          <Button @click="saveIntegrator" :disabled="!intForm.name || !intForm.ipAddress">保存</Button>
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
        <div class="grid gap-4 py-4 max-h-[60vh] overflow-y-auto pr-2">
          <!-- 基本信息 -->
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">名称</Label>
            <Input v-model="devForm.name" class="col-span-3" placeholder="如：流量计 #1" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">所属集成设备</Label>
            <Select v-model="devForm.integratorId" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue placeholder="选择集成设备" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="ig in integrators" :key="ig.id" :value="ig.id">
                  {{ ig.name }} ({{ ig.ipAddress }})
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">设备类型</Label>
            <Select v-model="devForm.deviceType" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="dt in deviceTypeOptions" :key="dt" :value="dt">{{ dt }}</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <!-- Modbus 参数 -->
          <div class="col-span-4 mt-2 mb-1">
            <p class="text-xs font-medium text-muted-foreground uppercase tracking-wider flex items-center gap-1.5">
              <Settings2 class="w-3.5 h-3.5" /> Modbus 通讯参数
            </p>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">从站地址</Label>
            <Input v-model.number="devForm.slaveAddress" type="number" class="col-span-3" min="1" max="247" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">波特率</Label>
            <Select v-model="devForm.baudRate" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="br in baudRateOptions" :key="br" :value="br">{{ br }}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">数据位</Label>
            <Select v-model="devForm.dataBits" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="db in dataBitsOptions" :key="db" :value="db">{{ db }}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">停止位</Label>
            <Select v-model="devForm.stopBits" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="sb in stopBitsOptions" :key="sb" :value="sb">{{ sb }}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">校验</Label>
            <Select v-model="devForm.parity" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="p in parityOptions" :key="p" :value="p">{{ p }}</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">读取模式</Label>
            <Select v-model="devForm.readMode" class="col-span-3">
              <SelectTrigger class="col-span-3">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="rm in readModeOptions" :key="rm" :value="rm">
                  {{ rm === 'HoldingRegisters' ? '保持寄存器 (03)' : '输入寄存器 (04)' }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">起始寄存器</Label>
            <Input v-model.number="devForm.startRegister" type="number" class="col-span-3" min="0" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">寄存器数量</Label>
            <Input v-model.number="devForm.registerCount" type="number" class="col-span-3" min="1" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">备注</Label>
            <Input v-model="devForm.remark" class="col-span-3" placeholder="可选" />
          </div>
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">启用</Label>
            <Switch :checked="devForm.isEnabled" @update:checked="devForm.isEnabled = $event" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showDevDialog = false">取消</Button>
          <Button @click="saveDevice" :disabled="!devForm.name || !devForm.integratorId">保存</Button>
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
          <div class="grid grid-cols-4 items-center gap-4">
            <Label class="text-right">启用</Label>
            <Switch :checked="tagForm.isEnabled" @update:checked="tagForm.isEnabled = $event" />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showTagDialog = false">取消</Button>
          <Button @click="saveTag" :disabled="!tagForm.valueName || !tagForm.plcTagAddress">保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
