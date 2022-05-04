<h2>Use the pre-built image for this solution</h2>

<br />
<p>1. Pull the latest image from <a href="https://hub.docker.com/r/consumerdataright/mock-register" title="Download the from docker hub here" alt="Download the from docker hub here">Docker Hub</a></p>

<span style="display:inline-block;margin-left:1em;">
	docker pull consumerdataright/mock-register
</span>

<br />
<p>2. Run the Mock Register container</p>

<span style="display:inline-block;margin-left:1em;">
	docker run -d -h mock-register -p 7000:7000 -p 7001:7001 -p 7006:7006 --add-host=host.docker.internal:host-gateway --name mock-register consumerdataright/mock-register
	<br \><br \>
	docker run -d -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pa{}w0rd2019" -p 1433:1433 --name sql1 -h sql1 -d mcr.microsoft.com/mssql/server:2019-latest
	<br \><br \>
	Please note - This docker compose file utilises the Microsoft SQL Server Image from Docker Hub.<br \>
	The Microsoft EULA for the Microsoft SQL Server Image must be accepted to continue.<br \>
	See the Microsoft SQL Server Image on Docker Hub for more information.<br \>
	Using the above command from a MS Windows command prompt will run the database.<br \>
</span>

<br />
<p>3. Use the docker compose file to run a multi-container mock CDR Ecosystem.</p>

<span style="display:inline-block;margin-left:1em;">
	The <a href="../../Source/DockerCompose/docker-compose.yml" title="/DockerCompose/docker-compose.yml" alt="Use the docker compose file located here - /DockerCompose/docker-compose.yml">docker compose file</a> can be used to run multiple containers from the Mock CDR Ecosystem, by starting the <a href="https://hub.docker.com/editions/community/docker-ce-desktop-windows" title="Docker Desktop for Windows" alt="Docker Desktop for Windows">docker desktop</a>
	 (if using a non MS Windows environment, you will need to add this route to the network), this will be added to your hosts file and is used for inter container connectivity via your host IP Address, eg C:\Windows\System32\drivers\etc\hosts
</span>

<br />

<span style="display:inline-block;margin-left:1em;">
	###.###.###.### host.docker.internal
</span>

<br />

[<img src="./images/docker-desktop.png" height='300' width='625' alt="MS Docker Desktop"/>](./images/docker-desktop.png)

<p>4. Execute the <a href="../../Source/DockerCompose/docker-compose.yml" title="/DockerCompose/docker-compose.yml" alt="Use the docker compose file located here - /DockerCompose/docker-compose.yml">docker compose file</a>, the default configuration is to run all mock solutions and executing the comand below will run all the solutions.
</p>

<span style="display:inline-block;margin-left:1em;margin-bottom:10px;">
	docker-compose up
</span>
<br \>
<br \>

<span style="display:inline-block;margin-left:1em;margin-bottom:16px;">
	Please note - This docker compose file utilises the Microsoft SQL Server Image from Docker Hub.<br \>
	The Microsoft EULA for the Microsoft SQL Server Image must be accepted to continue.<br \>
	Replace this unset ACCEPT_MSSQL_EULA variable with a Y if you accept the EULA. eg ACCEPT_EULA=Y<br \>
	See the Microsoft SQL Server Image on Docker Hub for more information.<br \>
</span>

[<img src="./images/containers-running.png" height='300' width='625' alt="Containers Running"/>](./images/containers-running.png)

<span style="display:inline-block;margin-left:1em;margin-top:10px">
	Should you wish to switch out your own solution, remark the relevant code out of this file.<br \>
	In this example we will be simulating the switching out of our Mock Data Recipient, we are using the<br \>
	database connection string Server=host.docker.internal and the endpoints shown below to<br \>
	connect to the running containers, this will result in the Mock Data Recipient running in MS Visual Studio,<br \>
	connected to the Mock Register and the Mock Data Holder running in docker.<br \>
	For details on how to run a Mock solution in MS Visual Studio 
	see <a href="../debugging/HELP.md" title="Debug Help Guide" alt="View the Debug Help Guide.">help guide</a>
</span>
<br />
<br />

[<img src="./images/mdr-switch-out-settings.png" height='300' width='625' alt="Mock Data Recipient switched out settings"/>](./images/mdr-switch-out-settings.png)

<span style="display:inline-block;margin-left:1em;margin-top:10px;margin-bottom:10px;">
	How to build your own image instead of downloading it from docker hub.<br \>
	navigate to .\mock-register\Source<br \>
	open a command prompt and execute the following;<br \>
	docker build -f Dockerfile.container -t mock-register .<br \>
	docker run -d -h mr-host -p 7000:7000 -p 7001:7001 -p 7006:7006 --add-host=host.docker.internal:host-gateway --name mock-register mock-register<br \>
	Please note - By default, the container above will be using a MS SQL database container, using this command from a MS Windows command prompt will run the database,<br \><br \>
	docker run -d -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pa{}w0rd2019" -p 1433:1433 --name sql1 -h sql1 -d mcr.microsoft.com/mssql/server:2019-latest
</span>

<span style="display:inline-block;margin-left:1em;margin-top:10px;margin-bottom:10px;">
	You can connect to the MS SQL database container from MS Sql Server Management Studio (SSMS) using
	the following settings; <br />
	Server type: Database Engine <br />
	Server name: localhost <br />
	Authentication: SQL Server Authentication <br />
	Login: sa <br />
	Password: Pa{}w0rd2019 <br />
</span>
<br />

[<img src="./images/ssms-login-error.png" height='300' width='400' alt="SSMS Login Error"/>](./images/ssms-login-error.png)

<p>
	(Please note - if the above error occurs whilst trying to connect to the MS SQL container, the SQL Server Service MUST BE STOPPED, you can do this from SQL Server Manager)
</p>

<p>5. The running solution</p>

<span style="display:inline-block;margin-left:1em;margin-bottom:1em;">
	Our switched out Mock Data Recipient solution will now be running.
</span>

[<img src="./images/mdr-switch-out-running.png" height='300' width='625' alt="The Mock Data Recipient solution"/>](./images/mdr-switch-out-running.png)
