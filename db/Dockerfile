FROM mcr.microsoft.com/mssql/server:2019-latest
ENV PATH "$PATH:/opt/mssql-tools/bin"
ENV ACCEPT_EULA "Y"
ENV SA_PASSWORD "Abcd1234#"
ENV MSSQL_PID "Express"

COPY db/createuser.sql /tmp/
COPY db/*.sh /tmp/
ENTRYPOINT [ "/bin/bash", "/tmp/entrypoint.sh" ]
