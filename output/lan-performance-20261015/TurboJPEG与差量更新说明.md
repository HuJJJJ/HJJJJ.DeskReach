# GZip 移除、TurboJPEG 与差量更新

日期：2026-10-15。适用主程序 HJJJJ.DeskReach.Windows。该文件记录本轮最终实现，前两份报告保留之前的定位与修改基线。

后续的窗口布局、静止画面缩放、鼠标坐标及 High / Quality 原始分辨率设置，见 [最大化与画质修复说明](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/output/lan-performance-20261015/最大化与画质修复说明.md>)。本文件中的性能数据保留本轮当时的测量口径。

## 已完成的变化

- 实际屏幕传输链路的 GZip 压缩、解压均已移除。
- 使用官方 libjpeg-turbo 3.2.0 的 TurboJPEG API 进行 JPEG 编解码，直接处理位图像素；编码器和解码器上下文复用。
- 首帧发送完整 JPEG，之后按 128×128 像素块比较，只发送变化块的 JPEG。
- 接收端先将更新合成为完整画面，再交给 UI；UI 可以合并完整快照，差量依赖在重建层处理。
- 接收端完成重建后确认对应的流和帧号；发送端最多保持一个尚未确认的更新。缺帧、解码失败或确认超时会触发关键帧恢复。
- 修复 TCP 包头未收齐时被清空缓存的问题，并纠正初始画质菜单显示：默认 Nomal 对应质量 50，Speed 对应 TurboJPEG 最低质量 1。

## TurboJPEG 实测收益

TurboJPEG 是 JPEG 的编解码 API，替换的是当前 GDI+ JPEG 实现，输出仍为 JPEG。官方库使用 SIMD 优化，但与 Windows 当前编码器的实际差距必须现场测量，不能直接套用相对其他 libjpeg 实现的倍数。[官方项目说明](https://github.com/libjpeg-turbo/libjpeg-turbo)。

以下使用同一张 1280×720 合成桌面图片，质量 50、4:2:0，预热后各测 100 次。解码两边都生成独立、可持有的位图；TurboJPEG 直接写入新建位图，GDI+ 使用上一轮已优化的 ToBitmap。耗时不含网络和 UI 绘制。

| 项目 | GDI+ | TurboJPEG |
|---|---:|---:|
| 编码平均耗时 | 1.675ms | 1.652ms |
| 编码 P95 | 2.091ms | 1.877ms |
| 解码为独立位图平均耗时 | 4.591ms | 2.429ms |
| 解码 P95 | 5.336ms | 2.994ms |
| 该图片编码后的字节数 | 98,142 | 98,142 |

这台机器上编码速度接近；解码耗时减少约 47%。本轮最大的传输收益来自局部变化时避免重发完整图片。

在一个 128×128 块内交替修改 48×48 区域，持续 100 次：

| 项目 | 结果 |
|---|---:|
| 每次完整帧平均大小，含屏幕帧头 | 97,582.5 字节 |
| 每次差量平均大小，含屏幕帧头 | 5,042.5 字节 |
| 字节量减少 | 94.83% |
| 比较、编码及序列化差量平均耗时 | 1.100ms |
| 差量解码、合成及复制完整快照平均耗时 | 1.637ms |

该节省比例只对应这个局部变化样例。整屏滚动、视频播放或复杂动画可能退回完整帧，不会持续保持约 95% 的节省。

本机实际 DXGI 取帧、缩放和完整 TurboJPEG 编码平均 19.01ms；完整关键帧接收、解码、重建快照和 1478 宽显示缩放平均 22.75ms。窗口最终绘制、真实网络往返及端到端帧率尚未测量。

原始结果：[turbo-delta-metrics-AnyCPU.json](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/output/lan-performance-20261015/turbo-delta-metrics-AnyCPU.json>)。

## 代码阅读顺序

| 方法 / 文件 | 负责什么 |
|---|---|
| [WindowsAPIScreenCapture.CaptureUpdate](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach.Windows/WindowsAPIScreenCapture.cs:69>) | 取得桌面、缩放，然后进入差量编码；恢复或画质变化时允许使用缓存的桌面图像 |
| [ScreenDeltaEncoder.Encode](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach.Windows/ScreenDeltaEncoder.cs:28>) | 比较原始像素、选择关键帧或差量、编码变化块并推进基准 |
| [TurboJpegCodec](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach.Windows/TurboJpegCodec.cs>) | 原生库加载、像素编码、JPEG 尺寸验证、直接解码到目标区域、原生资源释放 |
| [ScreenFrame](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach/Plugins/Screen/ScreenFrame.cs>) | 屏幕协议版本、帧序号、区域数据序列化和边界校验 |
| [ScreenPlugin.SendFrame](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach/Plugins/Screen/ScreenPlugin.cs:94>) | 非阻塞防重入、等待确认、超时转为关键帧、直接提交屏幕数据，无 GZip |
| [ScreenPlugin.ProcessFrame](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach/Plugins/Screen/ScreenPlugin.cs:196>) | 请求重建，成功后确认，失败则请求关键帧，无 GZip 解压 |
| [ScreenFrameDecoder.Apply](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach.Windows/ScreenFrameDecoder.cs:19>) | 检查差量依赖、解码各区域、重建完整画面、返回独立快照 |
| [PackageUnpacker.CalculatePacketLength](</C:/Users/o/Desktop/新建文件夹/HJJJJ.DeskReach/HJJJJ.DeskReach/PackageUnpacker.cs:17>) | 长度字段未收齐时保留 TCP 缓存继续等待 |

原来的 CaptureScreen 方法仍保留，供完整 JPEG 捕获和基准测量使用；主程序 GetImage 实际调用 CaptureUpdate。它们的 JPEG 编码都使用 TurboJPEG。

## 差量如何产生

比较发生在缩放后的原始 BGR 像素上，基准是上一张已编码画面的原始像素。没有拿上一次解码后的有损 JPEG 再编码，因此不会逐次累积重压缩失真。

图像被划分成 128×128 块，右边和底边不足一块时使用实际尺寸。每块逐行比较有效像素，忽略每行末尾的对齐字节。无变化时不产生更新包；变化块使用 TurboJPEG 单独编码。原图只 LockBits 一次，用对应区域的指针和完整行跨度直接编码，不为每个区域额外创建 Bitmap。

以下情况使用一张全图 JPEG：首帧、画质变化、尺寸变化、显式恢复请求、变化块面积达到全图的 60%、变化块超过 256 个，或距上次关键帧超过 5 秒后遇到下一张可处理桌面画面。静止且没有新 DXGI 画面时，不为周期关键帧反复唤醒编码。

只有所有区域编码成功后才更新上一帧像素基准和帧号；发送失败后会强制下一次发关键帧。

## 为什么不能沿用任意丢帧

差量 D3 可能依赖 D2 已经更新过的画面。直接覆盖 D2 会留下旧块，因此当前每帧携带 StreamId、FrameId 和 BaseFrameId。

发送端提交更新后，新的定时触发先检查是否收到对应确认。接收端完成重建后才返回 StreamId + FrameId，入队时不确认；错误或过期确认不能释放当前在途帧。超过 2 秒未确认时，发送端强制发送新的完整关键帧。

正常情况下只有一张在途更新。接收端的待处理槽仍然有界；超时期间即使发生替换，重建器也会检查 BaseFrameId，缺少基准就请求关键帧。重建完成后的完整 Bitmap 快照彼此独立，UI 可以安全地只保留最新快照。

一个包内有多个区域，若后面的区域损坏，本次不发布快照，并放弃部分写入的重建缓冲区。已显示或已提交到 UI 的旧快照保持不变，恢复关键帧后再继续显示。

一个在途帧的确认策略会受到网络往返时间影响；当前按局域网目标实现，高延迟链路的吞吐需另测。

## 包头与限制

屏幕帧采用 DRS2 标记和版本 2；52 字节固定头包含关键帧标志、画布尺寸、流标识、当前帧号、依赖帧号、区域数量。每个区域包含 x、y、宽、高、JPEG 长度及 JPEG 数据。关键帧必须只包含一个覆盖全图的区域。

接收时校验版本、帧序号、坐标溢出、JPEG 长度、帧大小、画布像素数、尾随字节和 JPEG 实际尺寸。当前单帧数据上限 32MiB，画布上限 16,777,216 像素、单边上限 8192。复制及解码均遵守像素行跨度，不能将带行对齐的图像当成一段紧密连续数据。

## 原生依赖与部署

使用 [libjpeg-turbo 官方 3.2.0 发布](https://github.com/libjpeg-turbo/libjpeg-turbo/releases/tag/3.2.0) 的原始 x64 和 x86 TurboJPEG DLL；下载归档先按官方 SHA-256 校验，再提取 DLL，未运行安装器。许可证、IJG 声明及来源摘要位于项目 third-party/libjpeg-turbo，构建时一起复制。

两端必须使用同一版完整构建输出，不能只替换 Windows.exe 或单独更新一端。屏幕协议已经变化，与旧版 JPEG + GZip 的帧载荷不兼容。

构建输出中的 runtimes/win-x64/native/turbojpeg.dll 和 runtimes/win-x86/native/turbojpeg.dll 必须保留；根据进程位数加载对应文件。Debug 和 Release 均已构建，两个位数的原生 API 和实际 DXGI 更新链路均已验证。

## 验证范围

最终自动检查包括防重入、有界待处理关键帧、UI 合并与资源释放、DXGI/GDI 捕获资源稳定性、完整帧重建、差量抑制、边缘 1 像素高更新块、未改变区域保持、重复帧、缺帧恢复、质量与尺寸变化、一个包内部分区域失败、错误及过期确认、超时恢复、损坏 JPEG、越界及截断数据。

另将两个完整外层数据包的包头按单字节拆开，包体分段并与下一个包连在一起输入实际解包器，验证拆包和粘包。该检查复现了旧实现会提前清空缓存的问题；修复后通过。

可在先构建主程序后运行：

```powershell
.\tests\Run-ScreenPipelineChecks.ps1
.\tests\Run-ScreenPipelineChecks.ps1 -Platform x86 -ChecksOnly
```

所有测量均为本机；包流检查使用真实序列化格式和解包器，不建立远程控制会话。没有保存桌面图像。真实两台机器的网络帧率、往返延迟、锁屏与设备切换仍需现场验证。本轮差量采用 CPU 像素比较，尚未利用 DXGI 的脏矩形元数据减少显卡读回和缩放工作。
