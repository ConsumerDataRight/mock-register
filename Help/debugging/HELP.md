<h2>To get started, clone the source code</h2>
<div style="margin-left:18px;">
1. Create a folder called CDR<br />
2. Navigate to this folder<br />
3. Clone the repo as a subfolder of this folder using the following command;<br />
<div style="margin-left:18px;">
git clone https://github.com/ConsumerDataRight/mock-register.git<br />
</div>
4. Start the projects in the solution, can be done in multiple ways, examples below are from .Net command line and using MS Visual Studio<br />
</div>

<h2>.Net command line</h2>
<div style="margin-left:18px;">
<p>1. Download and install the free <a href="https://docs.microsoft.com/en-us/windows/terminal/get-started" title="Download the free Windows Terminal here" alt="Download the free MS Windows Terminal here">MS Windows Terminal</a>
<br />
2. Use the <a href="https://github.com/ConsumerDataRight/mock-register/Source/Start-Register.bat" title="Use the Start-Register .Net CLI batch file here" alt="Use the Start-Register .Net CLI batch file here">Start-Register</a> batch file to build and run the required projects to start the Mock Register,
<br />
this will create the LocalDB instance by default and seed the database with the supplied sample data.
</p>

[<img src="./images/DotNet-CLI-Running.png" height='300' width='600' alt="Start projects from .Net CLI"/>](./images/DotNet-CLI-Running.png)

<p>LocalDB is installed as part of MS Visual Studio if using MS VSCode then adding the MS SQL extension includes the LocalDB Instance.</p>
<p>You can connect to the database from MS Visual Studio using the SQL Explorer, or from MS SQL Server Management Studio (SSMS) using
	the following settings; <br />
	Server type: Database Engine <br />
	Server name: (LocalDB)\MSSQLLocalDB <br />
	Authentication: Windows Authentication<br />
</p>
</div>

<h2>MS Visual Studio</h2>
<div style="margin-left:18px;">
<p>To launch the application using MS Visual Studio, multiple projects must be selected in the solution properties.</p>

[<img src="./images/MS-Visual-Studio-Solution-properties.png" height='300' width='600' alt="Solution properties"/>](./images/MS-Visual-Studio-Solution-properties.png)

<p>1. Navigate to the solution properties.</p>

[<img src="./images/MS-Visual-Studio-Select-multiple-projects.png" height='300' width='360' alt="Projects selected to be started"/>](./images/MS-Visual-Studio-Select-multiple-projects.png)

<p>2. From there select the projects to start.</p>

[<img src="./images/MS-Visual-Studio-Start.png" height='300' width='600' alt="Start the projects"/>](./images/MS-Visual-Studio-Start.png)

<p>3. Then start the projects.</p>

[<img src="./images/MS-Visual-Studio-Running.png" height='300' width='600' alt="Projects running"/>](./images/MS-Visual-Studio-Running.png)

Output windows will be launched for each of the projects set to start.  
These will show the logging messages as sent to the console in each of the running projects.
<br />

<p><h3>Debugging the running projects using MS Visual Studio can be performed as follows;</h3>
<br />

[<img src="./images/Debug-using-MS-Visual-Studio-pt1.png" height='300' width='600' alt="Place breakpoint(s) in the projects"/>](./images/Debug-using-MS-Visual-Studio-pt1.png)

<p>1. Select the project you want to debug.
	<br />
	Stop either the MS Windows Terminal or the Output Window for the selected project to debug above.
</p>

[<img src="./images/Debug-using-MS-Visual-Studio-pt2.png" height='300' width='600' alt="Start a new debug instance"/>](./images/Debug-using-MS-Visual-Studio-pt2.png)

<p>2. Start a new debug instance for the selected project to be debugged.</p>

[<img src="./images/Debug-using-MS-Visual-Studio-pt3.png" height='300' width='600' alt="Newly started output window"/>](./images/Debug-using-MS-Visual-Studio-pt3.png)

<p>A new output window for the debug project will be started.</p>
</p>
</div>