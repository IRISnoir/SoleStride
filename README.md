# SoleStride — Development Team Guidelines
**Đội ngũ:** 5 developers | **Quy trình:** Scrum, 4 sprint | **Phạm vi:** 6 epic, 40 user story (US01–US40)

---

## 1. Tổng quan dự án

SoleStride là website thương mại điện tử (ecommerce) bán giày, được phát triển theo mô hình Scrum trong 4 sprint. Backlog chính thức gồm 6 epic:

| Epic | Tên | Số US |
|---|---|---|
| E01 | Quản lý tài khoản & Phân quyền | US01–US07 |
| E02 | Danh mục sản phẩm & khuyến mãi | US08–US14 |
| E03 | Giỏ hàng & thanh toán | US15–US23 |
| E04 | Đánh giá sản phẩm | US24–US28 |
| E05 | Quản trị (Admin) | US29–US35 |
| E06 | Chăm sóc khách hàng (CSKH) | US36–US40 |

Tài liệu này là **guideline làm việc chung** cho cả team — không phải quy định cứng nhắc, mà là baseline để mọi người code, review, và giao tiếp theo cùng một chuẩn.

---

## 2. Vai trò trong team (5 người)

Gợi ý phân chia (điều chỉnh theo năng lực thực tế của từng thành viên):

| Vai trò | Thành viên | Trách nhiệm chính |
|---|---|---|
| Backend Developer | Hoàng, Giàu, Khoa | API, database, business logic (auth, order, payment) |
| Frontend Developer | Khoa, Hiệp | UI/UX, tích hợp API, responsive design |
| Fullstack / QA Lead | Hoàng | Hỗ trợ cả hai phía, review chéo, kiểm thử, phối hợp với Scrum Master (nếu không có PM riêng) |

Mỗi epic nên có **1 người chịu trách nhiệm chính (owner)** để tránh chồng chéo, dù các dev khác vẫn có thể hỗ trợ.

---

## 3. Quy trình Scrum

- **Sprint length:** 4 tuần/sprint, tổng 4 sprint.
- **Daily standup:** 15 phút/ngày — mỗi người trả lời 3 câu: hôm qua làm gì, hôm nay làm gì, có blocker không.
- **Sprint Planning:** đầu mỗi sprint, chọn US từ backlog, ước lượng story point, gán người phụ trách.
- **Sprint Review:** cuối sprint, demo tính năng đã hoàn thành cho cả team (và giảng viên/khách hàng nếu có).
- **Sprint Retrospective:** rút kinh nghiệm — cái gì tốt, cái gì cần cải thiện, action item cho sprint sau.

### Definition of Ready (DoR)
Một US được coi là "sẵn sàng" để đưa vào sprint khi:
- Có mô tả rõ ràng theo format "Là [role], tôi muốn [action], để [benefit]"
- Có acceptance criteria cụ thể
- Đã được team ước lượng story point

### Definition of Done (DoD)
Một US được coi là "hoàn thành" khi:
- Code đã được implement đầy đủ acceptance criteria
- Đã qua code review (ít nhất 1 người approve)
- Đã test (unit test tối thiểu cho logic quan trọng, test thủ công cho UI)
- Đã merge vào repo chính và không lỗi
- Không phá vỡ (break) các chức năng đã có

---

## 4. Git workflow

### Commit message
Tuân theo chuẩn Conventional Commits:
```
feat: Thêm tính năng...
```

### Pull Request (PR)
- PR cần có: mô tả ngắn, US liên quan, ảnh chụp màn hình (nếu có UI thay đổi).
- **Bắt buộc ít nhất 1 reviewer approve** trước khi merge.
- Không tự merge PR của chính mình.

---

## 5. Coding standards

- **Naming convention:** đặt tên biến/hàm rõ nghĩa, tiếng Anh, theo convention của camelCase.
- **Cấu trúc thư mục:** thống nhất theo epic/module (ví dụ: `auth/`, `catalog/`, `cart/`, `reviews/`, `admin/`, `support/`) để dễ map với 6 epic ở trên.
- **Code review checklist:**
  - Code có đúng acceptance criteria của US không?
  - Có xử lý edge case (input rỗng, lỗi mạng, quyền truy cập...) không?
  - Có hard-code giá trị nhạy cảm (API key, mật khẩu...) không?
  - Có comment/docstring cho phần logic phức tạp không?
- **Linter/formatter:** cả team dùng chung 1 bộ config (ESLint/Prettier hoặc tương đương) để tránh diff code chỉ vì format khác nhau.

---

## 6. Kiểm thử (Testing & QA)

- Mỗi US thuộc luồng quan trọng (đăng ký, đăng nhập, thanh toán, đơn hàng) cần có **test case thủ công** trước khi coi là Done.
- Ưu tiên viết unit test cho business logic (tính giá, xử lý giỏ hàng, phân quyền).
- QA lead review lại toàn bộ tính năng trước Sprint Review.

---

## 7. Giao tiếp trong team

- **Kênh chính:** nhóm chat chung (Discord/Zalo/Slack — thống nhất 1 kênh duy nhất) cho trao đổi nhanh; issue tracker (Trello/Jira/GitHub Projects) cho theo dõi task.
- **Thời gian phản hồi:** cố gắng phản hồi trong ngày làm việc; nếu bận, báo trước cho team để không bị block.
- **Khi gặp blocker:** báo ngay trong standup hoặc kênh chat, đừng để tự "âm thầm" xử lý quá lâu.
- **Xung đột ý kiến kỹ thuật:** ưu tiên thảo luận dựa trên trade-off cụ thể (hiệu năng, thời gian, độ phức tạp), không quyết định cảm tính.

---

## 8. Gợi ý phân bổ epic theo sprint

*(Chỉ mang tính gợi ý ban đầu — team tự điều chỉnh trong Sprint Planning theo thời gian.)*

| Sprint | Epic trọng tâm |
|---|---|
| Sprint 1 | E01 (Quản lý tài khoản & Phân quyền), bắt đầu E02 (Danh mục sản phẩm) |
| Sprint 2 | Hoàn thiện E02, E03 (Giỏ hàng & thanh toán) |
| Sprint 3 | Hoàn thiện E03, E04 (Đánh giá sản phẩm), bắt đầu E05 (Quản trị) |
| Sprint 4 | Hoàn thiện E05, E06 (CSKH), buffer để fix bug & test tổng thể |

---

## 9. Quy trình phát triển MVC (C#/ASP.NET)

Dự án sử dụng kiến trúc **MVC (Model – Controller – View)**. Thứ tự làm việc chuẩn cho mỗi feature/US:

1. **Model trước** — định nghĩa entity (`Product`, `User`, `Order`, `CartItem`...), tạo/migrate database (Entity Framework). Map trực tiếp theo field cần thiết của US liên quan.
2. **Controller** — tạo Controller tương ứng (`ProductController`, `AccountController`, `CartController`...), viết Action method xử lý logic, gọi Service/Repository.
3. **View** — tạo `.cshtml` sau cùng khi đã rõ Controller trả về dữ liệu gì.

### Lưu ý khi triển khai MVC
- **ViewModel ≠ Model:** tách riêng ViewModel cho từng View để không lộ field nhạy cảm (password hash...) và để gộp dữ liệu form (ví dụ confirm password) không có trong Model gốc.
- **Validation:** dùng Data Annotations (`[Required]`, `[EmailAddress]`...) trên Model/ViewModel, áp dụng cho cả client-side lẫn server-side — đặc biệt quan trọng với US01 (đăng ký), US04 (quên mật khẩu).
- **Routing:** kiểm tra route mặc định (`{controller}/{action}/{id}`) có đủ dùng không, hay cần custom route (ví dụ US13 tìm kiếm sản phẩm với query string).
- **Dependency Injection:** inject Service/Repository vào Controller qua constructor, không `new` trực tiếp — dễ test, dễ maintain.
- **Partial View / Layout:** tách Partial View cho phần lặp lại (header, mini-cart, navbar danh mục) để tránh duplicate code.
- **Async:** dùng `async`/`await` (`Task<IActionResult>`) cho Action gọi database, tránh block thread khi traffic tăng.

---

## 10. Tài liệu & bàn giao

- Mỗi sprint có file `GUIDELINES_SprintN.md` tại thư mục gốc, mô tả API endpoint / component chính liên quan trong các User Story.
- Cuối mỗi sprint, cập nhật changelog ngắn gọn (US nào Done, US nào carry-over sang sprint sau) vào file `CHANGELOG.md` tại root repo.