# opcua-to-prometheus
Reading nodes from a OPC UA server and presenting them as a plaintext metrics file that can be read by prometheus. The reading of the nodes is done by subscriptions.

## Build and run
* Either use docker to build and run the application (best for deployment on a server)
* Use Visual Studio debugging (easiest for testing)
* Build an exe using Visual Studio

## Using docker
* copy `example.docker-compose.yml` to `docker-compose.yml` and adjust for your purposes
* copy `./opcua-to-prometheus/config.yml` to `./config.yml` fill in your tags
* execute `sudo docker-compose up --build`

Configure prometheus to use the metrics file that is available on `http://<host>:5489/metrics`

## Config file
The config file `./opcua-to-prometheus/config.yml` has the connection details of the PLC and a list of tags that will be exposed in the metrics file.
This config file is automatically reloaded when changes are detected. Automatic config file reload does not work with docker, restart the container after changing config.

Only Anonymous authentication and none encryption is used.

`OPCUAEndpoint` Endpoint of the OPC UA Server.
`SubscriptionInterval` Sampling time and minimum delay between two updates of the subscription. Can be used globally and or for each tag separatly. If nothing is specified for the tag the global value is used.
`MetricsName` The name of the metric that will be used for the prometheus metrics file.
`NodeID` NodeID of the tag in the OPC UA Server including namespace.
