-- SoleStride seed script: categories + 30 products + stock
-- Safe to run multiple times (skips if products already exist).

IF (SELECT COUNT(*) FROM dbo.Shoes) = 0
BEGIN
    PRINT 'Seeding categories and 30 products...';

    -- ===== Categories =====
    INSERT INTO dbo.Category (CategoryId, CategoryName) VALUES
    (N'SNK', N'Sneakers'),
    (N'RUN', N'Running'),
    (N'BST', N'Basketball'),
    (N'CSL', N'Casual'),
    (N'BOT', N'Boots'),
    (N'SFD', N'Sandals');

    -- ===== Products =====
    DECLARE @products TABLE (
        RowId INT IDENTITY(1,1),
        ProductId uniqueidentifier,
        ShoesName nvarchar(100),
        CategoryId nvarchar(10),
        ShoesGender int,
        ShoesSize int,
        ShoesColor nvarchar(50),
        Material nvarchar(50),
        Description nvarchar(500),
        Price decimal(18,2),
        SalePercentage real
    );

    INSERT INTO @products (ProductId, ShoesName, CategoryId, ShoesGender, ShoesSize, ShoesColor, Material, Description, Price, SalePercentage) VALUES
    (NEWID(), N'Nike Air Max 270', N'SNK', 2, 10, N'Black', N'Mesh', N'Iconic Air cushioning with a sleek urban look.', 129.99, 10),
    (NEWID(), N'Adidas Ultraboost 22', N'RUN', 0, 11, N'White', N'Knit', N'Responsive Boost foam for all-day comfort.', 179.99, 15),
    (NEWID(), N'Puma RS-X', N'SNK', 0, 9, N'Blue', N'Leather', N'Retro-inspired chunky sneaker with bold styling.', 89.99, 0),
    (NEWID(), N'Converse Chuck Taylor All Star', N'SNK', 2, 8, N'Red', N'Canvas', N'Timeless canvas sneaker for everyday style.', 49.99, 20),
    (NEWID(), N'Vans Old Skool', N'CSL', 2, 7, N'Black', N'Suede', N'Classic skate shoe with the iconic side stripe.', 59.99, 0),
    (NEWID(), N'New Balance 574', N'CSL', 2, 10, N'Grey', N'Suede', N'Versatile everyday sneaker with ENCAP midsole.', 74.99, 5),
    (NEWID(), N'Asics Gel-Kayano 29', N'RUN', 1, 7, N'Pink', N'Mesh', N'Premium stability running shoe for long miles.', 159.99, 0),
    (NEWID(), N'Nike Air Force 1', N'SNK', 0, 10, N'White', N'Leather', N'The classic hoops sneaker turned street icon.', 109.99, 12),
    (NEWID(), N'Adidas Superstar', N'SNK', 1, 6, N'White', N'Leather', N'Shelly-toe classic with the iconic three stripes.', 84.99, 0),
    (NEWID(), N'Reebok Classic Leather', N'SNK', 2, 9, N'Navy', N'Leather', N'Understated leather sneaker that never goes out of style.', 69.99, 10),
    (NEWID(), N'Skechers Go Run', N'RUN', 1, 8, N'Purple', N'Mesh', N'Lightweight and breathable for your daily run.', 89.99, 0),
    (NEWID(), N'Brooks Ghost 15', N'RUN', 0, 12, N'Blue', N'Mesh', N'Smooth and balanced cushioning for neutral runners.', 139.99, 0),
    (NEWID(), N'Hoka Bondi 8', N'RUN', 2, 11, N'Grey', N'Knit', N'Maximum cushioning for plush, smooth landings.', 164.99, 0),
    (NEWID(), N'Saucony Ride 15', N'RUN', 0, 10, N'Red', N'Mesh', N'Reliable daily trainer with comfortable support.', 129.99, 8),
    (NEWID(), N'Nike Pegasus 40', N'RUN', 1, 7, N'Black', N'Mesh', N'The dependable everyday running shoe.', 124.99, 0),
    (NEWID(), N'Jordan Retro 1', N'BST', 0, 10, N'Red', N'Leather', N'The shoe that started it all. A true legend.', 169.99, 25),
    (NEWID(), N'LeBron 20', N'BST', 0, 11, N'Gold', N'Knit', N'Elite performance basketball shoe for the court.', 199.99, 0),
    (NEWID(), N'Curry 10', N'BST', 0, 9, N'White', N'Mesh', N'Lightweight and grippy for quick cuts.', 159.99, 0),
    (NEWID(), N'Kyrie 8', N'BST', 0, 10, N'Blue', N'Knit', N'Aggressive traction for fast ball handlers.', 119.99, 15),
    (NEWID(), N'Timberland Classic Boot', N'BOT', 0, 10, N'Wheat', N'Leather', N'Rugged waterproof boot built for any terrain.', 189.99, 0),
    (NEWID(), N'Dr. Martens 1460', N'BOT', 1, 6, N'Black', N'Leather', N'Iconic 8-eye boot with air-cushioned sole.', 169.99, 10),
    (NEWID(), N'Ugg Classic Boot', N'BOT', 1, 7, N'Chestnut', N'Sheepskin', N'Cozy sheepskin boot perfect for winter.', 139.99, 0),
    (NEWID(), N'Havaianas Flip Flop', N'SFD', 2, 9, N'Green', N'Rubber', N'Simple, comfortable rubber flip flops.', 19.99, 0),
    (NEWID(), N'Birkenstock Arizona', N'SFD', 1, 8, N'Brown', N'Suede', N'Contoured footbed sandal with two straps.', 99.99, 0),
    (NEWID(), N'Teva Hurricane', N'SFD', 0, 10, N'Grey', N'Nylon', N'Adventure sandal with quick-dry straps.', 49.99, 20),
    (NEWID(), N'Crocs Classic Clog', N'SFD', 2, 8, N'White', N'Foam', N'Lightweight foam clog with ventilated design.', 39.99, 10),
    (NEWID(), N'Adidas Stan Smith', N'SNK', 2, 9, N'White', N'Leather', N'Clean minimal tennis shoe with retro charm.', 79.99, 0),
    (NEWID(), N'Nike Dunk Low', N'SNK', 0, 10, N'Black', N'Leather', N'Streetwear staple with classic hoops DNA.', 109.99, 0),
    (NEWID(), N'On Cloud', N'RUN', 1, 7, N'White', N'Mesh', N'CloudTec cushioning for a soft landing feel.', 139.99, 5),
    (NEWID(), N'Salomon Speedcross 5', N'RUN', 0, 11, N'Black', N'Mesh', N'Trail shoe with aggressive grip for off-road.', 129.99, 0);

    -- ===== Insert products (generate SkuId like the app does) =====
    INSERT INTO dbo.Shoes (ProductId, ShoesName, SkuId, CategoryId, ShoesGender, ShoesSize, ShoesColor, Material, Description, Price, SalePercentage)
    SELECT
        p.ProductId,
        p.ShoesName,
        CONCAT(p.CategoryId, '-',
               CASE p.ShoesGender WHEN 0 THEN 'M' WHEN 1 THEN 'W' ELSE 'U' END,
               '-', p.ShoesSize,
               '-', UPPER(LEFT(p.ShoesColor, 3))),
        p.CategoryId,
        p.ShoesGender,
        p.ShoesSize,
        p.ShoesColor,
        p.Material,
        p.Description,
        p.Price,
        p.SalePercentage
    FROM @products p;

    -- ===== Stock: 6 available units per product =====
    INSERT INTO dbo.ShoeStocks (ProductId, Status, EntryDate, PurchaseDate)
    SELECT p.ProductId, 0, DATEADD(day, -ABS(CHECKSUM(NEWID())) % 30, GETDATE()), NULL
    FROM @products p
    CROSS JOIN (VALUES (1),(2),(3),(4),(5),(6)) AS v(n);

    PRINT 'Done. Seeded 30 products with 180 stock units.';
END
ELSE
BEGIN
    PRINT 'Shoes table already has data. Skipping seed.';
END
