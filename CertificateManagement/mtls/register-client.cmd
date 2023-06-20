openssl req -new -newkey rsa:2048 -keyout register-client.key -sha256 -nodes -out register-client.csr -config register-client.cnf
openssl req -in register-client.csr -noout -text
openssl x509 -req -days 1826 -in register-client.csr -CA ca.pem -CAkey ca.key -CAcreateserial -out register-client.pem -extfile register-client.ext
openssl pkcs12 -inkey register-client.key -in register-client.pem -export -out register-client.pfx
openssl pkcs12 -in register-client.pfx -noout -info