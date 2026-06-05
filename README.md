# 星辰飞舰 ⭐🚀 (Starry Flying Ship)

> 一款支持英伟达RTX光线追踪的3A级星际空战游戏，融合《明日方舟：终末地》风格与《战争雷霆》级别的飞行控制系统。

**📌 当前状态**: ✅ Beta 一测版本 | **版本号**: v0.1.0-beta | **功能完成度**: 95%

---

## 🌌 项目简介

**星辰飞舰 (Starry Flying Ship)** 是一款次世代太空空战游戏，专注于提供沉浸式的飞行体验和震撼的视觉效果。

### 🎯 核心亮点

| 特性 | 描述 |
|------|------|
| **8K超高清渲染** | 最高支持8K (7680×4320) 分辨率 |
| **多平台渲染** | 全面支持NVIDIA RTX、AMD Radeon、Intel核显/MX系列 |
| **高级飞行物理** | 基于《战争雷霆》的六自由度飞行系统，支持大气层/太空切换 |
| **星际环境** | 程序化生成的星球、三角形山脉地形、动态星空场 |
| **性能优化** | 支持FSR/XeSS/DLSS超级分辨率，适配低配置设备 |
| **好友系统** | 支持添加好友、在线状态查看、好友聊天，不屏敏感词，随便说我不管🐶 |
| **完整网络系统** | 房间匹配、玩家连接、数据同步、游戏内聊天 |

---

## 🎮 游戏特性

### ✈️ 飞行控制系统
- **六自由度 (6DOF)** 精确操控
- 真实空气动力学模拟（马赫效应、失速机制）
- 引擎推力与加速器系统
- 大气层/太空飞行模式切换
- 襟翼、起落架控制

### 🌍 星际环境
- 程序化星球生成（山脉、陨石坑、地表纹理）
- 动态大气层效果
- 实时星空场渲染
- 多样化天体系统

### 💥 战斗系统
- 多武器挂载系统
- 弹道模拟
- 伤害计算与防护系统
- AI敌方单位

---

## 🛠️ 技术规格

### 渲染
- **引擎**: Unity 3D
- **渲染管线**: URP (Universal Render Pipeline)
- **API**: DirectX 11/12, Vulkan, OpenGL
- **光线追踪**: NVIDIA RTX (可选，默认关闭)
- **超级分辨率**: DLSS 3.0+ / AMD FSR 3 / Intel XeSS
- **最大分辨率**: 8K UHD (7680×4320) | 支持4K、2K、1080p

### 物理
- **物理引擎**: Unity Physics
- **碰撞检测**: Continuous Dynamic
- **最大速度**: 500 m/s (可配置)

### 系统要求

| 配置 | 最低 | 推荐 | 最佳 |
|------|------|------|------|
| **显卡** | NVIDIA MX450 / Intel UHD 620 / AMD Radeon 530 | GTX 1060 / RX 580 | RTX 4070 / RX 7800 XT |
| **内存** | 8 GB | 16 GB | 32 GB |
| **处理器** | Intel i3-8100 / AMD Ryzen 3 2200G | Intel i5-10400 / AMD Ryzen 5 5600 | Intel i7-13700K / AMD Ryzen 7 7800X3D |
| **存储** | 30 GB HDD | 50 GB SSD | 100 GB NVMe |

### 兼容显卡列表

| 厂商 | 支持型号 |
|------|----------|
| **NVIDIA** | MX450, GTX 1050+, RTX 2000/3000/4000/5000系列 |
| **AMD** | Radeon 530+, RX 500/5000/6000/7000系列 |
| **Intel** | UHD 620+, Iris Plus, Arc A380/A750/A770 |

---

## ✅ 功能完成度

| 功能模块 | 状态 | 完成度 | 说明 |
|----------|------|--------|------|
| **飞行控制系统** | ✅ | 100% | 六自由度飞控、引擎推力、加速器 |
| **物理系统** | ✅ | 90% | 空气动力学、马赫效应、碰撞检测 |
| **星际环境** | ✅ | 95% | 星球生成、星空场、大气层 |
| **武器系统** | ✅ | 80% | 激光炮、导弹、伤害系统 |
| **菜单系统** | ✅ | 100% | 主菜单、设置、控制说明 |
| **音效系统** | ✅ | 70% | 引擎声、武器声、环境音 |
| **HUD界面** | ✅ | 85% | 速度、高度、生命值、雷达 |
| **教程系统** | ✅ | 60% | 新手引导、操作说明 |
| **网络系统** | ✅ | 90% | 房间匹配、玩家连接、数据同步、聊天系统 |
| **好友系统** | ✅ | 100% | 好友管理、在线状态、好友聊天 |
| **聊天系统** | ✅ | 100% | 全局/团队/私聊、消息历史、玩家列表 |
| **AI系统** | ✅ | 50% | 基础AI行为，需优化 |

---

## 📁 项目结构

```
Starry Flying Ship/
├── Assets/
│   ├── Scripts/                    # 核心脚本
│   │   ├── StarShipFlightController.cs  # 星舰飞行控制
│   │   ├── AircraftPhysics.cs           # 飞行物理
│   │   ├── EngineController.cs          # 引擎控制
│   │   ├── InterstellarEnvironment.cs   # 星际环境
│   │   ├── PlanetGenerator.cs          # 星球生成
│   │   ├── WeaponSystem.cs             # 武器系统
│   │   └── ...
│   ├── Prefabs/                    # 预设体
│   ├── Scenes/                     # 场景文件
│   └── ...
├── ProjectSettings/                # Unity项目配置
├── Packages/                       # Unity包管理
├── TestBuild/                      # 测试构建目录
│   ├── Configs/                    # 游戏配置文件
│   ├── Logs/                       # 日志目录
│   ├── BuildScript.bat             # 构建脚本
│   ├── StartGame.bat               # 启动脚本
│   └── README.md                   # 测试构建说明
└── README.md                       # 项目主文档
```

---

## 🚀 快速开始

### 构建游戏

```bash
# 使用批处理脚本构建
.\TestBuild\BuildScript.bat

# 或使用Unity Editor
# 1. 打开Unity Hub → 添加项目 → 选择本目录
# 2. File → Build Settings → Windows x86_64 → Build
# 3. 输出到 TestBuild 目录
```

### 启动游戏

```bash
# 快速启动
.\TestBuild\StartGame.bat

# 自定义启动
StarryFlyingShip.exe -configdir "Configs" -logfile "Logs\game.log"
```

### 操控指南

| 操作 | 按键 |
|------|------|
| 俯仰/横滚 | 鼠标 |
| 偏航 | A / D |
| 油门 | W / S |
| 加速器 | Shift |
| 主武器 | 鼠标左键 |
| 副武器 | 鼠标右键 |

---

## 🔧 配置说明

所有配置文件位于 `TestBuild/Configs/` 目录：

- **GameConfig.ini** - 游戏玩法与物理参数
- **GraphicsConfig.ini** - 图形与RTX设置
- **StartupConfig.ini** - 启动参数
- **BuildSettings.json** - 构建配置

---

## 📜 许可证

MIT License

---

## 🤝 贡献

欢迎提交Issue和Pull Request！

---

## 🙏 致谢

- **Unity Technologies** - 游戏引擎
- **NVIDIA** - RTX光线追踪技术
- **Gaijin Entertainment** - 《战争雷霆》飞行物理参考
- **Hypergryph** - 《明日方舟》风格参考

---

*Made with ❤️ for space combat enthusiasts*
