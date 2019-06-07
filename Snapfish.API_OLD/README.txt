Configuration on Windows 10

To make api available from outside localhost on port 5002:

Run cmd as administrator:

netsh 
interface 
portproxy>
add v4tov6 listenport=5002 connectaddress=[::1] connectport=5000
add v4tov6 listenport=5006 connectaddress=[::1] connectport=5005

To check that it worked, type: dump


Under Windows Security, Firewall & network, select: Advanced settings
In Windows Defeneder window, select "New Rule...", select Port, and add specify 5002, 5006 as open
