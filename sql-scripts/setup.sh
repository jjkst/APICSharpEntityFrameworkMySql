#!/bin/bash

# Set your MySQL credentials
MYSQL_USER="root"
MYSQL_PASSWORD="Love@Everyone12"
MYSQL_DATABASE="RukuITServicesProd"
MYSQL_HOST="localhost"  # or 'mysql' if running inside Docker

# Run the create database script first
echo "Running CreateDatabase.sql..."
mysql -h $MYSQL_HOST -u $MYSQL_USER -p$MYSQL_PASSWORD < CreateDatabase.sql

# Loop through all other .sql files except CreateDatabase.sql and execute them
for sql_file in *.sql; do
  if [[ "$sql_file" != "CreateDatabase.sql" && "$sql_file" != "CreateTestDatabase.sql" ]]; then
    echo "Running $sql_file..."
    mysql -h $MYSQL_HOST -u $MYSQL_USER -p$MYSQL_PASSWORD $MYSQL_DATABASE < "$sql_file"
  fi
done

echo "All SQL scripts executed successfully."