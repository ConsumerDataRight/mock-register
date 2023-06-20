openssl req -new -newkey rsa:2048 -keyout client.key -sha256 -nodes -out client.csr -config client.cnf
openssl req -in client.csr -noout -text
openssl x509 -req -days 1826 -in client.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out client.pem -extfile client.ext
openssl pkcs12 -inkey client.key -in client.pem -export -out client.pfx
openssl pkcs12 -in client.pfx -noout -info
