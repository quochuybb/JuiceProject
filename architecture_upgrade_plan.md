# 🏗️ Architecture Upgrade Plan — JuiceProject (Final Version)

## Đánh Giá Hiện Trạng: Cái gì đã có, cái gì còn thiếu?

### ✅ Đã có (Không cần sửa)
| Thành phần | File | Trạng thái |
|-----------|------|-----------|
| Server-Authoritative Match | `GameRoom.ProcessMatch()` | Hoạt động tốt |
| Client Prediction cho Match | `PvPBoardManager.ProcessInput()` | Hoạt động tốt |
| Recipe Matching (ghép đúng công thức) | `RecipeManager.cs` | Hoạt động tốt |
| AddNumber Sync | `NetworkPlayer.CmdAddNumberServerRpc` | Hoạt động tốt |
| HP System | `GameRoom.Player1HP / Player2HP` | Cần mở rộng |

### ❌ Chưa có (Cần xây mới)
| Thành phần | Mô tả | Độ ưu tiên |
|-----------|-------|-----------|
| 🛡️ Shield System | Giáp hấp thụ sát thương | P1 |
| ⚡ Mana System | Thanh sạc để nhận Item | P1 |
| 🎁 Item System | 20 Item, 4 cấp hiếm, Gacha | P2 |
| 🔒 Locked Cell | Ô bị khóa trên bàn cờ | P2 |
| ☠️ Debuff System | Độc, Đóng băng, Làm chậm | P3 |
| 📢 Event Bus | Kiến trúc sự kiện | P1 |
| 📋 Command Queue | Hàng đợi xử lý lệnh | P1 |
| 📦 ScriptableObjects | Data-Driven cho Recipe/Item | P1 |
| 🔄 Server Reconciliation | Sửa sai khi Client đoán trượt | P3 |

---

## Roadmap 5 Phase

### PHASE 1 — Đặt Nền Móng ⭐ (LÀM ĐẦU TIÊN)
> *Mục tiêu: Xây xong hạ tầng kỹ thuật. Game vẫn chạy y như cũ nhưng bên trong đã sẵn sàng đón tính năng mới.*

**Bước 1.1: Tạo Data Models (ScriptableObjects)**

Tạo thư mục mới: `Assets/Scripts/Shared/DataModels/BuffSystem/`

| File cần tạo | Nội dung |
|-------------|---------|
| `BuffEnums.cs` | Định nghĩa `EffectType` (Damage, TrueDamage, Heal, Shield, Mana, LockCell, ClearJunk) và `EffectTarget` (SelfPlayer, EnemyPlayer, SelfGrid, EnemyGrid) |
| `BuffEffect.cs` | Struct chứa `EffectType`, `EffectTarget`, `float value` |
| `MicroBuffData.cs` | ScriptableObject chứa `formulaId`, `skillName`, `overrideBaseDamage`, `List<BuffEffect>` |
| `ItemData.cs` | ScriptableObject chứa `itemName`, `ItemRarity`, `List<BuffEffect>`, `float duration` |
| `ItemEnums.cs` | Enum `ItemRarity` (Common, Rare, Epic, Legendary) |

**Bước 1.2: Tạo Event Bus**

| File cần tạo | Nội dung |
|-------------|---------|
| `GameEventBus.cs` | Class static chứa các `event Action`: `OnMatchSuccess`, `OnItemUsed`, `OnDamageTaken`, `OnShieldBroken` |

**Bước 1.3: Tạo Command Queue**

| File cần tạo | Nội dung |
|-------------|---------|
| `ICommand.cs` | Interface: `void Execute(GameRoomContext ctx)` |
| `DamageCommand.cs` | Trừ HP (check Giáp trước) |
| `TrueDamageCommand.cs` | Trừ HP (bỏ qua Giáp) |
| `HealCommand.cs` | Hồi HP |
| `ShieldCommand.cs` | Tăng Giáp |
| `ManaCommand.cs` | Cộng Mana |
| `LockCellCommand.cs` | Khóa ô trên bàn địch |
| `ClearJunkCommand.cs` | Dọn ô rác trên bàn mình |
| `CommandProcessor.cs` | Hàm duyệt Queue, gọi Execute từng Command |

**Bước 1.4: Thêm Shield + Mana vào GameRoom**

| File cần sửa | Thay đổi |
|-------------|---------|
| `GameRoom.cs` | Thêm: `Player1Shield`, `Player2Shield`, `Player1Mana`, `Player2Mana`, `Player1Items`, `Player2Items` |

**Bước 1.5: Chuyển ProcessMatch sang Event Bus**

| File cần sửa | Thay đổi |
|-------------|---------|
| `GameRoom.cs` | Hàm `ProcessMatch` cũ gọi trực tiếp `HandleAttack()`. Sửa thành: Bắn `EventBus.OnMatchSuccess()` → Listeners tạo Command → `CommandProcessor.Execute()` → Gửi RPC kết quả |

> [!IMPORTANT]
> **Sau Phase 1, game phải chạy Y HỆT như trước.** Chỉ khác là bên trong đã dùng Event Bus + Command Queue thay vì gọi trực tiếp. Đây là bước Refactor an toàn.

---

### PHASE 2 — Recipe Buffs + Shield
> *Mục tiêu: 36 công thức có hiệu ứng riêng. Giáp hoạt động.*

| Công việc | Chi tiết |
|----------|---------|
| Tạo 36 file `.asset` ScriptableObject | Kéo thả trên Unity Inspector, nhập Effect cho từng công thức theo bảng `recipe_design_36.md` |
| `MicroBuffManager.cs` | Lắng nghe `OnMatchSuccess`, tra Dictionary tìm MicroBuffData, tạo Command tương ứng |
| `DamageCommand` xử lý Shield | Khi trừ HP, check Shield trước. Shield > 0 → Trừ Shield. Shield = 0 → Trừ HP |
| RPC đồng bộ Shield | Thêm `RpcUpdateShieldClientRpc()` vào `NetworkPlayer.cs` |
| UI thanh Shield | Hiển thị thanh Giáp bên cạnh thanh HP trên Client |

---

### PHASE 3 — Mana + Item System
> *Mục tiêu: Sạc Mana khi ghép, nhận Item ngẫu nhiên, dùng Item chủ động.*

| Công việc | Chi tiết |
|----------|---------|
| Mana tăng mỗi lần Match | `OnMatchSuccess` listener cộng Mana cho player |
| Mana đầy → Random Item | Server roll theo tỷ lệ Drop Rate (40/30/20/10). Comeback Rate nếu HP < 30% |
| Tạo 20 file `.asset` Item | Kéo thả trên Inspector theo bảng `item_design.md` |
| `CmdUseItemServerRpc()` | Client gửi yêu cầu dùng Item lên Server |
| `ItemManager.cs` (Server) | Nhận yêu cầu, đọc `ItemData`, tạo Command Queue, Execute |
| RPC đồng bộ Item | `RpcReceiveItemClientRpc()` (nhận Item), `RpcItemUsedClientRpc()` (hiệu ứng hình ảnh) |
| UI Item Slot | Hiển thị 2 ô chứa Item + nút bấm sử dụng |

---

### PHASE 4 — Debuff + Board Effects
> *Mục tiêu: Ô khóa, Nhiễm độc, Đóng băng, Bẫy hoạt động.*

| Công việc | Chi tiết |
|----------|---------|
| `CellData` thêm trạng thái | Thêm `CellState` enum (Normal, Locked, Trapped, Frozen) vào `CellData.cs` |
| Server quản lý ô khóa | `GameRoom` lưu danh sách ô bị khóa theo thời gian, tự unlock khi hết duration |
| Poison Tick System | Server chạy `Coroutine` hoặc timer: mỗi 2 giây kiểm tra Player có đang bị Poison → Trừ 1 HP |
| Freeze System | Khóa input của player bị Freeze, Server từ chối mọi RPC trong thời gian Freeze |
| Trap System (Vỏ Chuối) | Khi Server nhận `TryMatch`, check xem ô đó có trap không → Nếu có: Trừ HP thay vì gây sát thương |
| RPC đồng bộ Cell State | `RpcUpdateCellStateClientRpc(int index, CellState state)` |
| UI hiệu ứng ô | Hiển thị khóa/băng/bẫy trên CellUI |

---

### PHASE 5 — Server Reconciliation + Polish
> *Mục tiêu: Chống lag, chống desync, sửa sai Client.*

| Công việc | Chi tiết |
|----------|---------|
| TickID System | Mỗi Action Client gửi kèm 1 TickID tăng dần |
| Board Snapshot | Client lưu copy `dataList` trước mỗi Action |
| Reconciliation | Server gửi về State chuẩn kèm TickID. Client so sánh, nếu sai → Rollback bàn cờ |
| HP/Shield Lerp | Thanh HP/Shield dùng `Mathf.Lerp` hoặc DOTween để chạy mượt |
| Item VFX | Hiệu ứng hình ảnh cho 20 Item (Bom nổ, Sao rơi, Rồng phun lửa...) |

---

## Bước Đầu Tiên Cậu Cần Làm Ngay

> [!IMPORTANT]
> **Phase 1, Bước 1.1: Tạo `BuffEnums.cs`**
> 
> Đây là file đầu tiên cần tạo. Nó định nghĩa "ngôn ngữ chung" cho toàn bộ hệ thống. Mọi file khác đều phụ thuộc vào nó.

Khi cậu sẵn sàng, nói **"Làm Phase 1"** và tôi sẽ bắt tay tạo toàn bộ file cho Phase 1 ngay lập tức.
