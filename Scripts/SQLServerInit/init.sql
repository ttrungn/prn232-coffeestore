USE master;
GO
    
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CoffeeStore')
BEGIN
    CREATE DATABASE CoffeeStore;
END
GO
