# UdpService
Testet på en Azure Standard F2s v2 (2 vcpus, 4 GiB memory) VM. Sender data til en MSSQL Server i et table kaldet "Staging", i kolonnen "Message". 

# Opsætning
 - Tilføj connectionstring, samt opdater port/type i appsettings.json
 - Kompiler koden
 - Registrer servicen i windows - f.eks. i PS - "sc.exe create "NB IOT SERVICE" binpath="C:\service\UdpService.exe""
 - Start servic - f.eks. via Windows service manager, eller PS "sc.exe start "NB IOT SERVICE"



