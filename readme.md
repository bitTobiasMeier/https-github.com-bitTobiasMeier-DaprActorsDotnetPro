# Virtual Actors with dapr
## Example applikation for the article in the dotnetpro 9/2020

Prerequisite:

* dapr 

Requirements:

1) Start OrderActor with Visual Studio standalone (not IISExpress)
2) Open command prompt and start daprd:

daprd --app-id "orderservice" --app-port "5000" --components-path "C:\Users\TMeier\.dapr\components" --placement-host-address "localhost:6050"

3) Start the OrderClient


-------------

History:
*  07/2021 Initial version
* 07/2021: Upgrade to dapr 1.2
