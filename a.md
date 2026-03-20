项目背景：
说明一下我的当前项目结构，基于winform架构webview2（.\Device_Monitor_App） 前端使用vue3（.\device-monitor-web）。当前物理设备结构是使用一个工控机通过网口使用tcp对接多个集成设备，集成设备上面通过485接口接入多个设备有电能表、流量表、风速仪、空调，持久层数据使用sqlite数据库结构如下：


 当前SQLite 表结构方案：

DROP TABLE IF EXISTS "Device";
CREATE TABLE "Device" (
  "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  "IntegratorId" integer NOT NULL,
  "Name" varchar(100) NOT NULL,
  "DeviceType" varchar(50),
  "SlaveAddress" integer NOT NULL,
  "ReadFunctionCode" integer NOT NULL,
  "ReadStartRegister" integer,
  "ReadRegisterCount" integer,
  "IsOnline" integer,
  "IsEnabled" integer
);

-- ----------------------------
-- Records of Device
-- ----------------------------

-- ----------------------------
-- Table structure for DeviceTagMapping
-- ----------------------------
DROP TABLE IF EXISTS "DeviceTagMapping";
CREATE TABLE "DeviceTagMapping" (
  "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  "DeviceId" integer NOT NULL,
  "ValueName" varchar(50) NOT NULL,
  "RegisterOffset" integer,
  "DataType" varchar(20),
  "Scale" float,
  "PlcOffset" integer,
  "IsEnabled" integer
);

-- ----------------------------
-- Records of DeviceTagMapping
-- ----------------------------

-- ----------------------------
-- Table structure for Integrator
-- ----------------------------
DROP TABLE IF EXISTS "Integrator";
CREATE TABLE "Integrator" (
  "Id" integer NOT NULL PRIMARY KEY AUTOINCREMENT,
  "Name" varchar(100) NOT NULL,
  "IpAddress" varchar(50) NOT NULL,
  "Port" integer NOT NULL,
  "PlcBaseAddress" varchar(50) NOT NULL,
  "PlcBlockSize" integer NOT NULL,
  "IsEnabled" integer
);

-- ----------------------------
-- Records of Integrator
-- ----------------------------

-- ----------------------------
-- Table structure for sqlite_sequence
-- ----------------------------
DROP TABLE IF EXISTS "sqlite_sequence";
CREATE TABLE "sqlite_sequence" (
  "name",
  "seq"
);

-- ----------------------------
-- Records of sqlite_sequence
-- ----------------------------

-- ----------------------------
-- Indexes structure for table Device
-- ----------------------------
CREATE INDEX "Device_IntegratorId"
ON "Device" (
  "IntegratorId" ASC
);

-- ----------------------------
-- Indexes structure for table DeviceTagMapping
-- ----------------------------
CREATE INDEX "DeviceTagMapping_DeviceId"
ON "DeviceTagMapping" (
  "DeviceId" ASC
);

PRAGMA foreign_keys = true;



核心业务：

通讯库这边使用的是公司的dll，但是我需要设计后端数据采样的业务，要满足以下需求：可以动态添加设备一个集成设备上，在读取modbus rtu的时候要、以及写入PLC的时候要注重效率。我做了如下数据流转业务设计：
![image-20260320154440597](C:\Users\uviJ\AppData\Roaming\Typora\typora-user-images\image-20260320154440597.png)



modbus设备点位整理：

| 设备名称          | 测点参数              | 功能码 | 寄存器地址 (Hex) | 数据说明及换算法则                                           |
| ----------------- | --------------------- | ------ | ---------------- | ------------------------------------------------------------ |
| 风速仪(5路皮托管) | 1~5号传感器 压力      | 3      | 0x0000 ~ 0x0004  | 地址对应: 0x00~0x04 分别对应 1~5号。类型: Signed Int。说明: 无小数位，负压为补码 。+1 |
|                   | 1~5号传感器 风速      | 3      | 0x0005 ~ 0x0009  | 地址对应: 0x05~0x09 分别对应 1~5号。类型: Unsigned Int。说明: 读取值 ÷ 10.0 = 实际风速 。+1 |
|                   | 1~5号传感器 温度      | 3      | 0x000A ~ 0x000E  | 地址对应: 0x0A~0x0E (十进制10~14) 分别对应 1~5号。类型: Unsigned long int (跨寄存器读取需要注意字节序，通常为 2 寄存器/参数，请依实际读取核对) 。 |
| 电能表(AMC系列)   | 相电压 UA, UB, UC     | 3      | 0x0100 ~ 0x0102  | 说明: 二次侧参数。读取值 ÷ 10.0 = 实际电压 (V) 。            |
|                   | 线电压 UAB, UBC, UCA  | 3      | 0x0103 ~ 0x0105  | 说明: 二次侧参数。读取值 ÷ 10.0 = 实际线电压 (V) 。          |
|                   | 相电流 IA, IB, IC     | 3      | 0x0106 ~ 0x0108  | 说明: 二次侧参数。读取值 ÷ 1000.0 = 实际电流 (A) 。          |
|                   | A, B, C相有功功率     | 3      | 0x0109 ~ 0x010B  | 说明: 二次侧参数。读取值 ÷ 1000.0 = 实际功率 (kW) 。         |
|                   | 总有功功率 (Pt)       | 3      | 0x010C           | 说明: 二次侧参数。读取值 ÷ 1000.0 = 实际有功功率 (kW) 。     |
|                   | 总无功功率 (Qt)       | 3      | 0x0110           | 说明: 二次侧参数。读取值 ÷ 1000.0 = 实际无功功率 (kvar) 。   |
|                   | 总功率因数            | 3      | 0x0114           | 说明: 二次侧参数。读取值 ÷ 1000.0 = 实际功率因数 。          |
| 空调( BTW-289+)   | 柜温探头温度          | 3      | 0x0000           | 说明: 读取值 ÷ 10.0 = 实际温度 (℃) 。                        |
|                   | 蒸发器探头温度        | 3      | 0x0001           | 说明: 读取值 ÷ 10.0 = 实际温度 (℃) 。                        |
|                   | 制冷继电器状态        | 3      | 0x0002           | 说明: 0: 断开，1: 闭合 。                                    |
|                   | 风机继电器状态        | 3      | 0x0003           | 说明: 0: 断开，1: 闭合 。                                    |
|                   | 报警继电器状态        | 3      | 0x0004           | 说明: 0: 断开，1: 闭合 。                                    |
| 流量计(FL3W7系列) | 实时流量值            | 4      | 0x0000           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 显示OUT1的设定值      | 4      | 0x0001           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | OUT1 累计流量值高16位 | 4      | 0x0002           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | OUT1 累计流量值低16位 | 4      | 0x0003           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 流量谷值显示          | 4      | 0x0004           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 流量峰值显示          | 4      | 0x0005           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 温度峰值              | 4      | 0x0006           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 温度谷值              | 4      | 0x0007           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |
|                   | 流体温度值            | 4      | 0x0008           | 说明: 读取值 ÷ 100.0 = 实际值 。                             |


PLC在点位信息在路径   .\刻线机通讯协议.xlsx中。

个人意见：当前表结构设计有问题，一个网关并不与PLC一组地址相对应，所以网关表里面填写PLC地址没用，还有设备表也应该分类，因为各种设备所需要监控得字段长度并不统一，可以使用继承关系来复用相同的属性。整体架构业务架构需要重新搭建





