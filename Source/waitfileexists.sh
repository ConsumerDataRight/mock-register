while [ ! -f "/app/admin/_healthcheck_ready" ]; do

	echo $1

    echo 'sleeping for 5s as checking for _healthcheck_ready file.'
	
    sleep 5s

done

/usr/bin/dotnet  $1 

exit
