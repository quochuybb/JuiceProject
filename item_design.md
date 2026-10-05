# 🎁 Item Design Document — JuiceProject

## Triết Lý Thiết Kế

> *"Item phải tạo ra những khoảnh khắc mà người chơi sẽ HÉT lên, chụp màn hình gửi bạn bè, và kể lại cho nhau nghe."*

Lấy cảm hứng từ hệ thống Item của **Mario Kart** — nơi mà một quả Mai Rùa Xanh có thể lật ngược cả cuộc đua ở vòng cuối cùng. Item trong JuiceProject phải mang lại cảm giác:
- **Bất ngờ** (Không biết sẽ nhận được gì)
- **Kịch tính** (Mỗi Item đều có tác động rõ ràng, nhìn thấy được)
- **Comeback** (Người đang thua có cơ hội lật kèo)

---

## Cơ Chế Nhận Item

### Sạc Mana → Quay Gacha Item
- Mỗi lần ghép thành công (cả ghép thường lẫn công thức) → Thanh **Mana +1**
- Khi Mana **đầy** → Hệ thống **Random** 1 Item theo tỷ lệ hiếm
- Item được nhận sẽ nằm trong **Ô chứa Item** (tối đa giữ 2 Item cùng lúc)
- Người chơi **chủ động bấm** để sử dụng Item

### Tỷ Lệ Rơi (Drop Rate)

| Độ hiếm | Tỷ lệ bình thường | Tỷ lệ khi đang THUA (< 30% HP) |
|---------|--------------------|---------------------------------|
| ⭐ Common | 40% | 20% |
| ⭐⭐ Rare | 30% | 30% |
| ⭐⭐⭐ Epic | 20% | 30% |
| ⭐⭐⭐⭐ Legendary | 10% | 20% |

> [!TIP]
> **Cơ chế Comeback (Lấy cảm hứng Mario Kart):** Khi máu dưới 30%, tỷ lệ nhận Item Epic/Legendary tăng gấp đôi. Điều này ngăn việc "một chiều" (Snowball) và tạo ra những màn lội ngược dòng kinh điển.

---

## Danh Sách Item Theo Độ Hiếm

### ⭐ COMMON — Đồ Phổ Thông (5 Item)
> *Tác động nhẹ, hỗ trợ cơ bản. Người chơi nhận được và nghĩ: "Được, cũng ổn."*

| # | Item | Hình thù | Hiệu ứng | Mô tả ngắn |
|---|------|----------|-----------|-------------|
| 1 | 🍬 Kẹo Hồi Phục | Viên kẹo xoắn ốc màu hồng | 💚 Hồi 3 HP | Hồi lượng máu nhỏ, cứu nguy tạm thời |
| 2 | 🪨 Viên Sỏi | Hòn đá nhỏ xám nhọn | ⚔️ Gây 2 HP | Ném nhẹ, sát thương khiêm tốn |
| 3 | 🫧 Bong Bóng | Quả bóng trong suốt lấp lánh | 🛡️ +2 Giáp | Bọc mình trong bong bóng bảo vệ |
| 4 | 🪙 Xu May Mắn | Đồng xu vàng xoay tròn | ⚡ +3 Mana bonus | Sạc nhanh thanh Item tiếp theo |
| 5 | 🧹 Chổi Phép | Cây chổi nhỏ có tia sáng | 🧹 Dọn 3 ô rác/khóa trên bàn mình | Giải phóng bàn cờ khỏi hiệu ứng Control |

---

### ⭐⭐ RARE — Đồ Hiếm (5 Item)
> *Tác động trung bình, bắt đầu tạo chiến thuật. Người chơi: "Ooh, cái này ngon đây!"*

| # | Item | Hình thù | Hiệu ứng | Mô tả ngắn |
|---|------|----------|-----------|-------------|
| 6 | 🍌 Vỏ Chuối | Vỏ chuối vàng đặt nghiêng | 🪤 Đặt bẫy lên 2 ô ngẫu nhiên của địch. Khi địch ghép trúng ô bẫy → Mất 3 HP thay vì gây sát thương | Giống Banana Peel trong Mario Kart! Địch không thấy bẫy ở đâu |
| 7 | 🌶️ Ớt Cay | Trái ớt đỏ cháy lửa | ⚔️ Gây 3 HP + Hủy hiệu ứng của lần ghép công thức tiếp theo của địch | Vừa đánh vừa phá combo đối thủ |
| 8 | 🐙 Mực Phun | Con mực tím phun mực | 🌫️ Che giấu tất cả số trên bàn cờ địch trong 3 giây | Địch bị "mù" không thấy số, ghép bừa! Cảm giác y hệt Blooper trong Mario Kart |
| 9 | ⚡ Tia Sét | Tia chớp vàng zigzag | ⚔️ Gây 4 HP trực tiếp | Đánh thẳng, không hoa mỹ, sát thương cao |
| 10 | 🍯 Mật Ong | Lọ mật ong vàng sánh | 🐌 Làm chậm toàn bộ hoạt ảnh trên bàn địch 50% trong 4 giây | Địch thấy ô rơi chậm như rùa, cực kỳ bứt rứt nhưng vẫn chơi được |

---

### ⭐⭐⭐ EPIC — Đồ Sử Thi (5 Item)
> *Tác động mạnh, thay đổi cục diện trận đấu. Người chơi: "LET'S GOOO!"*

| # | Item | Hình thù | Hiệu ứng | Mô tả ngắn |
|---|------|----------|-----------|-------------|
| 11 | 🌪️ Lốc Xoáy | Vòi rồng xoáy màu xám | 🔀 Xáo trộn TOÀN BỘ vị trí ô trên bàn cờ địch | Bàn cờ địch đang đẹp → bị vò nát! Mọi kế hoạch ghép bị hủy |
| 12 | ☠️ Bình Thuốc Độc | Lọ thuốc xanh lá bốc khói | ☠️ Nhiễm độc: Địch mất 1 HP mỗi 2 giây, kéo dài 8 giây (tổng -4 HP) | Sát thương rỉ rả, tạo áp lực thời gian cực lớn |
| 13 | 💣 Bom Hẹn Giờ | Quả bom tròn đen có đồng hồ đỏ | 💣 Đặt bom lên bàn địch. Sau 5 giây: NỔ! Phá hủy vùng 3×3 + gây 3 HP | Địch thấy bom đếm ngược trên bàn mình, hoảng loạn! Có thể cố ghép ô xung quanh bom để giảm thiệt hại |
| 14 | 🪞 Gương Phản Chiếu | Tấm gương oval có viền bạc | 🔄 Phản chiếu Item tiếp theo mà địch dùng ngược lại chính họ | Địch ném Bom → Bom nổ trên bàn họ! Mind game cực đỉnh |
| 15 | 🔥 Lửa Cuồng Nộ | Ngọn lửa đỏ-cam bùng cháy | 💪 x2 hiệu ứng cho 3 lần ghép công thức tiếp theo | Damage x2, Shield x2, Mana x2. Kết hợp với build Aggro = HỦY DIỆT |

---

### ⭐⭐⭐⭐ LEGENDARY — Đồ Huyền Thoại (5 Item)
> *Game-changer tuyệt đối. Ai nhận được Item này, cả hai người chơi đều HÉT. Tạo ra khoảnh khắc "Hall of Fame" đáng nhớ mãi.*

| # | Item | Hình thù | Hiệu ứng | Mô tả ngắn |
|---|------|----------|-----------|-------------|
| 16 | 🌠 Sao Băng | Ngôi sao vàng rực kéo theo vệt lửa | ☄️ Gây 8 HP + Phá hủy toàn bộ 1 hàng ngang trên bàn địch | Đòn tấn công mạnh nhất game. Nhìn sao rơi xuống bàn địch cực mãn nhãn |
| 17 | 🕳️ Hố Đen | Vòng xoáy tím đen hút mọi thứ | 🔄 **HOÁN ĐỔI** toàn bộ bàn cờ của bạn với bàn cờ của địch | Địch sắp xếp bàn cờ đẹp? Giờ là của bạn! Bạn bàn bừa bộn? Giờ là của họ! Rủi ro + Phần thưởng cực cao |
| 18 | ⏳ Đồng Hồ Cát | Đồng hồ cát vàng lấp lánh | ⏸️ Đóng băng hoàn toàn địch 3 giây + Bạn gây x2 damage trong thời gian đó | 3 giây vàng: Địch chỉ biết đứng nhìn bạn tàn sát. Kết hợp với Cuồng Nộ = x4 damage! |
| 19 | 🧲 Nam Châm | Thanh nam châm đỏ-xanh hình chữ U | 🫳 **ĐÁNH CẮP** Item đang giữ của đối thủ. Nếu địch không có Item → Cướp 3 Giáp | Cướp Item Legendary của địch rồi dùng ngược lại họ. Tàn nhẫn! |
| 20 | 🐉 Rồng Lửa | Con rồng nhỏ phun lửa | 🔥 Phun lửa thiêu bàn địch: Phá ngẫu nhiên 6 ô + Mỗi ô bị phá gây 1 HP (tổng tối đa 6 HP) | Cả bàn cờ địch bốc cháy, ô rơi lả tả. Hiệu ứng hình ảnh hoành tráng nhất game |

---

## Tương Tác Đặc Biệt Giữa Các Item

Các tổ hợp Item tạo ra chiến thuật sâu hơn:

| Combo | Hiệu ứng | Kịch tính |
|-------|----------|-----------|
| 🪞 Gương + Địch ném 💣 Bom | Bom phản ngược, nổ trên bàn địch! | ⭐⭐⭐⭐⭐ |
| 🔥 Cuồng Nộ + Recipe Aggro (Đỏ-Cam) | -4 HP biến thành -8 HP (x2)! | ⭐⭐⭐⭐⭐ |
| ⏳ Đồng Hồ Cát + 🔥 Cuồng Nộ | x2 × x2 = x4 damage trong 3 giây! | ⭐⭐⭐⭐⭐ |
| 🧲 Nam Châm + Cướp 🌠 Sao Băng | Cướp vũ khí mạnh nhất của địch rồi dùng luôn | ⭐⭐⭐⭐⭐ |
| 🍌 Vỏ Chuối + ☠️ Thuốc Độc | Địch vừa dính bẫy mất 3 HP vừa bị nhiễm độc | ⭐⭐⭐⭐ |
| 🐙 Mực Phun + 🌪️ Lốc Xoáy | Xáo trộn bàn địch + Che mất số = Bàn cờ hỗn loạn hoàn toàn | ⭐⭐⭐⭐ |

---

## Item vs Build Matchup (Tương tác với Lối Chơi)

| Build | Item yêu thích | Item sợ nhất |
|-------|---------------|--------------|
| 🔴 Aggro | 🔥 Cuồng Nộ (x2 damage) | 🫧 Bong Bóng (Giáp chặn burst) |
| 🔵 Tank | 🍬 Kẹo Hồi Phục (Thêm máu) | ☠️ Thuốc Độc (Damage rỉ rả qua Giáp) |
| 🟡 Tempo | 🪙 Xu May Mắn (Sạc thêm) | 🧲 Nam Châm (Bị cướp Item) |
| ⚫ Control | 💣 Bom Hẹn Giờ (Thêm áp lực) | 🧹 Chổi Phép (Dọn hết ô khóa) |

---

## Quy Tắc Cân Bằng (Balance Rules)

1. **Tối đa giữ 2 Item** — Không tích trữ quá nhiều
2. **Không thể dùng 2 Item cùng lúc** — Phải chờ hiệu ứng Item trước kết thúc
3. **Gương Phản Chiếu chỉ phản 1 lần** — Không tạo vòng lặp phản chiếu vô hạn
4. **Comeback Rate** — Người thua được Item tốt hơn, đảm bảo trận nào cũng hồi hộp đến phút cuối
5. **Thuốc Độc không cộng dồn** — Chỉ 1 hiệu ứng Độc tại 1 thời điểm, tránh spam chết tức tưởi
6. **Hố Đen hoán đổi nguyên trạng** — Ô khóa, bẫy, bom trên bàn cũng bị hoán đổi theo!
