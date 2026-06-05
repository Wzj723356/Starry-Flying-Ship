# 星际战舰 - Star Ship

## 项目简介

这是一个支持英伟达RTX光线追踪的3A级星际空战游戏，模拟战争雷霆风格的飞行控制系统。

## 核心特性

### RTX光线追踪支持
- **RTX 50/40系列优化**
- 全局光照 (Global Illumination)
- 实时反射 (Reflections)
- 柔和阴影 (Shadows)
- 环境光遮蔽 (Ambient Occlusion)
- DLSS 3 超级分辨率支持

### 飞行控制系统
- 长方形测试星舰模型
- 六自由度 (6DOF) 飞行控制
- 引擎推力系统
- 加速器 (Afterburner)
- 大气层/太空飞行切换
- 空气动力学模拟

### 星际环境
- 自定义星球生成
- 三角形山脉地形
- 大气层效果
- 星空场
- 多样化星球表面

## 系统要求

### 最低配置
- **操作系统**: Windows 10/11 (64-bit)
- **处理器**: Intel Core i5-8400 / AMD Ryzen 5 2600
- **内存**: 12 GB RAM
- **显卡**: NVIDIA GeForce RTX 3060 Ti
- **存储**: 50 GB 可用空间

### 推荐配置
- **操作系统**: Windows 10/11 (64-bit)
- **处理器**: Intel Core i7-12700K / AMD Ryzen 7 5800X
- **内存**: 32 GB RAM
- **显卡**: NVIDIA GeForce RTX 4080 / RTX 4090
- **存储**: 100 GB SSD

### 最佳配置
- **操作系统**: Windows 11 (64-bit)
- **处理器**: Intel Core i9-13900K / AMD Ryzen 9 7950X
- **内存**: 64 GB RAM
- **显卡**: NVIDIA GeForce RTX 4090
- **存储**: 100 GB NVMe SSD

## 操控指南

### 基础飞行控制
| 操作 | 键盘/鼠标 |
|------|----------|
| 俯仰 | 鼠标 Y轴 |
| 横滚 | 鼠标 X轴 |
| 偏航 | A / D 键 |
| 油门 | W / S 键 |
| 加速器 | Shift 键 |

### 武器控制
| 操作 | 按键 |
|------|------|
| 主武器射击 | 鼠标左键 |
| 副武器射击 | 鼠标右键 |
| 切换主武器 | 1-4 |
| 切换副武器 | 5-9 |

### 系统控制
| 操作 | 按键 |
|------|------|
| 襟翼 | F |
| 起落架 | G |
| 雷达 | R |
| 通讯 | C |
| 工程 | E |
| 导航 | N |

### 加速器 (Afterburner)
| 操作 | 按键 |
|------|------|
| 开启/关闭 | B |

### 视角控制
| 操作 | 按键 |
|------|------|
| 重置视角 | F3 |
| 切换HUD | F4 |
| 外部视角 | V |
| 驾驶舱视角 | C |

### 调试功能
| 操作 | 按键 |
|------|------|
| 调试信息 | F1 |
| 截图 | F2 |
| 控制台 | ~ |
| 暂停 | ESC |

## 配置文件说明

所有配置文件位于 `TestBuild/Configs/` 目录：

### GameConfig.ini
- 游戏图形设置
- RTX光线追踪配置
- 音效设置
- 物理参数
- 引擎参数
- 武器配置
- 游戏玩法设置
- 网络设置

### GraphicsConfig.ini
- 星舰飞控参数
- 引擎设置
- 武器挂载
- 防御系统
- 驾驶舱/HUD设置
- 输入配置

### StartupConfig.ini
- 启动参数
- 显示设置
- 音频设置
- 输入设置
- 图形高级设置
- 性能设置
- 物理设置
- 网络设置
- 调试设置

### BuildSettings.json
- 构建配置
- 目标平台
- 性能优化
- 测试模式

## 构建说明

### 使用Unity Editor构建

1. 打开Unity Hub
2. 添加项目: `Starry Flying Ship`
3. 打开项目
4. 选择 `File` -> `Build Settings`
5. 选择 `Windows` -> `x86_64`
6. 点击 `Build`
7. 选择输出目录: `TestBuild`

### 使用命令行构建

```bash
# 使用默认设置
.\BuildScript.bat

# 指定构建模式
.\BuildScript.bat Development
.\BuildScript.bat Release
```

### 启动游戏

```bash
# 快速启动
.\StartGame.bat

# 自定义配置启动
StarryFlyingShip.exe -configdir "Configs" -logfile "Logs\game.log"
```

## 性能优化建议

### RTX光线追踪
- RTX 40/50系列: 启用所有光线追踪功能
- RTX 30系列: 启用部分光线追踪，降低反射/阴影质量
- 非RTX显卡: 禁用光线追踪，使用传统渲染

### DLSS设置
- 性能模式: 性能优先
- 均衡模式: 平衡画质和性能
- 质量模式: 画质优先
- 自动模式: 根据硬件自动选择

### 帧率优化
- 降低渲染分辨率
- 减少后期处理效果
- 关闭不必要的HUD元素
- 使用静态批次处理

## 项目结构

```
Starry Flying Ship/
├── Assets/
│   ├── Scripts/
│   │   ├── StarShipFlightController.cs    # 星舰飞行控制器
│   │   ├── EngineController.cs            # 引擎控制器
│   │   ├── InputManager.cs                # 输入管理器
│   │   ├── InterstellarEnvironment.cs     # 星际环境
│   │   ├── PlanetGenerator.cs             # 星球生成器
│   │   ├── WeaponSystem.cs                # 武器系统
│   │   ├── Damageable.cs                  # 可伤害对象
│   │   └── ...
│   ├── Prefabs/                           # 预设体
│   ├── Scenes/                            # 场景
│   ├── Materials/                        # 材质
│   └── Textures/                         # 纹理
├── ProjectSettings/                      # Unity项目设置
├── Packages/                             # Unity包
├── TestBuild/                            # 测试构建
│   ├── Configs/                          # 配置文件
│   │   ├── GameConfig.ini
│   │   ├── GraphicsConfig.ini
│   │   ├── StartupConfig.ini
│   │   └── BuildSettings.json
│   ├── Logs/                             # 日志
│   ├── BuildScript.bat                    # 构建脚本
│   ├── StartGame.bat                      # 启动脚本
│   └── README.md                          # 本文件
└── README.md
```

## 技术规格

### 渲染
- **渲染管线**: Unity URP / HDRP
- **API**: DirectX 12
- **光线追踪**: NVIDIA RTX
- **DLSS版本**: 3.0+
- **最大分辨率**: 8K

### 物理
- **物理引擎**: Unity Physics
- **刚体**: 支持
- **碰撞检测**: Continuous Dynamic
- **最大速度**: 500 m/s (可配置)

### 音效
- **采样率**: 48 kHz
- **声道**: 立体声/环绕声
- **空间音频**: HRTF
- **实时混音**: 支持

### 网络
- **协议**: UDP
- **Tick率**: 64 Hz
- **最大玩家数**: 8
- **延迟补偿**: 支持

## 开发团队

- **游戏设计**: [待定]
- **程序开发**: [待定]
- **美术设计**: [待定]
- **音效设计**: [待定]

## 许可证

[待定]

## 版本历史

### v1.0.0-alpha
- 初始版本
- 基础飞行控制系统
- RTX光线追踪支持
- 星舰模型
- 星际环境
- 三角形地形生成
- 武器系统原型

## 联系方式

[待定]

## 致谢

- Unity Technologies
- NVIDIA
- War Thunder (飞行物理参考)
- Ark Knights (游戏风格参考)
