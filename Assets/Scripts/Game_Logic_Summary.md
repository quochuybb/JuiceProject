# TECHNICAL GAME DESIGN DOCUMENT (GDD) - JUICE PROJECT
**Target Audience**: AI Assistants / System Architects / Senior Developers
**Version**: 1.0 (Server-Authoritative Networking + Command Pattern Refactor)

---

## 1. TỔNG QUAN KIẾN TRÚC (HIGH-LEVEL ARCHITECTURE)
- **Thể loại**: PvP 1v1 Puzzle Match, Multiplayer Online.
- **Engine & Framework**: Unity 3D, C# .NET, **Unity Netcode for GameObjects (NGO)**.
- **Network Topology**: **Server-Authoritative**. Server (Host/Dedicated) giữ thẩm quyền tuyệt đối về Logic và State. Client "mù" (Dumb Client), chỉ đóng vai trò hiển thị (View), phát Effect và gửi Input.
- **Backend Integration**: Node.js API xử lý Database, Account, và MMR Ranking. Kết nối thông qua HTTP Requests ở giai đoạn kết thúc Match (`ServerMatchManager`).

---

## 2. QUẢN LÝ PHÒNG CHƠI (GAME ROOM & STATE MANAGEMENT)
Trái tim của hệ thống mạng là class `GameRoom` (Kế thừa `NetworkBehaviour`). Mỗi GameRoom chứa chính xác 2 người chơi (gọi là `Player1` và `Player2`).
- **Identifier**: Quản lý bằng `RoomId`, `Player1Id` (ulong clientId), `Player2Id` (ulong clientId).
- **Core State Variables** (Lưu trữ độc lập cho 2 người chơi):
  - `Player1HP` / `Player2HP` (Mặc định: 1000)
  - `Player1Shield` / `Player2Shield` (Mặc định: 0)
  - `Player1Mana` / `Player2Mana` (Mặc định: 0)
  - `Player1AddCount` / `Player2AddCount` (Lượt thêm số, Mặc định: 5)
  - `Player1Board` / `Player2Board` (Data bàn cờ, kiểu `List<CellData>`)
  - `Player1Recipe` / `Player2Recipe` (List công thức `RecipeData` được trang bị)
- **Đồng bộ hóa**: Thông qua các hàm `ClientRpc` (VD: `RpcUpdateHPClientRpc`). ClientRpcParams được dùng để target cụ thể tới `TargetClientIds` nhằm che giấu thông tin (Player 1 không nhìn thấy tay bài ẩn của Player 2 nếu có).

---

## 3. CƠ CHẾ BÀN CỜ (BOARD SYSTEM & GENERATION)
- **Data Structure**: Bàn cờ không dùng mảng 2D mà dùng mảng 1D (`List<CellData>`) nhằm tối ưu Serialization qua mạng. Số cột cố định `COLUMNS = 9`.
- **Tọa độ (Coordinates)**: 
  - `X = index % COLUMNS`
  - `Y = index / COLUMNS`
- **Đồng bộ hóa khởi tạo (Deterministic Generation)**:
  - Khởi tạo ngẫu nhiên một biến `BoardSeed = Random.Range(1000, 999999)`.
  - Cả 2 bảng `Player1Board` và `Player2Board` đều được sinh ra từ hàm `BoardGenerator.GenerateInitialBoard(1, COLUMNS)` sau khi set `Random.InitState(BoardSeed)`. Đảm bảo 100% hai người chơi có bàn cờ giống hệt nhau khi khai cuộc.

---

## 4. LUẬT GHÉP SỐ (MATCHING LOGIC)
Được xử lý hoàn toàn trong hàm `HandleMatching(ulong clientId, int index1, int index2)`.
- **Điều kiện giá trị (Value Condition)**:
  1. `val1 == val2` (Ghép 2 số giống nhau).
  2. `val1 + val2 == 10` (Ghép tổng 10).
  3. **Recipe Override**: Bỏ qua luật 1 và 2 nếu 2 ô khớp với `foodFirst` và `foodSecond` của bất kỳ `RecipeData` nào mà user trang bị.
- **Điều kiện đường đi (Line-of-sight / Pathing)**:
  1. **Consecutive (Nằm kề nhau)**: Các ô nằm giữa 2 ô chọn (từ `minIndex + 1` đến `maxIndex - 1`) phải có `value == 0` hoặc `isCleared == true`.
  2. **Trục X / Y**: Nếu không Consecutive, buộc phải nằm cùng một hàng (`deltaY == 0`) hoặc cùng một cột (`deltaX == 0`). Tiến hành quét bước nhảy `stepX`, `stepY`. Bất kỳ ô chặn nào có `value != 0` và `isCleared == false` sẽ khiến bước ghép thất bại (Return False).

---

## 5. TÍNH TOÁN SÁT THƯƠNG & RECIPE (DAMAGE CALCULATION)
Hàm `ProcessMatch` xử lý logic ngay khi `HandleMatching` trả về True:
- Đánh dấu 2 ô là đã ăn: `isCleared = true`.
- **Base Damage**: `(val1 + val2) * 2`.
- **Recipe Modifier**: Gọi `GetMatchingRecipeData()`. Nếu có Recipe trùng khớp, kích hoạt Bonus:
  - `FinalDamage = (BaseDamage * 3) + Recipe.recipeCost`.
- Trigger các Effect thông qua hệ thống Command Pattern.

---

## 6. CƠ CHẾ EXTENSION: THÊM SỐ (SERVER ADD NUMBER)
Giới hạn chiến thuật thông qua biến `AddCount`. Được xử lý bởi `ServerAddNumber(clientId)`.
- **Workflow**:
  1. Duyệt toàn bộ `playerBoard`. Lọc ra các `CellData` có `value != 0` và `isCleared == false`.
  2. Đưa vào danh sách sao chép (listCopyNumber).
  3. Tìm ô trống đầu tiên (`value == 0`) trên `playerBoard` (insertIndex).
  4. Nếu số lượng ô cần chèn vượt quá `playerBoard.Count`, kích hoạt hàm `ServerAddMoreCell()`.
- **ServerAddMoreCell**: Tự động cấp phát thêm các hàng ảo (`rowsToAdd * COLUMNS + COLUMNS * 2`). Khởi tạo các CellData trống rỗng (isCleared = true, value = 0) ở cuối mảng. Sau đó điền `listCopyNumber` vào. Điều này biến bàn cờ thành một mảng động (Dynamic Grid), thay vì fix cứng kích thước.

---

## 7. KIẾN TRÚC COMMAND PATTERN & EVENT BUS (CORE REFACTORING)
Đây là kiến trúc mới áp dụng để loại bỏ tính trạng Hardcode/Spaghetti code, phục vụ thiết kế Item, Buff, Debuff phức tạp:
- **GameRoomContext (Pure DTO)**: Là một lớp trung gian. Đầu trận, `GameRoom` chép dữ liệu của `Player1` và `Player2` vào Context dưới cái tên generic là `Player` (Người cast skill) và `Enemy` (Nạn nhân).
  - Tác dụng: Giúp các file Logic (Command) không cần dùng câu lệnh `if (clientId == Player1Id)`. Các file Command chỉ việc tác động lên `EnemyHP` hoặc `PlayerHP`.
- **ICommand Interface**: Khuôn mẫu cho mọi hành động.
  - Các Command triển khai cụ thể: `DamageCommand` (Trừ HP), `TrueDamageCommand` (Xuyên giáp), `ShieldCommand` (Cộng giáp), `ManaCommand` (Cộng năng lượng), `HealCommand` (Hồi máu), `LockCellCommand` (Khóa ô), `ClearJunkCommand` (Xóa rác).
- **CommandProcessor**: Lưu trữ hàng đợi `Queue<ICommand>`. Có method `ExecuteAll(context)` để chạy tuần tự tất cả Command.
- **Event Bus (GameEventBus)**: Hệ thống Pub/Sub tĩnh, decoupling (tách rời) hoàn toàn các module. Khi ghép số, `ProcessMatch` không gọi trực tiếp tới UI, mà chỉ gọi `GameEventBus.TriggerMatchSuccess()`. Các module khác (BuffSystem, SoundManager, UI) sẽ Lắng nghe (Subscribe) để tự thực hiện công việc của chúng.

---

## 8. BUFF SYSTEM (HỆ THỐNG TRẠNG THÁI & HIỆU ỨNG)
(Module Đang Phát Triển)
- **Kiến trúc**: Sử dụng `MicroBuffData` (ScriptableObject) hoặc các struct nhẹ.
- Hỗ trợ các BuffEnums: `BuffType` (Buff/Debuff), `TriggerTiming` (OnMatch, OnDamageReceived, OnTurnEnd), `EffectType` (Poison, Stun, HealOverTime).
- Tích hợp chặt chẽ với Command Pattern: Khi EventBus báo có người nhận sát thương, Buff System sẽ check xem người đó có Buff "Phản sát thương" không. Nếu có, nó tự động sinh ra một `DamageCommand` nhét vào `CommandProcessor`.

---

## 9. ĐIỀU KIỆN KẾT THÚC & DATABASE INTEGRATION
Giám sát thông qua `CheckWinCondition()`.
- **Điều kiện**: Trận đấu dừng lập tức khi `Player1HP <= 0` HOẶC `Player2HP <= 0`.
- **Luồng kết thúc**:
  1. Xác định `winnerClientId` và `loserClientId`.
  2. Mapping ulong ClientId thành chuỗi Username thông qua `ServerAuthManager.GetUsernameForClient()`.
  3. Gọi `ServerMatchManager.Instance.SubmitMatchResult(winner, loser, Action Callback)`.
  4. Server gửi POST request tới Backend Node.js để ghi log trận đấu và tính MMR (Elo rating).
  5. Đợi API trả về MMR mới, gọi `RpcEndMatchClientRpc` gửi cờ `isWinner` và MMR hiển thị lên UI màn hình Victory/Defeat của từng client.
  6. Xóa sổ căn phòng khỏi bộ nhớ Server (`NetworkObject.Despawn()`).

---
**Tổng kết (Summary)**: 
Juice Project sử dụng cách tiếp cận chuẩn mực của Game Server Hiện Đại: **State Pattern** (GameRoom), **Command Pattern** (CommandProcessor), **Event-Driven Architecture** (GameEventBus), và **Dynamic Memory Allocation** (Bàn cờ mở rộng động). Code tối ưu cho việc bổ sung cơ chế mới (Item, Skill) mà không phá vỡ Logic cốt lõi của Network.
