# retryCount = 0
# until sqlcmd -S mssql,1433 -U TEST -P Abcd1234# -Q "exit"
# do
#     if [retryCount++ -gt 20] then exit fi
#     sleep 1s
# done

retryCount=0
until dotnet ef database update --project /source/aspnetapp/SampleMvcApp.csproj --no-build
do
    if [retryCount++ -gt 5]; then
        exit 1
    fi

    sleep 1s
done

# exec dotnet /app/SampleMvcApp.dll
exec dotnet /app/SampleMvcApp.dll
