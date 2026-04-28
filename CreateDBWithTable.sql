CREATE DATABASE MyDB;
GO

USE MyDB;
GO

CREATE TABLE Employees (
    Id INT IDENTITY(1,1) PRIMARY KEY, 
    FirstName NVARCHAR(50) NOT NULL,  
    SecondName NVARCHAR(50) NOT NULL,  
    Position NVARCHAR(50) NOT NULL,   
    Salary DECIMAL(18, 2) NULL,
    HireDate DATE NOT NULL,         
    IsRemote BIT NOT NULL DEFAULT 0,
    IsDeleted BIT NOT NULL DEFAULT 0
);
GO

INSERT INTO Employees (FirstName, SecondName, Position, Salary, HireDate, IsRemote) VALUES 
('Адриано', 'Челентано', 'Стиажёр', 10.0, '2024-01-15', 0),
('Денни', 'Де Вито', 'Глава департамента', 1100.0, '2010-05-20', 1),
('Дэнни', 'Трехо', 'Мачете', 1000.0, '2015-11-01', 0),
('Чак', 'Норрис', 'Служба безопасности', 9999.99, '1999-01-01', 1);
GO