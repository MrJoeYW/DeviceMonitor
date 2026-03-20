<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { Cable, ChevronRight, Cpu, Loader2, Network, Pencil, Plus, RefreshCw, Settings2, Trash2 } from 'lucide-vue-next'
import { sendMessage } from '@/api/bridge'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'

interface Integrator { id:number; name:string; ipAddress:string; port:number; isEnabled:boolean }
interface Device { id:number; integratorId:number; name:string; deviceType:string; deviceModel:string; templateKey:string; slaveAddress:number; pollIntervalMs:number; isOnline:boolean; isEnabled:boolean }
interface DeviceTemplate { key:string; deviceType:string; deviceModel:string; description:string; readGroupCount:number; pointCount:number }
interface DeviceReadGroup { id:number; deviceId:number; name:string; functionCode:number; startRegister:number; registerCount:number; sortOrder:number; isEnabled:boolean }
interface DevicePoint { id:number; deviceId:number; readGroupId:number; pointKey:string; displayName:string; registerAddress:number; registerLength:number; dataType:string; scale:number; unit:string; plcAddress:string; notes:string; sortOrder:number; isEnabled:boolean }

const loading = ref(false)
const errorMsg = ref('')
const expandedDeviceId = ref<number | null>(null)
const integrators = ref<Integrator[]>([])
const devices = ref<Device[]>([])
const templates = ref<DeviceTemplate[]>([])
const readGroupsMap = ref<Record<number, DeviceReadGroup[]>>({})
const pointsMap = ref<Record<number, DevicePoint[]>>({})

const showIntegratorDialog = ref(false)
const integratorDialogMode = ref<'add' | 'edit'>('add')
const integratorSaving = ref(false)
const integratorForm = ref<Integrator>({ id: 0, name: '', ipAddress: '', port: 502, isEnabled: true })

const showDeviceDialog = ref(false)
const deviceDialogMode = ref<'add' | 'edit'>('add')
const deviceSaving = ref(false)
const deviceForm = ref<Device>({ id: 0, integratorId: 0, name: '', deviceType: 'FlowMeter', deviceModel: '', templateKey: '', slaveAddress: 1, pollIntervalMs: 1000, isOnline: false, isEnabled: true })

const showGroupDialog = ref(false)
const groupDialogMode = ref<'add' | 'edit'>('add')
const groupSaving = ref(false)
const groupForm = ref<DeviceReadGroup>({ id: 0, deviceId: 0, name: '', functionCode: 3, startRegister: 0, registerCount: 1, sortOrder: 1, isEnabled: true })

const showPointDialog = ref(false)
const pointDialogMode = ref<'add' | 'edit'>('add')
const pointSaving = ref(false)
const pointForm = ref<DevicePoint>({ id: 0, deviceId: 0, readGroupId: 0, pointKey: '', displayName: '', registerAddress: 0, registerLength: 1, dataType: 'UInt16', scale: 1, unit: '', plcAddress: '', notes: '', sortOrder: 1, isEnabled: true })

const dataTypeOptions = ['Int16', 'UInt16', 'Int32', 'UInt32', 'Float32']
const functionCodeOptions = [{ value: 3, label: 'FC03 保持寄存器' }, { value: 4, label: 'FC04 输入寄存器' }]
const deviceTypeOptions = computed(() => Array.from(new Set(templates.value.map(item => item.deviceType))))
const templateOptions = computed(() => !deviceForm.value.deviceType ? templates.value : templates.value.filter(item => item.deviceType === deviceForm.value.deviceType))
const stats = computed(() => ({
  integratorCount: integrators.value.length,
  deviceCount: devices.value.length,
  enabledDeviceCount: devices.value.filter(item => item.isEnabled).length,
  groupCount: Object.values(readGroupsMap.value).reduce((sum, list) => sum + list.length, 0),
  pointCount: Object.values(pointsMap.value).reduce((sum, list) => sum + list.length, 0),
}))

function showError(message: string) {
  errorMsg.value = message
  window.setTimeout(() => { if (errorMsg.value === message) errorMsg.value = '' }, 5000)
}

function getIntegratorName(id: number) { return integrators.value.find(item => item.id === id)?.name ?? '未分配' }
function getGroups(deviceId: number) { return readGroupsMap.value[deviceId] ?? [] }
function getPoints(deviceId: number) { return pointsMap.value[deviceId] ?? [] }
function getPointsByGroup(deviceId: number, readGroupId: number) { return getPoints(deviceId).filter(item => item.readGroupId === readGroupId) }
function getTemplateText(device: Device) { const t = templates.value.find(item => item.key === device.templateKey); return t ? `${t.deviceModel} · ${t.pointCount} 点` : (device.deviceModel || device.deviceType) }
function toggleExpandDevice(deviceId: number) { expandedDeviceId.value = expandedDeviceId.value === deviceId ? null : deviceId }

async function loadDeviceDetails(deviceId: number) {
  const [groups, points] = await Promise.all([
    sendMessage<DeviceReadGroup[]>('readGroup:getByDeviceId', { deviceId }),
    sendMessage<DevicePoint[]>('point:getByDeviceId', { deviceId }),
  ])
  readGroupsMap.value[deviceId] = groups || []
  pointsMap.value[deviceId] = points || []
}

async function loadData() {
  loading.value = true
  try {
    const [igs, devs, tpls] = await Promise.all([
      sendMessage<Integrator[]>('integrator:getAll'),
      sendMessage<Device[]>('device:getAll'),
      sendMessage<DeviceTemplate[]>('device:getTemplates'),
    ])
    integrators.value = igs || []
    devices.value = devs || []
    templates.value = tpls || []
    await Promise.all(devices.value.map(device => loadDeviceDetails(device.id)))
  } catch (e: any) {
    showError(`加载数据失败: ${e.message}`)
  } finally {
    loading.value = false
  }
}

onMounted(loadData)

function openAddIntegrator() {
  integratorDialogMode.value = 'add'
  integratorForm.value = { id: 0, name: '', ipAddress: '', port: 502, isEnabled: true }
  showIntegratorDialog.value = true
}
function openEditIntegrator(item: Integrator) { integratorDialogMode.value = 'edit'; integratorForm.value = { ...item }; showIntegratorDialog.value = true }
async function saveIntegrator() {
  integratorSaving.value = true
  try {
    if (integratorDialogMode.value === 'add') {
      const id = await sendMessage<number>('integrator:add', integratorForm.value)
      integrators.value.push({ ...integratorForm.value, id })
    } else {
      await sendMessage<boolean>('integrator:update', integratorForm.value)
      const idx = integrators.value.findIndex(item => item.id === integratorForm.value.id)
      if (idx >= 0) integrators.value[idx] = { ...integratorForm.value }
    }
    showIntegratorDialog.value = false
  } catch (e: any) { showError(`保存网关失败: ${e.message}`) } finally { integratorSaving.value = false }
}
async function deleteIntegrator(id: number) {
  try {
    await sendMessage<boolean>('integrator:delete', { id })
    const ids = devices.value.filter(item => item.integratorId === id).map(item => item.id)
    ids.forEach(deviceId => { delete readGroupsMap.value[deviceId]; delete pointsMap.value[deviceId] })
    devices.value = devices.value.filter(item => item.integratorId !== id)
    integrators.value = integrators.value.filter(item => item.id !== id)
  } catch (e: any) { showError(`删除网关失败: ${e.message}`) }
}

function applyTemplate(templateKey: string) {
  const t = templates.value.find(item => item.key === templateKey)
  if (!t) return
  deviceForm.value.templateKey = t.key
  deviceForm.value.deviceType = t.deviceType
  deviceForm.value.deviceModel = t.deviceModel
}
function openAddDevice() {
  const t = templates.value[0]
  deviceDialogMode.value = 'add'
  deviceForm.value = { id: 0, integratorId: integrators.value[0]?.id ?? 0, name: '', deviceType: t?.deviceType ?? 'FlowMeter', deviceModel: t?.deviceModel ?? '', templateKey: t?.key ?? '', slaveAddress: 1, pollIntervalMs: 1000, isOnline: false, isEnabled: true }
  showDeviceDialog.value = true
}
function openEditDevice(item: Device) { deviceDialogMode.value = 'edit'; deviceForm.value = { ...item }; showDeviceDialog.value = true }
async function saveDevice() {
  deviceSaving.value = true
  try {
    if (deviceDialogMode.value === 'add') {
      const id = await sendMessage<number>('device:add', deviceForm.value)
      devices.value.push({ ...deviceForm.value, id })
      await loadDeviceDetails(id)
    } else {
      await sendMessage<boolean>('device:update', deviceForm.value)
      const idx = devices.value.findIndex(item => item.id === deviceForm.value.id)
      if (idx >= 0) devices.value[idx] = { ...deviceForm.value }
      await loadDeviceDetails(deviceForm.value.id)
    }
    showDeviceDialog.value = false
  } catch (e: any) { showError(`保存设备失败: ${e.message}`) } finally { deviceSaving.value = false }
}
async function deleteDevice(id: number) {
  try {
    await sendMessage<boolean>('device:delete', { id })
    devices.value = devices.value.filter(item => item.id !== id)
    delete readGroupsMap.value[id]
    delete pointsMap.value[id]
    if (expandedDeviceId.value === id) expandedDeviceId.value = null
  } catch (e: any) { showError(`删除设备失败: ${e.message}`) }
}
async function rebuildTemplate(device: Device) {
  try { await sendMessage<boolean>('device:rebuildTemplate', { id: device.id }); await loadDeviceDetails(device.id); expandedDeviceId.value = device.id }
  catch (e: any) { showError(`按模板重建失败: ${e.message}`) }
}
function toggleDeviceEnabled(device: Device) {
  const original = device.isEnabled
  device.isEnabled = !device.isEnabled
  sendMessage<boolean>('device:update', { ...device }).catch((e: any) => { device.isEnabled = original; showError(`更新设备状态失败: ${e.message}`) })
}

function openAddGroup(deviceId: number) { groupDialogMode.value = 'add'; groupForm.value = { id: 0, deviceId, name: '', functionCode: 3, startRegister: 0, registerCount: 1, sortOrder: getGroups(deviceId).length + 1, isEnabled: true }; showGroupDialog.value = true }
function openEditGroup(item: DeviceReadGroup) { groupDialogMode.value = 'edit'; groupForm.value = { ...item }; showGroupDialog.value = true }
async function saveGroup() {
  groupSaving.value = true
  try {
    const deviceId = groupForm.value.deviceId
    if (groupDialogMode.value === 'add') {
      const id = await sendMessage<number>('readGroup:add', groupForm.value)
      getGroups(deviceId).push({ ...groupForm.value, id })
    } else {
      await sendMessage<boolean>('readGroup:update', groupForm.value)
      const idx = getGroups(deviceId).findIndex(item => item.id === groupForm.value.id)
      if (idx >= 0) getGroups(deviceId)[idx] = { ...groupForm.value }
    }
    showGroupDialog.value = false
  } catch (e: any) { showError(`保存采集块失败: ${e.message}`) } finally { groupSaving.value = false }
}
async function deleteGroup(item: DeviceReadGroup) {
  try {
    await sendMessage<boolean>('readGroup:delete', { id: item.id })
    readGroupsMap.value[item.deviceId] = getGroups(item.deviceId).filter(group => group.id !== item.id)
    pointsMap.value[item.deviceId] = getPoints(item.deviceId).filter(point => point.readGroupId !== item.id)
  } catch (e: any) { showError(`删除采集块失败: ${e.message}`) }
}

function openAddPoint(deviceId: number, readGroupId?: number) { pointDialogMode.value = 'add'; pointForm.value = { id: 0, deviceId, readGroupId: readGroupId ?? getGroups(deviceId)[0]?.id ?? 0, pointKey: '', displayName: '', registerAddress: 0, registerLength: 1, dataType: 'UInt16', scale: 1, unit: '', plcAddress: '', notes: '', sortOrder: getPoints(deviceId).length + 1, isEnabled: true }; showPointDialog.value = true }
function openEditPoint(item: DevicePoint) { pointDialogMode.value = 'edit'; pointForm.value = { ...item }; showPointDialog.value = true }
async function savePoint() {
  pointSaving.value = true
  try {
    const deviceId = pointForm.value.deviceId
    if (pointDialogMode.value === 'add') {
      const id = await sendMessage<number>('point:add', pointForm.value)
      getPoints(deviceId).push({ ...pointForm.value, id })
    } else {
      await sendMessage<boolean>('point:update', pointForm.value)
      const idx = getPoints(deviceId).findIndex(item => item.id === pointForm.value.id)
      if (idx >= 0) getPoints(deviceId)[idx] = { ...pointForm.value }
    }
    showPointDialog.value = false
  } catch (e: any) { showError(`保存测点失败: ${e.message}`) } finally { pointSaving.value = false }
}
async function deletePoint(item: DevicePoint) {
  try { await sendMessage<boolean>('point:delete', { id: item.id }); pointsMap.value[item.deviceId] = getPoints(item.deviceId).filter(point => point.id !== item.id) }
  catch (e: any) { showError(`删除测点失败: ${e.message}`) }
}
</script>

<template>
  <div class="p-6 space-y-6">
    <div v-if="errorMsg" class="fixed right-4 top-4 z-50 rounded-md border border-destructive/40 bg-destructive/10 px-4 py-2 text-sm text-destructive shadow-lg">{{ errorMsg }}</div>
    <div class="flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 class="text-xl font-semibold tracking-tight">设备配置重构视图</h1>
        <p class="mt-1 text-sm text-muted-foreground">现在按“网关 → 设备 → 采集块 → 测点/PLC 映射”管理，替代旧的三表结构。</p>
      </div>
      <div class="flex flex-wrap items-center gap-3 text-xs text-muted-foreground">
        <div class="flex items-center gap-1.5"><Network class="h-3.5 w-3.5" /><span>网关 {{ stats.integratorCount }}</span></div>
        <div class="flex items-center gap-1.5"><Cpu class="h-3.5 w-3.5" /><span>设备 {{ stats.enabledDeviceCount }}/{{ stats.deviceCount }}</span></div>
        <div class="flex items-center gap-1.5"><Settings2 class="h-3.5 w-3.5" /><span>采集块 {{ stats.groupCount }}</span></div>
        <div class="flex items-center gap-1.5"><Cable class="h-3.5 w-3.5" /><span>测点 {{ stats.pointCount }}</span></div>
        <Button variant="outline" size="sm" class="gap-1" @click="loadData"><RefreshCw class="h-3.5 w-3.5" />刷新</Button>
      </div>
    </div>

    <div v-if="loading" class="flex items-center justify-center gap-3 py-16 text-muted-foreground">
      <Loader2 class="h-5 w-5 animate-spin" />
      <span>正在加载配置...</span>
    </div>

    <div v-else class="grid gap-6 xl:grid-cols-[0.95fr_1.45fr]">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0">
          <div>
            <CardTitle>TCP 网关</CardTitle>
            <CardDescription>网关只负责通信连接，PLC 地址映射下沉到测点层。</CardDescription>
          </div>
          <Button size="sm" class="gap-1" @click="openAddIntegrator"><Plus class="h-4 w-4" />新建</Button>
        </CardHeader>
        <CardContent class="space-y-3">
          <div v-for="item in integrators" :key="item.id" class="rounded-lg border px-4 py-3">
            <div class="flex items-start justify-between gap-3">
              <div>
                <div class="flex items-center gap-2">
                  <span class="font-medium">{{ item.name }}</span>
                  <Badge :variant="item.isEnabled ? 'default' : 'outline'">{{ item.isEnabled ? '启用' : '停用' }}</Badge>
                </div>
                <div class="mt-1 text-xs text-muted-foreground">{{ item.ipAddress }}:{{ item.port }} · 设备 {{ devices.filter(device => device.integratorId === item.id).length }}</div>
              </div>
              <div class="flex gap-2">
                <Button variant="ghost" size="icon" class="h-8 w-8" @click="openEditIntegrator(item)"><Pencil class="h-4 w-4" /></Button>
                <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive" @click="deleteIntegrator(item.id)"><Trash2 class="h-4 w-4" /></Button>
              </div>
            </div>
          </div>
          <div v-if="integrators.length === 0" class="rounded-lg border border-dashed px-4 py-8 text-center text-sm text-muted-foreground">暂无网关。</div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0">
          <div>
            <CardTitle>设备拓扑</CardTitle>
            <CardDescription>设备绑定模板后会自动生成默认采集块和测点，再补 PLC 地址。</CardDescription>
          </div>
          <Button size="sm" class="gap-1" :disabled="integrators.length === 0" @click="openAddDevice"><Plus class="h-4 w-4" />新建设备</Button>
        </CardHeader>
        <CardContent class="space-y-3">
          <div v-for="device in devices" :key="device.id" class="overflow-hidden rounded-xl border">
            <div class="flex flex-wrap items-center justify-between gap-3 px-4 py-3">
              <button type="button" class="flex flex-1 items-center gap-3 text-left" @click="toggleExpandDevice(device.id)">
                <ChevronRight class="h-4 w-4 shrink-0 text-muted-foreground transition-transform" :class="{ 'rotate-90': expandedDeviceId === device.id }" />
                <div class="min-w-0">
                  <div class="flex flex-wrap items-center gap-2">
                    <span class="font-medium">{{ device.name }}</span>
                    <Badge variant="outline">{{ device.deviceType }}</Badge>
                    <Badge variant="secondary">{{ getTemplateText(device) }}</Badge>
                  </div>
                  <div class="mt-1 flex flex-wrap gap-3 text-xs text-muted-foreground">
                    <span>{{ getIntegratorName(device.integratorId) }}</span>
                    <span>从站 {{ device.slaveAddress }}</span>
                    <span>轮询 {{ device.pollIntervalMs }} ms</span>
                    <span>{{ getGroups(device.id).length }} 块 / {{ getPoints(device.id).length }} 点</span>
                  </div>
                </div>
              </button>
              <div class="flex items-center gap-2">
                <button type="button" class="inline-flex h-[1.15rem] w-8 shrink-0 items-center rounded-full border border-transparent shadow-xs transition-all outline-none" :class="device.isEnabled ? 'bg-primary' : 'bg-input'" @click="toggleDeviceEnabled(device)">
                  <span class="pointer-events-none block size-4 rounded-full bg-background transition-transform" :class="device.isEnabled ? 'translate-x-[calc(100%-2px)]' : 'translate-x-0'" />
                </button>
                <Button variant="outline" size="sm" class="gap-1" @click="rebuildTemplate(device)"><RefreshCw class="h-3.5 w-3.5" />重建模板</Button>
                <Button variant="ghost" size="icon" class="h-8 w-8" @click="openEditDevice(device)"><Pencil class="h-4 w-4" /></Button>
                <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive" @click="deleteDevice(device.id)"><Trash2 class="h-4 w-4" /></Button>
              </div>
            </div>

            <div v-if="expandedDeviceId === device.id" class="border-t bg-muted/20 px-4 py-4">
              <div class="mb-4 flex flex-wrap items-center justify-between gap-2">
                <div class="text-sm text-muted-foreground">先定义高效批量读取块，再把测点映射到寄存器与 PLC 地址。</div>
                <div class="flex gap-2">
                  <Button variant="outline" size="sm" class="gap-1" @click="openAddGroup(device.id)"><Plus class="h-3.5 w-3.5" />采集块</Button>
                  <Button variant="outline" size="sm" class="gap-1" :disabled="getGroups(device.id).length === 0" @click="openAddPoint(device.id)"><Plus class="h-3.5 w-3.5" />测点</Button>
                </div>
              </div>

              <div v-if="getGroups(device.id).length === 0" class="rounded-lg border border-dashed px-4 py-6 text-center text-sm text-muted-foreground">当前设备还没有采集块，可手动新增或按模板重建。</div>
              <div v-else class="space-y-4">
                <div v-for="group in getGroups(device.id)" :key="group.id" class="rounded-lg border bg-background px-4 py-3">
                  <div class="flex flex-wrap items-start justify-between gap-3">
                    <div>
                      <div class="font-medium">{{ group.name }}</div>
                      <div class="mt-1 text-xs text-muted-foreground">FC {{ group.functionCode }} · 起始 {{ group.startRegister }} · 长度 {{ group.registerCount }} · 排序 {{ group.sortOrder }}</div>
                    </div>
                    <div class="flex gap-2">
                      <Button variant="outline" size="sm" class="gap-1" @click="openAddPoint(device.id, group.id)"><Plus class="h-3.5 w-3.5" />测点</Button>
                      <Button variant="ghost" size="icon" class="h-8 w-8" @click="openEditGroup(group)"><Pencil class="h-4 w-4" /></Button>
                      <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive" @click="deleteGroup(group)"><Trash2 class="h-4 w-4" /></Button>
                    </div>
                  </div>
                  <div class="mt-3 space-y-2">
                    <div v-for="point in getPointsByGroup(device.id, group.id)" :key="point.id" class="flex flex-wrap items-center justify-between gap-2 rounded-md border px-3 py-2 text-sm">
                      <div class="min-w-0">
                        <div class="font-medium">{{ point.displayName }}</div>
                        <div class="text-xs text-muted-foreground">{{ point.pointKey }} · 寄存器 {{ point.registerAddress }} x {{ point.registerLength }} · {{ point.dataType }} · PLC {{ point.plcAddress || '-' }}</div>
                      </div>
                      <div class="flex gap-2">
                        <Button variant="ghost" size="icon" class="h-8 w-8" @click="openEditPoint(point)"><Pencil class="h-4 w-4" /></Button>
                        <Button variant="ghost" size="icon" class="h-8 w-8 text-destructive" @click="deletePoint(point)"><Trash2 class="h-4 w-4" /></Button>
                      </div>
                    </div>
                    <div v-if="getPointsByGroup(device.id, group.id).length === 0" class="rounded-md border border-dashed px-3 py-4 text-center text-sm text-muted-foreground">这个采集块还没有测点。</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div v-if="devices.length === 0" class="rounded-lg border border-dashed px-4 py-8 text-center text-sm text-muted-foreground">暂无设备，请先新建网关。</div>
        </CardContent>
      </Card>
    </div>

    <Dialog v-model:open="showIntegratorDialog">
      <DialogContent class="sm:max-w-[460px]">
        <DialogHeader>
          <DialogTitle>{{ integratorDialogMode === 'add' ? '新建网关' : '编辑网关' }}</DialogTitle>
          <DialogDescription>这里只保留 TCP 通信参数。</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">名称</Label><Input v-model="integratorForm.name" class="col-span-3" /></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">IP</Label><Input v-model="integratorForm.ipAddress" class="col-span-3" /></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">端口</Label><Input v-model.number="integratorForm.port" type="number" class="col-span-3" min="1" /></div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showIntegratorDialog = false">取消</Button>
          <Button :disabled="!integratorForm.name || !integratorForm.ipAddress || integratorSaving" @click="saveIntegrator"><Loader2 v-if="integratorSaving" class="mr-2 h-4 w-4 animate-spin" />保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <Dialog v-model:open="showDeviceDialog">
      <DialogContent class="sm:max-w-[560px]">
        <DialogHeader>
          <DialogTitle>{{ deviceDialogMode === 'add' ? '新建设备' : '编辑设备' }}</DialogTitle>
          <DialogDescription>设备模板会自动带出默认采集块和测点。</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-2"><Label>设备名称</Label><Input v-model="deviceForm.name" /></div>
            <div class="space-y-2">
              <Label>所属网关</Label>
              <Select v-model="deviceForm.integratorId"><SelectTrigger><SelectValue placeholder="选择网关" /></SelectTrigger><SelectContent><SelectItem v-for="item in integrators" :key="item.id" :value="item.id">{{ item.name }}</SelectItem></SelectContent></Select>
            </div>
            <div class="space-y-2">
              <Label>设备类型</Label>
              <Select v-model="deviceForm.deviceType"><SelectTrigger><SelectValue placeholder="选择类型" /></SelectTrigger><SelectContent><SelectItem v-for="type in deviceTypeOptions" :key="type" :value="type">{{ type }}</SelectItem></SelectContent></Select>
            </div>
            <div class="space-y-2">
              <Label>模板</Label>
              <Select v-model="deviceForm.templateKey" @update:model-value="value => applyTemplate(String(value))"><SelectTrigger><SelectValue placeholder="选择模板" /></SelectTrigger><SelectContent><SelectItem v-for="item in templateOptions" :key="item.key" :value="item.key">{{ item.deviceModel }}</SelectItem></SelectContent></Select>
            </div>
            <div class="space-y-2"><Label>设备型号</Label><Input v-model="deviceForm.deviceModel" /></div>
            <div class="space-y-2"><Label>从站地址</Label><Input v-model.number="deviceForm.slaveAddress" type="number" min="1" max="247" /></div>
            <div class="space-y-2"><Label>轮询间隔(ms)</Label><Input v-model.number="deviceForm.pollIntervalMs" type="number" min="100" step="100" /></div>
          </div>
          <div v-if="deviceForm.templateKey" class="rounded-lg border bg-muted/30 px-3 py-3 text-xs text-muted-foreground">{{ templates.find(item => item.key === deviceForm.templateKey)?.description ?? '模板会自动生成默认结构。' }}</div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showDeviceDialog = false">取消</Button>
          <Button :disabled="!deviceForm.name || !deviceForm.integratorId || deviceSaving" @click="saveDevice"><Loader2 v-if="deviceSaving" class="mr-2 h-4 w-4 animate-spin" />保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <Dialog v-model:open="showGroupDialog">
      <DialogContent class="sm:max-w-[460px]">
        <DialogHeader>
          <DialogTitle>{{ groupDialogMode === 'add' ? '新增采集块' : '编辑采集块' }}</DialogTitle>
          <DialogDescription>采集块描述一次高效批量读取的寄存器范围。</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">名称</Label><Input v-model="groupForm.name" class="col-span-3" /></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">功能码</Label><Select v-model="groupForm.functionCode"><SelectTrigger class="col-span-3"><SelectValue /></SelectTrigger><SelectContent><SelectItem v-for="item in functionCodeOptions" :key="item.value" :value="item.value">{{ item.label }}</SelectItem></SelectContent></Select></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">起始寄存器</Label><Input v-model.number="groupForm.startRegister" type="number" class="col-span-3" min="0" /></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">读取长度</Label><Input v-model.number="groupForm.registerCount" type="number" class="col-span-3" min="1" /></div>
          <div class="grid grid-cols-4 items-center gap-4"><Label class="text-right">排序</Label><Input v-model.number="groupForm.sortOrder" type="number" class="col-span-3" min="1" /></div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showGroupDialog = false">取消</Button>
          <Button :disabled="!groupForm.name || groupSaving" @click="saveGroup"><Loader2 v-if="groupSaving" class="mr-2 h-4 w-4 animate-spin" />保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>

    <Dialog v-model:open="showPointDialog">
      <DialogContent class="sm:max-w-[620px]">
        <DialogHeader>
          <DialogTitle>{{ pointDialogMode === 'add' ? '新增测点' : '编辑测点' }}</DialogTitle>
          <DialogDescription>测点描述寄存器解析规则，以及最终 PLC 目标地址。</DialogDescription>
        </DialogHeader>
        <div class="grid gap-4 py-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-2"><Label>显示名称</Label><Input v-model="pointForm.displayName" /></div>
            <div class="space-y-2"><Label>Point Key</Label><Input v-model="pointForm.pointKey" /></div>
            <div class="space-y-2">
              <Label>所属采集块</Label>
              <Select v-model="pointForm.readGroupId"><SelectTrigger><SelectValue placeholder="选择采集块" /></SelectTrigger><SelectContent><SelectItem v-for="item in getGroups(pointForm.deviceId)" :key="item.id" :value="item.id">{{ item.name }}</SelectItem></SelectContent></Select>
            </div>
            <div class="space-y-2"><Label>寄存器地址</Label><Input v-model.number="pointForm.registerAddress" type="number" min="0" /></div>
            <div class="space-y-2"><Label>寄存器长度</Label><Input v-model.number="pointForm.registerLength" type="number" min="1" /></div>
            <div class="space-y-2">
              <Label>数据类型</Label>
              <Select v-model="pointForm.dataType"><SelectTrigger><SelectValue /></SelectTrigger><SelectContent><SelectItem v-for="item in dataTypeOptions" :key="item" :value="item">{{ item }}</SelectItem></SelectContent></Select>
            </div>
            <div class="space-y-2"><Label>缩放</Label><Input v-model.number="pointForm.scale" type="number" step="0.001" /></div>
            <div class="space-y-2"><Label>单位</Label><Input v-model="pointForm.unit" /></div>
            <div class="space-y-2"><Label>PLC 地址</Label><Input v-model="pointForm.plcAddress" placeholder="例如 D1000" /></div>
            <div class="space-y-2"><Label>排序</Label><Input v-model.number="pointForm.sortOrder" type="number" min="1" /></div>
          </div>
          <div class="space-y-2"><Label>备注</Label><Input v-model="pointForm.notes" placeholder="例如：来自刻线机 PLC 点位表" /></div>
        </div>
        <DialogFooter>
          <Button variant="outline" @click="showPointDialog = false">取消</Button>
          <Button :disabled="!pointForm.displayName || !pointForm.readGroupId || pointSaving" @click="savePoint"><Loader2 v-if="pointSaving" class="mr-2 h-4 w-4 animate-spin" />保存</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  </div>
</template>
