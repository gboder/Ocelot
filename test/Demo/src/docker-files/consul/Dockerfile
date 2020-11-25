# escape=`

FROM consul:latest AS consul

WORKDIR /consul
# Configuration file
ADD consul.server.json config/consul.json
ADD all/ssl/consul-agent-ca.pem ssl/consul-agent-ca.pem
ADD all/ssl/consuldemo01-server-consul-3.pem ssl/agent.pem
ADD all/ssl/consuldemo01-server-consul-3-key.pem ssl/agent-key.pem

# Start consul using the configuration
ENTRYPOINT ["consul","agent", "-config-file","/consul/config/consul.json"]