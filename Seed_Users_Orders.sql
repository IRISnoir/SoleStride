-- SoleStride seed script: 15 demo accounts + order history
-- 1 admin, 3 staff, 11 regular users
-- Passwords (SHA256): admin=Admin@123, staff*=Staff@123, user*=User@123
-- Safe to run multiple times (skips if 'admin' already exists / orders already exist).

SET NOCOUNT ON;

-- ===== 1. USERS =====
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = N'admin')
BEGIN
    PRINT 'Seeding 15 demo accounts...';

    INSERT INTO dbo.Users (Username, Password, Role, FirstName, LastName, Phone, EmailAddress, Birthdate, UserGender) VALUES
    -- 1 admin
    (N'admin',  N'e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7', 0, N'Jordan',   N'Smith',    N'0901000001', N'admin@solestride.com',  '1990-01-15', 0),
    -- 3 staff
    (N'staff1', N'dfd48f36338aa36228ebb9e204bba6b4e18db0b623e25c458901edc831fb18e9', 1, N'Emily',    N'Johnson',  N'0901000002', N'staff1@solestride.com', '1995-03-22', 1),
    (N'staff2', N'dfd48f36338aa36228ebb9e204bba6b4e18db0b623e25c458901edc831fb18e9', 1, N'Michael',  N'Brown',    N'0901000003', N'staff2@solestride.com', '1993-07-11', 0),
    (N'staff3', N'dfd48f36338aa36228ebb9e204bba6b4e18db0b623e25c458901edc831fb18e9', 1, N'Sarah',    N'Davis',    N'0901000004', N'staff3@solestride.com', '1998-12-05', 1),
    -- 11 users
    (N'user1',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'James',    N'Wilson',   N'0902000001', N'user1@solestride.com',  '2000-02-10', 0),
    (N'user2',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Olivia',   N'Martinez', N'0902000002', N'user2@solestride.com',  '2001-05-18', 1),
    (N'user3',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Ethan',    N'Anderson', N'0902000003', N'user3@solestride.com',  '1999-09-30', 0),
    (N'user4',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Sophia',   N'Thomas',   N'0902000004', N'user4@solestride.com',  '2002-01-25', 1),
    (N'user5',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Ava',      N'Moore',    N'0902000005', N'user5@solestride.com',  '2000-11-08', 1),
    (N'user6',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Mia',      N'Clark',    N'0902000006', N'user6@solestride.com',  '2001-07-14', 1),
    (N'user7',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Noah',     N'Jackson',  N'0902000007', N'user7@solestride.com',  '1998-04-02', 0),
    (N'user8',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Isabella', N'Lee',      N'0902000008', N'user8@solestride.com',  '2003-06-19', 1),
    (N'user9',  N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Lucas',    N'Harris',   N'0902000009', N'user9@solestride.com',  '1997-10-27', 0),
    (N'user10', N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Emma',     N'Davis',    N'0902000010', N'user10@solestride.com', '2000-08-12', 1),
    (N'user11', N'3e7c19576488862816f13b512cacf3e4ba97dd97243ea0bd6a2ad1642d86ba72', 2, N'Mason',    N'Lewis',    N'0902000011', N'user11@solestride.com', '2001-12-31', 0);

    -- Demote any leftover test admins so exactly one admin exists
    UPDATE dbo.Users SET Role = 2 WHERE Role = 0 AND Username <> N'admin';

    PRINT 'Done. Seeded 15 demo accounts.';
END
ELSE
BEGIN
    PRINT 'Admin account already exists. Skipping user seed.';
END

-- ===== 2. ORDERS =====
IF (SELECT COUNT(*) FROM dbo.Orders) = 0
BEGIN
    PRINT 'Seeding order history...';

    DECLARE @products TABLE (ProductId uniqueidentifier, Price decimal(18,2), Sale real);
    INSERT INTO @products SELECT ProductId, Price, ISNULL(SalePercentage, 0) FROM dbo.Shoes;

    DECLARE @customers TABLE (RowId int IDENTITY(1,1), Username nvarchar(100));
    INSERT INTO @customers SELECT Username FROM dbo.Users WHERE Role = 2;

    DECLARE @statuses TABLE (RowId int IDENTITY(1,1), Status nvarchar(20));
    INSERT INTO @statuses VALUES
    (N'Delivered'), (N'Delivered'), (N'Delivered'), (N'Delivered'), (N'Delivered'),
    (N'Shipped'), (N'Processing'), (N'Pending');

    DECLARE @addrs TABLE (Addr nvarchar(500));
    INSERT INTO @addrs VALUES
    (N'123 Nguyễn Trãi, P.Bến Thành, Q.1, TP.HCM'),
    (N'45 Lê Lợi, P.Bến Nghé, Q.1, TP.HCM'),
    (N'78 Hai Bà Trưng, P.Đa Kao, Q.1, TP.HCM'),
    (N'12 Lý Thường Kiệt, P.7, Q.10, TP.HCM'),
    (N'66 Cách Mạng Tháng 8, Q.1, TP.HCM'),
    (N'233 Trần Hưng Đạo, Q.1, TP.HCM'),
    (N'15 Nguyễn Huệ, P.Bến Nghé, Q.1, TP.HCM'),
    (N'89 Phan Xích Long, P.2, Q.Phú Nhuận, TP.HCM'),
    (N'100 Võ Văn Ngân, P.Linh Chiểu, TP.Thủ Đức'),
    (N'30 Trần Quang Khải, P.Tân Định, Q.1, TP.HCM');

    DECLARE @phones TABLE (Phone nvarchar(20));
    INSERT INTO @phones VALUES
    (N'0901234567'), (N'0918765432'), (N'0987123456'), (N'0935556677'),
    (N'0908989898'), (N'0912345678'), (N'0977111222'), (N'0900123456');

    DECLARE @receivers TABLE (Receiver nvarchar(100));
    INSERT INTO @receivers VALUES
    (N'James Wilson'), (N'Emily Johnson'), (N'Michael Brown'), (N'Sarah Davis'),
    (N'Olivia Martinez'), (N'Ethan Anderson'), (N'Sophia Thomas'), (N'Liam Taylor'),
    (N'Ava Moore'), (N'Noah Jackson'), (N'Isabella Lee'), (N'Mason Lewis');

    DECLARE @notes TABLE (Note nvarchar(500));
    INSERT INTO @notes VALUES
    (N''), (N''), (N''), (N''), (N''), (N''), (N''),
    (N'Giao hàng trong giờ hành chính nhé.'),
    (N'Đóng gói cẩn thận giúp em ạ.'),
    (N'Đổi size sang lớn hơn nếu còn hàng.'),
    (N'Gọi điện trước khi giao hàng.'),
    (N'Là quà tặng, đóng gói đẹp giúp mình.');

    DECLARE @i int = 1;
    WHILE @i <= 70
    BEGIN
        DECLARE @username nvarchar(100), @receiver nvarchar(100), @phone nvarchar(20), @addr nvarchar(500), @note nvarchar(500), @status nvarchar(20);
        DECLARE @monthOffset int = ABS(CHECKSUM(NEWID())) % 7;

        SELECT TOP 1 @username = Username FROM @customers ORDER BY NEWID();
        SELECT TOP 1 @receiver = Receiver FROM @receivers ORDER BY NEWID();
        SELECT TOP 1 @phone = Phone FROM @phones ORDER BY NEWID();
        SELECT TOP 1 @addr = Addr FROM @addrs ORDER BY NEWID();
        SELECT TOP 1 @note = Note FROM @notes ORDER BY NEWID();
        SELECT TOP 1 @status = Status FROM @statuses ORDER BY NEWID();

        IF @monthOffset >= 2 SET @status = N'Delivered';
        IF @monthOffset = 1 AND (ABS(CHECKSUM(NEWID())) % 2 = 0) SET @status = N'Shipped';

        DECLARE @orderDate datetime = DATEADD(day, ABS(CHECKSUM(NEWID())) % 26,
                DATEADD(month, -@monthOffset, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)));

        INSERT INTO dbo.Orders (Username, OrderDate, TotalAmount, Status, ShippingAddress, ReceiverName, Phone, CustomerNote)
        VALUES (@username, @orderDate, 0, @status, @addr, @receiver, @phone,
                CASE WHEN @note = N'' THEN NULL ELSE @note END);

        DECLARE @orderId int = SCOPE_IDENTITY();
        DECLARE @itemCount int = 1 + ABS(CHECKSUM(NEWID())) % 3;
        DECLARE @total decimal(18,2) = 0;
        DECLARE @j int = 1;
        WHILE @j <= @itemCount
        BEGIN
            DECLARE @productId uniqueidentifier, @price decimal(18,2), @sale real, @qty int;
            SELECT TOP 1 @productId = ProductId, @price = Price, @sale = Sale FROM @products ORDER BY NEWID();
            SET @qty = 1 + ABS(CHECKSUM(NEWID())) % 3;
            DECLARE @finalPrice decimal(18,2) = ROUND(@price * (1 - ISNULL(@sale, 0) / 100.0), 2);
            INSERT INTO dbo.OrderDetails (OrderId, ProductId, Quantity, Price)
            VALUES (@orderId, @productId, @qty, @finalPrice);
            SET @total = @total + @finalPrice * @qty;
            SET @j = @j + 1;
        END

        UPDATE dbo.Orders SET TotalAmount = @total WHERE OrderId = @orderId;
        SET @i = @i + 1;
    END

    -- Mark ~50% of stock units as sold to reflect inventory stats
    UPDATE dbo.ShoeStocks
    SET Status = 1,
        PurchaseDate = DATEADD(day, -ABS(CHECKSUM(NEWID())) % 180, GETDATE())
    WHERE StockId IN (SELECT TOP 90 StockId FROM dbo.ShoeStocks ORDER BY NEWID());

    PRINT 'Done. Seeded 70 orders with order details.';
END
ELSE
BEGIN
    PRINT 'Orders table already has data. Skipping order seed.';
END
