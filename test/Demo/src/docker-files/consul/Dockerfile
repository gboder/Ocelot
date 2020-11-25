# escape=`

FROM consul:latest AS consul

WORKDIR /consul

# Configuration file
ADD consul.server.json config/consul.json

# Certificates
ADD all/ssl/consul-agent-ca.pem ssl/consul-agent-ca.pem
ADD all/ssl/consuldemo01-server-consul-3.pem ssl/agent.pem
ADD all/ssl/consuldemo01-server-consul-3-key.pem ssl/agent-key.pem

# Policies
COPY all/policy/*.hcl /consul/policy/*.hcl
# RUN consul acl policy create -token 3b623a30-3b9d-6d58-8252-d07099c5c998 -name "agent-token-policy" -description "Agent Token Policy" -rules /consul/policy/agent-policy.hcl

# Start consul using the configuration
ENTRYPOINT ["consul","agent", "-config-file","/consul/config/consul.json"]