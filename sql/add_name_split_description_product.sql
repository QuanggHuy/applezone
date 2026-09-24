-- Thêm cột Name (tách khỏi Description), mở rộng Description thành mô tả dài thật (Unicode, không bắt buộc)

ALTER TABLE Product ADD Name nvarchar(200) NULL;
GO

UPDATE Product SET Name = Description;
GO

ALTER TABLE Product ALTER COLUMN Name nvarchar(200) NOT NULL;
GO

ALTER TABLE Product ALTER COLUMN Description nvarchar(1000) NULL;
GO

UPDATE Product SET Description = NULL;
GO
