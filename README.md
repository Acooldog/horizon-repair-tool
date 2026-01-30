# 地平线修复工具

---
**⚠️ 重要提醒**：如果程序无法打开，请务必先安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)
---
一个专为 Windows 平台设计的系统服务管理工具，用于修复和优化系统服务配置，解决"地平线"相关软件的服务冲突问题。

## 项目简介

本工具是一个基于 .NET 8.0 开发的 Windows 桌面应用程序，主要功能是自动管理系统服务，包括禁用冲突服务和启用必要服务，以确保"地平线"相关软件能够正常运行。

## 功能特性

### 核心功能

- **禁用冲突服务**：自动禁用与地平线冲突的 NI (National Instruments) 相关服务
  - NI Application Web Server
  - NI System Web Server
  - NI Authentication Service
  - NI Citadel 4 Service
  - NI Variable Engine
  - NI Device Loader
  - NI Configuration Manager
  - NI Domain Service
  - NI Network Discovery
  - NI mDNS Responder Service
  - NI Time Synchronization
  - NI PSP Service Locator
  - NI PXI Resource Manager
  - NI Service Locator

- **启用必要服务**：启用并配置游戏所需的系统服务
  - 手动启动服务：IP Helper, Xbox Live 游戏保存
  - 自动启动服务：IKE and AuthIP IPsec Keying Modules, TCP/IP NetBIOS Helper, Xbox Accessory Management Service, Xbox Live 身份验证管理器, Xbox Live 网络服务, Peer Networking Grouping

### 其他功能

- **版本检查**：自动检测本地版本和远程版本，提示更新
- **日志记录**：完整的日志系统，记录所有操作和错误信息
- **管理员权限验证**：启动时自动检查管理员权限

## 系统要求

### 运行环境

- **操作系统**：Windows 10/11 或更高版本
- **运行时**：.NET 8.0 Runtime

### ⚠️ 重要提示

如果您的系统无法打开此程序，请前往以下链接下载并安装 .NET 8.0 Runtime：

**下载地址**：https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0

请选择适合您系统架构的版本（通常为 x64）：
- **运行时（Runtime）**：用于运行已编译的应用程序
- **SDK**：如果您需要从源代码编译开发此项目

### 权限要求

- **管理员权限**：必须以管理员身份运行此程序，否则程序将自动退出

## 安装说明

### 方法一：使用编译好的可执行文件（推荐）

1. 下载最新版本的 `地平线修复工具.exe`
2. 右键点击程序，选择"以管理员身份运行"
3. 按照界面提示操作

### 方法二：从源代码编译

如果您需要从源代码编译：

1. **安装开发环境**
   - 安装 [Visual Studio 2022](https://visualstudio.microsoft.com/)（Community 版本免费）
   - 或安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

2. **克隆项目**
   ```bash
   git clone https://gitee.com/daoges_x/horizon-repair-tool.git
   cd horizon-repair-tool
   ```

3. **编译项目**
   ```bash
   dotnet build
   ```

4. **运行程序**
   - 生成的可执行文件位于 `bin\Debug\net8.0-windows\地平线修复工具.exe`
   - 以管理员身份运行该文件

## 使用指南

### 启动程序

1. 右键点击 `地平线修复工具.exe`
2. 选择"以管理员身份运行"
3. 如果没有管理员权限，程序将弹出警告并自动退出

### 主要操作

#### 1. 启用必要服务

点击"启用必要服务"按钮，程序将：
- 启用手动启动服务（IP Helper, Xbox Live 游戏保存）
- 启用自动启动服务（IKE and AuthIP IPsec Keying Modules 等）

#### 2. 禁用冲突服务

点击"禁用冲突服务"按钮，程序将：
- 禁用所有 NI 相关服务
- 停止正在运行的服务
- 将服务启动类型设置为"禁用"

#### 3. 检查更新

程序启动时会自动检查版本：
- 如果有新版本，点击版本号可前往下载页面
- 当前版本和远程版本都会显示在界面上

## 项目结构

```
test/
├── Program.cs                    # 程序入口点
├── test.csproj                   # 项目配置文件
├── plugins/                      # 插件配置目录
│   └── plugins.json             # 服务配置文件
├── src/                         # 源代码目录
│   ├── Services/               # 服务层
│   │   ├── Helpers/            # 辅助工具类
│   │   │   └── Logs.cs         # 日志记录
│   │   ├── Managers/           # 管理器
│   │   │   ├── ServiceManager.cs    # 服务管理核心
│   │   │   ├── getVersion.cs         # 版本管理
│   │   │   ├── fixSoft.cs            # 修复逻辑
│   │   │   ├── JsonEdit.cs           # JSON 配置读取
│   │   │   └── pathEdit.cs           # 路径处理
│   │   ├── Model/              # 数据模型
│   │   └── repairs/            # 修复模块
│   └── UI/                     # 用户界面
│       └── Forms/              # 窗体
│           ├── Home.cs         # 主窗体逻辑
│           └── Home.Designer.cs # 主窗体设计
├── img/                        # 图片资源
└── logs/                       # 日志文件（运行时生成）
```

## 配置文件

程序使用 `plugins/plugins.json` 文件配置服务列表，您可以修改此文件来自定义需要操作的服务：

```json
{
  "Application": {
    "Name": "地平线修复工具",
    "Version": "v1.0.0"
  },
  "fix": {
    "disableClashName": [ ... ],
    "EnableNotAuto": [ ... ],
    "EnableAuto": [ ... ]
  }
}
```

## 日志文件

程序运行时会在 `logs` 目录下生成日志文件：
- 文件命名格式：`app_YYYYMMDD.log`
- 包含所有操作的详细信息
- 遇到错误时，请将 `logs` 文件夹发送给开发者

## 技术栈

- **框架**：.NET 8.0 Windows
- **UI 框架**：Windows Forms
- **依赖库**：
  - Newtonsoft.Json (13.0.4) - JSON 处理
  - System.ServiceProcess.ServiceController (10.0.2) - 服务控制
  - System.Management (10.0.2) - WMI 管理

## 常见问题

### Q1: 程序打不开怎么办？

**A**: 这通常是因为系统缺少 .NET 8.0 Runtime。请前往 [https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) 下载并安装。

### Q2: 提示"请以管理员身份运行"

**A**: 右键点击程序图标，选择"以管理员身份运行"。这是必需的，因为修改系统服务需要管理员权限。

### Q3: 操作失败怎么办？

**A**: 请检查：
- 是否以管理员身份运行
- `logs` 文件夹中的日志文件，查看具体错误信息
- 将日志文件夹发送给开发者以获得支持

### Q4: 是否支持自动更新？

**A**: 程序会自动检查版本更新，如果有新版本，点击版本号会跳转到下载页面，需要手动下载新版本。

## 开发者信息

- **项目地址**：https://gitee.com/daoges_x/horizon-repair-tool
- **更新地址**：https://gitee.com/daoges_x/horizon-repair-tool/releases

## 许可证

本项目遵循相应的开源许可证，详见 LICENSE.txt 文件。

## 更新日志

### v1.0.0
- 初始版本发布
- 支持禁用冲突服务
- 支持启用必要服务
- 版本检查功能
- 日志记录功能

## 贡献

欢迎提交 Issue 和 Pull Request 来帮助改进这个项目。

## 注意事项

1. **备份建议**：在执行服务修改操作前，建议记录当前服务状态
2. **使用谨慎**：修改系统服务可能影响系统稳定性，请确保理解操作的后果
3. **问题反馈**：遇到问题时，请提供日志文件以便快速定位问题

