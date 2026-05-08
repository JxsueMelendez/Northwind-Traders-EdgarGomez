#!/bin/bash

# Wait for SQL Server to start
echo "[DB Init] Waiting for SQL Server to be ready..."
sleep 20

# Check if DB already seeded by looking for a sentinel table
RESULT=$(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = 'NorthwindDB'" 2>/dev/null)

if echo "$RESULT" | grep -q "NorthwindDB"; then
    echo "[DB Init] NorthwindDB already exists. Skipping seed."
else
    echo "[DB Init] Creating NorthwindDB and running seed script..."

    # Create the database first
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
        -Q "CREATE DATABASE NorthwindDB;" 

    # Run the Northwind seed script
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C \
        -d NorthwindDB -i /docker-entrypoint-initdb.d/instnwnd.sql

    echo "[DB Init] Seed complete!"
fi
