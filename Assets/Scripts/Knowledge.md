# 🏛️ Kiến Trúc Server Production: "Matchmaker & Dedicated Server Pattern"

Tài liệu này được biên soạn để ghi nhớ kiến trúc hệ thống mạng chuẩn mực (Industry Standard) khi triển khai các game Multiplayer (đặc biệt là dạng PvP đối kháng) lên môi trường Production thực tế (VPS, AWS, Google Cloud, v.v.).

## ❌ Sai Lầm Thường Gặp Của Người Mới (Beginner Mistake)
- **Hardcode IP ở Client:** Việc nhúng cứng địa chỉ IP (VD: `192.168.1.1` hoặc IP tĩnh của VPS) trực tiếp vào file của Unity Client (như `ConnectionManager.cs`).
- **Hậu quả:** 
  1. Rất dễ bị tấn công DDoS vì IP lộ liễu.
  2. Bất khả thi trong việc mở rộng (Scale). Nếu 10.000 người cùng chơi, 1 Server duy nhất sẽ sập.
  3. Rất khó bảo trì. Mỗi lần đổi IP Server là phải ép người chơi cập nhật lại toàn bộ Game.

## ✅ Giải Pháp Chuẩn Mực: Mô Hình "Director / Matchmaker"
Kiến trúc này tách biệt hoàn toàn việc "Tìm Trận" và "Chơi Game". Nó được cấu thành từ 3 phần tử chính:

1. **Game Client (Unity):**
   - Hoàn toàn KHÔNG biết IP của Game Server.
   - Chỉ kết nối HTTP đến Web API (NodeJS) để thực hiện các tính năng như Đăng nhập, Mua đồ, Tìm trận.
2. **Matchmaker / Web API (NodeJS):**
   - Đóng vai trò là "Giám đốc điều phối" (Orchestrator).
   - Tiếp nhận yêu cầu tìm trận của người chơi. Nhóm những người cùng rank (MMR) lại với nhau.
   - Khi ghép cặp thành công, NodeJS sẽ **chỉ định** (Allocate) một Dedicated Server đang rảnh rỗi.
   - NodeJS lấy IP động (Dynamic IP) và Cổng (Port) của Server rảnh rỗi đó, đóng gói vào JSON (ví dụ biến `serverIp` và `serverPort`) rồi gửi về lại cho Game Client.
3. **Dedicated Game Server (Unity Server Build):**
   - Là phiên bản Game chạy ẩn (Headless) nằm trên VPS hoặc Cloud (Kubernetes, AWS GameLift).
   - Chúng có IP động (thay đổi liên tục khi bị tắt/bật lại). 
   - Chỉ chờ Game Client cầm "Vé" (JWT Token, IP, Port) đến để kết nối và điều hành trận đấu.

### Luồng Hoạt Động Cốt Lõi (Flow)
1. `Client` gọi `POST /match/find` lên NodeJS.
2. `NodeJS` tìm thấy đối thủ, cấp phát một phòng trên Server #X.
3. `NodeJS` gửi về `{"status": "match_found", "serverIp": "203.0.113.1", "serverPort": "7777"}`.
4. `Client` đọc JSON, giải mã IP và Port, sau đó gọi `NetworkManager.Singleton.StartClient()` để kết nối ĐÚNG vào Server #X đó.

---

## 🔗 Các Nguồn Tham Khảo Thực Tế (Industry References)

Nếu bạn muốn đào sâu hơn về cách các "Ông lớn" trong ngành Game xây dựng hệ thống cấp phát IP Động này, đây là các tài liệu đáng giá:

1. **Agones (Google & Ubisoft phát triển):** Tiêu chuẩn vàng hiện tại để quản lý Dedicated Server bằng Kubernetes. Nó tự động cấp phát IP/Port động mỗi khi có trận mới. 
   - [Đọc về Agones Architecture](https://agones.dev/site/docs/getting-started/game-server-concepts/)
2. **AWS GameLift:** Dịch vụ của Amazon giúp Scale Game Server tự động toàn cầu. Matchmaker (FlexMatch) của AWS hoạt động chính xác theo cơ chế cấp phát IP/Port trả về cho Client.
   - [Kiến trúc AWS GameLift](https://aws.amazon.com/gamelift/)
3. **Edgegap / i3D.net:** Các bài blog phân tích về tầm quan trọng của việc tách biệt Matchmaker và Dedicated Server để giảm độ trễ (Ping).
   - [Bài báo: Multiplayer Game Architecture (Medium)](https://medium.com/@anton_6802/game-server-architecture-overview-for-multiplayer-games-87e07662c5b3)
   - [Bài báo: How Dedicated Game Servers Work](https://gameye.com/blog/dedicated-game-servers-explained/)

> **Ghi chú cho JuiceProject:** Code Backend NodeJS của dự án đã được thiết kế đúng theo chuẩn này (trả về `serverIp` và `serverPort`). Công việc Refactor duy nhất cần làm là sửa `ConnectionManager.cs` để nhận IP/Port truyền vào từ `MainMenuManager.cs`.
