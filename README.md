# 地平线修复工具 (Horizon Repair Tool)

一款专为《极限竞速：地平线4》和《极限竞速：地平线5》玩家设计的系统服务管理工具，帮助解决游戏无法进入线上模式、服务冲突等问题。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

## 📖 项目简介

本工具是一个基于 .NET 8.0 开发的 Windows 桌面应用程序，主要功能是自动管理系统服务，禁用与游戏冲突的系统服务并启用必要的游戏服务，确保《极限竞速：地平线》系列游戏能够正常运行。

### 支持的游戏
- 🏎️ **极限竞速：地平线4 (Forza Horizon 4)**
- 🏎️ **极限竞速：地平线5 (Forza Horizon 5)**

## ✨ 功能特性

### 当前已实现功能

#### 1. 自定义修复 (Custom Repair)
- **禁用冲突服务**：自动禁用可能与游戏冲突的系统服务
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

- **启用必要服务**：自动启用游戏所需的系统服务
  - **手动启动服务**：IP Helper, Xbox Live 游戏保存
  - **自动启动服务**：IKE and AuthIP IPsec Keying Modules, TCP/IP NetBIOS Helper, Xbox Accessory Management Service, Xbox Live 身份验证管理器, Xbox Live 网络服务, Peer Networking Grouping

#### 2. 版本检查
- 自动检测本地版本和远程版本
- 一键跳转到下载页面获取最新版本
- 实时更新提示

#### 3. 日志系统
- 完整的日志记录功能
- 按日期自动分割日志文件
- 详细的操作记录和错误信息

### 计划中功能 (Coming Soon)

#### 🚀 一键修复
- 自动化完整修复流程
- 智能诊断和问题检测
- 一键解决常见连接问题

#### 🔧 高级修复功能
- **网络修复**：重置 Winsock、IP 配置、防火墙规则、DNS 缓存
- **Hosts 文件清理**：移除被屏蔽的游戏相关域名
- **防火墙端口配置**：自动配置游戏所需的端口（3074, 53, 80, 88, 500, 3544, 4500）
- **Windows Update 修复**：修复 Windows 更新组件和服务
- **Microsoft Store 修复**：重置商店缓存和应用注册
- **游戏缓存清理**：清理游戏临时文件和缓存
- **DNS 优化**：推荐并配置优化的 DNS 服务器
- **时间同步修复**：修复 Windows 时间服务

#### 📊 诊断工具
- 网络连接测试
- Xbox Live 服务器连通性检测
- NAT 类型检测
- 端口开放状态检测
- 游戏服务状态监控
- 诊断报告生成

#### 💾 备份还原
- 配置文件自动备份
- 游戏存档备份
- 设置文件备份
- Hosts 文件备份
- 一键还原功能

## 🛠️ 技术栈

- **开发框架**: .NET 8.0 (Windows)
- **UI 框架**: Windows Forms
- **开发语言**: C#
- **主要依赖**:
  - Newtonsoft.Json (13.0.4) - JSON 配置文件处理
  - System.ServiceProcess.ServiceController (10.0.2) - Windows 服务管理
  - System.Management (10.0.2) - WMI 系统管理
- **自动化工具**:
  - Semantic Release - 自动化版本管理和发布
  - GitHub Actions - CI/CD 自动化流程

## 📦 安装说明

### 方式一：下载发布版（推荐）

1. 前往 [Releases](https://github.com/daoges_x/horizon-repair-tool/releases) 页面
2. 下载最新版本的 `horizon-repair-tool-{version}.zip`
3. 解压到任意目录
4. 右键点击 `地平线修复工具.exe`，选择"**以管理员身份运行**"

### ⚠️ 重要提示：运行环境要求

如果您的系统无法打开此程序，**请务必先安装 .NET 8.0 Runtime**：

👉 **下载地址**: https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0

请选择适合您系统架构的版本（通常为 **x64**）：
- **运行时 (Runtime)**：用于运行已编译的应用程序（选择此项）
- **SDK**：如果您需要从源代码编译开发此项目

### 方式二：从源代码编译

如果您需要从源代码编译：

1. **安装开发环境**
   - 安装 [Visual Studio 2022](https://visualstudio.microsoft.com/)（Community 版本免费）
   - 或安装 [.NET 8.0 SDK](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

2. **克隆项目**
   ```bash
   git clone https://github.com/daoges_x/horizon-repair-tool.git
   cd horizon-repair-tool
   ```

3. **还原依赖**
   ```bash
   npm install
   ```

4. **编译项目**
   ```bash
   dotnet build -c Release
   ```

5. **运行程序**
   - 生成的可执行文件位于 `bin/Release/net8.0-windows/地平线修复工具.exe`
   - 以管理员身份运行该文件

## 📖 使用指南

### 启动程序

1. 右键点击 `地平线修复工具.exe`
2. 选择"**以管理员身份运行**"
3. 如果没有管理员权限，程序将弹出警告并自动退出

### 主界面说明

程序主界面分为两个部分：

#### 🎮 地平线4 (Forza Horizon 4)
- **一键修复**：自动执行完整的修复流程（计划中）
- **自定义修复**：手动选择需要执行的修复操作

#### 🎮 地平线5 (Forza Horizon 5)
- **一键修复**：自动执行完整的修复流程（计划中）
- **自定义修复**：手动选择需要执行的修复操作

### 自定义修复操作

点击"自定义修复"按钮后，您可以：

1. **禁用冲突服务**
   - 点击"禁用冲突服务"按钮
   - 程序将自动禁用所有 NI 相关系统服务
   - 等待操作完成

2. **启用必要服务**
   - 点击"启用必要服务"按钮
   - 程序将自动启用游戏所需的系统服务
   - 包括手动启动和自动启动服务
   - 等待操作完成

### 检查更新

程序启动时会自动检查版本更新：
- 当前版本显示在界面底部左侧
- 如果有新版本，右侧会显示"新版本：vX.X.X, 点我下载"
- 点击版本号可跳转到下载页面

## 📁 项目结构

```
horizon-repair-tool/
├── .github/                      # GitHub 配置
│   └── workflows/
│       └── release.yml          # 自动化发布工作流
├── bin/                         # 编译输出和资源
│   └── 实现方法/                # 实现文档和流程图
├── img/                         # 图标和图片资源
│   └── ke.ico                  # 应用程序图标
├── plugins/                     # 插件配置目录
│   └── plugins.json            # 服务配置文件
├── scripts/                     # 构建脚本
│   └── package.js              # 发布打包脚本
├── src/                         # 源代码目录
│   ├── Services/               # 服务层
│   │   ├── Helpers/            # 辅助工具类
│   │   │   └── Logs.cs         # 日志记录
│   │   ├── Managers/           # 业务管理器
│   │   │   ├── ServiceManager.cs   # 服务管理核心
│   │   │   ├── getVersion.cs       # 版本管理
│   │   │   ├── fixSoft.cs          # 修复逻辑
│   │   │   ├── JsonEdit.cs         # JSON 配置读取
│   │   │   ├── pathEdit.cs         # 路径处理
│   │   │   └── ICON.cs             # 图标处理
│   │   ├── Model/              # 数据模型
│   │   ├── repairs/            # 修复模块
│   │   └── PublicFuc/          # 公共函数
│   └── UI/                     # 用户界面
│       └── Forms/              # 窗体
│           ├── HomePage/        # 主界面
│           │   ├── Home.cs
│           │   ├── Home.Designer.cs
│           │   ├── HomeFH4Solt.cs   # 地平线4解决方案
│           │   ├── HomeFH5Solt.cs   # 地平线5解决方案
│           │   ├── HomeLoad.cs      # 窗体加载逻辑
│           │   └── HomeSolt.cs      # 通用解决方案
│           └── FH4/            # 地平线4修复界面
│               └── CustomRepair/
│                   ├── Fh4CustomRepair.cs
│                   ├── Fh4CustomRepair.Designer.cs
│                   ├── F4CusReWork.cs    # 工作窗体
│                   ├── F4CusReSolt.cs    # 结果窗体
│                   └── F4CusReProgressBar.cs  # 进度条
├── .gitignore                   # Git 忽略文件
├── .releaserc.json            # Semantic Release 配置
├── LICENSE.txt                 # MIT 许可证
├── package.json               # Node.js 依赖和脚本
├── package-lock.json          # Node.js 依赖锁定文件
├── Program.cs                 # 程序入口点
├── README.md                  # 项目说明文档
├── test.csproj                # .NET 项目配置
└── test.sln                   # Visual Studio 解决方案
```

## ⚙️ 配置文件

程序使用 `plugins/plugins.json` 文件配置服务列表，您可以修改此文件来自定义需要操作的服务：

```json
{
  "Application": {
    "Name": "地平线修复工具",
    "Version": "v1.1.0"
  },
  "icon": "img/ke.ico",
  "fix": {
    "disableClashName": [
      // 需要禁用的服务列表
    ],
    "EnableNotAuto": [
      // 需要手动启动的服务列表
    ],
    "EnableAuto": [
      // 需要自动启动的服务列表
    ]
  }
}
```

## 📝 日志文件

程序运行时会在 `logs` 目录下生成日志文件：
- **文件命名格式**：`app_YYYYMMDD.log`
- **内容**：包含所有操作的详细信息和错误堆栈
- **用途**：遇到问题时，请将 `logs` 文件夹发送给开发者以便快速定位问题

## 🔨 开发指南

### 本地开发

1. **克隆仓库**
   ```bash
   git clone https://github.com/daoges_x/horizon-repair-tool.git
   cd horizon-repair-tool
   ```

2. **安装依赖**
   ```bash
   # 安装 .NET 8.0 SDK（如尚未安装）
   # 安装 Node.js 依赖
   npm install
   ```

3. **运行项目**
   ```bash
   dotnet run
   ```

### 构建发布

```bash
# Release 模式编译
dotnet build -c Release

# 打包发布版本
npm run package

# 执行完整发布流程（会自动编译和打包）
npm run release
```

### 贡献代码

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 开启 Pull Request

## ❓ 常见问题 (FAQ)

### Q1: 程序打不开怎么办？

**A**: 这通常是因为系统缺少 .NET 8.0 Runtime。

**解决方案**：
1. 前往 [https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)
2. 下载并安装 .NET 8.0 Runtime（不是 SDK）
3. 重新运行程序

### Q2: 提示"请以管理员身份运行"

**A**: 修改系统服务需要管理员权限，这是必需的。

**解决方案**：
1. 右键点击程序图标
2. 选择"以管理员身份运行"

### Q3: 操作失败怎么办？

**A**: 请检查以下几点：
- ✅ 是否以管理员身份运行
- ✅ `logs` 文件夹中的日志文件，查看具体错误信息
- ✅ 将日志文件夹发送给开发者以获得支持

### Q4: 工具会影响其他游戏或程序吗？

**A**: 不会。本工具专门针对《极限竞速：地平线》系列游戏进行优化，禁用的服务主要是 NI (National Instruments) 相关服务，通常只会影响特定的工业自动化软件，不会影响普通游戏和日常使用。

### Q5: 一键修复功能什么时候开放？

**A**: 一键修复功能正在开发中，预计将在 v2.0.0 版本发布。目前请使用自定义修复功能。

### Q6: 支持哪些 Windows 版本？

**A**:
- ✅ Windows 10 (64位)
- ✅ Windows 11 (64位)
- ❌ Windows 7 或更早版本（不支持 .NET 8.0）

### Q7: 是否支持 Steam 版和 Microsoft Store 版本？

**A**: 是的，本工具支持所有版本的《极限竞速：地平线》游戏，包括 Steam 版和 Microsoft Store 版。

### Q8: 使用工具后还需要重启电脑吗？

**A**:
- **自定义修复**：通常不需要重启
- **一键修复**：某些操作可能需要重启才能生效（届时会有明确提示）
- 建议修复后重启电脑以确保所有更改生效

## 🔗 相关链接

- **GitHub 仓库**: https://github.com/daoges_x/horizon-repair-tool
- **Releases 页面**: https://github.com/daoges_x/horizon-repair-tool/releases
- **问题反馈**: https://github.com/daoges_x/horizon-repair-tool/issues
- **Gitee 镜像**: https://gitee.com/daoges_x/horizon-repair-tool

## 📄 许可证

本项目采用 [MIT License](LICENSE.txt) 开源许可证。

```
Copyright (c) 2024 Horizon Repair Tool Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions.
```

## 📋 更新日志

### v1.1.0 (最新版本)
- ✨ 支持地平线4和地平线5
- ✨ 优化服务管理逻辑
- ✨ 添加自定义修复功能
- 🐛 修复版本检查问题
- 📝 完善日志系统

### v1.0.0 (初始版本)
- 🎉 初始版本发布
- ✅ 基础服务修复功能
- ✅ 禁用冲突服务
- ✅ 启用必要服务
- ✅ 版本检查功能

### 计划中 (Coming Soon)
- 🚀 一键修复功能
- 🚀 网络修复功能
- 🚀 Hosts 文件清理
- 🚀 防火墙端口配置
- 🚀 诊断工具
- 🚀 备份还原功能

## ⚠️ 免责声明

1. **使用风险**：本工具仅供学习和研究使用，不保证能解决所有问题。使用本工具产生的任何后果由用户自行承担。

2. **数据备份**：建议在使用本工具前备份重要数据和系统配置。

3. **第三方软件**：本工具不会收集、上传任何用户数据，所有操作均在本地完成。

4. **技术支持**：如遇问题，请提交 Issue 或查看日志文件寻求帮助。

5. **更新通知**：程序会自动检查更新，但不会自动下载或安装更新，需要用户手动确认。

---

## 🙏 致谢

- 感谢所有为本项目贡献代码的开发者
- 感谢提出建议和反馈的用户
- 感谢 .NET 团队提供的优秀开发框架

---

<div align="center">

**如果这个工具对您有帮助，请给一个 ⭐ Star 支持一下！**

Made with ❤️ by Horizon Repair Tool Team

**⚠️ 重要提醒**：如果程序无法打开，请务必先安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)

</div>
