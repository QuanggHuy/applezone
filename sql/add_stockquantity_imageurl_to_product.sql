-- Muc 3: Product CRUD hoan thien
-- Them cot StockQuantity (ton kho) va ImageUrl (link anh san pham) vao bang Product

ALTER TABLE Product ADD StockQuantity int NOT NULL DEFAULT 0;
GO

ALTER TABLE Product ADD ImageUrl nvarchar(max) NULL;
GO
