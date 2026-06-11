# GameLauncher

<p align="center">
  <img src="Assets/AppIcon_128.png" alt="GameLauncher Logo" width="128">
</p>

<p align="center">
  <strong>一款现代化、轻量级的 Windows 游戏启动器</strong>
</p>

<p align="center">
  <a href="#功能特性">功能特性</a> •
  <a href="#截图">截图</a> •
  <a href="#快速开始">快速开始</a> •
  <a href="#详细功能">详细功能</a> •
  <a href="#技术架构">技术架构</a> •
  <a href="#许可证">许可证</a>
</p>

---

## 简介

**GameLauncher** 是一款专为 PC 游戏玩家设计的本地游戏管理工具。它提供了一个优雅的界面来组织、管理和快速启动你收藏的所有游戏——无论是 Steam、Epic 还是本地独立游戏。

与臃肿的大型游戏平台不同，GameLauncher 追求**极简、高效、个性化**，让你专注于游戏本身。

### 为什么选择 GameLauncher？

- **零依赖**：自包含发布，无需安装 .NET 运行时或其他依赖
- **即开即用**：解压即用，不写注册表，不创建系统服务
- **隐私优先**：所有数据本地存储，不联网、不收集、不追踪
- **高度可定制**：主题、背景、轮播图、AI 描述，一切由你掌控

---

## 功能特性

### 游戏管理

| 功能 | 说明 |
|------|------|
| 游戏库管理 | 添加、删除、编辑游戏信息 |
| 启动路径 | 支持 .exe 和 .lnk 快捷方式 |
| Steam 自动导入 | 扫描 Steam 库，自动识别已安装游戏 |
| Epic 自动导入 | 扫描 Epic Games 已安装游戏 |
| Steam AppID | 通过 Steam 协议启动，兼容 D 加密游戏 |
| 自定义 Steam 路径 | 支持多个 Steam 库文件夹 |
| 收藏夹 | 标记常玩游戏，置顶显示 |
| 搜索过滤 | 快速搜索游戏名称和描述 |
| 拖拽排序 | 自由调整游戏排列顺序 |
| 游玩记录 | 记录启动次数和最后游玩时间 |

### 媒体管理

| 功能 | 说明 |
|------|------|
| 轮播图 | 为每个游戏添加多张展示图，选中时自动轮播 |
| 视频轮播 | 支持在轮播中添加视频（MP4/AVI/MKV 等） |
| 游戏截图 | 独立的截图库管理 |
| 图片裁切 | 支持 16:9、4:3、1:1、自由比例裁切 |
| 安全存储 | 图片自动复制到应用目录，防止误删 |

### AI 智能

| 功能 | 说明 |
|------|------|
| 描述简化 | AI 一键简化冗长的游戏描述 |
| 多模型支持 | 兼容 OpenAI、Anthropic 等 API 格式 |
| 自定义配置 | 可设置 API 地址、Key 和模型 |
| 模型获取 | 一键获取可用模型列表 |

### 个性化

| 功能 | 说明 |
|------|------|
| 主题风格 | 浅色模式、深色模式、跟随系统 |
| 背景效果 | Mica（云母）或 Acrylic（亚克力） |
| 图标定制 | 支持自定义应用图标 |

---

## 快速开始

### 下载

从 [Releases](https://github.com/XiaxiaStudio/GameLauncher/releases) 页面下载最新版本。

### 安装

1. 下载 `GameLauncher_vX.X.X.zip`
2. 解压到任意目录（建议放在非系统盘）
3. 双击 `GameLauncher.exe` 运行

### 首次使用

```
1. 点击左下角 ⚙️ 齿轮按钮进入设置
2. 点击「添加游戏」手动添加，或点击「导入 Steam/Epic」自动扫描
3. 为游戏添加封面图和截图（可选）
4. 配置 AI 功能（可选）
5. 返回游戏库，点击游戏即可启动
```

---

## 详细功能

### Steam 游戏导入

GameLauncher 能够自动检测 Steam 安装目录下的所有游戏：

- 自动解析 `appmanifest_*.acf` 文件
- 提取游戏名称、安装路径和 AppID
- 支持自定义 Steam 库路径（设置 → Steam 库路径）
- 自动填充 Steam AppID，方便通过 Steam 协议启动

### D 加密游戏支持

部分使用 Denuvo 加密的游戏无法直接通过 .exe 启动。GameLauncher 通过 Steam 协议解决此问题：

1. 自动导入时会提取 Steam AppID
2. 启动时优先使用 `steam://rungameid/{AppID}` 协议
3. 通过 Steam 正确启动游戏，绕过 D 加密限制

### AI 描述简化

GameLauncher 集成了 AI 功能，可以一键简化冗长的游戏描述：

1. 在设置中配置 AI API（支持 OpenAI、Anthropic 等）
2. 编辑游戏时，点击「AI 简化」按钮
3. AI 会自动将描述精简为 150 字以内的简介
4. 支持自定义 API 地址和模型

### 轮播图与视频

为每个游戏配置专属的展示内容：

- 支持 PNG、JPG、GIF、WebP 等图片格式
- 支持 MP4、AVI、MKV、MOV 等视频格式
- 自动轮播展示（4 秒间隔）
- 视频播放时自动暂停轮播
- 内置图片裁切工具

### 主题与个性化

提供丰富的视觉定制选项：

- **浅色模式**：清爽明亮的界面风格
- **深色模式**：护眼的暗色主题
- **跟随系统**：自动匹配 Windows 系统主题
- **Mica 背景**：半透明云母效果，与桌面壁纸融合
- **Acrylic 背景**：毛玻璃亚克力效果

---

## 技术架构

### 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| WinUI 3 | 2.1 | UI 框架 |
| Windows App SDK | 2.1.3 | 应用平台 |
| .NET | 10 | 运行时 |
| C# | 14 | 开发语言 |

### 项目结构

```
GameLauncher/
├── Assets/                  # 应用资源（图标、图片）
├── Converters/              # 值转换器
│   ├── CarouselTemplateSelector.cs  # 轮播图模板选择器
│   └── Converters.cs        # 通用转换器
├── Models/                  # 数据模型
│   ├── AppData.cs           # 应用数据容器
│   └── GameItem.cs          # 游戏条目模型
├── Pages/                   # 页面
│   ├── HomePage.xaml         # 游戏库主页
│   ├── GameDetailPage.xaml   # 游戏详情页
│   ├── SettingsPage.xaml     # 设置页
│   └── AboutPage.xaml        # 关于页
├── Services/                # 服务层
│   ├── AiService.cs          # AI API 服务
│   ├── DataService.cs        # 数据持久化服务
│   ├── GameDetector.cs       # 游戏检测服务
│   └── ImageService.cs       # 图片处理服务
├── ViewModels/              # 视图模型
│   └── MainViewModel.cs      # 主视图模型
├── App.xaml                 # 应用入口
├── MainWindow.xaml          # 主窗口
└── CropDialog.xaml          # 图片裁切对话框
```

### 数据存储

所有数据存储在 `%LocalAppData%\GameLauncher\` 目录下：

- `appdata.json`：游戏列表、设置、AI 配置
- `Images/`：游戏封面、轮播图、截图

### 构建与发布

```bash
# Debug 构建
dotnet build GameLauncher.csproj

# Release 构建
dotnet build GameLauncher.csproj -c Release -r win-x64

# 发布（自包含）
dotnet publish GameLauncher.csproj -c Release -r win-x64 --self-contained
```

---

## 系统要求

| 要求 | 最低配置 |
|------|----------|
| 操作系统 | Windows 10 1809 (Build 17763) 及以上 |
| 架构 | x64 |
| 内存 | 4 GB |
| 磁盘空间 | 100 MB（不含游戏数据） |
| 运行时 | 无需安装（自包含） |

---

## 许可证

本项目基于 [GNU General Public License v3.0](LICENSE) 开源。

```
Copyright (C) 2026 XiaxiaStudio

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
```

---

## 致谢

- [Microsoft WinUI 3](https://github.com/microsoft/microsoft-ui-xaml) - UI 框架
- [Windows App SDK](https://github.com/microsoft/WindowsAppSDK) - 应用平台
- [Font Awesome](https://fontawesome.com/) - 图标资源

---

<p align="center">
  <strong>GameLauncher</strong> — 由 <a href="https://github.com/XiaxiaStudio">XiaxiaStudio</a> 用心打造
</p>

<p align="center">
  Made with ❤️ by XiaxiaStudio
</p>
