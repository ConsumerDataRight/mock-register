
while true; do

    # Check if admin api is healthy
    echo "wait-until-admin-healthy-then-start.sh -" $1 "- Waiting for https://localhost:7006/health to respond OK"
    wget --no-check-certificate --no-verbose --spider https://localhost:7006/health

    # If healthy then exit code will be 0, so exit loop
    if [ $? -eq 0 ]; then 
        break 
    fi

    # Otherwise wait for 5 seconds and try again
    echo "wait-until-admin-healthy-then-start.sh -" $1 "- sleeping for 5 seconds"
    sleep 5s

done

# Start
echo "wait-until-admin-healthy-then-start.sh -" $1 "- starting"
/usr/bin/dotnet $1 

exit
