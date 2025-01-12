CREATE TABLE Members (
    MemberID INT PRIMARY KEY IDENTITY(1,1),
    NationalCode NVARCHAR(10) NOT NULL UNIQUE, -- Unique identification like NID
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    StartDate DATETIME NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    MaxBorrowCount TINYINT DEFAULT 5, -- Max items a member can borrow
    BirthDate DATETIME NULL,
    Address NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(15) NULL,
    PostCode NVARCHAR(10) NULL,
    FatherName NVARCHAR(50) NULL,
    Debt DECIMAL(10,2) DEFAULT 0, -- Unpaid fines
    Status NVARCHAR(10) DEFAULT 'Active' -- Active, Suspended, etc.
);

CREATE TABLE Resources (
    ResourceID INT PRIMARY KEY IDENTITY(1,1),
    ResourceType NVARCHAR(20) NOT NULL, -- Book, Magazine, etc.
    Title NVARCHAR(100) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    Publisher NVARCHAR(100) NULL,
    PublishYear NVARCHAR(4) NULL,
    ISBN NVARCHAR(20) NULL UNIQUE,
    Quantity SMALLINT DEFAULT 1, -- Total copies available
    AvailableCopies SMALLINT DEFAULT 1, -- Copies currently available
    Price DECIMAL(10,2) NULL
);

CREATE TABLE BorrowRecords (
    BorrowID INT PRIMARY KEY IDENTITY(1,1),
    MemberID INT NOT NULL FOREIGN KEY REFERENCES Members(MemberID),
    ResourceID INT NOT NULL FOREIGN KEY REFERENCES Resources(ResourceID),
    BorrowDate DATETIME NOT NULL,
    DueDate DATETIME NOT NULL,
    ReturnDate DATETIME NULL,
    Fine DECIMAL(10,2) DEFAULT 0 -- Late return fine
);

CREATE TABLE Reservations (
    ReservationID INT PRIMARY KEY IDENTITY(1,1),
    MemberID INT NOT NULL FOREIGN KEY REFERENCES Members(MemberID),
    ResourceID INT NOT NULL FOREIGN KEY REFERENCES Resources(ResourceID),
    ReservationDate DATETIME NOT NULL,
    Status NVARCHAR(10) DEFAULT 'Pending' -- Pending, Completed, Expired
);

CREATE TABLE LibraryBranches (
    BranchID INT PRIMARY KEY IDENTITY(1,1),
    BranchName NVARCHAR(100) NOT NULL,
    Address NVARCHAR(200) NULL,
    PhoneNumber NVARCHAR(15) NULL
);

-- Insert sample data into Members table
INSERT INTO Members (NationalCode, FirstName, LastName, StartDate, ExpiryDate, MaxBorrowCount, BirthDate, Address, PhoneNumber, PostCode, FatherName, Debt, Status)
VALUES 
(N'0012345678', N'علی', N'محمدی', '2024-01-01', '2025-01-01', 5, '1998-05-14', N'تهران، خیابان ولیعصر', N'09121234567', N'1111111111', N'حسن', 0, N'Active'),
(N'9876543210', N'مریم', N'احمدی', '2023-06-15', '2024-06-15', 3, '1995-09-22', N'مشهد، بلوار وکیل آباد', N'09137654321', N'2222222222', N'علی', 0, N'Active'),
(N'4567890123', N'رضا', N'هاشمی', '2023-03-10', '2024-03-10', 2, '2000-01-10', N'اصفهان، میدان نقش جهان', N'09351234567', N'3333333333', N'حسین', 0, N'Active'),
(N'1234567890', N'زهرا', N'کاظمی', '2024-01-01', '2025-01-01', 4, '1997-11-30', N'شیراز، خیابان زند', N'09151237654', N'4444444444', N'محمد', 0, N'Active');

-- Insert sample data into Resources table
INSERT INTO Resources (ResourceType, Title, Author, Publisher, PublishYear, ISBN, Quantity, AvailableCopies, Price)
VALUES 
(N'کتاب', N'شازده کوچولو', N'آنتوان دو سنت اگزوپری', N'انتشارات امیرکبیر', '1943', N'1234567890123', 5, 5, 150000),
(N'کتاب', N'کلیدر', N'محمود دولت‌آبادی', N'انتشارات فرهنگ معاصر', '1977', N'9876543210123', 3, 3, 300000),
(N'مجله', N'دانستنیها', N'مجله دانستنیها', N'مجلات همشهری', '2023', N'DUMMY12345', 10, 7, 50000),
(N'پایان‌نامه', N'بررسی تاثیر فناوری اطلاعات', N'علی رضایی', N'دانشگاه تهران', '2020', N'DUMMY67890', 1, 1, 0);

-- Insert sample data into BorrowRecords table
INSERT INTO BorrowRecords (MemberID, ResourceID, BorrowDate, DueDate, ReturnDate, Fine)
VALUES 
(1, 5, '2024-01-10', '2024-01-24', NULL, 0),
(2, 6, '2023-12-15', '2023-12-29', '2023-12-28', 0),
(3, 7, '2023-11-20', '2023-12-04', '2023-12-06', 10000),
(4, 5, '2024-01-02', '2024-01-16', NULL, 0);


-- Insert sample data into Reservations table
INSERT INTO Reservations (MemberID, ResourceID, ReservationDate, Status)
VALUES 
(1, 5, '2023-12-25', N'Pending'),
(2, 6, '2024-01-01', N'Completed'),
(3, 7, '2023-12-20', N'Expired');

-- Insert sample data into LibraryBranches table (optional)
INSERT INTO LibraryBranches (BranchName, Address, PhoneNumber)
VALUES 
(N'کتابخانه مرکزی تهران', N'تهران، میدان انقلاب', N'02112345678'),
(N'کتابخانه مرکزی مشهد', N'مشهد، بلوار امام رضا', N'05112345678'),
(N'کتابخانه مرکزی اصفهان', N'اصفهان، میدان امام', N'03112345678');