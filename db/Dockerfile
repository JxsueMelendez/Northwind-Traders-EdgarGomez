FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Copy seed script and entrypoint
COPY instnwnd.sql /docker-entrypoint-initdb.d/instnwnd.sql
COPY db/entrypoint.sh /usr/local/bin/db-init.sh

# Make executable
RUN chmod +x /usr/local/bin/db-init.sh

# Use a wrapper entrypoint: start SQL Server in background, run init, then keep SQL Server in foreground
CMD /bin/bash -c "/opt/mssql/bin/sqlservr & /usr/local/bin/db-init.sh && wait"
