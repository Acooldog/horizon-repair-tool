# 地平线修复工具

> Windows 系统服务管理工具，解决地平线游戏服务冲突问题

[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.txt)
[![Language](https://img.shields.io/badge/Language-C%23-239120.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Framework](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)

**⭐ Stars**: ![Star](https://gitee.com/daoges_x/horizon-repair-tool/badge/star.svg?theme=dark) | **📥 下载**: ![Download](https://gitee.com/daoges_x/horizon-repair-tool/badge/downloads.svg?theme=dark) | **👀 访问**: ![View](https://gitee.com/daoges_x/horizon-repair-tool/badge/views.svg?theme=dark)

## 项目简介

基于 .NET 8.0 和 C# 开发的 Windows 桌面应用程序，自动管理系统服务，禁用冲突服务并启用必要服务。

## 主要功能

- ✅ **禁用冲突服务** - 禁用 14 个 NI (National Instruments) 相关服务
- ✅ **启用必要服务** - 启用 Xbox 游戏相关服务（手动/自动启动）
- ✅ **版本检查** - 自动检测更新
- ✅ **日志记录** - 完整记录所有操作
- ✅ **权限验证** - 启动时检查管理员权限

## 系统要求

- **操作系统**: Windows 10/11
- **运行时**: [.NET 8.0 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0) ⚠️ **必须安装**
- **权限**: 管理员权限

## 快速开始

### 安装

1. 下载 [地平线修复工具.exe](https://gitee.com/daoges_x/horizon-repair-tool/releases)
2. 右键 → 以管理员身份运行

### 使用

1. 点击 **启用必要服务** - 启用手动和自动启动的服务
2. 点击 **禁用冲突服务** - 禁用所有 NI 相关服务
3. 有新版本时点击版本号下载

## 技术栈

- **语言**: C#
- **框架**: .NET 8.0 Windows
- **UI**: Windows Forms
- **依赖**: Newtonsoft.Json, System.ServiceProcess, System.Management

## 许可证

[MIT License](LICENSE.txt)

## 链接

- **仓库**: https://gitee.com/daoges_x/horizon-repair-tool
- **发布**: https://gitee.com/daoges_x/horizon-repair-tool/releases

---

**⚠️ 提示**: 程序无法打开请先安装 [.NET 8.0 Runtime](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0)
