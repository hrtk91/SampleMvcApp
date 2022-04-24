
echo "executing createuser.sql"
retryCount=0
until sqlcmd -S localhost,1433 -U SA -P Abcd1234# -i /tmp/createuser.sql
do
    retryCount=$(($retryCount + 1))
    if [retryCount -gt 5]; then
        exit 1
    fi
    sleep 1s
done
echo "done createuser.sql"
