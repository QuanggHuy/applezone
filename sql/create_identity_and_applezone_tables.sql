-- ============================================================
-- applezone - Khoi tao toan bo bang (Database-First, khong dung EF Migrations)
-- Chay 1 lan duy nhat sau khi container SQL Server (Docker) da "Up".
-- ============================================================

IF DB_ID('applezone') IS NULL
BEGIN
    CREATE DATABASE applezone;
END
GO

USE applezone;
GO

-- Bat buoc phai bat option nay truoc khi tao filtered index
-- (RoleNameIndex, UserNameIndex ben duoi co dieu kien WHERE ... IS NOT NULL)
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- PHAN 1: Bang Identity (AspNetUsers, AspNetRoles...)
-- Cot/kieu du lieu copy dung tu Migrations/20240624140650_CustomUserData.cs
-- de dam bao khop 100% voi ApplicationDbContext : IdentityDbContext<MyUser>
-- ============================================================

CREATE TABLE AspNetRoles (
    Id nvarchar(450) NOT NULL,
    Name nvarchar(256) NULL,
    NormalizedName nvarchar(256) NULL,
    ConcurrencyStamp nvarchar(max) NULL,
    CONSTRAINT PK_AspNetRoles PRIMARY KEY (Id)
);
GO

CREATE TABLE AspNetUsers (
    Id nvarchar(450) NOT NULL,
    Name nvarchar(max) NULL,
    DOB datetime2 NOT NULL,
    Address nvarchar(max) NULL,
    UserName nvarchar(256) NULL,
    NormalizedUserName nvarchar(256) NULL,
    Email nvarchar(256) NULL,
    NormalizedEmail nvarchar(256) NULL,
    EmailConfirmed bit NOT NULL,
    PasswordHash nvarchar(max) NULL,
    SecurityStamp nvarchar(max) NULL,
    ConcurrencyStamp nvarchar(max) NULL,
    PhoneNumber nvarchar(max) NULL,
    PhoneNumberConfirmed bit NOT NULL,
    TwoFactorEnabled bit NOT NULL,
    LockoutEnd datetimeoffset NULL,
    LockoutEnabled bit NOT NULL,
    AccessFailedCount int NOT NULL,
    CONSTRAINT PK_AspNetUsers PRIMARY KEY (Id)
);
GO

CREATE TABLE AspNetRoleClaims (
    Id int IDENTITY(1,1) NOT NULL,
    RoleId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max) NULL,
    ClaimValue nvarchar(max) NULL,
    CONSTRAINT PK_AspNetRoleClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetRoleClaims_AspNetRoles_RoleId FOREIGN KEY (RoleId)
        REFERENCES AspNetRoles (Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserClaims (
    Id int IDENTITY(1,1) NOT NULL,
    UserId nvarchar(450) NOT NULL,
    ClaimType nvarchar(max) NULL,
    ClaimValue nvarchar(max) NULL,
    CONSTRAINT PK_AspNetUserClaims PRIMARY KEY (Id),
    CONSTRAINT FK_AspNetUserClaims_AspNetUsers_UserId FOREIGN KEY (UserId)
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserLogins (
    LoginProvider nvarchar(128) NOT NULL,
    ProviderKey nvarchar(128) NOT NULL,
    ProviderDisplayName nvarchar(max) NULL,
    UserId nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserLogins PRIMARY KEY (LoginProvider, ProviderKey),
    CONSTRAINT FK_AspNetUserLogins_AspNetUsers_UserId FOREIGN KEY (UserId)
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserRoles (
    UserId nvarchar(450) NOT NULL,
    RoleId nvarchar(450) NOT NULL,
    CONSTRAINT PK_AspNetUserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_AspNetUserRoles_AspNetRoles_RoleId FOREIGN KEY (RoleId)
        REFERENCES AspNetRoles (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AspNetUserRoles_AspNetUsers_UserId FOREIGN KEY (UserId)
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE TABLE AspNetUserTokens (
    UserId nvarchar(450) NOT NULL,
    LoginProvider nvarchar(128) NOT NULL,
    Name nvarchar(128) NOT NULL,
    Value nvarchar(max) NULL,
    CONSTRAINT PK_AspNetUserTokens PRIMARY KEY (UserId, LoginProvider, Name),
    CONSTRAINT FK_AspNetUserTokens_AspNetUsers_UserId FOREIGN KEY (UserId)
        REFERENCES AspNetUsers (Id) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AspNetRoleClaims_RoleId ON AspNetRoleClaims (RoleId);
CREATE UNIQUE INDEX RoleNameIndex ON AspNetRoles (NormalizedName) WHERE NormalizedName IS NOT NULL;
CREATE INDEX IX_AspNetUserClaims_UserId ON AspNetUserClaims (UserId);
CREATE INDEX IX_AspNetUserLogins_UserId ON AspNetUserLogins (UserId);
CREATE INDEX IX_AspNetUserRoles_RoleId ON AspNetUserRoles (RoleId);
CREATE INDEX EmailIndex ON AspNetUsers (NormalizedEmail);
CREATE UNIQUE INDEX UserNameIndex ON AspNetUsers (NormalizedUserName) WHERE NormalizedUserName IS NOT NULL;
GO

-- ============================================================
-- PHAN 2: Bang nghiep vu (Category, Product, Order, OrderDetail)
-- Cot/kieu du lieu copy dung tu Models/AppleZoneContext.cs (OnModelCreating)
-- ============================================================

CREATE TABLE Category (
    Id int IDENTITY(1,1) NOT NULL,
    Name varchar(50) NOT NULL,
    CONSTRAINT PK_Category PRIMARY KEY (Id)
);
GO

CREATE TABLE Product (
    Id int IDENTITY(1,1) NOT NULL,
    Description varchar(50) NOT NULL,
    Price float NOT NULL,
    Discount float NOT NULL,
    CategoryId int NOT NULL,
    Brand nvarchar(max) NULL,
    Origin nvarchar(max) NULL,
    Guarantee nvarchar(max) NULL,
    CONSTRAINT PK_Product PRIMARY KEY (Id),
    CONSTRAINT FK_Product_Category_CategoryId FOREIGN KEY (CategoryId)
        REFERENCES Category (Id)
);
GO

CREATE TABLE [Order] (
    Id int IDENTITY(1,1) NOT NULL,
    Date datetime NOT NULL,
    Status varchar(30) NULL,
    CustomerId nvarchar(450) NULL,
    EmployeeId nvarchar(450) NULL,
    CONSTRAINT PK_Order PRIMARY KEY (Id),
    CONSTRAINT FK_Order_AspNetUsers_CustomerId FOREIGN KEY (CustomerId)
        REFERENCES AspNetUsers (Id),
    CONSTRAINT FK_Order_AspNetUsers_EmployeeId FOREIGN KEY (EmployeeId)
        REFERENCES AspNetUsers (Id)
);
GO

CREATE TABLE OrderDetail (
    Id int IDENTITY(1,1) NOT NULL,
    OrderId int NOT NULL,
    ProductId int NOT NULL,
    Quantity int NOT NULL,
    Price float NOT NULL,
    Discount float NOT NULL,
    CONSTRAINT PK_OrderDetail PRIMARY KEY (Id),
    CONSTRAINT FK_OrderDetail_Order_OrderId FOREIGN KEY (OrderId)
        REFERENCES [Order] (Id),
    CONSTRAINT FK_OrderDetail_Product_ProductId FOREIGN KEY (ProductId)
        REFERENCES Product (Id)
);
GO
